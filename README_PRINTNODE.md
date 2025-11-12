# 🖨️ PrintNode Clone - Web App per Condivisione Stampanti

Una soluzione completa e open-source per condividere stampanti locali tramite server VPS, ispirata a PrintNode.

## 🎯 Cosa Fa Questa Applicazione?

Permette di:
- **Stampare da remoto** su qualsiasi stampante connessa a computer locali
- **Gestire centralmente** tutte le stampanti da un'unica dashboard web
- **Monitorare in tempo reale** lo stato delle stampanti e dei lavori di stampa
- **Supportare più locazioni** - ogni computer con stampanti può connettersi al server
- **Inviare documenti** via web o API e stamparli automaticamente

## 📁 Struttura del Progetto

```
printnode-clone/
├── server/              # Server VPS (API + WebSocket + Database)
├── client/              # Client desktop (rilevamento stampanti)
├── web-dashboard/       # Dashboard web per gestione
├── scripts/             # Script di setup e avvio
├── README.md           # Documentazione completa
└── QUICKSTART.md       # Guida rapida per iniziare
```

## 🚀 Avvio Rapido

### 1. Setup Server

```bash
cd printnode-clone/server
npm install
cp .env.example .env
npm start
```

### 2. Apri Dashboard

Vai su **http://localhost:3000** e registra un nuovo client dalla dashboard.

### 3. Setup Client

```bash
cd printnode-clone/client
npm install
cp .env.example .env
# Configura .env con l'API key ottenuta dalla dashboard
npm start
```

## 📖 Documentazione

- **[QUICKSTART.md](printnode-clone/QUICKSTART.md)** - Guida rapida 5 minuti
- **[README.md](printnode-clone/README.md)** - Documentazione completa con API, deployment, troubleshooting

## ✨ Caratteristiche Principali

✅ **Architettura Client-Server** con comunicazione real-time
✅ **Dashboard Web** moderna e responsive
✅ **API REST** completa per integrazioni
✅ **WebSocket** per notifiche istantanee
✅ **Multi-client** - supporta computer illimitati
✅ **Sicurezza** con API key univoche per ogni client
✅ **Auto-discovery** delle stampanti
✅ **Supporto formati** PDF, TXT, JPG, PNG
✅ **Monitoraggio** lavori in tempo reale
✅ **Open Source** - MIT License

## 🏗️ Tecnologie Utilizzate

- **Backend**: Node.js + Express + Socket.io
- **Database**: SQLite
- **Frontend**: HTML/CSS/JavaScript
- **Client**: Node.js con modulo `printer`

## 🌐 Casi d'Uso

1. **E-commerce**: Stampa automatica di etichette di spedizione
2. **Ristoranti**: Invio ordini alle stampanti di cucina
3. **Uffici**: Stampa centralizzata da applicazioni web
4. **Ticket System**: Stampa automatica di biglietti/ricevute
5. **Magazzini**: Stampa etichette barcode da gestionale web

## 📊 Screenshot Dashboard

La dashboard include:
- Panoramica con statistiche in tempo reale
- Gestione client connessi
- Lista stampanti disponibili
- Cronologia lavori di stampa
- Form per invio nuovi lavori

## 🔐 Sicurezza

- Autenticazione tramite API key crittografiche
- Nessun dato persistente dei documenti stampati
- Supporto HTTPS per connessioni sicure
- Isolamento tra client diversi

## 🤝 Contributi

Questo progetto è open source! Contributi, issue e feature request sono benvenuti.

## 📄 Licenza

MIT License - Libero per uso personale e commerciale

---

**Sviluppato come alternativa open-source a PrintNode.com** 🚀

Per iniziare subito, leggi la [Guida Rapida](printnode-clone/QUICKSTART.md)!
