#!/bin/bash

# Script di Installazione PrintNode Clone su VPS
# Esegui: bash install-vps-fixed.sh

set -e

echo "================================================"
echo "🖨️  PrintNode Clone - Installazione VPS"
echo "================================================"
echo ""

# Verifica ambiente
echo "📋 Verifica ambiente..."
CURRENT_USER=$(whoami)
echo "Utente: $CURRENT_USER"
echo "Home: $HOME"
echo ""

# Verifica Node.js
echo "🔍 Verifica Node.js..."
if command -v node &> /dev/null; then
    echo "✓ Node.js: $(node --version)"
    echo "✓ NPM: $(npm --version)"
else
    echo "✗ Node.js non trovato"
    echo ""
    echo "Installazione Node.js 18.x..."
    curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
    sudo apt-get install -y nodejs
    echo "✓ Node.js installato: $(node --version)"
fi

echo ""

# Verifica Git
echo "🔍 Verifica Git..."
if ! command -v git &> /dev/null; then
    echo "Installazione Git..."
    sudo apt-get update
    sudo apt-get install -y git
fi
echo "✓ Git: $(git --version)"

echo ""

# Directory progetto
PROJECT_DIR="$HOME/printnode-clone"
echo "📥 Preparazione directory..."

if [ -d "$PROJECT_DIR/printnode-clone" ]; then
    echo "⚠️  Directory già esistente"
    echo "Vuoi sovrascriverla? (s/n)"
    read -r risposta
    if [[ $risposta == "s" || $risposta == "S" ]]; then
        rm -rf "$PROJECT_DIR"
    else
        echo "Installazione annullata"
        exit 1
    fi
fi

# Siamo già dentro Morse-LMC-AI, usa quello
if [ -f "$HOME/Morse-LMC-AI/printnode-clone/server/server.js" ]; then
    echo "✓ Progetto trovato in ~/Morse-LMC-AI"
    PROJECT_DIR="$HOME/Morse-LMC-AI/printnode-clone"
else
    echo "Clonazione repository..."
    cd ~
    if [ ! -d "Morse-LMC-AI" ]; then
        git clone https://github.com/iu8lmc/Morse-LMC-AI.git
    fi
    cd Morse-LMC-AI
    git checkout claude/printnode-webapp-clone-011CV4BQyrJmcyZdek9y9Z4t
    git pull
    PROJECT_DIR="$HOME/Morse-LMC-AI/printnode-clone"
fi

echo "✓ Directory: $PROJECT_DIR"
echo ""

# Installa dipendenze
echo "📦 Installazione dipendenze server..."
cd "$PROJECT_DIR/server"
npm install
echo "✓ Dipendenze installate"

echo ""

# Configura .env
echo "⚙️  Configurazione ambiente..."
if [ ! -f .env ]; then
    cp .env.example .env

    # Genera JWT secret
    if command -v openssl &> /dev/null; then
        JWT_SECRET=$(openssl rand -base64 32)
        sed -i "s/your-secret-key-change-this/$JWT_SECRET/" .env
        echo "✓ JWT Secret generato"
    fi

    echo "✓ File .env creato"
else
    echo "ℹ File .env già esistente"
fi

echo ""

# Firewall
echo "🔥 Configurazione firewall..."
if command -v ufw &> /dev/null && sudo ufw status | grep -q "Status: active"; then
    sudo ufw allow 3000/tcp
    echo "✓ Porta 3000 aperta"
else
    echo "ℹ Verifica manualmente che la porta 3000 sia aperta"
fi

echo ""

# PM2
echo "🔧 Installazione PM2..."
if ! command -v pm2 &> /dev/null; then
    sudo npm install -g pm2
    echo "✓ PM2 installato"
else
    echo "✓ PM2 già presente"
fi

echo ""
echo "🚀 Avvio server..."
cd "$PROJECT_DIR/server"

# Rimuovi processo esistente
pm2 delete printnode-server 2>/dev/null || true

# Avvia
pm2 start server.js --name printnode-server
pm2 save

echo "✓ Server avviato"

echo ""
echo "================================================"
echo "🎉 Installazione completata!"
echo "================================================"
echo ""

PUBLIC_IP=$(curl -s ifconfig.me 2>/dev/null || echo "87.106.40.164")

echo "🌐 Dashboard: http://$PUBLIC_IP:3000"
echo ""
echo "📋 Comandi utili:"
echo "   pm2 list                      # Processi"
echo "   pm2 logs printnode-server     # Log"
echo "   pm2 restart printnode-server  # Riavvia"
echo "   pm2 monit                     # Monitor"
echo ""

echo "🧪 Test server..."
sleep 3
if curl -s http://localhost:3000/health > /dev/null; then
    echo "✓ Server raggiungibile!"
    curl -s http://localhost:3000/health
else
    echo "⚠️  Server non risponde ancora, controlla con: pm2 logs"
fi

echo ""
echo "Configurazione avvio automatico..."
echo "Esegui: pm2 startup"
echo ""
