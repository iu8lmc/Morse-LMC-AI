@echo off
REM Script per creare build standalone di RadioLoggerApp
REM Questo crea un eseguibile che non richiede .NET installato

echo ============================================
echo  RadioLoggerApp - Standalone Build Script
echo ============================================
echo.

REM Verifica presenza di dotnet
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERRORE: .NET SDK non trovato!
    echo Scarica e installa .NET 7.0 SDK da:
    echo https://dotnet.microsoft.com/download/dotnet/7.0
    pause
    exit /b 1
)

echo [1/5] Ripristino pacchetti NuGet...
dotnet restore RadioLoggerApp.sln
if %ERRORLEVEL% NEQ 0 (
    echo ERRORE: Ripristino pacchetti fallito!
    pause
    exit /b 1
)

echo.
echo [2/5] Pulizia build precedenti...
if exist ".\publish-standalone" rd /s /q ".\publish-standalone"

echo.
echo [3/5] Compilazione in modalita Release...
dotnet build RadioLoggerApp.sln -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERRORE: Compilazione fallita!
    pause
    exit /b 1
)

echo.
echo [4/5] Pubblicazione applicazione standalone...
echo ATTENZIONE: Questo processo richiede qualche minuto...
dotnet publish RadioLoggerApp.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish-standalone
if %ERRORLEVEL% NEQ 0 (
    echo ERRORE: Pubblicazione fallita!
    pause
    exit /b 1
)

echo.
echo [5/5] Build standalone completata con successo!
echo.
echo L'eseguibile standalone si trova in: .\publish-standalone\RadioLoggerApp.exe
echo.
echo DIMENSIONE: Circa 150-200 MB (include tutto il runtime .NET)
echo VANTAGGI: Non richiede .NET installato sul PC di destinazione
echo.
echo Puoi distribuire l'intera cartella "publish-standalone" su altri PC Windows
echo senza dover installare .NET Framework.
echo.

pause
