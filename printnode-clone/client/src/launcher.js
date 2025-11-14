#!/usr/bin/env node

const path = require('path');
const fs = require('fs');

// Determina la directory dell'eseguibile o dello script
const appDir = process.pkg ? path.dirname(process.execPath) : __dirname;
const envPath = path.join(appDir, '.env');

// Carica .env dalla directory dell'eseguibile
require('dotenv').config({ path: envPath });

const readline = require('readline');

// Crea file di log
const logPath = path.join(appDir, 'printnode-client.log');
const logStream = fs.createWriteStream(logPath, { flags: 'a' });

function log(message) {
  const timestamp = new Date().toISOString();
  const logMessage = `[${timestamp}] ${message}\n`;
  console.log(message);
  logStream.write(logMessage);
}

log('='.repeat(60));
log('PrintNode Client - Avvio');

// Impedisci chiusura immediata su errore
process.on('uncaughtException', (error) => {
  log('\n❌ ERRORE CRITICO: ' + error.message);
  log('Stack trace: ' + error.stack);
  log(`Log salvato in: ${logPath}`);
  console.error('\n❌ ERRORE CRITICO:', error.message);
  console.error('\nStack trace:', error.stack);
  console.log(`\nLog salvato in: ${logPath}`);
  console.log('\nPremi INVIO per chiudere...');
  waitForEnter();
});

process.on('unhandledRejection', (error) => {
  log('\n❌ ERRORE: ' + error);
  log(`Log salvato in: ${logPath}`);
  console.error('\n❌ ERRORE:', error);
  console.log(`\nLog salvato in: ${logPath}`);
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

log(`Directory eseguibile: ${appDir}`);
log(`File .env cercato in: ${envPath}`);
log(`File .env esiste: ${fs.existsSync(envPath) ? 'SI' : 'NO'}`);

const SERVER_URL = process.env.SERVER_URL;
const API_KEY = process.env.API_KEY;
const CLIENT_NAME = process.env.CLIENT_NAME || 'Client Desktop';

console.log('📋 Configurazione:');
console.log(`   Directory: ${appDir}`);
console.log(`   File .env: ${fs.existsSync(envPath) ? '✓ Trovato' : '❌ Non trovato'}`);
console.log(`   SERVER_URL: ${SERVER_URL || '❌ NON CONFIGURATO'}`);
console.log(`   API_KEY: ${API_KEY ? '✓ Presente' : '❌ NON CONFIGURATA'}`);
console.log(`   CLIENT_NAME: ${CLIENT_NAME}`);
console.log(`   Log: ${logPath}`);
console.log('');

log(`SERVER_URL: ${SERVER_URL || 'NON CONFIGURATO'}`);
log(`API_KEY: ${API_KEY ? 'Presente' : 'NON CONFIGURATA'}`);
log(`CLIENT_NAME: ${CLIENT_NAME}`);

if (!SERVER_URL) {
  log('ERRORE: SERVER_URL non configurato');
  console.error('❌ ERRORE: SERVER_URL non configurato nel file .env');
  console.log('\n📝 Come risolvere:');
  console.log(`1. Crea un file chiamato ".env" in: ${appDir}`);
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
  console.log(`\nLog salvato in: ${logPath}`);
  console.log('\nPremi INVIO per chiudere...');
  waitForEnter();
  return;
}

if (!API_KEY) {
  log('ERRORE: API_KEY non configurata');
  console.error('❌ ERRORE: API_KEY non configurata nel file .env');
  console.log('\n📝 Come ottenere l\'API KEY:');
  console.log(`1. Apri ${SERVER_URL} nel browser`);
  console.log('2. Vai nella tab "Client"');
  console.log('3. Clicca "Registra Nuovo Client"');
  console.log('4. Copia l\'API Key generata');
  console.log(`5. Incollala nel file .env in: ${appDir}\n`);
  console.log('Esempio: API_KEY=abc123def456...\n');
  console.log(`Log salvato in: ${logPath}`);
  console.log('Premi INVIO per chiudere...');
  waitForEnter();
  return;
}

// Tutto ok, avvia il client vero
log('Configurazione valida, avvio client...');
console.log('✓ Configurazione valida, avvio client...\n');

// Usa SOLO la versione semplificata (senza dipendenze native)
// Questo è necessario per gli eseguibili pkg
try {
  require('./client-simple.js');
} catch (e) {
  log('ERRORE caricamento client: ' + e.message);
  log('Stack: ' + e.stack);
  console.error('❌ Impossibile caricare il client');
  console.error('Errore:', e.message);
  console.error('Stack:', e.stack);
  console.log(`\nLog salvato in: ${logPath}`);
  console.log('\nPremi INVIO per chiudere...');
  waitForEnter();
}
