# Istruzioni per la Compilazione - RadioLoggerApp

Questa guida spiega come compilare RadioLoggerApp e creare l'eseguibile.

## Requisiti

### Obbligatori
- **Windows 10/11** o superiore
- **.NET 7.0 SDK** - [Download qui](https://dotnet.microsoft.com/download/dotnet/7.0)
- **Visual Studio 2022** (opzionale ma consigliato) o **Visual Studio Code**

### Verifica Installazione .NET

Apri il Prompt dei comandi o PowerShell e digita:

```bash
dotnet --version
```

Se vedi un numero di versione (es: `7.0.xxx`), .NET SDK è installato correttamente.

## Metodo 1: Script Automatici (CONSIGLIATO)

Abbiamo preparato script automatici per semplificare il processo di build.

### Opzione A: Build Standard (Windows Batch)

**Per una build che richiede .NET Runtime:**

```batch
build.bat
```

Questo crea un eseguibile leggero (~2-5 MB) in `./publish/RadioLoggerApp.exe`

**NOTA**: Gli utenti finali dovranno avere .NET 7.0 Runtime installato.

---

### Opzione B: Build Standalone (Windows Batch)

**Per una build completamente standalone:**

```batch
build-standalone.bat
```

Questo crea un eseguibile standalone (~150-200 MB) in `./publish-standalone/RadioLoggerApp.exe`

**VANTAGGI**:
- ✅ Non richiede .NET installato sul PC di destinazione
- ✅ Può essere distribuito su qualsiasi PC Windows
- ✅ Include tutte le dipendenze

---

### Opzione C: Build PowerShell (AVANZATA)

Per opzioni avanzate, usa lo script PowerShell:

```powershell
# Build standard
.\Build.ps1

# Build standalone
.\Build.ps1 -Standalone

# Build standalone in file singolo
.\Build.ps1 -Standalone -SingleFile

# Build con pulizia preventiva
.\Build.ps1 -Clean

# Build in modalità Debug
.\Build.ps1 -Configuration Debug

# Combinazione di opzioni
.\Build.ps1 -Standalone -SingleFile -Clean
```

**NOTA**: Se PowerShell blocca l'esecuzione, esegui prima:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

---

## Metodo 2: Visual Studio 2022

### Passo 1: Aprire il Progetto
1. Avvia **Visual Studio 2022**
2. File → Apri → Progetto/Soluzione
3. Seleziona `RadioLoggerApp.sln`

### Passo 2: Ripristinare Pacchetti
1. Click destro sulla soluzione in **Esplora Soluzioni**
2. Seleziona **Ripristina pacchetti NuGet**
3. Attendi il completamento

### Passo 3: Compilare
1. Seleziona **Release** dalla barra degli strumenti (invece di Debug)
2. Build → Compila soluzione (o premi `Ctrl+Shift+B`)

### Passo 4: Pubblicare
1. Click destro su **RadioLoggerApp** in Esplora Soluzioni
2. Seleziona **Pubblica**
3. Scegli **Cartella** come destinazione
4. Configura:
   - **Configurazione**: Release
   - **Framework di destinazione**: net7.0-windows
   - **Runtime di destinazione**: win-x64
   - **Modalità di distribuzione**:
     - **Dipendente dal framework** (più piccolo, richiede .NET)
     - oppure **Autonomo** (più grande, include .NET)
5. Click su **Pubblica**

L'eseguibile sarà in `bin\Release\net7.0-windows\win-x64\publish\`

---

## Metodo 3: Linea di Comando (Manuale)

### Build Standard

```bash
# 1. Ripristina pacchetti
dotnet restore RadioLoggerApp.sln

# 2. Compila in Release
dotnet build RadioLoggerApp.sln -c Release

# 3. Pubblica
dotnet publish RadioLoggerApp.csproj -c Release -r win-x64 --self-contained false -o ./publish
```

### Build Standalone

```bash
# 1. Ripristina pacchetti
dotnet restore RadioLoggerApp.sln

# 2. Compila in Release
dotnet build RadioLoggerApp.sln -c Release

# 3. Pubblica standalone
dotnet publish RadioLoggerApp.csproj -c Release -r win-x64 --self-contained true -o ./publish-standalone
```

### Build Standalone File Singolo

```bash
dotnet publish RadioLoggerApp.csproj ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true ^
  -o ./publish-singlefile
```

---

## Troubleshooting

### Errore: "Il termine 'dotnet' non è riconosciuto"

**Soluzione**: .NET SDK non è installato o non è nel PATH
1. Scarica e installa .NET 7.0 SDK da https://dotnet.microsoft.com/download/dotnet/7.0
2. Riavvia il terminale dopo l'installazione

---

### Errore: Pacchetti NuGet non trovati

**Soluzione**: Ripristina i pacchetti manualmente
```bash
dotnet restore RadioLoggerApp.sln --force
```

---

### Errore: "Project targets framework 'net7.0-windows' but the current SDK does not support it"

**Soluzione**: Versione SDK non corretta
1. Verifica versione: `dotnet --version`
2. Assicurati di avere .NET 7.0 o superiore
3. Aggiorna se necessario

---

### Errore di compilazione su file XAML

**Soluzione**: Pulisci e ricompila
```bash
dotnet clean
dotnet build RadioLoggerApp.sln -c Release
```

---

### L'eseguibile non si avvia su altro PC

**Problema**: .NET Runtime mancante

**Soluzioni**:
1. **Opzione A**: Installa .NET 7.0 Runtime sul PC di destinazione
   - Download: https://dotnet.microsoft.com/download/dotnet/7.0

2. **Opzione B**: Usa build standalone
   - Esegui `build-standalone.bat`
   - Distribuisci l'intera cartella `publish-standalone`

---

## Ottimizzazioni Avanzate

### Ridurre Dimensione Eseguibile Standalone

Aggiungi al file `.csproj`:

```xml
<PropertyGroup>
  <PublishTrimmed>true</PublishTrimmed>
  <PublishReadyToRun>true</PublishReadyToRun>
  <TieredCompilation>false</TieredCompilation>
</PropertyGroup>
```

Poi compila con:
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishTrimmed=true
```

**ATTENZIONE**: Il trimming può causare problemi con reflection. Testa accuratamente!

---

### Build per Diverse Piattaforme

```bash
# Windows 64-bit
dotnet publish -c Release -r win-x64 --self-contained true

# Windows 32-bit
dotnet publish -c Release -r win-x86 --self-contained true

# Windows ARM64
dotnet publish -c Release -r win-arm64 --self-contained true
```

---

## Distribuzione

### Per Utenti con .NET Installato

Distribuisci solo i file in `./publish/`:
- Dimensione: ~2-5 MB
- Richiede: .NET 7.0 Runtime sul PC di destinazione

### Per Utenti senza .NET

Distribuisci i file in `./publish-standalone/`:
- Dimensione: ~150-200 MB
- Richiede: Nulla, tutto incluso
- Può essere compresso in ZIP per distribuzione

---

## Firma Digitale (Opzionale)

Per firmare l'eseguibile con certificato digitale:

```bash
signtool sign /f "certificato.pfx" /p "password" /t http://timestamp.digicert.com RadioLoggerApp.exe
```

**Vantaggi**:
- Windows non mostrerà avvisi "Publisher sconosciuto"
- Maggiore fiducia degli utenti

---

## Creazione Installer

Per creare un installer professionale, usa:

### WiX Toolset
```bash
# Installa WiX
dotnet tool install --global wix

# Crea installer MSI
# (richiede file .wxs configurato)
```

### Inno Setup (Più semplice)
1. Scarica Inno Setup: https://jrsoftware.org/isinfo.php
2. Crea script `.iss` per il tuo progetto
3. Compila per ottenere un installer `.exe`

---

## Checklist Pre-Release

Prima di distribuire una release ufficiale:

- [ ] Compilato in modalità Release (non Debug)
- [ ] Testato su PC pulito senza Visual Studio
- [ ] Verificate tutte le funzionalità principali:
  - [ ] Radio Logger
  - [ ] Morse Code Decoder
  - [ ] SliceMaster per SmartSDR 4.0
- [ ] Database SQLite funzionante
- [ ] Audio capture funzionante (Morse Decoder)
- [ ] Connessione di rete funzionante (SliceMaster)
- [ ] README aggiornato con versione corretta
- [ ] Change log aggiornato
- [ ] Eventuali file di configurazione inclusi

---

## Supporto

Se incontri problemi durante la compilazione:

1. Verifica di avere l'ultima versione di .NET 7.0 SDK
2. Controlla che tutti i pacchetti NuGet siano ripristinati
3. Prova a pulire la soluzione: `dotnet clean`
4. Apri una issue su GitHub con i dettagli dell'errore

---

## Versioning

Il progetto usa Semantic Versioning (MAJOR.MINOR.PATCH):

Per aggiornare la versione, modifica `RadioLoggerApp.csproj`:

```xml
<PropertyGroup>
  <Version>1.0.0</Version>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
  <FileVersion>1.0.0.0</FileVersion>
</PropertyGroup>
```

---

**Buona compilazione! 73!** 📻
