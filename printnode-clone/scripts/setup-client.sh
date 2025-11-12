#!/bin/bash

echo "================================================"
echo "🖨️  PrintNode Clone - Setup Client"
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

# Naviga nella cartella client
cd "$(dirname "$0")/../client" || exit 1

# Installa dipendenze
echo "📦 Installazione dipendenze client..."
npm install

# Crea file .env se non esiste
if [ ! -f .env ]; then
    echo ""
    echo "📝 Creazione file .env..."
    cp .env.example .env
    echo "✓ File .env creato"
fi

echo ""
echo "================================================"
echo "⚠️  IMPORTANTE: Configurazione Richiesta"
echo "================================================"
echo ""
echo "Prima di avviare il client, devi configurare:"
echo ""
echo "1. Apri la dashboard web del server"
echo "2. Vai nella sezione 'Client'"
echo "3. Registra un nuovo client"
echo "4. Copia l'API Key generata"
echo "5. Incolla l'API Key nel file: client/.env"
echo ""
echo "Esempio .env:"
echo "  SERVER_URL=http://localhost:3000"
echo "  API_KEY=your-api-key-here"
echo "  CLIENT_NAME=My Computer"
echo ""
echo "Poi avvia il client con:"
echo "  cd client"
echo "  npm start"
echo ""
