# Script PowerShell per build di RadioLoggerApp - SliceMaster Suite
# Questo script offre opzioni avanzate per la compilazione

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",

    [Parameter(Mandatory=$false)]
    [switch]$Standalone,

    [Parameter(Mandatory=$false)]
    [switch]$SingleFile,

    [Parameter(Mandatory=$false)]
    [switch]$Clean
)

$ErrorActionPreference = "Stop"

Write-Host "============================================" -ForegroundColor Cyan
Write-Host " RadioLoggerApp - SliceMaster Build Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Verifica presenza di dotnet
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK trovato: versione $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ ERRORE: .NET SDK non trovato!" -ForegroundColor Red
    Write-Host "Scarica e installa .NET 7.0 SDK da:" -ForegroundColor Yellow
    Write-Host "https://dotnet.microsoft.com/download/dotnet/7.0" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Pulizia se richiesta
if ($Clean) {
    Write-Host "[1/X] Pulizia build precedenti..." -ForegroundColor Yellow
    if (Test-Path "./bin") { Remove-Item -Path "./bin" -Recurse -Force }
    if (Test-Path "./obj") { Remove-Item -Path "./obj" -Recurse -Force }
    if (Test-Path "./publish") { Remove-Item -Path "./publish" -Recurse -Force }
    if (Test-Path "./publish-standalone") { Remove-Item -Path "./publish-standalone" -Recurse -Force }
    Write-Host "✓ Pulizia completata" -ForegroundColor Green
    Write-Host ""
}

# Ripristino pacchetti
Write-Host "[1/4] Ripristino pacchetti NuGet..." -ForegroundColor Yellow
dotnet restore RadioLoggerApp.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ ERRORE: Ripristino pacchetti fallito!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Pacchetti ripristinati" -ForegroundColor Green
Write-Host ""

# Compilazione
Write-Host "[2/4] Compilazione in modalità $Configuration..." -ForegroundColor Yellow
dotnet build RadioLoggerApp.sln -c $Configuration --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ ERRORE: Compilazione fallita!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Compilazione completata" -ForegroundColor Green
Write-Host ""

# Pubblicazione
Write-Host "[3/4] Pubblicazione applicazione..." -ForegroundColor Yellow

$publishArgs = @(
    "publish",
    "RadioLoggerApp.csproj",
    "-c", $Configuration,
    "-r", "win-x64",
    "--no-restore"
)

if ($Standalone) {
    $publishArgs += "--self-contained", "true"
    $outputPath = "./publish-standalone"

    if ($SingleFile) {
        $publishArgs += "-p:PublishSingleFile=true"
        $publishArgs += "-p:IncludeNativeLibrariesForSelfExtract=true"
        Write-Host "  → Creazione file singolo standalone..." -ForegroundColor Cyan
    }
} else {
    $publishArgs += "--self-contained", "false"
    $outputPath = "./publish"
}

$publishArgs += "-o", $outputPath

& dotnet $publishArgs
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ ERRORE: Pubblicazione fallita!" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Pubblicazione completata" -ForegroundColor Green
Write-Host ""

# Verifica file creati
Write-Host "[4/4] Verifica output..." -ForegroundColor Yellow
$exePath = Join-Path $outputPath "RadioLoggerApp.exe"

if (Test-Path $exePath) {
    $fileSize = (Get-Item $exePath).Length / 1MB
    Write-Host "✓ Eseguibile creato: $exePath" -ForegroundColor Green
    Write-Host "  Dimensione: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Cyan
} else {
    Write-Host "✗ ERRORE: Eseguibile non trovato!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host " BUILD COMPLETATA CON SUCCESSO!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""

if ($Standalone) {
    Write-Host "Tipo build: STANDALONE" -ForegroundColor Cyan
    Write-Host "  ✓ Non richiede .NET installato" -ForegroundColor Green
    Write-Host "  ✓ Può essere distribuito su qualsiasi PC Windows" -ForegroundColor Green
    Write-Host "  ⚠ Dimensione maggiore (~150-200 MB)" -ForegroundColor Yellow
} else {
    Write-Host "Tipo build: FRAMEWORK-DEPENDENT" -ForegroundColor Cyan
    Write-Host "  ✓ Dimensione ridotta (~2-5 MB)" -ForegroundColor Green
    Write-Host "  ⚠ Richiede .NET 7.0 Runtime installato" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Eseguibile: $exePath" -ForegroundColor White
Write-Host ""
Write-Host "Esempi di utilizzo:" -ForegroundColor Yellow
Write-Host "  .\Build.ps1                           # Build standard" -ForegroundColor Gray
Write-Host "  .\Build.ps1 -Standalone               # Build standalone" -ForegroundColor Gray
Write-Host "  .\Build.ps1 -Standalone -SingleFile   # Build file singolo" -ForegroundColor Gray
Write-Host "  .\Build.ps1 -Clean                    # Pulizia + build" -ForegroundColor Gray
Write-Host "  .\Build.ps1 -Configuration Debug      # Build debug" -ForegroundColor Gray
Write-Host ""
