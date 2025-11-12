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

      // Salva il file temporaneamente
      const tempFilePath = path.join(
        this.tempDir,
        `${job.id}_${job.title || 'document'}`.replace(/[^a-z0-9._-]/gi, '_')
      );

      fs.writeFileSync(tempFilePath, job.file_data);

      // Trova la stampante
      const printer = this.printerScanner.getPrinter(job.printer_name);

      if (!printer) {
        throw new Error(`Stampante non trovata: ${job.printer_name}`);
      }

      // Invia alla stampante
      await this.printerScanner.printFile(printer.name, tempFilePath);

      // Aggiorna stato a "completed"
      await this.apiClient.updateJobStatus(job.id, 'completed');

      // Pulisci file temporaneo
      setTimeout(() => {
        try {
          fs.unlinkSync(tempFilePath);
        } catch (e) {
          // Ignora errori di cleanup
        }
      }, 5000);

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
