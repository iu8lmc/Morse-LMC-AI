# ⚙ Finestra Impostazioni Avanzate - Morse Decoder AI

## Panoramica

La finestra **Impostazioni Avanzate** permette di configurare tutti i parametri del decodificatore Morse AI, offrendo un controllo granulare sulle funzionalità DSP, Machine Learning e Audio.

---

## 📋 Sezioni delle Impostazioni

### 📻 Parametri Codice Morse

#### Auto-calibrazione WPM
- **Abilitato di default**: Il sistema rileva automaticamente la velocità del codice Morse
- **Disabilitato**: Permette di impostare manualmente la velocità WPM (5-60)
- **Consiglio**: Lasciare abilitato per risultati migliori

#### WPM (Words Per Minute)
- **Range**: 5-60 WPM
- **Default**: 20 WPM
- **Nota**: Basato sulla parola standard "PARIS" (50 unità di tempo)

#### Rilevamento Automatico Frequenza
- **Abilitato di default**: Il sistema rileva automaticamente la frequenza portante
- **Disabilitato**: Permette di forzare una frequenza specifica

#### Frequenza Target
- **Range**: 200-2000 Hz
- **Default**: 800 Hz
- **Uso**: Ottimale per segnali CW radioamatoriali standard

#### Soglia Rilevamento
- **Range**: 0.1-0.9
- **Default**: 0.3
- **Descrizione**: Livello minimo per considerare un segnale come "presente"
- **Suggerimento**:
  - Aumentare se troppi falsi positivi
  - Diminuire se segnali deboli non vengono rilevati

---

### 🔊 Digital Signal Processing

#### Bandwidth Filtro
- **Range**: 50-500 Hz
- **Default**: 100 Hz
- **Descrizione**: Larghezza del filtro passa-banda Butterworth
- **Effetto**:
  - Valori bassi → Più selettivo (migliore per segnali puliti)
  - Valori alti → Meno selettivo (migliore per segnali variabili)

#### AGC (Automatic Gain Control)
- **Default**: Abilitato
- **Funzione**: Normalizza automaticamente l'ampiezza del segnale
- **Beneficio**: Compensa variazioni di volume dell'audio

#### Noise Gate
- **Default**: Abilitato
- **Funzione**: Riduce il rumore di fondo durante i silenzi
- **Beneficio**: Migliora il rapporto segnale/rumore (SNR)

#### FFT Size
- **Opzioni**: 1024, 2048, 4096, 8192 samples
- **Default**: 4096
- **Trade-off**:
  - Valori bassi → Minore latenza, minor precisione frequenza
  - Valori alti → Maggiore latenza, maggior precisione frequenza

---

### 🧠 Machine Learning

#### Abilita AI
- **Default**: Abilitato
- **Funzione**: Usa ML.NET per classificare Dit/Dah
- **Beneficio**: Maggiore accuratezza rispetto alla classificazione basata su regole

#### Re-training Interval
- **Range**: 10-200 campioni
- **Default**: 50
- **Descrizione**: Ogni N campioni, il modello viene riaddestrato
- **Suggerimento**:
  - Valori bassi → Training più frequente, maggiore adattamento
  - Valori alti → Training meno frequente, maggiore stabilità

#### Min Dataset Size
- **Range**: 50-500 campioni
- **Default**: 100
- **Descrizione**: Dimensione minima del dataset prima di iniziare il training
- **Nota**: Il sistema parte con 100 esempi sintetici

#### Correzione Errori AI
- **Default**: Abilitato
- **Funzione**: Corregge errori comuni usando pattern recognition
- **Esempio**: "YELLO" → "HELLO", "TLST" → "TEST"

#### Gestione Modello

##### 💾 Salva Modello
- Esporta il modello ML addestrato in un file `.mdl`
- Utile per:
  - Backup del modello ottimizzato
  - Condivisione tra installazioni
  - Preservare il training fatto

##### 📂 Carica Modello
- Importa un modello ML precedentemente salvato
- Il modello viene caricato all'avvio della decodifica

##### 🔄 Reset Modello
- Elimina il modello corrente
- Il sistema ripartirà con dati sintetici
- Usa questa opzione se il modello ha "appreso" errori

---

### 🎤 Configurazione Audio

#### Buffer Size
- **Range**: 10-200 ms
- **Default**: 50 ms
- **Trade-off**:
  - Valori bassi → Minore latenza, possibili glitch audio
  - Valori alti → Maggiore latenza, audio più stabile

#### Sample Rate
- **Opzioni**: 22050, 44100, 48000 Hz
- **Default**: 44100 Hz
- **Suggerimenti**:
  - 22050 Hz: Sistemi con risorse limitate
  - 44100 Hz: Standard CD audio, ottimo bilanciamento
  - 48000 Hz: Professional audio, massima qualità

#### Latenza Stimata
- **Tipica**: ~70ms (buffer + processing + UI)
- **Componenti**:
  - Buffer audio: 20-50ms
  - DSP processing: 10-20ms
  - UI update: 10-30ms
  - Decoder state machine: < 5ms

---

## 💾 Persistenza delle Impostazioni

### Percorso File
Le impostazioni sono salvate automaticamente in:
```
Windows: %APPDATA%\RadioLoggerApp\MorseDecoder\settings.json
Linux:   ~/.config/RadioLoggerApp/MorseDecoder/settings.json
macOS:   ~/Library/Application Support/RadioLoggerApp/MorseDecoder/settings.json
```

