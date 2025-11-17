using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RadioLoggerApp.SliceMaster
{
    /// <summary>
    /// Gestisce la connessione alla radio FlexRadio tramite API SmartSDR 4.0
    /// </summary>
    public class FlexRadioConnection
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _networkStream;
        private StreamReader? _streamReader;
        private StreamWriter? _streamWriter;
        private Thread? _receiveThread;
        private bool _isConnected;
        private string _radioIp = "";
        private int _radioPort = 4992; // Porta predefinita SmartSDR API

        public event EventHandler<string>? MessageReceived;
        public event EventHandler<bool>? ConnectionStatusChanged;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsConnected => _isConnected;
        public string RadioIp => _radioIp;

        /// <summary>
        /// Connette alla radio FlexRadio
        /// </summary>
        public async Task<bool> ConnectAsync(string ipAddress, int port = 4992)
        {
            try
            {
                if (_isConnected)
                {
                    await DisconnectAsync();
                }

                _radioIp = ipAddress;
                _radioPort = port;

                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(IPAddress.Parse(ipAddress), port);

                _networkStream = _tcpClient.GetStream();
                _streamReader = new StreamReader(_networkStream, Encoding.ASCII);
                _streamWriter = new StreamWriter(_networkStream, Encoding.ASCII) { AutoFlush = true };

                _isConnected = true;
                ConnectionStatusChanged?.Invoke(this, true);

                // Avvia il thread di ricezione messaggi
                _receiveThread = new Thread(ReceiveMessages) { IsBackground = true };
                _receiveThread.Start();

                // Invia comando di handshake
                await SendCommandAsync("client program SliceMaster");

                return true;
            }
            catch (Exception ex)
            {
                _isConnected = false;
                ConnectionStatusChanged?.Invoke(this, false);
                ErrorOccurred?.Invoke(this, $"Errore connessione: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnette dalla radio
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                _isConnected = false;

                if (_streamWriter != null)
                {
                    await _streamWriter.WriteLineAsync("client disconnect");
                    _streamWriter.Close();
                    _streamWriter = null;
                }

                _streamReader?.Close();
                _streamReader = null;

                _networkStream?.Close();
                _networkStream = null;

                _tcpClient?.Close();
                _tcpClient = null;

                _receiveThread?.Join(1000);
                _receiveThread = null;

                ConnectionStatusChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Errore disconnessione: {ex.Message}");
            }
        }

        /// <summary>
        /// Invia un comando alla radio
        /// </summary>
        public async Task<bool> SendCommandAsync(string command)
        {
            try
            {
                if (!_isConnected || _streamWriter == null)
                {
                    return false;
                }

                await _streamWriter.WriteLineAsync(command);
                return true;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Errore invio comando: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Thread per ricevere messaggi dalla radio
        /// </summary>
        private void ReceiveMessages()
        {
            try
            {
                while (_isConnected && _streamReader != null)
                {
                    string? message = _streamReader.ReadLine();
                    if (message != null)
                    {
                        MessageReceived?.Invoke(this, message);
                    }
                    else
                    {
                        // Connessione persa
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Errore ricezione: {ex.Message}");
            }
            finally
            {
                if (_isConnected)
                {
                    _isConnected = false;
                    ConnectionStatusChanged?.Invoke(this, false);
                }
            }
        }

        /// <summary>
        /// Richiede la lista delle radio disponibili sulla rete
        /// </summary>
        public static async Task<List<RadioInfo>> DiscoverRadiosAsync()
        {
            var radios = new List<RadioInfo>();

            try
            {
                // Implementazione discovery tramite UDP broadcast (porta 4992)
                using var udpClient = new UdpClient();
                udpClient.EnableBroadcast = true;

                var discoverMessage = Encoding.ASCII.GetBytes("client discovery");
                await udpClient.SendAsync(discoverMessage, discoverMessage.Length,
                    new IPEndPoint(IPAddress.Broadcast, 4992));

                // Attende risposte per 2 secondi
                var receiveTask = udpClient.ReceiveAsync();
                if (await Task.WhenAny(receiveTask, Task.Delay(2000)) == receiveTask)
                {
                    var result = await receiveTask;
                    string response = Encoding.ASCII.GetString(result.Buffer);

                    // Parser della risposta (formato dipende da SmartSDR)
                    var radio = new RadioInfo
                    {
                        IpAddress = result.RemoteEndPoint.Address.ToString(),
                        Model = "FlexRadio",
                        SerialNumber = "Unknown"
                    };
                    radios.Add(radio);
                }
            }
            catch (Exception)
            {
                // Discovery fallito, ritorna lista vuota
            }

            return radios;
        }
    }

    /// <summary>
    /// Informazioni su una radio scoperta
    /// </summary>
    public class RadioInfo
    {
        public string IpAddress { get; set; } = "";
        public string Model { get; set; } = "";
        public string SerialNumber { get; set; } = "";
        public override string ToString() => $"{Model} ({IpAddress})";
    }
}
