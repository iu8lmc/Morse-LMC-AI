const { execSync } = require('child_process');
const os = require('os');

class PrinterScannerSimple {
  constructor() {
    this.printers = [];
    this.lastScan = null;
  }

  // Scansiona le stampanti disponibili usando comandi di sistema
  scanPrinters() {
    try {
      const platform = os.platform();
      let systemPrinters = [];

      if (platform === 'win32') {
        // Windows: usa PowerShell con più dettagli
        try {
          const output = execSync('powershell "Get-Printer | Where-Object {$_.Type -eq \'Local\' -or $_.Type -eq \'Connection\'} | Select-Object Name,DriverName,Default,PrinterStatus,Color,Duplex | ConvertTo-Json"', { encoding: 'utf8' });
          systemPrinters = this.parsePowerShellPrinters(output);
        } catch (e) {
          // Fallback a wmic
          try {
            const output = execSync('wmic printer get name,default,status', { encoding: 'utf8' });
            systemPrinters = this.parseWindowsPrinters(output);
          } catch (e2) {
            console.error('Errore rilevamento stampanti Windows:', e2.message);
          }
        }
      } else if (platform === 'darwin') {
        // macOS: usa lpstat
        const output = execSync('lpstat -p -d', { encoding: 'utf8' });
        systemPrinters = this.parseMacPrinters(output);
      } else {
        // Linux: usa lpstat
        const output = execSync('lpstat -p -d', { encoding: 'utf8' });
        systemPrinters = this.parseLinuxPrinters(output);
      }

      this.printers = systemPrinters;
      this.lastScan = new Date();
      console.log(`✓ Rilevate ${this.printers.length} stampanti`);

      return this.printers;
    } catch (error) {
      console.error('Errore scansione stampanti:', error.message);
      return [];
    }
  }

  parseWindowsPrinters(output) {
    const lines = output.split('\n').slice(1); // Salta header
    const printers = [];

    // Lista di stampanti virtuali/inutili da escludere
    const excludeKeywords = [
      'microsoft print to pdf',
      'microsoft xps',
      'onenote',
      'fax',
      'send to',
      'foxit',
      'adobe pdf',
      'nitro',
      'pdfcreator',
      'doPDF',
      'novaPDF',
      'cutepdf',
      'print to file',
      'snagit'
    ];

    for (const line of lines) {
      if (line.trim()) {
        const parts = line.trim().split(/\s+/);
        if (parts.length >= 1) {
          const printerName = parts.slice(0, -2).join(' ') || parts[0];

          // Filtra stampanti virtuali
          const isVirtual = excludeKeywords.some(keyword =>
            printerName.toLowerCase().includes(keyword)
          );

          if (!isVirtual && printerName) {
            printers.push({
              name: printerName,
              driver: 'Windows Printer',
              isDefault: parts[parts.length - 2] === 'TRUE',
              status: parts[parts.length - 1] || 'idle',
              capabilities: {}
            });
          }
        }
      }
    }

    return printers;
  }

  parsePowerShellPrinters(output) {
    try {
      const data = JSON.parse(output);
      const printers = Array.isArray(data) ? data : [data];

      // Lista di stampanti virtuali da escludere
      const excludeKeywords = [
        'microsoft print to pdf',
        'microsoft xps',
        'onenote',
        'fax',
        'send to',
        'foxit',
        'adobe pdf',
        'nitro',
        'pdfcreator',
        'doPDF',
        'novaPDF',
        'cutepdf',
        'print to file',
        'snagit'
      ];

      return printers
        .filter(p => {
          const name = (p.Name || '').toLowerCase();
          return !excludeKeywords.some(keyword => name.includes(keyword));
        })
        .map(p => ({
          name: p.Name,
          driver: p.DriverName || 'Windows Printer',
          isDefault: p.Default || false,
          status: p.PrinterStatus === 0 ? 'idle' : 'busy',
          capabilities: {
            color: p.Color || false,
            duplex: p.Duplex || false
          }
        }));
    } catch (e) {
      return [];
    }
  }

  parseMacPrinters(output) {
    const lines = output.split('\n');
    const printers = [];
    let defaultPrinter = '';

    for (const line of lines) {
      if (line.includes('system default destination:')) {
        defaultPrinter = line.split(':')[1].trim();
      } else if (line.startsWith('printer')) {
        const name = line.split(' ')[1];
        printers.push({
          name: name,
          driver: 'CUPS Printer',
          isDefault: name === defaultPrinter,
          status: line.includes('disabled') ? 'offline' : 'idle',
          capabilities: {}
        });
      }
    }

    return printers;
  }

  parseLinuxPrinters(output) {
    return this.parseMacPrinters(output); // Stesso formato CUPS
  }

  getPrinter(name) {
    return this.printers.find(p => p.name === name);
  }

  getDefaultPrinter() {
    return this.printers.find(p => p.isDefault) || this.printers[0];
  }

  // Stampa un file usando comandi di sistema
  async printFile(printerName, filePath, options = {}) {
    return new Promise((resolve, reject) => {
      try {
        const platform = os.platform();
        let command;

        if (platform === 'win32') {
          // Windows: usa copy per invio RAW alla stampante
          // Questo funziona per tutti i tipi di file
          const escapedPath = filePath.replace(/\\/g, '\\\\');
          command = `copy /b "${filePath}" "\\\\%COMPUTERNAME%\\${printerName}"`;
        } else if (platform === 'darwin') {
          // macOS: usa lp
          command = `lp -d "${printerName}" "${filePath}"`;
        } else {
          // Linux: usa lp
          command = `lp -d "${printerName}" "${filePath}"`;
        }

        execSync(command, { stdio: 'pipe' });
        console.log(`✓ Lavoro di stampa inviato a ${printerName}`);
        resolve('success');
      } catch (error) {
        console.error('Errore stampa:', error.message);
        reject(error);
      }
    });
  }

  // Stampa dati RAW direttamente
  async printDirect(printerName, data, type = 'RAW') {
    return new Promise((resolve, reject) => {
      try {
        const platform = os.platform();
        const fs = require('fs');
        const path = require('path');

        if (platform === 'win32') {
          // Windows: scrivi direttamente sulla porta della stampante
          const tmpFile = path.join(os.tmpdir(), `printraw_${Date.now()}.bin`);
          fs.writeFileSync(tmpFile, data);

          try {
            // Usa copy /b per invio RAW
            execSync(`copy /b "${tmpFile}" "\\\\%COMPUTERNAME%\\${printerName}"`, { stdio: 'pipe' });
            console.log(`✓ Dati RAW inviati a ${printerName}`);
            resolve('success');
          } finally {
            // Pulisci file temporaneo
            setTimeout(() => {
              try { fs.unlinkSync(tmpFile); } catch (e) {}
            }, 2000);
          }
        } else {
          // Linux/macOS: usa lp con dati stdin
          const tmpFile = path.join(os.tmpdir(), `printraw_${Date.now()}.bin`);
          fs.writeFileSync(tmpFile, data);

          try {
            execSync(`lp -d "${printerName}" -o raw "${tmpFile}"`, { stdio: 'pipe' });
            console.log(`✓ Dati RAW inviati a ${printerName}`);
            resolve('success');
          } finally {
            setTimeout(() => {
              try { fs.unlinkSync(tmpFile); } catch (e) {}
            }, 2000);
          }
        }
      } catch (error) {
        console.error('Errore stampa RAW:', error.message);
        reject(error);
      }
    });
  }
}

module.exports = PrinterScannerSimple;
