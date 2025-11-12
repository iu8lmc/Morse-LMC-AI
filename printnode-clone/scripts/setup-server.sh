#!/bin/bash

echo "================================================"
echo "🖨️  PrintNode Clone - Setup Server"
echo "================================================"
echo ""

# Controlla Node.js
if ! command -v node &> /dev/null; then
    echo "❌ Node.js non trovato. Installalo da: https://nodejs.org/"
    exit 1
fi

echo "✓ Node.js versione: $(node --version)"
echo "✓ NPM versione: $(npm --version)"
echo ""

# Naviga nella cartella server
cd "$(dirname "$0")/../server" || exit 1

# Installa dipendenze
echo "📦 Installazione dipendenze server..."
npm install

# Crea file .env se non esiste
if [ ! -f .env ]; then
    echo ""
    echo "📝 Creazione file .env..."
    cp .env.example .env

    # Genera JWT secret casuale
    JWT_SECRET=$(openssl rand -base64 32 2>/dev/null || head -c 32 /dev/urandom | base64)
    sed -i.bak "s/your-secret-key-change-this/$JWT_SECRET/" .env && rm .env.bak

    echo "✓ File .env creato con chiave segreta casuale"
fi

echo ""
echo "================================================"
echo "✅ Setup completato!"
echo "================================================"
echo ""
echo "Per avviare il server:"
echo "  cd server"
echo "  npm start"
echo ""
echo "Il server sarà disponibile su http://localhost:3000"
echo ""
