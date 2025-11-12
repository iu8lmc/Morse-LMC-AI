#!/bin/bash

# Script di Installazione PrintNode Clone su VPS
# ==============================================
# Copia questo script sul VPS e eseguilo
#
# Utilizzo:
#   chmod +x install-on-vps.sh
#   ./install-on-vps.sh

set -e

echo "================================================"
echo "🖨️  PrintNode Clone - Installazione VPS"
echo "================================================"
echo ""

# Colori per output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Funzioni helper
print_success() {
    echo -e "${GREEN}✓${NC} $1"
}

print_error() {
    echo -e "${RED}✗${NC} $1"
}

print_info() {
    echo -e "${YELLOW}ℹ${NC} $1"
}

# Verifica se siamo sul VPS
echo "📋 Verifica ambiente..."
CURRENT_USER=$(whoami)
print_info "Utente corrente: $CURRENT_USER"
print_info "Home directory: $HOME"
echo ""

# Verifica Node.js
echo "🔍 Verifica Node.js..."
if command -v node &> /dev/null; then
    NODE_VERSION=$(node --version)
    print_success "Node.js trovato: $NODE_VERSION"
else
    print_error "Node.js non trovato"
    echo ""
    echo "Vuoi installare Node.js 18.x? (s/n)"
    read -r risposta

    if [[ $risposta == "s" || $risposta == "S" ]]; then
        print_info "Installazione Node.js..."
        curl -fsSL https://deb.nodesource.com/setup_18.x | sudo -E bash -
        sudo apt-get install -y nodejs
        print_success "Node.js installato: $(node --version)"
    else
        print_error "Node.js è richiesto. Installalo e riprova."
        exit 1
    fi
fi

if command -v npm &> /dev/null; then
    NPM_VERSION=$(npm --version)
    print_success "NPM trovato: $NPM_VERSION"
else
    print_error "NPM non trovato"
    exit 1
fi

echo ""

# Verifica Git
echo "🔍 Verifica Git..."
if command -v git &> /dev/null; then
    GIT_VERSION=$(git --version)
    print_success "Git trovato: $GIT_VERSION"
else
    print_info "Git non trovato. Installazione..."
    sudo apt-get update
    sudo apt-get install -y git
    print_success "Git installato"
fi

echo ""

# Scarica progetto
PROJECT_DIR="$HOME/printnode-clone"
echo "📥 Download progetto..."

if [ -d "$PROJECT_DIR" ]; then
    print_info "Directory $PROJECT_DIR già esistente"
    echo "Vuoi sovrascriverla? (s/n)"
    read -r risposta

    if [[ $risposta == "s" || $risposta == "S" ]]; then
        rm -rf "$PROJECT_DIR"
        print_info "Directory rimossa"
    else
        print_error "Installazione annullata"
        exit 1
    fi
fi

print_info "Clonazione repository..."
git clone https://github.com/iu8lmc/Morse-LMC-AI.git "$PROJECT_DIR"
cd "$PROJECT_DIR"
git checkout claude/printnode-webapp-clone-011CV4BQyrJmcyZdek9y9Z4t
cd printnode-clone
print_success "Progetto scaricato in: $PROJECT_DIR/printnode-clone"

echo ""

# Installa dipendenze server
echo "📦 Installazione dipendenze server..."
cd "$PROJECT_DIR/printnode-clone/server"
npm install
print_success "Dipendenze installate"

echo ""

# Configura ambiente
echo "⚙️  Configurazione ambiente..."
if [ ! -f .env ]; then
    cp .env.example .env

    # Genera JWT secret casuale
    if command -v openssl &> /dev/null; then
        JWT_SECRET=$(openssl rand -base64 32)
        sed -i "s/your-secret-key-change-this/$JWT_SECRET/" .env
        print_success "JWT Secret generato automaticamente"
    else
        print_info "Modifica manualmente JWT_SECRET nel file .env"
    fi

    print_success "File .env creato"
