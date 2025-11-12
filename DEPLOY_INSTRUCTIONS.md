# 🚀 Come Installare PrintNode Clone sul VPS 87.106.40.164

## 📋 Informazioni VPS
- **IP**: 87.106.40.164
- **User**: iu8lmc
- **Directory destinazione**: /home/iu8lmc/printnode-clone

---

## ⚡ METODO RAPIDO (Consigliato)

### 1. Connettiti al VPS

```bash
ssh iu8lmc@87.106.40.164
```

### 2. Scarica e Esegui lo Script Automatico

```bash
# Scarica lo script di installazione
curl -O https://raw.githubusercontent.com/iu8lmc/Morse-LMC-AI/claude/printnode-webapp-clone-011CV4BQyrJmcyZdek9y9Z4t/install-on-vps.sh

# Oppure scarica tutto il repository
git clone https://github.com/iu8lmc/Morse-LMC-AI.git
cd Morse-LMC-AI
git checkout claude/printnode-webapp-clone-011CV4BQyrJmcyZdek9y9Z4t

# Rendi eseguibile lo script
chmod +x install-on-vps.sh

# Esegui lo script
./install-on-vps.sh
```

Lo script farà tutto automaticamente:
- ✅ Verifica e installa Node.js se necessario
- ✅ Scarica il progetto dal repository
- ✅ Installa tutte le dipendenze
- ✅ Configura l'ambiente
- ✅ Apre la porta nel firewall
- ✅ Installa e configura PM2
- ✅ Avvia il server

### 3. Apri la Dashboard

Dopo l'installazione, apri nel browser:

**http://87.106.40.164:3000**

---

## 📝 METODO MANUALE (Passo-Passo)

Se preferisci un controllo totale, segui la guida dettagliata:

**[INSTALL_VPS.md](INSTALL_VPS.md)** - Guida completa passo-passo

---

## 🎯 METODO 3: Deploy dal Tuo Computer

Se hai accesso SSH configurato sul tuo computer:

```bash
# Dal tuo computer locale
cd /home/user/Morse-LMC-AI
./deploy-to-vps.sh
```

Questo script:
1. Testa la connessione SSH
2. Crea la directory sul VPS
3. Trasferisce i file
4. Installa tutto automaticamente
5. Avvia il server

---

## ✅ Verifica Installazione

Dopo l'installazione, verifica che tutto funzioni:

### Test 1: Health Check

```bash
curl http://87.106.40.164:3000/health
```

Dovresti vedere: `{"status":"ok","timestamp":"..."}`

### Test 2: Dashboard

Apri nel browser: **http://87.106.40.164:3000**

Dovresti vedere la dashboard con:
- Statistiche (client, stampanti, lavori)
- Tab per navigazione
- Form di registrazione client

### Test 3: PM2 Status (se installato)

```bash
ssh iu8lmc@87.106.40.164
pm2 list
```

Dovresti vedere `printnode-server` nello stato `online`.

---

## 🔧 Comandi Utili

### Sul VPS

```bash
# Vedi lo stato del server
pm2 list

# Vedi i log in tempo reale
pm2 logs printnode-server

# Riavvia il server
pm2 restart printnode-server

# Ferma il server
pm2 stop printnode-server

# Avvia il server (se fermo)
pm2 start printnode-server

# Monitor risorse
pm2 monit
```

### Test Locale (sul VPS)

```bash
# Test health check
curl http://localhost:3000/health

# Test API
curl http://localhost:3000/api/clients

# Vedi le porte in ascolto
sudo netstat -tulpn | grep 3000
```

---

## 🐛 Risoluzione Problemi

### Problema: "Porta 3000 già in uso"

```bash
# Trova il processo sulla porta 3000
sudo lsof -i :3000

# Uccidi il processo (sostituisci PID)
kill -9 PID
```

### Problema: "Server non raggiungibile da internet"

```bash
# Verifica firewall
sudo ufw status
sudo ufw allow 3000/tcp

# Verifica che il server sia in ascolto su 0.0.0.0
sudo netstat -tulpn | grep 3000
```

### Problema: "npm install fallisce"

```bash
# Pulisci cache npm
npm cache clean --force

# Prova di nuovo
cd ~/printnode-clone/printnode-clone/server
rm -rf node_modules package-lock.json
npm install
```

### Problema: "Errori di permessi"

```bash
# Cambia owner dei file
sudo chown -R iu8lmc:iu8lmc ~/printnode-clone

# Verifica permessi
ls -la ~/printnode-clone
```

---

## 📊 Struttura File sul VPS

Dopo l'installazione:

```
/home/iu8lmc/printnode-clone/
├── printnode-clone/
│   ├── server/              # Server VPS
│   │   ├── server.js        # File principale
│   │   ├── .env             # Configurazione
│   │   ├── package.json
│   │   └── node_modules/
│   ├── client/              # Client (per installare sui PC)
│   ├── web-dashboard/       # Dashboard (servita dal server)
│   ├── README.md
│   └── QUICKSTART.md
└── install-on-vps.sh        # Script di installazione
```

---

## 🔒 (Opzionale) Configura HTTPS

Per usare HTTPS con un dominio:

### 1. Punta il dominio all'IP del VPS

Aggiungi un record A nel tuo DNS:
```
A    @    87.106.40.164
```

### 2. Installa Nginx

```bash
sudo apt update
sudo apt install nginx
```

### 3. Configura Nginx

```bash
sudo nano /etc/nginx/sites-available/printnode
```

Inserisci:

```nginx
server {
    listen 80;
    server_name tuo-dominio.com;

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

Abilita:

```bash
sudo ln -s /etc/nginx/sites-available/printnode /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### 4. Aggiungi SSL con Let's Encrypt

```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d tuo-dominio.com
```

---

## 📚 Prossimi Passi

Dopo l'installazione del server:

1. **Apri la dashboard**: http://87.106.40.164:3000
2. **Registra un client**: Tab "Client" → "Registra Nuovo Client"
3. **Copia l'API Key** generata
4. **Installa il client** sui computer con le stampanti
5. **Inizia a stampare**!

---

## 💡 Tips

- **Backup Database**: Il database SQLite è in `~/printnode-clone/printnode-clone/server/database.sqlite`
- **Log**: I log sono visibili con `pm2 logs printnode-server`
- **Aggiornamenti**: Fai `git pull` nella directory del progetto
- **Sicurezza**: Cambia il `JWT_SECRET` nel file `.env`

---

## 📞 Supporto

Documentazione completa: [README.md](printnode-clone/README.md)

Per problemi, controlla:
1. I log: `pm2 logs printnode-server`
2. Lo stato: `pm2 list`
3. La connessione: `curl http://localhost:3000/health`
4. Il firewall: `sudo ufw status`

---

**Buona installazione! 🚀**
