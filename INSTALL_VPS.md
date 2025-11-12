# 🖥️ Installazione su VPS - Guida Passo-Passo

## Informazioni VPS
- **IP**: 87.106.40.164
- **User**: iu8lmc
- **Directory**: /home/iu8lmc/printnode-clone

---

## 📝 Passo 1: Connettiti al VPS

Dal tuo computer locale:

```bash
ssh iu8lmc@87.106.40.164
```

---

## 📦 Passo 2: Installa Node.js (se non presente)

Verifica se Node.js è installato:

```bash
node --version
npm --version
```

Se non è installato:

```bash
# Installa Node.js 18.x
curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
sudo apt-get install -y nodejs

# Verifica installazione
node --version
npm --version
```

---

## 📥 Passo 3: Scarica il Progetto

### Opzione A: Con Git (consigliata)

```bash
cd ~
git clone https://github.com/iu8lmc/Morse-LMC-AI.git printnode-clone
cd printnode-clone
git checkout claude/printnode-webapp-clone-011CV4BQyrJmcyZdek9y9Z4t
cd printnode-clone
```

### Opzione B: Download Diretto

```bash
cd ~
mkdir -p printnode-clone
cd printnode-clone

# Scarica l'archivio (se disponibile su un server web)
# oppure trasferiscilo manualmente con SCP
```

### Opzione C: Copia Manuale

Dal tuo computer locale:

```bash
# Comprimi la cartella
cd /home/user/Morse-LMC-AI
tar -czf printnode-deploy.tar.gz printnode-clone/

# Trasferisci sul VPS
scp printnode-deploy.tar.gz iu8lmc@87.106.40.164:~/

# Sul VPS, estrai
ssh iu8lmc@87.106.40.164
cd ~
tar -xzf printnode-deploy.tar.gz
```

---

## ⚙️ Passo 4: Installa Dipendenze Server

```bash
cd ~/printnode-clone/server
npm install
```

Attendi il completamento (potrebbe richiedere 1-2 minuti).

---

## 🔐 Passo 5: Configura Ambiente

```bash
cd ~/printnode-clone/server
cp .env.example .env
nano .env
```

Modifica il file `.env`:

```env
PORT=3000
JWT_SECRET=your-super-secret-key-change-this-to-something-random
DATABASE_PATH=./database.sqlite
NODE_ENV=production
```

**IMPORTANTE**: Cambia `JWT_SECRET` con una stringa casuale!

Per generare una chiave casuale:

```bash
openssl rand -base64 32
```

Salva il file (CTRL+O, INVIO, CTRL+X in nano).

---

## 🔥 Passo 6: Configura Firewall

Apri la porta 3000:

```bash
# Se usi ufw
sudo ufw allow 3000/tcp
sudo ufw status

# Se usi firewalld
sudo firewall-cmd --permanent --add-port=3000/tcp
sudo firewall-cmd --reload
```

---

## 🚀 Passo 7: Avvia il Server

### Avvio Semplice (per test)

```bash
cd ~/printnode-clone/server
npm start
```

Il server sarà disponibile su: **http://87.106.40.164:3000**

### Avvio con PM2 (per produzione)

PM2 mantiene il server sempre attivo, anche dopo riavvio:

```bash
# Installa PM2
sudo npm install -g pm2

# Avvia il server
cd ~/printnode-clone/server
pm2 start server.js --name printnode-server

# Configura avvio automatico
pm2 save
pm2 startup
# Esegui il comando suggerito da PM2

# Comandi utili PM2
pm2 list              # Vedi processi attivi
pm2 logs printnode-server  # Vedi i log
pm2 restart printnode-server  # Riavvia
pm2 stop printnode-server     # Ferma
pm2 delete printnode-server   # Rimuovi
```

---

## 🌐 Passo 8: Testa il Server

Apri nel browser:

**http://87.106.40.164:3000**

Dovresti vedere la dashboard!

---

## 🔒 (Opzionale) Configura Nginx + HTTPS

Se vuoi usare un dominio e HTTPS:

### 1. Installa Nginx

```bash
sudo apt update
sudo apt install nginx
```

### 2. Configura Nginx

```bash
sudo nano /etc/nginx/sites-available/printnode
```

Inserisci:

```nginx
server {
    listen 80;
    server_name 87.106.40.164;  # O il tuo dominio

    location / {
        proxy_pass http://localhost:3000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}
```

Abilita il sito:

```bash
sudo ln -s /etc/nginx/sites-available/printnode /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### 3. (Opzionale) Aggiungi HTTPS con Let's Encrypt

```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d tuo-dominio.com
```

---

## 📊 Monitoraggio

### Vedi i log del server

```bash
# Con PM2
pm2 logs printnode-server

# O direttamente
tail -f ~/printnode-clone/server/logs/*.log
```

### Verifica stato

```bash
curl http://localhost:3000/health
```

Dovresti vedere: `{"status":"ok","timestamp":"..."}`

---

## 🐛 Troubleshooting

### Porta 3000 già in uso

```bash
# Trova il processo
sudo lsof -i :3000

# Oppure cambia porta nel .env
nano ~/printnode-clone/server/.env
# Cambia PORT=3000 in PORT=3001
```

### Server non raggiungibile

```bash
# Verifica che il server sia attivo
pm2 list
# O
ps aux | grep node

# Verifica firewall
sudo ufw status
# O
sudo firewall-cmd --list-all

# Testa localmente
curl http://localhost:3000
```

### Errori di permessi

```bash
# Assicurati che i file appartengano all'utente corretto
sudo chown -R iu8lmc:iu8lmc ~/printnode-clone
```

---

## ✅ Installazione Completata!

Il tuo server PrintNode Clone è ora attivo su:
- **http://87.106.40.164:3000**

Prossimi passi:
1. Apri la dashboard nel browser
2. Registra un nuovo client
3. Installa il client sui computer con le stampanti
4. Inizia a stampare!

---

## 📞 Supporto

Per problemi, controlla:
- I log: `pm2 logs printnode-server`
- Lo stato: `pm2 list`
- La connessione: `curl http://localhost:3000/health`