else
    print_info "File .env già esistente"
fi

echo ""

# Configura firewall
echo "🔥 Configurazione firewall..."
if command -v ufw &> /dev/null; then
    if sudo ufw status | grep -q "Status: active"; then
        print_info "Apertura porta 3000..."
        sudo ufw allow 3000/tcp
        print_success "Porta 3000 aperta"
    else
        print_info "UFW non attivo, salta configurazione firewall"
    fi
else
    print_info "UFW non trovato, verifica manualmente il firewall"
fi

echo ""

# Chiedi se installare PM2
echo "🔧 Installazione PM2 (process manager)..."
echo "Vuoi installare PM2 per mantenere il server sempre attivo? (s/n)"
read -r risposta

if [[ $risposta == "s" || $risposta == "S" ]]; then
    if command -v pm2 &> /dev/null; then
        print_success "PM2 già installato"
    else
        print_info "Installazione PM2..."
        sudo npm install -g pm2
        print_success "PM2 installato"
    fi

    echo ""
    echo "🚀 Avvio server con PM2..."
    cd "$PROJECT_DIR/printnode-clone/server"

    # Ferma eventuali processi esistenti
    pm2 delete printnode-server 2>/dev/null || true

    # Avvia il server
    pm2 start server.js --name printnode-server
    pm2 save

    print_success "Server avviato con PM2"

    echo ""
    print_info "Configurazione avvio automatico..."
    echo "Esegui questo comando per configurare l'avvio automatico:"
    echo ""
    pm2 startup | grep "sudo"

else
    echo ""
    print_info "Per avviare il server manualmente:"
    echo "  cd $PROJECT_DIR/printnode-clone/server"
    echo "  npm start"
fi

echo ""
echo "================================================"
echo "🎉 Installazione completata con successo!"
echo "================================================"
echo ""
echo "📊 Informazioni:"
echo "   Directory: $PROJECT_DIR/printnode-clone"
echo "   Server: $PROJECT_DIR/printnode-clone/server"
echo "   Client: $PROJECT_DIR/printnode-clone/client"
echo ""

# Ottieni IP pubblico
PUBLIC_IP=$(curl -s ifconfig.me 2>/dev/null || echo "87.106.40.164")

echo "🌐 URL Dashboard:"
echo "   http://$PUBLIC_IP:3000"
echo ""

if command -v pm2 &> /dev/null; then
    echo "📋 Comandi PM2 utili:"
    echo "   pm2 list                      # Vedi processi attivi"
    echo "   pm2 logs printnode-server     # Vedi log in tempo reale"
    echo "   pm2 restart printnode-server  # Riavvia server"
    echo "   pm2 stop printnode-server     # Ferma server"
    echo "   pm2 monit                     # Monitor interattivo"
    echo ""
fi

echo "📚 Documentazione:"
echo "   README: $PROJECT_DIR/printnode-clone/README.md"
echo "   Quick Start: $PROJECT_DIR/printnode-clone/QUICKSTART.md"
echo ""

echo "✨ Prossimi passi:"
echo "   1. Apri http://$PUBLIC_IP:3000 nel browser"
echo "   2. Registra un nuovo client dalla dashboard"
echo "   3. Installa il client sui computer con le stampanti"
echo "   4. Inizia a stampare!"
echo ""

# Test connessione
echo "🧪 Test connessione al server..."
sleep 2
if curl -s http://localhost:3000/health > /dev/null; then
    print_success "Server raggiungibile!"
    echo ""
    curl -s http://localhost:3000/health | python3 -m json.tool 2>/dev/null || curl -s http://localhost:3000/health
else
    print_error "Server non raggiungibile"
    echo ""
    print_info "Verifica che il server sia avviato:"
    echo "   pm2 list"
    echo "   pm2 logs printnode-server"
fi

echo ""
echo "================================================"
