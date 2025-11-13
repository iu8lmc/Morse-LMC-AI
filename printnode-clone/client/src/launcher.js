#!/usr/bin/env node

require('dotenv').config();
const readline = require('readline');

// Impedisci chiusura immediata su errore
process.on('uncaughtException', (error) => {
  console.error('\n❌ ERRORE CRITICO:', error.message);
  console.error('\nStack trace:', error.stack);
  console.log('\nPremi INVIO per chiudere...');
  waitForEnter();
});

process.on('unhandledRejection', (error) => {
  console.error('\n❌ ERRORE:', error);
  console.log('\nPremi INVIO per chiudere...');
  waitForEnter();
});

function waitForEnter() {
  const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
  });

  rl.question('', () => {
    rl.close();
    process.exit(1);
  });
}

// Verifica configurazione
console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
console.log('🖨️  PrintNode Client - Avvio');
console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');

const SERVER_URL = process.env.SERVER_URL;
const API_KEY = process.env.API_KEY;
const CLIENT_NAME = process.env.CLIENT_NAME || 'Client Desktop';

console.log('📋 Configurazione:');
console.log(`   SERVER_URL: ${SERVER_URL || '❌ NON CONFIGURATO'}`);
console.log(`   API_KEY: ${API_KEY ? '✓ Presente' : '❌ NON CONFIGURATA'}`);
console.log(`   CLIENT_NAME: ${CLIENT_NAME}`);
console.log('');

if (!SERVER_URL) {
  console.error('❌ ERRORE: SERVER_URL non configurato nel file .env');
  console.log('\n📝 Come risolvere:');
  console.log('1. Crea un file chiamato ".env" nella stessa cartella dell\'eseguibile');
  console.log('2. Apri il file .env con Notepad');
  console.log('3. Aggiungi queste righe:\n');
  console.log('   SERVER_URL=http://87.106.40.164:3200');
  console.log('   API_KEY=la-tua-api-key');
  console.log('   CLIENT_NAME=Computer Ufficio\n');
  console.log('4. Salva il file e riavvia il client\n');
  console.log('Per ottenere l\'API KEY:');
  console.log('- Apri http://87.106.40.164:3200 nel browser');
  console.log('- Vai nella sezione "Client"');
  console.log('- Registra un nuovo client e copia l\'API Key');
  console.log('\nPremi INVIO per chiudere...');
  waitForEnter();
  return;
}

if (!API_KEY) {
  console.error('❌ ERRORE: API_KEY non configurata nel file .env');
  console.log('\n📝 Come ottenere l\'API KEY:');
  console.log(`1. Apri ${SERVER_URL} nel browser`);
  console.log('2. Vai nella tab "Client"');
  console.log('3. Clicca "Registra Nuovo Client"');
  console.log('4. Copia l\'API Key generata');
  console.log('5. Incollala nel file .env dopo "API_KEY="\n');
  console.log('Esempio: API_KEY=abc123def456...\n');
  console.log('Premi INVIO per chiudere...');
  waitForEnter();
  return;
}

// Tutto ok, avvia il client vero
console.log('✓ Configurazione valida, avvio client...\n');

// Prova a usare il client normale, altrimenti usa quello semplificato
let clientModule;
try {
  clientModule = require('./client-simple.js');
} catch (e) {
  try {
    clientModule = require('./client.js');
  } catch (e2) {
    console.error('❌ Impossibile caricare il modulo client');
    console.error('Errore:', e2.message);
    console.log('\nPremi INVIO per chiudere...');
    waitForEnter();
  }
}
