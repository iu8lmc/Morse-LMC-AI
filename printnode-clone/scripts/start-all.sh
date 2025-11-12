#!/bin/bash

echo "================================================"
echo "🖨️  PrintNode Clone - Avvio Completo"
echo "================================================"
echo ""

# Avvia il server in background
cd "$(dirname "$0")/../server" || exit 1

echo "🚀 Avvio server..."
npm start &
SERVER_PID=$!

echo "✓ Server avviato (PID: $SERVER_PID)"
echo ""
echo "🌐 Dashboard disponibile su: http://localhost:3000"
echo ""
echo "Per avviare un client:"
echo "  cd client"
echo "  npm start"
echo ""
echo "Per fermare il server: kill $SERVER_PID"
echo ""

# Mantieni lo script attivo
wait $SERVER_PID
