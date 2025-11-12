# 🖨️ PrintNode Clone

Una web application completa per condividere stampanti locali su server VPS, clone open-source di PrintNode.

## 📋 Caratteristiche

- **Server VPS**: API REST + WebSocket per gestione centralizzata
- **Client Desktop**: Software multi-piattaforma per rilevamento automatico stampanti
- **Dashboard Web**: Interfaccia moderna per gestire stampanti e lavori di stampa
- **Real-time**: Comunicazione istantanea tramite WebSocket
- **Multi-client**: Supporto per più computer/stampanti contemporaneamente
- **Sicurezza**: Autenticazione tramite API key univoche

## 🏗️ Architettura

```
┌─────────────────┐
│  Web Dashboard  │  ← Interfaccia web per gestione
└────────┬────────┘
         │
    ┌────▼─────┐
    │  Server  │  ← VPS con API REST + WebSocket
    │   VPS    │     Database SQLite
    └────┬─────┘
         │
    ┌────▼──────────────────┐
    │                       │
┌───▼────┐            ┌────▼────┐
│ Client │            │ Client  │  ← Software desktop
│   #1   │            │   #2    │     sulle macchine locali
└───┬────┘            └────┬────┘
    │                      │
┌───▼────┐            ┌────▼────┐
│Stampanti│            │Stampanti│  ← Stampanti fisiche
└────────┘            └─────────┘
```

## 🚀 Installazione

### 1. Server VPS

```bash
cd printnode-clone/server

# Installa dipendenze
npm install

# Configura ambiente
cp .env.example .env
nano .env  # Modifica le impostazioni se necessario

# Avvia il server
npm start
```

Il server sarà disponibile su `http://localhost:3000`

### 2. Client Desktop

```bash
cd printnode-clone/client

# Installa dipendenze
npm install

# Configura ambiente
cp .env.example .env
nano .env
```

