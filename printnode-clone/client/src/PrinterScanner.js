const printer = require('printer');

class PrinterScanner {
  constructor() {
    this.printers = [];
    this.lastScan = null;
  }

  // Scansiona le stampanti disponibili
  scanPrinters() {
    try {
      const systemPrinters = printer.getPrinters();

      this.printers = systemPrinters.map(p => ({
        name: p.name,
        driver: p.driverName || 'Unknown',
        isDefault: p.isDefault || false,
        status: this.getPrinterStatus(p),
        capabilities: {
          color: p.options?.color || false,
          duplex: p.options?.duplex || false,
          paperSizes: p.options?.paperSizes || []
        }
      }));

      this.lastScan = new Date();
      console.log(`✓ Rilevate ${this.printers.length} stampanti`);

      return this.printers;
    } catch (error) {
      console.error('Errore scansione stampanti:', error.message);
      return [];
    }
  }

  getPrinterStatus(printer) {
    // Determina lo stato della stampante
    if (printer.status) {
      if (printer.status.includes('error')) return 'error';
      if (printer.status.includes('offline')) return 'offline';
    }
    return 'idle';
  }

  // Ottieni una stampante specifica per nome
  getPrinter(name) {
    return this.printers.find(p => p.name === name);
  }

  // Ottieni la stampante predefinita
  getDefaultPrinter() {
    try {
      const defaultPrinterName = printer.getDefaultPrinterName();
      return this.printers.find(p => p.name === defaultPrinterName);
    } catch (error) {
      return this.printers.find(p => p.isDefault) || this.printers[0];
    }
  }

  // Stampa un file
  async printFile(printerName, filePath, options = {}) {
    return new Promise((resolve, reject) => {
      try {
        printer.printFile({
          name: printerName,
          filename: filePath,
          options: options,
          success: (jobID) => {
            console.log(`✓ Lavoro di stampa inviato: ${jobID}`);
            resolve(jobID);
          },
          error: (err) => {
            console.error('Errore stampa:', err);
            reject(err);
          }
        });
      } catch (error) {
        reject(error);
      }
    });
  }

  // Stampa dati RAW (per etichette ZPL/EPL, ecc.)
  async printDirect(printerName, data, type = 'RAW') {
    return new Promise((resolve, reject) => {
      try {
        printer.printDirect({
          data: data,
          printer: printerName,
          type: type,
          success: (jobID) => {
            console.log(`✓ Stampa diretta completata: ${jobID}`);
            resolve(jobID);
          },
          error: (err) => {
            console.error('Errore stampa diretta:', err);
            reject(err);
          }
        });
      } catch (error) {
        reject(error);
      }
    });
  }
}

module.exports = PrinterScanner;
