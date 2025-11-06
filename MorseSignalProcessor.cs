using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;

namespace RadioLoggerApp.MorseDecoder
{
    /// <summary>
    /// Processore avanzato di segnale per l'analisi audio del codice Morse
    /// Implementa FFT, filtri digitali, rilevamento automatico della frequenza e noise reduction
    /// </summary>
    public class MorseSignalProcessor
    {
        private readonly int sampleRate;
        private readonly int fftSize;
        private readonly double[] window;

        // Parametri del filtro passa-banda
        private double targetFrequency;
        private readonly double bandwidth;

        // Filtri IIR (Infinite Impulse Response)
        private readonly List<double> filterStateX;
        private readonly List<double> filterStateY;

        // Rilevamento automatico della frequenza
        private readonly Queue<double> frequencyHistory;
        private const int FrequencyHistorySize = 10;

        // Noise gate e AGC (Automatic Gain Control)
        private double noiseFloor;
        private readonly Queue<double> noiseFloorHistory;
        private double agcGain;

        // Rilevamento envelope per il segnale Morse
        private double envelopeLevel;
        private readonly double attackTime;
        private readonly double releaseTime;
        private double envelopeAttackCoef;
        private double envelopeReleaseCoef;

        public double TargetFrequency
        {
            get => targetFrequency;
            set
            {
                targetFrequency = value;
                CalculateFilterCoefficients();
            }
        }

        public double NoiseFloor => noiseFloor;
        public double CurrentFrequency { get; private set; }
        public double SignalStrength { get; private set; }
        public double SNR { get; private set; } // Signal to Noise Ratio

        /// <summary>
        /// Inizializza il processore di segnale
        /// </summary>
        public MorseSignalProcessor(int sampleRate = 44100, int fftSize = 4096,
                                    double targetFrequency = 800, double bandwidth = 100)
        {
            this.sampleRate = sampleRate;
            this.fftSize = fftSize;
            this.targetFrequency = targetFrequency;
            this.bandwidth = bandwidth;

            // Inizializza la finestra di Hamming per la FFT
            this.window = Window.Hamming(fftSize);

            // Inizializza gli stati del filtro
            this.filterStateX = new List<double>(new double[3]);
            this.filterStateY = new List<double>(new double[3]);

            // Inizializza le code
            this.frequencyHistory = new Queue<double>(FrequencyHistorySize);
            this.noiseFloorHistory = new Queue<double>(100);

            // Parametri AGC e noise gate
            this.noiseFloor = 0.01;
            this.agcGain = 1.0;

            // Parametri envelope detector (in secondi)
            this.attackTime = 0.001;  // 1ms
            this.releaseTime = 0.005; // 5ms

            CalculateEnvelopeCoefficients();
            CalculateFilterCoefficients();
        }

