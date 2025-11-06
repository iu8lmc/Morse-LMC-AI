# 🏗 Istruzioni di Build e Packaging - Morse Decoder AI

Questa guida spiega come creare un pacchetto eseguibile standalone del **Morse Decoder AI** pronto per la distribuzione.

---

## 📋 Prerequisiti

### Software Necessario

1. **.NET 7.0 SDK**
   - Download: https://dotnet.microsoft.com/download/dotnet/7.0
   - Verifica installazione: `dotnet --version`
   - Versione minima richiesta: 7.0.0

2. **PowerShell 5.1+** (Windows) o **Command Prompt**
   - Incluso in Windows 10/11

3. **Git** (opzionale, per clonare il repository)
   - Download: https://git-scm.com/downloads

### Spazio su Disco
- **Spazio richiesto**: ~500 MB
  - Dipendenze NuGet: ~150 MB
  - Build output: ~200 MB
  - Pacchetto finale: ~150 MB

---

## 🚀 Build Automatico (Consigliato)

### Opzione 1: PowerShell Script (Windows)

```powershell
# 1. Apri PowerShell nella directory del progetto
cd C:\path\to\Morse-LMC-AI

# 2. Esegui lo script di build
.\build-release.ps1

# Se ricevi errore "script non firmato":
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
.\build-release.ps1
```

**Output**:
- Eseguibile: `MorseDecoderAI_v1.1.0_win-x64\MorseDecoderAI.exe`
- Pacchetto ZIP: `MorseDecoderAI_v1.1.0_win-x64.zip`

---

### Opzione 2: Batch Script (Windows)

```cmd
# 1. Apri Command Prompt nella directory del progetto
cd C:\path\to\Morse-LMC-AI

# 2. Esegui lo script di build
build-release.bat
```

**Output**:
- Directory: `MorseDecoderAI_v1.1.0_win-x64\`
- Devi creare manualmente il ZIP (vedi sotto)

---

## ⚙️ Build Manuale

### Passo 1: Restore Dipendenze

```bash
dotnet restore RadioLoggerApp.csproj
```

**Cosa fa**:
- Scarica tutti i pacchetti NuGet necessari
- Risolve le dipendenze
- Cache locale in `~/.nuget/packages`

---

### Passo 2: Build Release

```bash
dotnet build RadioLoggerApp.csproj --configuration Release
```

**Cosa fa**:
- Compila il progetto in modalità Release
- Output: `bin\Release\net7.0-windows\`
- **NOTA**: Non è ancora un eseguibile standalone!

---

### Passo 3: Publish Self-Contained

```bash
dotnet publish RadioLoggerApp.csproj \
    --configuration Release \
    --runtime win-x64 \
    --self-contained true \
    --output bin\Release\Publish \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -p:IncludeNativeLibrariesForSelfExtract=true
```

**Parametri spiegati**:
- `--runtime win-x64`: Target Windows 64-bit
- `--self-contained true`: Include .NET Runtime nell'exe
- `--output`: Directory di output
- `PublishSingleFile`: Crea un singolo file .exe
- `PublishReadyToRun`: Pre-compila per startup più veloce
- `IncludeNativeLibrariesForSelfExtract`: Include librerie native

**Output**:
- `bin\Release\Publish\MorseDecoderAI.exe` (~140 MB)

---

### Passo 4: Crea Pacchetto di Distribuzione

```bash
# Crea directory del pacchetto
mkdir MorseDecoderAI_v1.1.0_win-x64

# Copia l'eseguibile
copy bin\Release\Publish\MorseDecoderAI.exe MorseDecoderAI_v1.1.0_win-x64\

# Copia database (se esiste)
copy radiologger.db MorseDecoderAI_v1.1.0_win-x64\

# Crea directory documentazione
mkdir MorseDecoderAI_v1.1.0_win-x64\Docs

