const fs = require('fs');
const path = require('path');
const os = require('os');

class PrintJobHandler {
  constructor(printerScanner, apiClient) {
    this.printerScanner = printerScanner;
    this.apiClient = apiClient;
    this.tempDir = path.join(os.tmpdir(), 'printnode-client');

    // Crea directory temporanea se non esiste
    if (!fs.existsSync(this.tempDir)) {
      fs.mkdirSync(this.tempDir, { recursive: true });
    }
  }

  // Processa un lavoro di stampa
  async processJob(job) {
    console.log(`\n📄 Processando lavoro: ${job.title} (${job.id})`);

    try {
      // Aggiorna stato a "printing"
      await this.apiClient.updateJobStatus(job.id, 'printing');

      // Converti file_data in Buffer se necessario
      let fileData;
      if (Buffer.isBuffer(job.file_data)) {
        fileData = job.file_data;
      } else if (job.file_data && job.file_data.type === 'Buffer' && Array.isArray(job.file_data.data)) {
        // Converti da oggetto JSON Buffer a Buffer vero
        fileData = Buffer.from(job.file_data.data);
      } else if (typeof job.file_data === 'string') {
        // Se è una stringa base64
        fileData = Buffer.from(job.file_data, 'base64');
      } else {
        throw new Error('Formato file_data non riconosciuto');
      }

      // Determina l'estensione dal content_type
      let extension = '';
      if (job.content_type) {
        const typeMap = {
          'application/pdf': '.pdf',
          'text/plain': '.txt',
          'image/jpeg': '.jpg',
          'image/png': '.png',
          'application/octet-stream': '.bin'
        };
        extension = typeMap[job.content_type] || '';
      }

      // Trova la stampante
      const printer = this.printerScanner.getPrinter(job.printer_name);

      if (!printer) {
        throw new Error(`Stampante non trovata: ${job.printer_name}`);
      }

      // Se è testo/binario/senza estensione (probabile ESC/POS), invia RAW
      if (!extension || extension === '.bin' || extension === '.txt') {
        console.log('📡 Invio dati RAW alla stampante ESC/POS...');
        await this.printerScanner.printDirect(printer.name, fileData);
      } else {
        // Altrimenti usa stampa normale con file
        console.log('🖨️ Stampa file tramite driver Windows...');

        const tempFilePath = path.join(
          this.tempDir,
          `${job.id}_${job.title || 'document'}`.replace(/[^a-z0-9._-]/gi, '_') + extension
        );

        fs.writeFileSync(tempFilePath, fileData);

        try {
          await this.printerScanner.printFile(printer.name, tempFilePath);
        } finally {
          // Pulisci file temporaneo subito dopo la stampa
          setTimeout(() => {
            try {
              fs.unlinkSync(tempFilePath);
            } catch (e) {}
          }, 2000);
        }
      }

      // Aggiorna stato a "completed"
      await this.apiClient.updateJobStatus(job.id, 'completed');

      console.log(`✓ Lavoro completato: ${job.title}`);
      return true;

    } catch (error) {
      console.error(`✗ Errore stampa: ${error.message}`);

      // Aggiorna stato a "failed"
      await this.apiClient.updateJobStatus(job.id, 'failed', error.message);

      return false;
    }
  }

  // Pulisci file temporanei vecchi
  cleanupTempFiles(maxAge = 3600000) {
    try {
      const files = fs.readdirSync(this.tempDir);
      const now = Date.now();

      files.forEach(file => {
        const filePath = path.join(this.tempDir, file);
        const stats = fs.statSync(filePath);

        if (now - stats.mtimeMs > maxAge) {
          fs.unlinkSync(filePath);
          console.log(`🗑️  File temporaneo rimosso: ${file}`);
        }
      });
    } catch (error) {
      console.error('Errore cleanup:', error.message);
    }
  }
}

module.exports = PrintJobHandler;