        /// <summary>
        /// Elabora un buffer di campioni audio
        /// </summary>
        public ProcessedSignalData ProcessAudioBuffer(float[] samples)
        {
            if (samples == null || samples.Length == 0)
            {
                return new ProcessedSignalData();
            }

            // 1. Rilevamento automatico della frequenza dominante
            DetectDominantFrequency(samples);

            // 2. Applica il filtro passa-banda
            var filteredSamples = ApplyBandpassFilter(samples);

            // 3. Rilevamento envelope (ampiezza del segnale)
            var envelope = CalculateEnvelope(filteredSamples);

            // 4. Aggiorna il noise floor
            UpdateNoiseFloor(envelope);

            // 5. Applica AGC (Automatic Gain Control)
            var normalizedEnvelope = ApplyAGC(envelope);

            // 6. Calcola SNR (Signal to Noise Ratio)
            CalculateSNR(normalizedEnvelope);

            return new ProcessedSignalData
            {
                FilteredSamples = filteredSamples,
                Envelope = normalizedEnvelope,
                Frequency = CurrentFrequency,
                NoiseFloor = noiseFloor,
                SignalStrength = SignalStrength,
                SNR = SNR,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// Rileva la frequenza dominante usando FFT
        /// </summary>
        private void DetectDominantFrequency(float[] samples)
        {
            if (samples.Length < fftSize)
            {
                return; // Non abbastanza dati
            }

            // Prepara i dati per la FFT
            var complexSamples = new Complex[fftSize];

            for (int i = 0; i < fftSize && i < samples.Length; i++)
            {
                // Applica la finestra di Hamming
                complexSamples[i] = new Complex(samples[i] * window[i], 0);
            }

            // Esegui la FFT
            Fourier.Forward(complexSamples, FourierOptions.Default);

            // Calcola lo spettro di potenza
            double maxMagnitude = 0;
            int maxIndex = 0;

            // Cerca solo nelle frequenze di interesse (200-2000 Hz per il Morse)
            int minBin = (int)(200.0 * fftSize / sampleRate);
            int maxBin = (int)(2000.0 * fftSize / sampleRate);

            for (int i = minBin; i < maxBin && i < complexSamples.Length / 2; i++)
            {
                double magnitude = complexSamples[i].Magnitude;

                if (magnitude > maxMagnitude)
                {
                    maxMagnitude = magnitude;
                    maxIndex = i;
                }
            }

            // Calcola la frequenza dominante
            double detectedFreq = maxIndex * (double)sampleRate / fftSize;

            // Aggiorna lo storico e calcola la media mobile
            frequencyHistory.Enqueue(detectedFreq);
            if (frequencyHistory.Count > FrequencyHistorySize)
            {
                frequencyHistory.Dequeue();
            }

            CurrentFrequency = frequencyHistory.Average();
            SignalStrength = maxMagnitude;

            // Auto-tuning: aggiorna la frequenza target se il segnale è forte
            if (maxMagnitude > noiseFloor * 10)
            {
                TargetFrequency = CurrentFrequency;
            }
        }

        /// <summary>
        /// Applica un filtro passa-banda Butterworth del secondo ordine
        /// </summary>
        private float[] ApplyBandpassFilter(float[] samples)
        {
            var filtered = new float[samples.Length];

            for (int i = 0; i < samples.Length; i++)
            {
                filtered[i] = (float)ApplyBiquadFilter(samples[i]);
            }

            return filtered;
        }

        /// <summary>
        /// Filtro biquad (Direct Form I)
        /// </summary>
        private double ApplyBiquadFilter(double input)
        {
            // Coefficienti calcolati in CalculateFilterCoefficients()
            // H(z) = (b0 + b1*z^-1 + b2*z^-2) / (a0 + a1*z^-1 + a2*z^-2)

            // Per semplicità, usiamo un filtro passa-banda semplice
            double Q = targetFrequency / bandwidth;
            double omega = 2.0 * Math.PI * targetFrequency / sampleRate;
            double alpha = Math.Sin(omega) / (2.0 * Q);

            double b0 = alpha;
            double b1 = 0;
            double b2 = -alpha;
            double a0 = 1 + alpha;
            double a1 = -2 * Math.Cos(omega);
            double a2 = 1 - alpha;

            // Normalizza
            b0 /= a0;
            b1 /= a0;
            b2 /= a0;
            a1 /= a0;
            a2 /= a0;

            // Applica il filtro
            double output = b0 * input + b1 * filterStateX[0] + b2 * filterStateX[1]
                           - a1 * filterStateY[0] - a2 * filterStateY[1];

            // Aggiorna gli stati
            filterStateX[1] = filterStateX[0];
            filterStateX[0] = input;
            filterStateY[1] = filterStateY[0];
            filterStateY[0] = output;

            return output;
        }

        /// <summary>
        /// Calcola l'envelope (inviluppo) del segnale
        /// </summary>
        private double[] CalculateEnvelope(float[] samples)
        {
            var envelope = new double[samples.Length];

            for (int i = 0; i < samples.Length; i++)
            {
                double rectified = Math.Abs(samples[i]);

                // Envelope detector con attack e release
                if (rectified > envelopeLevel)
                {
                    // Attack
                    envelopeLevel += (rectified - envelopeLevel) * envelopeAttackCoef;
                }
                else
                {
                    // Release
                    envelopeLevel += (rectified - envelopeLevel) * envelopeReleaseCoef;
                }

                envelope[i] = envelopeLevel;
            }

            return envelope;
        }

        /// <summary>
        /// Aggiorna il noise floor (livello di rumore di fondo)
        /// </summary>
        private void UpdateNoiseFloor(double[] envelope)
        {
            if (envelope.Length == 0) return;

            // Usa i percentili bassi per stimare il rumore
            var sorted = envelope.OrderBy(x => x).ToArray();
            double percentile10 = sorted[(int)(sorted.Length * 0.1)];

            noiseFloorHistory.Enqueue(percentile10);
            if (noiseFloorHistory.Count > 100)
            {
                noiseFloorHistory.Dequeue();
            }

            noiseFloor = noiseFloorHistory.Average();
        }

        /// <summary>
        /// Applica l'AGC (Automatic Gain Control)
        /// </summary>
        private double[] ApplyAGC(double[] envelope)
        {
            if (envelope.Length == 0) return envelope;

            double maxLevel = envelope.Max();

            // Calcola il guadagno target (mantieni il segnale tra 0.3 e 0.7)
            double targetLevel = 0.5;
            double targetGain = maxLevel > 0 ? targetLevel / maxLevel : 1.0;

            // Smooth gain changes
            agcGain += (targetGain - agcGain) * 0.1;

            // Limita il guadagno
            agcGain = Math.Clamp(agcGain, 0.1, 10.0);

            // Applica il guadagno
            return envelope.Select(x => x * agcGain).ToArray();
        }

        /// <summary>
        /// Calcola il rapporto segnale/rumore (SNR)
        /// </summary>
        private void CalculateSNR(double[] envelope)
        {
            if (envelope.Length == 0) return;

            double signalLevel = envelope.Max();
            SNR = noiseFloor > 0 ? 20 * Math.Log10(signalLevel / noiseFloor) : 0;
        }

        /// <summary>
        /// Calcola i coefficienti dell'envelope detector
        /// </summary>
        private void CalculateEnvelopeCoefficients()
        {
            envelopeAttackCoef = 1.0 - Math.Exp(-1.0 / (sampleRate * attackTime));
            envelopeReleaseCoef = 1.0 - Math.Exp(-1.0 / (sampleRate * releaseTime));
        }

        /// <summary>
        /// Calcola i coefficienti del filtro
        /// </summary>
        private void CalculateFilterCoefficients()
        {
            // I coefficienti vengono calcolati dinamicamente nel filtro biquad
            // Questo metodo è qui per future ottimizzazioni
        }

        /// <summary>
        /// Resetta lo stato del processore
        /// </summary>
        public void Reset()
        {
            for (int i = 0; i < filterStateX.Count; i++)
            {
                filterStateX[i] = 0;
                filterStateY[i] = 0;
            }

            envelopeLevel = 0;
            frequencyHistory.Clear();
            noiseFloorHistory.Clear();
            agcGain = 1.0;
        }
    }

    /// <summary>
    /// Dati del segnale processato
    /// </summary>
    public class ProcessedSignalData
    {
        public float[] FilteredSamples { get; set; } = Array.Empty<float>();
        public double[] Envelope { get; set; } = Array.Empty<double>();
        public double Frequency { get; set; }
        public double NoiseFloor { get; set; }
        public double SignalStrength { get; set; }
        public double SNR { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
