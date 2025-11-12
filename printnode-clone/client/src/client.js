require('dotenv').config();
const io = require('socket.io-client');
const PrinterScanner = require('./PrinterScanner');
const PrintJobHandler = require('./PrintJobHandler');
const ApiClient = require('./ApiClient');

const SERVER_URL = process.env.SERVER_URL || 'http://localhost:3000';
const API_KEY = process.env.API_KEY;
const CLIENT_NAME = process.env.CLIENT_NAME || 'Client Desktop';
const POLL_INTERVAL = parseInt(process.env.POLL_INTERVAL) || 5000;

if (!API_KEY) {
  console.error('❌ Errore: API_KEY non configurata nel file .env');
  console.log('\nPer configurare il client:');
  console.log('1. Copia .env.example in .env');
  console.log('2. Avvia il server e registra un nuovo client');
  console.log('3. Copia l\'API key nel file .env del client');
  process.exit(1);
}

class PrintNodeClient {
  constructor() {
    this.printerScanner = new PrinterScanner();
    this.apiClient = new ApiClient(SERVER_URL, API_KEY);
    this.socket = null;
    this.connected = false;
    this.printJobHandler = new PrintJobHandler(this.printerScanner, this.apiClient);
  }

  async start() {
    console.log('\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
    console.log('🖨️  PrintNode Client');
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
    console.log(`🌐 Connessione a: ${SERVER_URL}`);
    console.log(`📋 Client: ${CLIENT_NAME}`);
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');

    // Verifica connessione al server
    const serverOk = await this.apiClient.checkHealth();
    if (!serverOk) {
      console.error('❌ Server non raggiungibile. Riprovo...');
      setTimeout(() => this.start(), 5000);
      return;
    }

    // Scansiona stampanti iniziali
    const printers = this.printerScanner.scanPrinters();
    if (printers.length > 0) {
      console.log('\n📊 Stampanti rilevate:');
      printers.forEach(p => {
        console.log(`   - ${p.name}${p.isDefault ? ' (predefinita)' : ''}`);
      });
    } else {
      console.log('\n⚠️  Nessuna stampante rilevata');
    }

    // Connetti via WebSocket
    this.connectWebSocket();

    // Polling periodico per nuovi lavori
    this.startPolling();

    // Cleanup periodico file temporanei
    setInterval(() => {
      this.printJobHandler.cleanupTempFiles();
    }, 3600000); // Ogni ora
  }

  connectWebSocket() {
    this.socket = io(SERVER_URL, {
      transports: ['websocket', 'polling']
    });

    this.socket.on('connect', () => {
      console.log('\n✓ Connesso al server');

      // Autenticazione
      this.socket.emit('authenticate', { api_key: API_KEY });
    });

    this.socket.on('authenticated', (data) => {
      console.log(`✓ Autenticato come: ${data.name}`);
      this.connected = true;

      // Sincronizza stampanti
      this.syncPrinters();
    });

    this.socket.on('auth-error', (data) => {
      console.error('❌ Errore autenticazione:', data.error);
      console.log('Verifica che l\'API_KEY sia corretta');
      process.exit(1);
    });

    this.socket.on('new-print-job', (data) => {
      console.log(`\n🔔 Nuovo lavoro di stampa: ${data.title}`);
      this.checkForPendingJobs();
    });

    this.socket.on('disconnect', () => {
      console.log('\n✗ Disconnesso dal server');
      this.connected = false;
    });

    this.socket.on('error', (error) => {
      console.error('Errore WebSocket:', error.message);
    });
  }

  async syncPrinters() {
    try {
      const printers = this.printerScanner.scanPrinters();
      const result = await this.apiClient.syncPrinters(printers);
      console.log(`✓ Sincronizzate ${result.count} stampanti con il server`);

      // Notifica il server via socket
      if (this.socket && this.connected) {
        this.socket.emit('update-printers', printers);
      }
    } catch (error) {
      console.error('Errore sincronizzazione:', error.message);
    }
  }

  async checkForPendingJobs() {
    try {
      const jobs = await this.apiClient.getPendingJobs();

      if (jobs.length > 0) {
        console.log(`\n📥 ${jobs.length} lavoro/i in coda`);

        for (const job of jobs) {
          await this.printJobHandler.processJob(job);

          // Notifica il server
          if (this.socket && this.connected) {
            this.socket.emit('job-status-update', {
              job_id: job.id,
              status: 'completed'
            });
          }
        }
      }
    } catch (error) {
      console.error('Errore controllo lavori:', error.message);
    }
  }

  startPolling() {
    // Polling periodico per lavori pendenti (backup al WebSocket)
    setInterval(async () => {
      if (this.connected) {
        await this.checkForPendingJobs();
      }
    }, POLL_INTERVAL);

    // Re-scansione stampanti ogni minuto
    setInterval(async () => {
      if (this.connected) {
        await this.syncPrinters();
      }
    }, 60000);
  }
}

// Avvio client
const client = new PrintNodeClient();
client.start();

// Gestione chiusura graceful
process.on('SIGINT', () => {
  console.log('\n\n🛑 Arresto client...');
  if (client.socket) {
    client.socket.disconnect();
  }
  console.log('✓ Client chiuso');
  process.exit(0);
});
