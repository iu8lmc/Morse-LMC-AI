# ========================================
# Script di Pulizia e Build Automatico
# Morse Decoder AI v1.1
# ========================================
# Risolve problemi di file bloccati durante la build
# ========================================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "   Pulizia e Build Automatico" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Chiudi processi in esecuzione
Write-Host "[1/6] Chiusura processi in esecuzione..." -ForegroundColor Yellow
$processNames = @("MorseDecoderAI", "RadioLoggerApp")

foreach ($procName in $processNames) {
    $processes = Get-Process -Name $procName -ErrorAction SilentlyContinue
    if ($processes) {
        Write-Host "   Trovato processo: $procName (PID: $($processes.Id -join ', '))" -ForegroundColor Gray
        Stop-Process -Name $procName -Force -ErrorAction SilentlyContinue
        Write-Host "   ✓ Processo terminato" -ForegroundColor Green
    } else {
        Write-Host "   - Nessun processo $procName in esecuzione" -ForegroundColor Gray
    }
}

Write-Host ""

# 2. Attesa per rilascio file
Write-Host "[2/6] Attesa rilascio file..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
Write-Host "   ✓ Completato" -ForegroundColor Green
Write-Host ""

# 3. Pulizia directory bin
Write-Host "[3/6] Pulizia directory bin..." -ForegroundColor Yellow
if (Test-Path "bin") {
    $binSize = (Get-ChildItem -Path "bin" -Recurse -File | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "   Dimensione corrente: $($binSize.ToString('F2')) MB" -ForegroundColor Gray
    Remove-Item -Recurse -Force bin -ErrorAction SilentlyContinue
    Write-Host "   ✓ Directory bin eliminata" -ForegroundColor Green
} else {
    Write-Host "   - Directory bin non esistente" -ForegroundColor Gray
}
Write-Host ""

# 4. Pulizia directory obj
Write-Host "[4/6] Pulizia directory obj..." -ForegroundColor Yellow
if (Test-Path "obj") {
    Remove-Item -Recurse -Force obj -ErrorAction SilentlyContinue
    Write-Host "   ✓ Directory obj eliminata" -ForegroundColor Green
} else {
    Write-Host "   - Directory obj non esistente" -ForegroundColor Gray
}
Write-Host ""

# 5. Pulizia pacchetti precedenti
Write-Host "[5/6] Pulizia pacchetti precedenti..." -ForegroundColor Yellow
$packagesRemoved = 0
Get-ChildItem -Path . -Filter "MorseDecoderAI_v*" -Directory | ForEach-Object {
    Remove-Item -Recurse -Force $_.FullName
    $packagesRemoved++
}
Get-ChildItem -Path . -Filter "MorseDecoderAI_v*.zip" -File | ForEach-Object {
    Remove-Item -Force $_.FullName
    $packagesRemoved++
}

if ($packagesRemoved -gt 0) {
    Write-Host "   ✓ Rimossi $packagesRemoved pacchetti precedenti" -ForegroundColor Green
} else {
    Write-Host "   - Nessun pacchetto precedente da rimuovere" -ForegroundColor Gray
}
Write-Host ""

# 6. Attesa finale
Write-Host "[6/6] Attesa finale prima della build..." -ForegroundColor Yellow
Start-Sleep -Seconds 2
Write-Host "   ✓ Pronto per la build" -ForegroundColor Green
Write-Host ""

# Separator
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Avvio build
Write-Host "Avvio build-release.ps1..." -ForegroundColor Green
Write-Host ""

# Esegui lo script di build
& .\build-release.ps1

# Se la build fallisce, mostra suggerimenti
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "   BUILD FALLITA!" -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Suggerimenti:" -ForegroundColor Yellow
    Write-Host "  1. Riavvia PowerShell come Amministratore" -ForegroundColor Gray
    Write-Host "  2. Disabilita temporaneamente Windows Defender" -ForegroundColor Gray
    Write-Host "  3. Verifica manualmente che nessun processo blocchi i file:" -ForegroundColor Gray
    Write-Host "     Get-Process | Where-Object {`$_.Path -like '*Morse*'}" -ForegroundColor Gray
    Write-Host ""
}
