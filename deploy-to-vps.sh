#!/bin/bash

# Script di Deploy per VPS
# ========================

set -e

echo "================================================"
echo "🖨️  PrintNode Clone - Deploy su VPS"
echo "================================================"
echo ""

# Configurazione
VPS_USER="iu8lmc"
VPS_HOST="87.106.40.164"
REMOTE_DIR="/home/iu8lmc/printnode-clone"
ARCHIVE="printnode-clone-deploy.tar.gz"

echo "📋 Configurazione:"
echo "   VPS: $VPS_USER@$VPS_HOST"
echo "   Cartella: $REMOTE_DIR"
echo ""

# Test connessione
echo "🔌 Test connessione SSH..."
if ! ssh -o ConnectTimeout=5 -o StrictHostKeyChecking=no $VPS_USER@$VPS_HOST "echo 'Connessione OK'" 2>/dev/null; then
    echo "❌ Impossibile connettersi al VPS"
    echo ""
    echo "Verifica:"
    echo "  1. Che l'IP sia corretto: $VPS_HOST"
    echo "  2. Che l'utente sia corretto: $VPS_USER"
    echo "  3. Che tu abbia accesso SSH configurato"
    echo ""
    echo "Per configurare l'accesso SSH:"
    echo "  ssh-copy-id $VPS_USER@$VPS_HOST"
    echo ""
    exit 1
fi

echo "✓ Connessione SSH funzionante"
echo ""

# Crea directory sul VPS
echo "📁 Creazione directory sul VPS..."
ssh $VPS_USER@$VPS_HOST "mkdir -p $REMOTE_DIR"
echo "✓ Directory creata: $REMOTE_DIR"
echo ""

# Trasferisce l'archivio
echo "📤 Trasferimento file..."
scp $ARCHIVE $VPS_USER@$VPS_HOST:$REMOTE_DIR/
echo "✓ File trasferiti"
echo ""

# Estrae e installa
echo "📦 Installazione sul VPS..."
ssh $VPS_USER@$VPS_HOST << 'ENDSSH'
cd ~/printnode-clone
tar -xzf printnode-clone-deploy.tar.gz
rm printnode-clone-deploy.tar.gz

echo "📋 Verifica Node.js..."
if ! command -v node &> /dev/null; then
    echo "⚠️  Node.js non trovato. Installazione..."
    curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
    sudo apt-get install -y nodejs
fi

node --version
npm --version

echo ""
echo "📦 Installazione dipendenze server..."
cd server
npm install

echo ""
echo "⚙️  Configurazione ambiente..."
if [ ! -f .env ]; then
    cp .env.example .env
    # Genera JWT secret casuale
    JWT_SECRET=$(openssl rand -base64 32)
    sed -i "s/your-secret-key-change-this/$JWT_SECRET/" .env
    echo "✓ File .env creato"
fi

echo ""
echo "================================================"
echo "✅ Installazione completata!"
echo "================================================"
echo ""
echo "Per avviare il server:"
echo "  cd ~/printnode-clone/server"
echo "  npm start"
echo ""
echo "Oppure con PM2 (per tenerlo sempre attivo):"
echo "  npm install -g pm2"
echo "  cd ~/printnode-clone/server"
echo "  pm2 start server.js --name printnode-server"
echo "  pm2 save"
echo "  pm2 startup"
echo ""
echo "Il server sarà disponibile su:"
echo "  http://87.106.40.164:3000"
echo ""

ENDSSH

echo ""
echo "================================================"
echo "🎉 Deploy completato con successo!"
echo "================================================"
echo ""
echo "Il tuo server PrintNode Clone è pronto su:"
echo "  http://87.106.40.164:3000"
echo ""
echo "Vuoi avviarlo ora? (s/n)"
read -r risposta

if [[ $risposta == "s" || $risposta == "S" ]]; then
    echo ""
    echo "🚀 Avvio server..."
    ssh $VPS_USER@$VPS_HOST "cd ~/printnode-clone/server && npm start &"
    echo ""
    echo "✓ Server avviato!"
    echo ""
    echo "Apri nel browser: http://87.106.40.164:3000"
fi

echo ""
echo "Per connetterti al VPS:"
echo "  ssh $VPS_USER@$VPS_HOST"
echo ""
