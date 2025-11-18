using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using FlexMaster6000.Models;

namespace FlexMaster6000.Services
{
    /// <summary>
    /// Audio mixer service for controlling slice audio levels and mute/solo
    /// Similar to Slice Master 6000 Mix tab functionality
    /// </summary>
    public class AudioMixerService
    {
        private readonly ILogger<AudioMixerService> _logger;
        private readonly RadioManager _radioManager;
        private SliceInfo _soloSlice;

        public AudioMixerService(ILogger<AudioMixerService> logger, RadioManager radioManager)
        {
            _logger = logger;
            _radioManager = radioManager;
        }

        /// <summary>
        /// Set audio gain for a specific slice
        /// </summary>
        public void SetAudioGain(string sliceId, int gain)
        {
            try
            {
                var slice = _radioManager.Slices.FirstOrDefault(s => s.SliceId == sliceId);
                if (slice == null)
                {
                    _logger.LogWarning($"Slice {sliceId} not found");
                    return;
                }

                // Clamp gain between 0-100
                gain = Math.Clamp(gain, 0, 100);

                slice.AudioGain = gain;

                // FlexLib code to set actual audio gain
                // var flexSlice = _radioManager._radio.FindSliceByIndex(sliceId);
                // if (flexSlice != null)
                // {
                //     flexSlice.AudioGain = gain;
                // }

                _logger.LogDebug($"Set audio gain for slice {sliceId} to {gain}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to set audio gain for slice {sliceId}");
            }
        }

        /// <summary>
        /// Mute a specific slice
        /// </summary>
        public void MuteSlice(string sliceId, bool mute)
        {
            try
            {
                var slice = _radioManager.Slices.FirstOrDefault(s => s.SliceId == sliceId);
                if (slice == null) return;

                slice.IsMuted = mute;

                // FlexLib code
                // var flexSlice = _radioManager._radio.FindSliceByIndex(sliceId);
                // if (flexSlice != null)
                // {
                //     flexSlice.Mute = mute;
                // }

                _logger.LogInformation($"Slice {sliceId} {(mute ? "muted" : "unmuted")}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to mute slice {sliceId}");
            }
        }

        /// <summary>
        /// Solo a specific slice (mutes all others)
        /// </summary>
        public void SoloSlice(string sliceId, bool solo)
        {
            try
            {
                var targetSlice = _radioManager.Slices.FirstOrDefault(s => s.SliceId == sliceId);
                if (targetSlice == null) return;

                if (solo)
                {
                    // Mute all other slices
                    foreach (var slice in _radioManager.Slices)
                    {
                        if (slice.SliceId != sliceId)
                        {
                            MuteSlice(slice.SliceId, true);
                            slice.IsSolo = false;
                        }
                    }

                    targetSlice.IsSolo = true;
                    MuteSlice(sliceId, false); // Ensure target slice is not muted
                    _soloSlice = targetSlice;

                    _logger.LogInformation($"Slice {sliceId} soloed");
                }
                else
                {
                    // Unmute all slices
                    foreach (var slice in _radioManager.Slices)
                    {
                        MuteSlice(slice.SliceId, false);
                        slice.IsSolo = false;
                    }

                    _soloSlice = null;
                    _logger.LogInformation("Solo mode disabled");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to solo slice {sliceId}");
            }
        }

        /// <summary>
        /// Set AGC threshold for a slice
        /// </summary>
        public void SetAgcThreshold(string sliceId, int threshold)
        {
            try
            {
                var slice = _radioManager.Slices.FirstOrDefault(s => s.SliceId == sliceId);
                if (slice == null) return;

                slice.AgcThreshold = threshold;

                // FlexLib code
                // var flexSlice = _radioManager._radio.FindSliceByIndex(sliceId);
                // if (flexSlice != null)
                // {
                //     flexSlice.AGCThreshold = threshold;
                // }

                _logger.LogDebug($"Set AGC threshold for slice {sliceId} to {threshold}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to set AGC threshold for slice {sliceId}");
            }
        }

        /// <summary>
        /// Set AGC mode (OFF, SLOW, MED, FAST)
        /// </summary>
        public void SetAgcMode(string sliceId, string mode)
        {
            try
            {
                var slice = _radioManager.Slices.FirstOrDefault(s => s.SliceId == sliceId);
                if (slice == null) return;

                slice.AgcMode = mode;

                // FlexLib code
                // var flexSlice = _radioManager._radio.FindSliceByIndex(sliceId);
                // if (flexSlice != null)
                // {
                //     flexSlice.AGCMode = mode;
                // }

                _logger.LogInformation($"Set AGC mode for slice {sliceId} to {mode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to set AGC mode for slice {sliceId}");
            }
        }

        /// <summary>
        /// Reset audio settings to default for a slice
        /// </summary>
        public void ResetAudioSettings(string sliceId)
        {
            try
            {
                SetAudioGain(sliceId, 50);
                MuteSlice(sliceId, false);
                SetAgcThreshold(sliceId, 65);
                SetAgcMode(sliceId, "MED");

                _logger.LogInformation($"Reset audio settings for slice {sliceId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to reset audio settings for slice {sliceId}");
            }
        }
    }
}