# Copia documentazione
copy README.md MorseDecoderAI_v1.1.0_win-x64\Docs\
copy README_MORSE_DECODER.md MorseDecoderAI_v1.1.0_win-x64\Docs\
copy README_SETTINGS.md MorseDecoderAI_v1.1.0_win-x64\Docs\
copy TEST_CHECKLIST.md MorseDecoderAI_v1.1.0_win-x64\Docs\
```

---

### Passo 5: Crea File ZIP

**Windows Explorer**:
1. Tasto destro sulla cartella `MorseDecoderAI_v1.1.0_win-x64`
2. Invia a → Cartella compressa
3. Rinomina in `MorseDecoderAI_v1.1.0_win-x64.zip`

**PowerShell**:
```powershell
Compress-Archive -Path MorseDecoderAI_v1.1.0_win-x64 -DestinationPath MorseDecoderAI_v1.1.0_win-x64.zip
```

**7-Zip** (se installato):
```cmd
7z a -tzip MorseDecoderAI_v1.1.0_win-x64.zip MorseDecoderAI_v1.1.0_win-x64\
```

---

## 📦 Struttura del Pacchetto Finale

```
MorseDecoderAI_v1.1.0_win-x64.zip
├── MorseDecoderAI.exe           (~140 MB) - Eseguibile principale
├── radiologger.db               (opzionale) - Database SQLite
├── README.txt                   - Istruzioni rapide
└── Docs\
    ├── README.md                - README principale
    ├── README_MORSE_DECODER.md  - Guida completa decoder
    ├── README_SETTINGS.md       - Documentazione impostazioni
    └── TEST_CHECKLIST.md        - Checklist di testing
```

---

## 🎯 Target Platform Supportati

### Windows x64 (Principale)
```bash
--runtime win-x64
```
**Compatibile con**: Windows 10/11 64-bit

### Windows x86 (32-bit)
```bash
--runtime win-x86
```
**Per sistemi 32-bit più vecchi**

### Windows ARM64
```bash
--runtime win-arm64
```
**Per dispositivi Windows ARM** (es. Surface Pro X)

---

## 🔧 Opzioni di Pubblicazione Avanzate

### Build più Piccola (Framework-Dependent)

Se vuoi un eseguibile più piccolo (~50 MB), ma che richiede .NET Runtime installato:

```bash
dotnet publish RadioLoggerApp.csproj \
    --configuration Release \
    --runtime win-x64 \
    --self-contained false \
    --output bin\Release\Publish-FD \
    -p:PublishSingleFile=true
```

**Pro**: File più piccolo (~50 MB)
**Contro**: Richiede .NET 7.0 Runtime installato sul PC target

---

### Build con Trimming (Sperimentale)

Per ridurre ulteriormente la dimensione:

```bash
dotnet publish RadioLoggerApp.csproj \
    --configuration Release \
    --runtime win-x64 \
    --self-contained true \
    --output bin\Release\Publish-Trim \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=link
```

**⚠️ ATTENZIONE**: Il trimming può causare problemi con:
- Reflection
- ML.NET
- WPF XAML

**Test approfondito necessario!**

---

## 🐛 Risoluzione Problemi di Build

### Errore: "dotnet: command not found"

**Causa**: .NET SDK non installato o non nel PATH

**Soluzione**:
1. Scarica e installa .NET 7.0 SDK
2. Riavvia il terminale
3. Verifica: `dotnet --version`

---

### Errore: "The framework 'Microsoft.NETCore.App' version '7.0.0' was not found"

**Causa**: .NET 7.0 SDK non installato

**Soluzione**:
```bash
# Scarica e installa .NET 7.0 SDK da:
# https://dotnet.microsoft.com/download/dotnet/7.0
```

---

### Errore: "Package restore failed"

**Causa**: Problemi di connessione a NuGet.org

**Soluzione**:
```bash
# Pulisci cache NuGet
dotnet nuget locals all --clear

