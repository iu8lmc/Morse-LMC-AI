@echo off
echo ================================================
echo    PrintNode Client - Windows
echo ================================================
echo.

REM Verifica se esiste .env
if not exist ".env" (
    echo ERRORE: File .env non trovato!
    echo.
    echo Per configurare il client:
    echo 1. Copia .env.example in .env
    echo 2. Apri .env con Notepad
    echo 3. Modifica SERVER_URL e API_KEY
    echo.

    if exist ".env.example" (
        echo Vuoi creare il file .env ora? ^(s/n^)
        set /p risposta=
        if /i "%risposta%"=="s" (
            copy .env.example .env
            echo.
            echo File .env creato! Ora modificalo con Notepad:
            echo - Imposta SERVER_URL=http://87.106.40.164:3200
            echo - Imposta API_KEY con la chiave dalla dashboard
            echo.
            notepad .env
            echo.
            echo Configurazione completata? Premi INVIO per avviare il client
            pause > nul
        ) else (
            echo.
            echo Installazione annullata.
            pause
            exit
        )
    ) else (
        echo.
        pause
        exit
    )
)

echo Avvio client...
echo.

printnode-client-win.exe

echo.
echo Client terminato.
pause
