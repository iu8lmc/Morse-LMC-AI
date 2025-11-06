@echo off
REM ========================================
REM Build Script per Morse Decoder AI v1.1
REM ========================================
REM Crea un pacchetto eseguibile standalone per Windows
REM
REM Requisiti:
REM - .NET 7.0 SDK installato
REM
REM Uso: build-release.bat
REM ========================================

echo ========================================
echo    Morse Decoder AI - Build Release
echo              v1.1.0
echo ========================================
echo.

REM Variabili di configurazione
set PROJECT_FILE=RadioLoggerApp.csproj
set OUTPUT_DIR=bin\Release\Publish
set PACKAGE_DIR=MorseDecoderAI_v1.1.0_win-x64
set ZIP_FILE=MorseDecoderAI_v1.1.0_win-x64.zip

REM Verifica che dotnet sia installato
echo [1/6] Verifica .NET SDK...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERRORE: .NET SDK non trovato!
    echo Scarica e installa da: https://dotnet.microsoft.com/download/dotnet/7.0
    pause
    exit /b 1
)
echo OK - .NET SDK trovato
echo.

REM Pulisci build precedenti
echo [2/6] Pulizia build precedenti...
if exist "%OUTPUT_DIR%" rmdir /s /q "%OUTPUT_DIR%"
if exist "%PACKAGE_DIR%" rmdir /s /q "%PACKAGE_DIR%"
if exist "%ZIP_FILE%" del /f /q "%ZIP_FILE%"
echo OK - Pulizia completata
echo.

REM Restore delle dipendenze
echo [3/6] Restore dipendenze NuGet...
dotnet restore %PROJECT_FILE%
if errorlevel 1 (
    echo ERRORE nel restore delle dipendenze
    pause
    exit /b 1
)
echo OK - Dipendenze ripristinate
echo.

REM Build in modalità Release
echo [4/6] Build del progetto...
dotnet build %PROJECT_FILE% --configuration Release --no-restore
if errorlevel 1 (
    echo ERRORE nella build
    pause
    exit /b 1
)
echo OK - Build completata
echo.

REM Publish dell'applicazione
echo [5/6] Pubblicazione applicazione...
echo     Target: Windows x64 (self-contained)
echo     Output: %OUTPUT_DIR%

dotnet publish %PROJECT_FILE% ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output %OUTPUT_DIR% ^
    -p:PublishSingleFile=true ^
    -p:PublishReadyToRun=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true

if errorlevel 1 (
    echo ERRORE nella pubblicazione
    pause
    exit /b 1
)
echo OK - Pubblicazione completata
echo.

REM Crea pacchetto di distribuzione
echo [6/6] Creazione pacchetto di distribuzione...

REM Crea directory del pacchetto
mkdir "%PACKAGE_DIR%" 2>nul

REM Copia l'eseguibile
copy "%OUTPUT_DIR%\MorseDecoderAI.exe" "%PACKAGE_DIR%\" >nul
echo OK - Eseguibile copiato

REM Copia il database (se esiste)
if exist "radiologger.db" (
    copy "radiologger.db" "%PACKAGE_DIR%\" >nul
    echo OK - Database copiato
)

REM Copia la documentazione
mkdir "%PACKAGE_DIR%\Docs" 2>nul

if exist "README.md" copy "README.md" "%PACKAGE_DIR%\Docs\" >nul
if exist "README_MORSE_DECODER.md" copy "README_MORSE_DECODER.md" "%PACKAGE_DIR%\Docs\" >nul
if exist "README_SETTINGS.md" copy "README_SETTINGS.md" "%PACKAGE_DIR%\Docs\" >nul
if exist "TEST_CHECKLIST.md" copy "TEST_CHECKLIST.md" "%PACKAGE_DIR%\Docs\" >nul

echo OK - Documentazione copiata

REM Crea README di installazione
echo Creazione README.txt...
(
echo ================================================================================
echo                     MORSE DECODER AI v1.1.0
echo                   RadioLogger Pro - Windows x64
echo ================================================================================
echo.
echo Per istruzioni di installazione e uso, consulta i file nella cartella Docs\
echo.
echo AVVIO RAPIDO:
echo   1. Avvia MorseDecoderAI.exe
echo   2. Click su "Morse Decoder AI" nella barra inferiore
echo   3. Seleziona dispositivo audio e click "Avvia Decodifica"
echo.
echo DOCUMENTAZIONE COMPLETA: Docs\README_MORSE_DECODER.md
echo IMPOSTAZIONI: Docs\README_SETTINGS.md
echo.
echo 73 de RadioLogger Pro!
echo ================================================================================
) > "%PACKAGE_DIR%\README.txt"

echo OK - README creato
echo.

echo ========================================
echo           BUILD COMPLETATA!
echo ========================================
echo.
echo Pacchetto creato in: %PACKAGE_DIR%
echo.
echo Per creare un file ZIP, usa Windows Explorer:
echo   1. Tasto destro su "%PACKAGE_DIR%"
echo   2. Invia a ^> Cartella compressa
echo.
echo PRONTO PER IL TESTING!
echo.
pause
