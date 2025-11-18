using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FlexMaster6000.Models;

namespace FlexMaster6000.Services
{
    /// <summary>
    /// Ham Radio Deluxe (HRD) TCP protocol server
    /// Provides per-slice and TX-following HRD TCP client connections
    /// Compatible with SmartSDR 4.0
    /// </summary>
    public class HrdTcpServer : IDisposable
    {
        private readonly ILogger<HrdTcpServer> _logger;
        private readonly RadioManager _radioManager;
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private bool _isRunning;

        public int Port { get; private set; }
        public string TargetSliceId { get; set; } // null = TX following mode

        public HrdTcpServer(ILogger<HrdTcpServer> logger, RadioManager radioManager, int port = 7809)
        {
            _logger = logger;
            _radioManager = radioManager;
            Port = port;
        }

        /// <summary>
        /// Start the HRD TCP server
        /// </summary>
        public async Task StartAsync()
        {
            if (_isRunning)
            {
                _logger.LogWarning("HRD TCP server already running");
                return;
            }

            try
            {
                _listener = new TcpListener(IPAddress.Any, Port);
                _listener.Start();
                _cts = new CancellationTokenSource();
                _isRunning = true;

                _logger.LogInformation($"HRD TCP server started on port {Port}");

                // Accept connections in background
                _ = Task.Run(() => AcceptClientsAsync(_cts.Token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start HRD TCP server");
                throw;
            }
        }

        /// <summary>
        /// Stop the HRD TCP server
        /// </summary>
        public void Stop()
        {
            if (!_isRunning) return;

            try
            {
                _cts?.Cancel();
                _listener?.Stop();
                _isRunning = false;

                _logger.LogInformation("HRD TCP server stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping HRD TCP server");
            }
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var client = await _listener.AcceptTcpClientAsync();
                    _logger.LogInformation($"HRD TCP client connected from {client.Client.RemoteEndPoint}");

                    // Handle client in background
                    _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogError(ex, "Error accepting HRD TCP client");
                    }
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            try
            {
                using var stream = client.GetStream();
                var buffer = new byte[4096];

                while (!cancellationToken.IsCancellationRequested && client.Connected)
                {
                    var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) break;

                    var command = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                    _logger.LogDebug($"HRD command received: {command}");

                    var response = ProcessHrdCommand(command);
                    if (!string.IsNullOrEmpty(response))
                    {
                        var responseBytes = Encoding.ASCII.GetBytes(response + "\r\n");
                        await stream.WriteAsync(responseBytes, 0, responseBytes.Length, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling HRD TCP client");
            }
            finally
            {
                client?.Close();
                _logger.LogInformation("HRD TCP client disconnected");
            }
        }

        /// <summary>
        /// Process HRD protocol commands
        /// Reference: HRD TCP protocol documentation
        /// </summary>
        private string ProcessHrdCommand(string command)
        {
            try
            {
                var parts = command.Split(new[] { ' ' }, 2);
                var cmd = parts[0].ToUpper();

                // Get target slice (either specific slice or TX slice)
                var targetSlice = GetTargetSlice();
                if (targetSlice == null && cmd != "GET ID")
                {
                    return "ERROR: No target slice";
                }

                switch (cmd)
                {
                    case "GET":
                        if (parts.Length < 2) return "ERROR";
                        return ProcessGetCommand(parts[1], targetSlice);

                    case "SET":
                        if (parts.Length < 2) return "ERROR";
                        return ProcessSetCommand(parts[1], targetSlice);

                    case "GET-FREQUENCIES":
                        return GetFrequencies();

                    case "GET-MODES":
                        return GetModes();

                    default:
                        _logger.LogWarning($"Unknown HRD command: {cmd}");
                        return "ERROR";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing HRD command: {command}");
                return "ERROR";
            }
        }

        private string ProcessGetCommand(string parameter, SliceInfo slice)
        {
            switch (parameter.ToUpper())
            {
                case "ID":
                    return "FlexMaster6000";

                case "FREQUENCY":
                    return ((long)(slice.Frequency)).ToString();

                case "MODE":
                    return slice.Mode ?? "USB";

                case "STATUS":
                    return slice.IsActive ? "ONLINE" : "OFFLINE";

                case "CONTEXT":
                    return $"Slice {slice.SliceLetter}";

                default:
                    return "ERROR";
            }
        }

        private string ProcessSetCommand(string parameter, SliceInfo slice)
        {
            try
            {
                var parts = parameter.Split(new[] { ' ' }, 2);
                if (parts.Length < 2) return "ERROR";

                var prop = parts[0].ToUpper();
                var value = parts[1];

                switch (prop)
                {
                    case "FREQUENCY":
                        if (double.TryParse(value, out var freq))
                        {
                            _radioManager.SetSliceFrequency(slice.SliceId, freq);
                            return "OK";
                        }
                        return "ERROR";

                    case "MODE":
                        _radioManager.SetSliceMode(slice.SliceId, value.ToUpper());
                        return "OK";

                    case "TX":
                        // Set TX on/off
                        if (value == "1" || value.ToUpper() == "ON")
                        {
                            // Enable TX on this slice
                            slice.IsTxSlice = true;
                        }
                        return "OK";

                    default:
                        return "ERROR";
                }
            }
            catch
            {
                return "ERROR";
            }
        }

        private string GetFrequencies()
        {
            // Return list of common frequencies for HRD
            return "1800000|3500000|7000000|10100000|14000000|18068000|21000000|24890000|28000000|50000000";
        }

        private string GetModes()
        {
            // Return supported modes
            return "LSB|USB|CW|FM|AM|DIGL|DIGU|SAM|DRM";
        }

        private SliceInfo GetTargetSlice()
        {
            if (!string.IsNullOrEmpty(TargetSliceId))
            {
                // Return specific slice
                return _radioManager.Slices.FirstOrDefault(s => s.SliceId == TargetSliceId);
            }
            else
            {
                // Return TX slice (TX following mode)
                return _radioManager.Slices.FirstOrDefault(s => s.IsTxSlice);
            }
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
        }
    }
}
