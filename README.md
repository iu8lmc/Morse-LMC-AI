# Radio Logger App - Suite Radioamatoriale Completa

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-7.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)

## Descrizione

**Radio Logger App** è una suite software completa per radioamatori che integra tre potenti strumenti:

1. **Radio Logger** - Sistema di logging QSO con database SQLite
2. **Morse Code Decoder AI** - Decodificatore Morse intelligente con Machine Learning
3. **SliceMaster per SmartSDR 4.0** - Controller avanzato per radio FlexRadio

## Caratteristiche Principali

### 📝 Radio Logger
- Logging completo dei QSO (contatti radio)
- Database SQLite integrato
- Interfaccia utente intuitiva
- Salvataggio e caricamento log
- Gestione chiamate, bande, modi, report

### 🔊 Morse Code Decoder AI
- Decodifica in tempo reale del codice Morse
- Machine Learning per migliorare accuratezza
- Cattura audio da dispositivi di input
- Elaborazione del segnale avanzata
- Visualizzazione waterfall e spettro
- [Documentazione completa](README_MORSE_DECODER.md)

### 📡 SliceMaster per SmartSDR 4.0
- Controllo completo delle slice FlexRadio
- Gestione frequenza, modalità e filtri
- Sistema di profili salvabili
- Selezione banda rapida
- Controlli RF/AF, NB/NR/ANF
- Discovery automatico radio
- [Documentazione completa](README_SLICEMASTER.md)

## Requisiti di Sistema

- **Sistema Operativo**: Windows 7 o superiore
- **Framework**: .NET 7.0 o superiore
- **RAM**: Minimo 4 GB
- **Spazio disco**: 200 MB

### Requisiti Aggiuntivi per SliceMaster
- Radio FlexRadio con SmartSDR 4.0
- Connessione di rete (Ethernet o WiFi) alla radio

## Installazione

### Download
```bash
git clone https://github.com/iu8lmc/Morse-LMC-AI.git
cd Morse-LMC-AI
```

### Build
1. Aprire la soluzione in Visual Studio 2022 o superiore
2. Ripristinare i pacchetti NuGet
3. Compilare la soluzione (Build → Build Solution)
4. Eseguire il progetto

## Dipendenze

Il progetto utilizza le seguenti librerie NuGet:

- **System.Data.SQLite** (v1.0.118) - Database per logging
- **NAudio** (v2.2.1) - Elaborazione audio
- **MathNet.Numerics** (v5.0.0) - Calcoli matematici
- **Microsoft.ML** (v3.0.1) - Machine Learning
- **Accord.MachineLearning** (v3.8.0) - Algoritmi ML
- **OxyPlot.Wpf** (v2.1.2) - Visualizzazione grafici
- **Spectre.Console** (v0.48.0) - Output console

## Utilizzo Rapido

### Avvio Applicazione
1. Eseguire `RadioLoggerApp.exe`
2. Si aprirà la finestra principale del Radio Logger

### Accesso ai Moduli

#### Radio Logger
- Utilizzare l'interfaccia principale per inserire QSO
- Menu: **File** → Nuovo Log / Salva Log / Carica Log

#### Morse Code Decoder
- Menu: **Tools** → **Morse Code Decoder**
- Selezionare dispositivo audio
- Avviare la cattura

#### SliceMaster
- Menu: **Tools** → **SliceMaster for SmartSDR 4.0**
- Inserire IP della radio o usare "Scopri Radio"
- Cliccare "Connetti"
- Gestire le slice con l'interfaccia grafica

## Struttura del Progetto