### Formato JSON
Le impostazioni sono salvate in formato JSON leggibile:
```json
{
  "autoCalibrationEnabled": true,
  "initialWPM": 20,
  "autoFrequencyDetection": true,
  "targetFrequency": 800,
  "detectionThreshold": 0.3,
  "bandwidth": 100,
  "agcEnabled": true,
  "noiseGateEnabled": true,
  "fftSize": 4096,
  "aiEnabled": true,
  "retrainingInterval": 50,
  "minDatasetSize": 100,
  "errorCorrectionEnabled": true,
  "modelPath": "",
  "bufferSizeMs": 50,
  "sampleRate": 44100,
  "version": "1.0",
  "lastModified": "2025-01-15T10:30:00"
}
```

### Backup Automatico
Il sistema crea automaticamente backup con timestamp quando si salvano nuove impostazioni.

---

## 🔧 Utilizzo delle Impostazioni

### Primo Avvio
1. All'avvio, il sistema carica le impostazioni da file
2. Se il file non esiste, usa valori predefiniti
3. I valori predefiniti sono ottimizzati per la maggior parte degli scenari

### Modificare le Impostazioni
1. **Durante la decodifica**: Le modifiche saranno applicate al prossimo avvio
2. **A riposo**: Le modifiche sono applicate immediatamente reinizializzando i componenti

### Ripristinare Default
Il pulsante **"🔄 Ripristina Default"** reimposta tutti i valori ai default ottimali.

---

## 📊 Scenari d'Uso Ottimali

### Scenario 1: Segnale Pulito da Radio Professionale
```
WPM: Auto-calibrazione ✓
Frequenza: Auto-detection ✓
Soglia: 0.3
Bandwidth: 100 Hz
FFT Size: 4096
AI: Abilitato ✓
```
**Risultato atteso**: Accuratezza > 95%

### Scenario 2: Segnale Debole con Rumore
```
WPM: Auto-calibrazione ✓
Frequenza: Auto-detection ✓
Soglia: 0.2 (più bassa)
Bandwidth: 150 Hz (più larga)
FFT Size: 8192 (maggiore precisione)
AGC: Abilitato ✓
Noise Gate: Abilitato ✓
AI: Abilitato ✓
```
**Risultato atteso**: Accuratezza 70-85%

### Scenario 3: Alta Velocità (> 30 WPM)
```
WPM: 35 (fisso, se noto)
Frequenza: 800 Hz (fisso)
Soglia: 0.35 (più alta per evitare falsi positivi)
Bandwidth: 80 Hz (più stretto)
Buffer Size: 30 ms (latenza minore)
FFT Size: 2048
AI: Abilitato ✓
Re-training: 30 campioni (più frequente)
```
**Risultato atteso**: Accuratezza > 90%

### Scenario 4: WebSDR / Virtual Audio
```
WPM: Auto-calibrazione ✓
Frequenza: Auto-detection ✓
Soglia: 0.25
Bandwidth: 120 Hz
Sample Rate: 48000 Hz
Buffer Size: 50 ms
AI: Abilitato ✓
```
**Risultato atteso**: Accuratezza > 93%

---

## 🐛 Troubleshooting

### Problema: Molti caratteri "?"
**Soluzione**:
- Aumenta il buffer size
- Verifica SNR > 10 dB
- Riduci la soglia di rilevamento
- Aumenta bandwidth del filtro

### Problema: Decodifica Lenta
**Soluzione**:
- Riduci FFT Size (es. 2048)
- Riduci buffer size
- Disabilita grafici real-time (feature futura)

### Problema: Accuratezza AI Bassa
**Soluzione**:
- Reset del modello AI
- Aumenta min dataset size
- Riduci intervallo re-training
- Lascia il sistema "allenarsi" su segnali puliti

### Problema: Latenza Troppo Alta
**Soluzione**:
- Riduci buffer size a 20-30 ms
- Riduci FFT size a 2048
- Usa dispositivi audio con driver ASIO (se disponibile)

---

## 🎓 Consigli per Utenti Avanzati

### Ottimizzazione per Contest
- Disabilita auto-calibrazione, imposta WPM fisso
- Frequenza fissa (se conosci quella del contest)
- Soglia più alta per evitare falsi positivi
- Buffer size minimo per latenza ridotta

### Ottimizzazione per QSO Casual
- Lascia tutto in auto-mode
- AI abilitato
- Buffer e FFT size standard
- Goditi la decodifica automatica!

### Debugging DSP
- Monitora SNR in tempo reale
- Verifica frequenza rilevata vs. aspettata
- Osserva forma d'onda e envelope
- Controlla statistiche di confidenza

---

## 📝 Note di Sviluppo

### Versione Corrente
- **v1.1**: Implementazione completa della finestra impostazioni
- **Formato settings**: JSON con validazione
- **Persistenza**: File locale con backup automatici

### Future Implementazioni (v1.2+)
- [ ] Profili di impostazioni (Contest, QSO, Training)
- [ ] Import/Export configurazioni
- [ ] Impostazioni avanzate per filtri DSP
- [ ] Configurazione grafica dei filtri
- [ ] Visualizzazione waterfall con impostazioni
- [ ] Tema UI personalizzabile

---

## 🤝 Contributi

Se hai suggerimenti per migliorare le impostazioni o nuovi parametri da aggiungere, sei il benvenuto!

---

**73 de RadioLogger Pro** 📻✨

*Impostazioni avanzate per radioamatori avanzati!*
