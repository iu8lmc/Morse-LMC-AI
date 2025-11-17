@echo off
REM Script di build per RadioLoggerApp - SliceMaster Suite
REM Questo script compila il progetto e crea l'eseguibile

echo ============================================
echo  RadioLoggerApp - SliceMaster Build Script
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

echo [1/4] Ripristino pacchetti NuGet...
dotnet restore RadioLoggerApp.sln
if %ERRORLEVEL% NEQ 0 (
    echo ERRORE: Ripristino pacchetti fallito!
    pause
    exit /b 1
)

echo.
echo [2/4] Compilazione in modalita Release...
dotnet build RadioLoggerApp.sln -c Release
if %ERRORLEVEL% NEQ 0 (
    echo ERRORE: Compilazione fallita!
    pause
    exit /b 1
)

echo.
echo [3/4] Pubblicazione applicazione...
dotnet publish RadioLoggerApp.csproj -c Release -r win-x64 --self-contained false -o ./publish
if %ERRORLEVEL% NEQ 0 (
    echo ERRORE: Pubblicazione fallita!
    pause
    exit /b 1
)

echo.
echo [4/4] Build completata con successo!
echo.
echo L'eseguibile si trova in: .\publish\RadioLoggerApp.exe
echo.
echo Per creare una versione standalone (senza richiedere .NET installato):
echo dotnet publish RadioLoggerApp.csproj -c Release -r win-x64 --self-contained true -o ./publish-standalone
echo.

pause
