# 🚀 Quick Start Guide

Guida rapida per far partire PrintNode Clone in 5 minuti!

## 📋 Prerequisiti

- Node.js >= 14.x installato ([Download](https://nodejs.org/))
- Stampanti installate sul sistema (per il client)

## ⚡ Setup Veloce

### 1️⃣ Setup Server (su VPS o computer locale)

```bash
# Naviga nella cartella server
cd printnode-clone/server

# Installa dipendenze
npm install

# Configura ambiente
cp .env.example .env

# Avvia il server
npm start
```

**✅ Il server è attivo su http://localhost:3000**

### 2️⃣ Apri la Dashboard

Apri il browser e vai su: **http://localhost:3000**

Vedrai la dashboard con statistiche e gestione del sistema.

### 3️⃣ Registra un Client

1. Nella dashboard, clicca sulla tab **"Client"**
2. Clicca **"+ Registra Nuovo Client"**
3. Inserisci un nome (es: "Computer Ufficio")
4. **COPIA l'API Key** che appare (non sarà più visibile!)

### 4️⃣ Setup Client (sul computer con le stampanti)

```bash
# Naviga nella cartella client
cd printnode-clone/client

# Installa dipendenze
npm install

# Configura ambiente
cp .env.example .env
nano .env  # Oppure apri con un editor di testo
```

**Modifica il file `.env`:**

```env
SERVER_URL=http://localhost:3000
API_KEY=la-tua-api-key-copiata-prima
CLIENT_NAME=Computer Ufficio
```

**Avvia il client:**

```bash
npm start
```

**✅ Il client è connesso e le stampanti sono rilevate!**

### 5️⃣ Invia una Stampa di Test

1. Nella dashboard, vai alla tab **"Nuova Stampa"**
2. Seleziona una stampante
3. Carica un file PDF o immagine
4. Clicca **"Invia alla Stampante"**

**🎉 La stampa partirà automaticamente!**

## 🌐 Deploy su VPS Pubblico

Se il server è su un VPS remoto, nel file `.env` del client usa:

```env
SERVER_URL=http://IP-DEL-TUO-VPS:3000
API_KEY=la-tua-api-key
CLIENT_NAME=Computer Casa
```

**Importante**: Assicurati che la porta 3000 sia aperta nel firewall del VPS!

## 🔧 Comandi Utili

### Server

```bash
npm start          # Avvia il server
npm run dev        # Avvia in modalità sviluppo (auto-reload)
```

### Client

```bash
npm start          # Avvia il client
```

### Con PM2 (per mantenerli sempre attivi)

```bash
# Installa PM2
npm install -g pm2

# Avvia server
cd server
pm2 start server.js --name printnode-server

# Avvia client
cd ../client
pm2 start src/client.js --name printnode-client

# Vedi i processi attivi
pm2 list

# Vedi i log
pm2 logs

# Salva per riavvio automatico
pm2 save
pm2 startup
```

## ❓ Problemi Comuni

### "Errore: API key non valida"

- Verifica di aver copiato correttamente l'API key
- Controlla che non ci siano spazi extra nel file .env
- Assicurati che il client sia registrato nella dashboard

### "Server non raggiungibile"

- Verifica che il server sia avviato
- Controlla che l'URL in `.env` sia corretto
- Verifica che la porta 3000 non sia bloccata dal firewall

### "Nessuna stampante rilevata"

- Verifica che le stampanti siano installate nel sistema operativo
- Su Linux, installa CUPS: `sudo apt install cups`
- Riavvia il client dopo aver installato nuove stampanti

## 📚 Documentazione Completa

Per maggiori dettagli, consulta il [README.md](README.md) completo.

## 💡 Tips

1. **Multiple Client**: Puoi avere client su computer diversi, ognuno con la sua API key
2. **Stampanti Multiple**: Il client rileva automaticamente tutte le stampanti installate
3. **Formati Supportati**: PDF, TXT, JPG, PNG, e stampa RAW per etichette
4. **Monitoraggio Real-time**: La dashboard si aggiorna automaticamente ogni 5 secondi

---

**Buona stampa! 🖨️**