# Riprova il restore
dotnet restore RadioLoggerApp.csproj
```

---

### Errore: "Unable to load DLL 'SQLite.Interop.dll'"

**Causa**: SQLite native library non inclusa

**Soluzione**:
Assicurati di usare:
```bash
-p:IncludeNativeLibrariesForSelfExtract=true
```

---

### Warning: "IL2026" o "IL3050" (Trimming Warnings)

**Causa**: Codice non compatibile con trimming

**Soluzione**:
1. Disabilita il trimming: rimuovi `-p:PublishTrimmed=true`
2. Oppure sopprimi i warning nel `.csproj`:

```xml
<NoWarn>$(NoWarn);IL2026;IL3050</NoWarn>
```

---

## 📊 Dimensioni dei File

### Self-Contained (Raccomandato)
- **Eseguibile**: ~140 MB
- **Compressi (ZIP)**: ~60 MB
- **Include**: .NET Runtime + Tutte le dipendenze

### Framework-Dependent
- **Eseguibile**: ~50 MB
- **Compressi (ZIP)**: ~20 MB
- **Richiede**: .NET 7.0 Runtime sul target

### Con Trimming (Sperimentale)
- **Eseguibile**: ~80 MB
- **Compressi (ZIP)**: ~35 MB
- **Rischio**: Possibili runtime errors

---

## ✅ Verifica del Pacchetto

Prima di distribuire, verifica che:

1. ✅ L'eseguibile si avvia correttamente
2. ✅ La finestra principale appare
3. ✅ Il Morse Decoder AI si apre
4. ✅ Le impostazioni funzionano
5. ✅ L'audio viene acquisito
6. ✅ La decodifica funziona
7. ✅ I log non mostrano errori critici
8. ✅ Le impostazioni vengono salvate e caricate
9. ✅ Tutte le dipendenze sono incluse
10. ✅ Nessun crash al primo avvio

---

## 🚀 Distribuzione

### GitHub Releases
1. Crea un nuovo Release su GitHub
2. Carica il file ZIP
3. Aggiungi note di rilascio
4. Tagga la versione (es. v1.1.0)

### Download Diretto
1. Carica il ZIP su un server
2. Fornisci link diretto di download
3. Includi checksum (SHA256) per verifica

### Checksum SHA256
```bash
# PowerShell
Get-FileHash MorseDecoderAI_v1.1.0_win-x64.zip -Algorithm SHA256

# Linux/Mac
shasum -a 256 MorseDecoderAI_v1.1.0_win-x64.zip
```

---

## 📝 Checklist Pre-Release

Prima di rilasciare una nuova versione:

- [ ] Aggiorna `<Version>` in `RadioLoggerApp.csproj`
- [ ] Aggiorna README con nuove feature
- [ ] Testa build su Windows pulito (VM)
- [ ] Verifica che tutte le funzionalità funzionino
- [ ] Controlla che non ci siano errori nei log
- [ ] Genera checksum SHA256
- [ ] Crea note di rilascio (changelog)
- [ ] Tagga il commit con la versione
- [ ] Carica su GitHub Releases

---

## 🔄 Aggiornamenti Futuri

### Versioning
Seguiamo il **Semantic Versioning** (SemVer):
- **MAJOR**: Cambiamenti incompatibili (es. 2.0.0)
- **MINOR**: Nuove funzionalità compatibili (es. 1.2.0)
- **PATCH**: Bug fix (es. 1.1.1)

### Auto-Update (Futuro)
Pianificato per v1.3+:
- Controllo automatico aggiornamenti
- Download e installazione da GitHub Releases
- Notifica all'utente

---

## 📞 Supporto

Per problemi di build o packaging:
- **GitHub Issues**: https://github.com/iu8lmc/Morse-LMC-AI/issues
- **Documentazione .NET**: https://docs.microsoft.com/dotnet/core/deploying/

---

**Happy Building!** 🏗️✨

*Build once, deploy everywhere!*