**IMPORTANTE**: Prima di avviare il client, devi:
1. Aprire la dashboard web (http://localhost:3000)
2. Andare nella tab "Client"
3. Cliccare "Registra Nuovo Client"
4. Copiare l'API key generata
5. Incollare l'API key nel file `.env` del client:

```env
SERVER_URL=http://your-vps-ip:3000
API_KEY=your-api-key-here
CLIENT_NAME=My Computer
```

Poi avvia il client:

```bash
npm start
```

## 📖 Guida Completa

### Registrazione di un Nuovo Client

1. Apri la dashboard web: `http://your-server:3000`
2. Vai nella tab **"Client"**
3. Clicca **"+ Registra Nuovo Client"**
4. Inserisci un nome descrittivo (es: "Computer Ufficio")
5. Copia l'**API Key** generata (non sarà più visibile!)
6. Configura il client desktop con questa API key

### Invio di un Lavoro di Stampa

#### Tramite Dashboard Web

1. Vai nella tab **"Nuova Stampa"**
2. Seleziona la stampante di destinazione
3. (Opzionale) Inserisci un titolo per il documento
4. Carica il file da stampare (PDF, TXT, JPG, PNG)
5. Clicca **"Invia alla Stampante"**

Il lavoro verrà immediatamente inviato al client connesso e stampato.

#### Tramite API

```bash
curl -X POST http://your-server:3000/api/printjobs \
  -F "printer_id=PRINTER_ID" \
  -F "title=Documento Test" \
  -F "file=@document.pdf"
```

### Monitoraggio

La dashboard mostra in tempo reale:

- **Client connessi** (online/offline)
- **Stampanti disponibili** per ogni client
- **Lavori di stampa** (pendenti, in corso, completati, falliti)
- **Statistiche** generali del sistema

## 🔧 API REST

### Endpoints Principali

#### Client

```bash
# Registra nuovo client
POST /api/clients/register
Body: { "name": "Client Name" }

# Lista tutti i client
GET /api/clients

# Info client specifico
GET /api/clients/:id
```

#### Stampanti

```bash
# Lista tutte le stampanti
GET /api/printers

# Stampanti di un client
GET /api/printers/client/:clientId

# Sincronizza stampanti (usato dal client)
POST /api/printers/sync
Headers: X-API-Key: your-api-key
Body: { "printers": [...] }
```

#### Lavori di Stampa

```bash
# Crea nuovo lavoro
POST /api/printjobs
Form-data:
  - printer_id: ID stampante
  - title: Titolo (opzionale)
  - file: File da stampare

# Lista lavori recenti
GET /api/printjobs/recent?limit=50

# Lavori pendenti (usato dal client)
GET /api/printjobs/pending
Headers: X-API-Key: your-api-key

# Aggiorna stato lavoro (usato dal client)
PATCH /api/printjobs/:id/status
Headers: X-API-Key: your-api-key
Body: { "status": "completed" }
```

### Stati Lavoro

- `pending`: In attesa di essere stampato
- `printing`: In fase di stampa
- `completed`: Stampato con successo
- `failed`: Errore durante la stampa

## 🔐 Sicurezza

- Ogni client ha un'**API key univoca**
- Le API key sono generate con crittografia sicura (32 bytes random)
- La connessione WebSocket richiede autenticazione
- I file di stampa sono memorizzati temporaneamente e eliminati dopo l'uso

## 🌐 Deploy su VPS

### 1. Configura il Server

```bash
# Clona il repository
git clone <your-repo>
cd printnode-clone/server

# Installa dipendenze
npm install

# Configura .env
nano .env
```

**Importante**: Cambia `JWT_SECRET` con una chiave segreta casuale!

### 2. Usa PM2 per mantenerlo attivo

```bash
# Installa PM2
npm install -g pm2

# Avvia il server
pm2 start server.js --name printnode-server

# Salva configurazione
pm2 save

# Avvia all'avvio del sistema
pm2 startup
```

### 3. Configura Nginx (opzionale)

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

### 4. Configura i Client Desktop

Su ogni computer con stampanti:

1. Scarica Node.js (https://nodejs.org/)
2. Copia la cartella `client`
3. Installa dipendenze: `npm install`
4. Configura `.env` con l'indirizzo del tuo VPS:
   ```env
   SERVER_URL=http://your-vps-ip:3000
   API_KEY=your-api-key
   ```
5. Avvia: `npm start`

**Per avvio automatico all'avvio del sistema:**

```bash
# Installa PM2
npm install -g pm2

# Avvia il client
pm2 start src/client.js --name printnode-client

# Salva e configura avvio automatico
pm2 save
pm2 startup
```

## 📱 Compatibilità

- **Server**: Linux, Windows, macOS
- **Client**: Windows, macOS, Linux
- **Browser**: Chrome, Firefox, Safari, Edge (moderni)

## 🛠️ Requisiti

- **Node.js** >= 14.x
- **NPM** >= 6.x
- Stampanti installate e configurate sul sistema operativo

## 📦 Dipendenze Principali

### Server
- Express.js - Framework web
- Socket.io - WebSocket real-time
- SQLite3 - Database
- Multer - Upload file

### Client
- Socket.io-client - Connessione WebSocket
- printer - Accesso alle stampanti di sistema
- axios - Client HTTP

## 🐛 Troubleshooting

### Il client non rileva le stampanti

- Verifica che le stampanti siano installate correttamente nel sistema operativo
- Su Linux, potrebbe essere necessario installare CUPS
- Verifica i permessi di accesso alle stampanti

### Errore di connessione al server

- Verifica che il server sia avviato e raggiungibile
- Controlla che la porta 3000 sia aperta sul firewall
- Verifica l'URL nel file `.env` del client

### API Key non valida

- Assicurati di aver copiato correttamente l'API key
- Verifica che non ci siano spazi extra nel file `.env`
- Ricontrolla che l'API key corrisponda a un client registrato

## 📄 Licenza

MIT License - Libero per uso personale e commerciale

## 🤝 Contributi

Contributi, issues e feature requests sono benvenuti!

## 📧 Supporto

Per problemi o domande, apri un issue su GitHub.

---

**Sviluppato come alternativa open-source a PrintNode** 🚀
