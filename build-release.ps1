# ========================================
# Build Script per Morse Decoder AI v1.1
# ========================================
# Crea un pacchetto eseguibile standalone per Windows
#
# Requisiti:
# - .NET 7.0 SDK installato
# - PowerShell 5.1 o superiore
#
# Uso: .\build-release.ps1
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Morse Decoder AI - Build Release    " -ForegroundColor Cyan
Write-Host "             v1.1.0                     " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Variabili di configurazione
$ProjectFile = "RadioLoggerApp.csproj"
$OutputDir = "bin\Release\Publish"
$PackageDir = "MorseDecoderAI_v1.1.0_win-x64"
$ZipFile = "MorseDecoderAI_v1.1.0_win-x64.zip"

# Verifica che dotnet sia installato
Write-Host "[1/6] Verifica .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK $dotnetVersion trovato" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK non trovato!" -ForegroundColor Red
    Write-Host "Scarica e installa da: https://dotnet.microsoft.com/download/dotnet/7.0" -ForegroundColor Red
    exit 1
}

# Pulisci build precedenti
Write-Host ""
Write-Host "[2/6] Pulizia build precedenti..." -ForegroundColor Yellow
if (Test-Path $OutputDir) {
    Remove-Item -Path $OutputDir -Recurse -Force
    Write-Host "✓ Directory output pulita" -ForegroundColor Green
}
if (Test-Path $PackageDir) {
    Remove-Item -Path $PackageDir -Recurse -Force
}
if (Test-Path $ZipFile) {
    Remove-Item -Path $ZipFile -Force
}

# Restore delle dipendenze
Write-Host ""
Write-Host "[3/6] Restore dipendenze NuGet..." -ForegroundColor Yellow
dotnet restore $ProjectFile
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Errore nel restore delle dipendenze" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Dipendenze ripristinate" -ForegroundColor Green

# Build in modalità Release
Write-Host ""
Write-Host "[4/6] Build del progetto..." -ForegroundColor Yellow
dotnet build $ProjectFile --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Errore nella build" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Build completata" -ForegroundColor Green

# Publish dell'applicazione
Write-Host ""
Write-Host "[5/6] Pubblicazione applicazione..." -ForegroundColor Yellow
Write-Host "    Target: Windows x64 (self-contained)" -ForegroundColor Gray
Write-Host "    Output: $OutputDir" -ForegroundColor Gray

dotnet publish $ProjectFile `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $OutputDir `
    -p:PublishSingleFile=true `
    -p:PublishReadyToRun=true `
    -p:IncludeNativeLibrariesForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Errore nella pubblicazione" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Pubblicazione completata" -ForegroundColor Green

# Crea pacchetto di distribuzione
Write-Host ""
Write-Host "[6/6] Creazione pacchetto di distribuzione..." -ForegroundColor Yellow

# Crea directory del pacchetto
New-Item -ItemType Directory -Path $PackageDir -Force | Out-Null

# Copia l'eseguibile
Copy-Item -Path "$OutputDir\MorseDecoderAI.exe" -Destination $PackageDir
Write-Host "✓ Eseguibile copiato" -ForegroundColor Green

# Copia il database (se esiste)
if (Test-Path "radiologger.db") {
    Copy-Item -Path "radiologger.db" -Destination $PackageDir
    Write-Host "✓ Database copiato" -ForegroundColor Green
}

# Copia la documentazione
$DocsFiles = @(
    "README.md",
    "README_MORSE_DECODER.md",
    "README_SETTINGS.md",
    "TEST_CHECKLIST.md"
)

$DocsDir = "$PackageDir\Docs"
New-Item -ItemType Directory -Path $DocsDir -Force | Out-Null

foreach ($file in $DocsFiles) {
    if (Test-Path $file) {
        Copy-Item -Path $file -Destination $DocsDir
    }
}
Write-Host "✓ Documentazione copiata" -ForegroundColor Green

# Crea README di installazione
$InstallReadme = @"
================================================================================
                    MORSE DECODER AI v1.1.0
                  RadioLogger Pro - Windows x64
================================================================================

📦 CONTENUTO DEL PACCHETTO

  - MorseDecoderAI.exe      Applicazione principale (self-contained)
  - radiologger.db          Database SQLite (opzionale)
  - Docs\                   Documentazione completa


🚀 INSTALLAZIONE

  1. Estrai tutti i file in una cartella a tua scelta
     (es. C:\Program Files\MorseDecoderAI)

  2. Avvia MorseDecoderAI.exe facendo doppio click

  3. (Opzionale) Crea un collegamento sul desktop


