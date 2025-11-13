# 📦 Come Creare gli Eseguibili del Client

Se vuoi creare eseguibili standalone (.exe per Windows, binari per Linux/Mac):

## 🛠️ Compilazione Eseguibili

### 1. Installa pkg

```bash
cd ~/Morse-LMC-AI/printnode-clone/client
npm install
```

### 2. Compila per la tua piattaforma

**Per Windows (.exe):**
```bash
npm run build-win
```

**Per Linux:**
```bash
npm run build-linux
```

**Per macOS:**
```bash
npm run build-mac
```

**Per tutte le piattaforme:**
```bash
npm run build-all
```

### 3. Trova gli eseguibili

Gli eseguibili saranno in:
```
printnode-clone/client/dist/
├── printnode-client-win.exe     (Windows)
├── printnode-client-linux       (Linux)
└── printnode-client-macos       (macOS)
```

## 📤 Distribuire il Client

### Pacchetto Completo per gli Utenti

Crea una cartella con:
```
printnode-client/
├── printnode-client.exe         (o il binario per la tua piattaforma)
├── .env.example
└── README.txt
```

**README.txt:**
```
INSTALLAZIONE CLIENT PRINTNODE
==============================

1. Copia questo file .env.example in .env
2. Apri .env con un editor di testo
3. Modifica:
   - SERVER_URL con l'indirizzo del server
   - API_KEY con la chiave ottenuta dalla dashboard
   - CLIENT_NAME con un nome per questo computer

4. Avvia printnode-client.exe (Windows)
   o ./printnode-client-linux (Linux)
   o ./printnode-client-macos (macOS)

5. Il client rileverà automaticamente le stampanti
```

## 🪟 Avvio Automatico su Windows

Crea un file `start-client.bat`:

```batch
@echo off
cd /d "%~dp0"
printnode-client.exe
pause
```

Per avviarlo all'avvio di Windows:
1. Premi Win+R
2. Digita: shell:startup
3. Copia il file .bat in quella cartella

## 🐧 Avvio Automatico su Linux

Crea un servizio systemd:

```bash
sudo nano /etc/systemd/system/printnode-client.service
```

Contenuto:
```ini
[Unit]
Description=PrintNode Client
After=network.target

[Service]
Type=simple
User=tuouser
WorkingDirectory=/path/to/client
ExecStart=/path/to/printnode-client-linux
Restart=always

[Install]
WantedBy=multi-user.target
```

Abilita:
```bash
sudo systemctl enable printnode-client
sudo systemctl start printnode-client
```

## 🍎 Avvio Automatico su macOS

Crea un LaunchAgent:

```bash
nano ~/Library/LaunchAgents/com.printnode.client.plist
```

Contenuto:
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.printnode.client</string>
    <key>ProgramArguments</key>
    <array>
        <string>/path/to/printnode-client-macos</string>
    </array>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <true/>
</dict>
</plist>
```

Carica:
```bash
launchctl load ~/Library/LaunchAgents/com.printnode.client.plist
```

## ⚠️ Note Importanti

1. **Stampanti**: Devono essere installate nel sistema operativo prima di avviare il client
2. **Permessi**: Su Linux/Mac, rendi eseguibile: `chmod +x printnode-client-linux`
3. **Firewall**: Assicurati che il client possa connettersi al server (porta 3200)
4. **Antivirus**: Potrebbe bloccare l'eseguibile, aggiungi un'eccezione se necessario

## 🔧 Troubleshooting

### Windows: "Windows ha protetto il PC"
- Clicca "Ulteriori informazioni"
- Clicca "Esegui comunque"

### Linux: "Permission denied"
```bash
chmod +x printnode-client-linux
```

### "Cannot find module"
Gli eseguibili pkg includono tutte le dipendenze, ma alcuni moduli nativi potrebbero richiedere librerie di sistema:

**Linux:**
```bash
sudo apt-get install libcups2-dev
```

**Windows:** Installa Visual C++ Redistributable

### Test del Client

Avvia il client e verifica:
1. Si connette al server
2. Rileva le stampanti locali
3. Può ricevere e stampare lavori di test