```
RadioLoggerApp/
├── MainWindow.xaml/cs          # Finestra principale
├── Logger/
│   ├── LogEntry.cs             # Modello dati log
│   ├── Logger.cs               # Sistema logging
│   └── SQLiteDBHandler.cs      # Gestione database
├── MorseDecoder/
│   ├── MorseDecoderWindow.xaml/cs   # Finestra decoder
│   ├── MorseDecoder.cs              # Logica decodifica
│   ├── MorseAudioCapture.cs         # Cattura audio
│   ├── MorseSignalProcessor.cs      # Elaborazione segnale
│   └── MorseAIEnhancer.cs           # AI/ML
└── SliceMaster/
    ├── SliceMasterWindow.xaml/cs    # Finestra principale
    ├── FlexRadioConnection.cs       # Connessione radio
    ├── SliceController.cs           # Controller slice
    ├── SliceModels.cs               # Modelli dati
    ├── ProfileManager.cs            # Gestione profili
    └── WpfConverters.cs             # Converter WPF
```

## Documentazione Dettagliata

- [**Morse Code Decoder AI** - Guida completa](README_MORSE_DECODER.md)
- [**SliceMaster** - Guida completa](README_SLICEMASTER.md)

## Funzionalità Avanzate

### Radio Logger
- Export/Import dati in vari formati
- Statistiche QSO
- Ricerca e filtri avanzati

### Morse Decoder
- Training personalizzato dell'AI
- Supporto velocità variabile
- Cancellazione rumore adattiva
- Salvataggio sessioni di decodifica

### SliceMaster
- Controllo simultaneo di multiple slice
- Profili personalizzati importabili/esportabili
- Selezione banda rapida (tutte le bande ham)
- Lock slice per prevenire modifiche accidentali

## Problemi Noti e Soluzioni

### Audio non funziona (Morse Decoder)
- Verificare permessi microfono in Windows
- Controllare dispositivo audio nelle impostazioni
- Verificare driver audio aggiornati

### Connessione FlexRadio fallisce
- Verificare IP corretto della radio
- Controllare firewall Windows (porta 4992)
- Verificare connessione di rete
- Usare "Scopri Radio" per auto-rilevamento

### Database corrotto
- Backup regolare del file `radiologger.db`
- Utilizzare strumenti SQLite per riparazioni

## Contribuire

Contributi sono benvenuti! Per contribuire:

1. Fork del repository
2. Creare un branch per la feature (`git checkout -b feature/NuovaCaratteristica`)
3. Commit delle modifiche (`git commit -m 'Aggiunta nuova caratteristica'`)
4. Push al branch (`git push origin feature/NuovaCaratteristica`)
5. Aprire una Pull Request

## Roadmap

### v1.1 (Prossimo rilascio)
- [ ] Export log in formato ADIF
- [ ] Integrazione con QRZ.com
- [ ] Tema dark/light per tutte le finestre
- [ ] Supporto multi-lingua

### v1.2 (Futuro)
- [ ] Controllo rotore antenne
- [ ] Integrazione con contest logger
- [ ] Supporto cluster DX
- [ ] Applicazione mobile companion

### v2.0 (Visione futura)
- [ ] Web interface per controllo remoto
- [ ] Cloud sync dei log
- [ ] AI per predizione propagazione
- [ ] Supporto SDR generico (non solo FlexRadio)

## Licenza

Questo progetto è rilasciato sotto licenza MIT. Vedi il file [LICENSE](LICENSE) per i dettagli.

## Crediti

Sviluppato da **IU8LMC** e contributo della comunità radioamatoriale.

### Ringraziamenti
- FlexRadio Systems per l'API SmartSDR
- Comunità radioamatoriale italiana e internazionale
- Contributori open source

## Contatti e Supporto

- **GitHub Issues**: [Segnala un problema](https://github.com/iu8lmc/Morse-LMC-AI/issues)
- **Discussions**: [Forum discussioni](https://github.com/iu8lmc/Morse-LMC-AI/discussions)
- **Email**: [tuo-email@esempio.com]

## Note Legali

Questo software è fornito "così com'è", senza garanzie di alcun tipo. L'uso di apparecchiature radio deve rispettare le normative locali e le licenze radioamatoriali vigenti.

---

**73 de IU8LMC!** 📻

*Made with ❤️ for the Ham Radio Community*