⚙️ REQUISITI DI SISTEMA

  - Windows 10/11 (64-bit)
  - 4 GB RAM minimo (8 GB consigliati)
  - Dispositivo audio (microfono, line-in, virtual audio cable)
  - .NET Runtime 7.0 (incluso nell'eseguibile)


📝 PRIMO AVVIO

  1. Apri MorseDecoderAI.exe
  2. Click su "🎙 Morse Decoder AI" nella barra inferiore
  3. Seleziona il dispositivo audio dal menu a tendina
  4. Click "▶ Avvia Decodifica"
  5. Invia un segnale Morse (radio, generatore toni, file audio)


🔧 IMPOSTAZIONI AVANZATE

  Click su "⚙ Impostazioni" per configurare:
  - Parametri Morse (WPM, frequenza, soglia)
  - DSP (bandwidth, AGC, noise gate, FFT)
  - AI/ML (training, dataset, modelli)
  - Audio (buffer size, sample rate)

  Le impostazioni sono salvate in:
  %APPDATA%\RadioLoggerApp\MorseDecoder\settings.json


📚 DOCUMENTAZIONE

  Consulta i file nella cartella "Docs\":
  - README_MORSE_DECODER.md  : Guida completa al decodificatore
  - README_SETTINGS.md       : Documentazione impostazioni avanzate
  - TEST_CHECKLIST.md        : Checklist di testing


🐛 RISOLUZIONE PROBLEMI

  PROBLEMA: Nessun audio rilevato
  SOLUZIONE:
    - Verifica che il dispositivo audio sia selezionato correttamente
    - Controlla i livelli audio nel mixer di Windows
    - Testa con un altro dispositivo

  PROBLEMA: Decodifica errata
  SOLUZIONE:
    - Migliora il rapporto segnale/rumore (SNR > 10 dB)
    - Regola i livelli audio (30-70% nella barra)
    - Lascia il sistema auto-calibrarsi per 10-20 caratteri

  PROBLEMA: Bassa confidenza
  SOLUZIONE:
    - Apri Impostazioni → Riduci la soglia di rilevamento
    - Aumenta bandwidth del filtro
    - Addestra il modello AI con più dati


📞 SUPPORTO

  Per bug, richieste di funzionalità o domande:
  - GitHub: https://github.com/iu8lmc/Morse-LMC-AI


📄 LICENZA

  Questo progetto è fornito "as-is" per scopi educativi e radioamatoriali.


================================================================================
                    73 de RadioLogger Pro 📻✨
                         Happy decoding!
================================================================================
"@

Set-Content -Path "$PackageDir\README.txt" -Value $InstallReadme -Encoding UTF8
Write-Host "✓ README di installazione creato" -ForegroundColor Green

# Crea file ZIP
Write-Host ""
Write-Host "Compressione in file ZIP..." -ForegroundColor Yellow
Compress-Archive -Path $PackageDir -DestinationPath $ZipFile -Force
Write-Host "✓ Pacchetto ZIP creato" -ForegroundColor Green

# Statistiche finali
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "          BUILD COMPLETATA!             " -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$exeSize = (Get-Item "$PackageDir\MorseDecoderAI.exe").Length / 1MB
$zipSize = (Get-Item $ZipFile).Length / 1MB

Write-Host "📊 STATISTICHE:" -ForegroundColor Yellow
Write-Host "   Eseguibile: $($exeSize.ToString('F2')) MB" -ForegroundColor White
Write-Host "   Pacchetto:  $($zipSize.ToString('F2')) MB" -ForegroundColor White
Write-Host ""
Write-Host "📦 PACCHETTO CREATO:" -ForegroundColor Yellow
Write-Host "   $ZipFile" -ForegroundColor White
Write-Host ""
Write-Host "📁 PERCORSO COMPLETO:" -ForegroundColor Yellow
Write-Host "   $((Get-Location).Path)\$ZipFile" -ForegroundColor White
Write-Host ""
Write-Host "🚀 PRONTO PER IL TESTING!" -ForegroundColor Green
Write-Host ""
Write-Host "Per testare:" -ForegroundColor Yellow
Write-Host "  1. Estrai il file ZIP" -ForegroundColor Gray
Write-Host "  2. Avvia MorseDecoderAI.exe" -ForegroundColor Gray
Write-Host "  3. Leggi README.txt per istruzioni" -ForegroundColor Gray
Write-Host ""
