# 🎙 Decodificatore Morse AI - RadioLogger Pro

## Panoramica

Sistema professionale di decodifica del codice Morse in tempo reale con **intelligenza artificiale integrata**. Il software utilizza tecniche avanzate di Digital Signal Processing (DSP) e Machine Learning per offrire una decodifica accurata e affidabile.

![Version](https://img.shields.io/badge/version-1.0-blue)
![.NET](https://img.shields.io/badge/.NET-7.0-purple)
![AI](https://img.shields.io/badge/AI-ML.NET-green)

---

## ✨ Caratteristiche Principali

### 🎯 Acquisizione Audio in Tempo Reale
- **Supporto multi-dispositivo**: Microfono, Line-In, dispositivi audio virtuali
- **Campionamento**: 44.1 kHz, 16-bit, mono/stereo
- **Buffer ottimizzati**: Latenza minima < 50ms
- **Monitoraggio livelli**: VU meter in tempo reale

### 🔬 Elaborazione Avanzata del Segnale (DSP)
- **FFT (Fast Fourier Transform)**: Analisi spettrale in tempo reale
- **Filtro Passa-Banda Butterworth**: Isolamento del segnale Morse (200-2000 Hz)
- **Rilevamento automatico della frequenza**: Auto-tuning sulla portante
- **AGC (Automatic Gain Control)**: Compensazione automatica dell'ampiezza
- **Noise Gate intelligente**: Riduzione del rumore di fondo
- **Envelope Detector**: Estrazione dell'inviluppo del segnale con attack/release ottimizzati

### 🧠 Intelligenza Artificiale (Machine Learning)
- **Framework**: ML.NET con algoritmi SDCA (Stochastic Dual Coordinate Ascent)
- **Classificazione Dit/Dah**: Riconoscimento pattern con confidenza
- **Auto-apprendimento**: Il modello migliora con l'uso
- **Correzione errori**: Pattern recognition per frasi comuni
- **Bootstrap dataset**: Dati sintetici iniziali per training immediato
- **Metriche di accuratezza**: Monitoring in tempo reale delle performance

### 📊 Decodifica Morse Intelligente
- **Codice Morse Internazionale Completo**:
  - Lettere A-Z
  - Numeri 0-9
  - Punteggiatura e simboli speciali
  - Prosigns (AR, BT, SK, SOS)

- **Auto-calibrazione velocità**: Rilevamento automatico WPM (Words Per Minute)
- **Adaptive timing**: Si adatta allo stile dell'operatore
- **Rilevamento errori**: Identificazione caratteri ambigui
- **Calcolo confidenza**: Affidabilità della decodifica

### 📈 Visualizzazione Professionale
- **Forma d'onda**: Visualizzazione audio in tempo reale (OxyPlot)
- **Spettro/Envelope**: Grafico dell'inviluppo del segnale
- **Statistiche live**:
  - Frequenza portante (Hz)
  - SNR - Signal to Noise Ratio (dB)
  - Velocità WPM
  - Confidenza decodifica
  - Contatori caratteri/errori
  - Accuratezza percentuale

### 💾 Funzionalità Aggiuntive
- **Esportazione risultati**: Salvataggio in formato TXT con statistiche complete
- **Log eventi**: Tracciamento completo delle operazioni
- **Interfaccia dark theme**: UI professionale e confortevole per lunghe sessioni
- **Multi-threading**: Elaborazione parallela per performance ottimali

---

## 🏗 Architettura del Sistema

```
┌─────────────────────────────────────────────────────────────┐
│                    INTERFACCIA UTENTE                        │
│                  (MorseDecoderWindow)                        │
└──────────────┬───────────────────────────────┬──────────────┘
               │                               │
      ┌────────▼────────┐            ┌────────▼────────┐
      │  Audio Capture   │            │   Visualizer    │
      │    (NAudio)      │            │   (OxyPlot)     │
      └────────┬─────────┘            └─────────────────┘
               │
      ┌────────▼────────┐
      │ Signal Processor │
      │  (FFT, Filters)  │
      └────────┬─────────┘
               │
      ┌────────▼────────┐
      │  Morse Decoder   │
      │ (Auto-calibrate) │
      └────────┬─────────┘
               │
      ┌────────▼────────┐
      │   AI Enhancer    │
      │    (ML.NET)      │
      └──────────────────┘
```

### Componenti Principali

#### 1. **MorseAudioCapture.cs**
Gestisce l'acquisizione audio in tempo reale:
- Enumerazione dispositivi audio
- Cattura stream audio con NAudio
- Conversione e normalizzazione campioni
- Calcolo livelli audio (peak, RMS)
- Buffer circolare per l'analisi

#### 2. **MorseSignalProcessor.cs**
Elaborazione avanzata del segnale:
- FFT per analisi spettrale (MathNet.Numerics)
- Filtro passa-banda biquad (Butterworth)
- Rilevamento frequenza dominante
- AGC (Automatic Gain Control)
- Envelope detection con attack/release
- Calcolo SNR (Signal to Noise Ratio)
- Noise floor estimation

#### 3. **MorseDecoder.cs**
Core della decodifica Morse:
- State machine (Idle → Signal → Gap)
- Auto-calibrazione timing (dit, dah, gaps)
- Dizionario Morse completo (ITU-R M.1677-1)
- Rilevamento automatico velocità WPM
- Calcolo confidenza decodifica
- Gestione prosigns

#### 4. **MorseAIEnhancer.cs**
Machine Learning per migliorare l'accuratezza:
- Training model con ML.NET
- Feature extraction (duration, amplitude, SNR, frequency stability)
- Classificazione Dit/Dah con probabilità
- Correzione errori comuni
- Pattern recognition per frasi radio
- Persistenza modello (save/load)

#### 5. **MorseDecoderWindow.xaml / .cs**
Interfaccia utente WPF:
- UI professionale dark theme
- Grafici tempo reale (waveform, spectrum)
- Controlli audio
- Statistiche live
- Export funzionalità
- Logging eventi

---

## 🚀 Installazione e Utilizzo

### Requisiti di Sistema
- **Sistema Operativo**: Windows 10/11 (64-bit)
- **.NET Runtime**: 7.0 o superiore
- **RAM**: Minimo 4 GB (consigliati 8 GB)
- **Audio**: Dispositivo di input (microfono, line-in, o virtual audio cable)

### Installazione Dipendenze

Il progetto utilizza NuGet per la gestione delle dipendenze:

```xml
<!-- NAudio - Acquisizione audio -->
<PackageReference Include="NAudio" Version="2.2.1" />

<!-- MathNet.Numerics - DSP e FFT -->
<PackageReference Include="MathNet.Numerics" Version="5.0.0" />

<!-- ML.NET - Machine Learning -->
<PackageReference Include="Microsoft.ML" Version="3.0.1" />

<!-- Accord.MachineLearning - Algoritmi ML aggiuntivi -->
<PackageReference Include="Accord.MachineLearning" Version="3.8.0" />

<!-- OxyPlot - Visualizzazione grafici -->
<PackageReference Include="OxyPlot.Wpf" Version="2.1.2" />
```

### Build del Progetto

```bash
# Clone o apri il progetto
cd RadioLoggerApp

# Restore delle dipendenze NuGet
dotnet restore

# Build del progetto
dotnet build --configuration Release

# Run dell'applicazione
dotnet run
```

### Avvio dell'Applicazione

1. **Apri RadioLogger**
2. **Clicca su "🎙 Morse Decoder AI"** nella barra inferiore
3. **Seleziona il dispositivo audio** dal menu a tendina
4. **Clicca "▶ Avvia Decodifica"**
5. **Invia un segnale Morse** (tramite radio, generatore toni, o file audio)
6. **Osserva la decodifica in tempo reale** nell'area di testo

---

## 📚 Guida Operativa

### Configurazione Audio

#### Dispositivi Supportati
- **Microfono**: Per segnali acustici diretti
- **Line-In**: Per collegamento diretto a ricevitore radio
- **Virtual Audio Cable**: Per decodificare audio da software (WebSDR, GQRX, etc.)

#### Livelli Audio Ottimali
- **Barra livello audio**: Dovrebbe oscillare tra 30-70%
- **SNR consigliato**: > 10 dB per decodifica affidabile
- **Evitare**: Clipping (livello > 95%) o segnale troppo debole (< 10%)

### Parametri di Decodifica

#### Auto-Calibrazione
Il sistema si auto-calibra automaticamente:
- **Velocità (WPM)**: Rileva automaticamente da 5 a 60 WPM
- **Frequenza**: Auto-tuning tra 200-2000 Hz
- **Timing**: Adatta dit/dah/gaps in base ai segnali ricevuti

#### Calibrazione Manuale (Futuro)
Nelle impostazioni sarà possibile:
- Impostare WPM fisso
- Forzare frequenza specifica
- Regolare soglie di rilevamento

### Interpretazione Risultati

#### Confidenza Decodifica
- **> 90%**: Eccellente - Segnale pulito e stabile
- **70-90%**: Buona - Decodifica affidabile
- **50-70%**: Accettabile - Possibili errori occasionali
- **< 50%**: Scarsa - Verificare livelli audio e rumore

#### Caratteri Speciali
- **?**: Carattere non riconosciuto (pattern Morse non valido)
- **Spazio**: Gap lungo tra parole rilevato
- **<AR>**, **<BT>**, **<SK>**: Prosigns radioamatoriali

### Addestramento AI

Il modello ML migliora automaticamente con l'uso:

1. **Bootstrap**: 100 esempi sintetici pre-caricati
2. **Learning continuo**: Ogni segnale decodificato viene aggiunto al dataset
3. **Re-training**: Ogni 50 nuovi esempi
4. **Accuratezza**: Monitorata in tempo reale nel pannello AI

**Training manuale**:
- Clicca **"Addestra AI"** per forzare il training immediato
- Verifica **"Accuratezza ML"** nel pannello statistiche

---

## 🧪 Testing e Validazione

### Test con Generatore Toni

Utilizza un generatore di toni audio (es. Audacity, online tone generator) per testare:

```
Frequenza: 800 Hz
Waveform: Sine
```

**Sequenze di test**:
- **"SOS"**: `... --- ...` (3 dit, 3 dah, 3 dit)
- **"CQ CQ DE"**: Chiamata generale
- **"TEST"**: `- . ... -`
- **"HELLO"**: `.... . .-.. .-.. ---`

### Test con Radio

1. **Sintonizza su una frequenza CW** (es. 7.040 MHz, 14.050 MHz)
2. **Collega l'audio del ricevitore** al line-in del PC
3. **Regola i livelli** audio per SNR ottimale
4. **Avvia la decodifica**

### Metriche di Performance

Il sistema fornisce metriche in tempo reale:
- **Caratteri decodificati**: Totale caratteri riconosciuti
- **Errori**: Caratteri ambigui (?)
- **Accuratezza**: (Corretti / Totali) × 100%
- **SNR**: Rapporto segnale/rumore in dB
- **WPM**: Velocità rilevata

---

## 🛠 Risoluzione Problemi

### Problema: Nessun Audio Rilevato

**Soluzioni**:
- Verifica che il dispositivo audio sia selezionato correttamente
- Controlla i livelli audio nel mixer di Windows
- Assicurati che l'applicazione abbia i permessi audio
- Testa con un altro dispositivo

### Problema: Decodifica Errata

**Cause comuni**:
- **Rumore di fondo eccessivo**: Migliora SNR (> 10 dB)
- **Livelli audio**: Regola guadagno microfono/line-in
- **Frequenza fuori range**: Verifica 200-2000 Hz
- **Velocità troppo alta**: Il sistema supporta fino a 60 WPM

**Soluzioni**:
- Usa filtri audio hardware o software
- Avvicina il microfono alla sorgente
- Lascia il sistema auto-calibrarsi per 10-20 caratteri
- Attendi il training del modello AI

### Problema: Bassa Confidenza

**Ottimizzazioni**:
- Migliora il rapporto segnale/rumore
- Verifica stabilità della frequenza portante
- Controlla che non ci siano distorsioni
- Addestra il modello AI con più dati

### Problema: Performance Lente

**Ottimizzazioni**:
- Chiudi applicazioni non necessarie
- Riduce dimensione FFT (impostazioni avanzate)
- Verifica utilizzo CPU/RAM
- Usa dispositivi audio con driver ASIO per latenza minima

---

## 📖 Riferimenti Tecnici

### Standard Codice Morse
- **ITU-R M.1677-1**: International Morse Code
- **Timing standard**:
  - Dit = 1 unità
  - Dah = 3 unità
  - Gap tra simboli = 1 unità
  - Gap tra lettere = 3 unità
  - Gap tra parole = 7 unità

### WPM Calculation
```
WPM = (50 × dit_length_ms) / 1200
```
Basato sulla parola standard "PARIS" (50 unità di tempo)

### SNR Calculation
```
SNR (dB) = 20 × log₁₀(signal_amplitude / noise_floor)
```

### Algoritmi Implementati

#### FFT (Fast Fourier Transform)
- **Libreria**: MathNet.Numerics
- **Dimensione**: 4096 samples
- **Window**: Hamming
- **Scopo**: Rilevamento frequenza dominante

#### Filtro Butterworth Passa-Banda
- **Ordine**: 2° ordine (biquad)
- **Tipo**: Band-pass IIR
- **Q Factor**: Dinamico basato su bandwidth
- **Scopo**: Isolamento segnale Morse

#### Envelope Detector
- **Attack time**: 1 ms
- **Release time**: 5 ms
- **Scopo**: Estrazione inviluppo per rilevamento dit/dah

#### ML Algorithm (ML.NET)
- **Trainer**: SDCA (Stochastic Dual Coordinate Ascent)
- **Features**: 6 (duration, amplitude, SNR, freq_stability, rise_time, fall_time)
- **Classes**: 2 (Dit, Dah)
- **Output**: Multi-class classification con probabilità

---

## 🔮 Roadmap Future

### v1.1 - Miglioramenti UI
- [ ] Finestra impostazioni avanzate
- [ ] Temi personalizzabili
- [ ] Waterfall display dello spettro
- [ ] Registrazione sessioni

### v1.2 - Funzionalità Avanzate
- [ ] Decodifica multi-canale (stereo)
- [ ] Supporto per RTTY e PSK31
- [ ] Integrazione con contest logger
- [ ] Database QSO automatico

### v1.3 - AI Enhancement
- [ ] Deep Learning con reti neurali (LSTM)
- [ ] Denoising AI-powered
- [ ] Riconoscimento "fist" (stile operatore)
- [ ] Correzione errori context-aware

### v1.4 - Radio Integration
- [ ] Controllo CAT per radio (Hamlib)
- [ ] Auto-tuning frequenza radio
- [ ] Split-screen RX/TX
- [ ] Keyer integrato per trasmissione

---

## 👨‍💻 Contributori e Supporto

### Autore
Sistema sviluppato con **Claude AI** (Anthropic) per RadioLogger Pro

### Licenza
Questo progetto è fornito "as-is" per scopi educativi e radioamatoriali.

### Supporto
Per bug, richieste di funzionalità o domande:
- Apri una issue nel repository
- Contatta il maintainer del progetto

---

## 📝 Note Tecniche

### Ottimizzazioni Performance

#### Buffer Management
- Buffer circolare per audio: 2× sample rate
- Buffer grafici: 1000 samples
- Queue thread-safe per elaborazione

#### Multi-threading
- Audio capture: Thread dedicato (NAudio)
- Signal processing: Asincrono
- UI updates: Dispatcher (100ms timer)
- ML training: Background task

#### Memory Management
- Garbage collection ottimizzata
- Dispose pattern per risorse non-managed
- Weak references dove appropriato

### Considerazioni sulla Latenza

**Latenza totale tipica**: < 100ms
- Audio buffer: 20-50ms
- Processing: 10-20ms
- UI update: 10-30ms
- Decoder state machine: < 5ms

**Per applicazioni critiche**: Usa dispositivi ASIO e riduci buffer size.

---

## 🎓 Risorse Educative

### Imparare il Codice Morse
- [ARRL Morse Code Course](http://www.arrl.org/morse-code)
- [LCWO - Learn CW Online](https://lcwo.net/)
- [CW Academy](https://cwops.org/cw-academy/)

### Digital Signal Processing
- [The Scientist and Engineer's Guide to DSP](http://www.dspguide.com/)
- [Understanding Digital Signal Processing (Richard Lyons)](https://www.amazon.com/Understanding-Digital-Signal-Processing-3rd/dp/0137027419)

### Machine Learning
- [ML.NET Documentation](https://docs.microsoft.com/en-us/dotnet/machine-learning/)
- [Hands-On Machine Learning with Scikit-Learn and TensorFlow](https://www.oreilly.com/library/view/hands-on-machine-learning/9781492032632/)

---

## 🎉 Ringraziamenti

Grazie alle seguenti librerie open-source:
- **NAudio** - Audio processing
- **MathNet.Numerics** - Mathematics and DSP
- **ML.NET** - Machine Learning
- **OxyPlot** - Data visualization
- **WPF** - User interface framework

E alla community radioamatoriale per test e feedback!

---

**73 de RadioLogger Pro** 📻✨

*Happy decoding!*
