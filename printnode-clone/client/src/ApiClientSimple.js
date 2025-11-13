const https = require('https');
const http = require('http');
const { URL } = require('url');

class ApiClientSimple {
  constructor(serverUrl, apiKey) {
    this.serverUrl = serverUrl;
    this.apiKey = apiKey;
  }

  // Helper per fare richieste HTTP
  async request(method, path, data = null) {
    return new Promise((resolve, reject) => {
      const url = new URL(path, this.serverUrl);
      const isHttps = url.protocol === 'https:';
      const lib = isHttps ? https : http;

      const options = {
        hostname: url.hostname,
        port: url.port || (isHttps ? 443 : 80),
        path: url.pathname + url.search,
        method: method,
        headers: {
          'X-API-Key': this.apiKey,
          'Content-Type': 'application/json'
        }
      };

      const req = lib.request(options, (res) => {
        let body = '';

        res.on('data', (chunk) => {
          body += chunk;
        });

        res.on('end', () => {
          try {
            const jsonData = JSON.parse(body);
            if (res.statusCode >= 200 && res.statusCode < 300) {
              resolve(jsonData);
            } else {
              reject(new Error(`HTTP ${res.statusCode}: ${JSON.stringify(jsonData)}`));
            }
          } catch (e) {
            if (res.statusCode >= 200 && res.statusCode < 300) {
              resolve(body);
            } else {
              reject(new Error(`HTTP ${res.statusCode}: ${body}`));
            }
          }
        });
      });

      req.on('error', (error) => {
        reject(error);
      });

      if (data) {
        req.write(JSON.stringify(data));
      }

      req.end();
    });
  }

  // Sincronizza le stampanti con il server
  async syncPrinters(printers) {
    try {
      const response = await this.request('POST', '/api/printers/sync', { printers });
      return response;
    } catch (error) {
      console.error('Errore sincronizzazione stampanti:', error.message);
      throw error;
    }
  }

  // Recupera lavori di stampa pendenti
  async getPendingJobs() {
    try {
      const response = await this.request('GET', '/api/printjobs/pending');
      return response;
    } catch (error) {
      console.error('Errore recupero lavori:', error.message);
      return [];
    }
  }

  // Aggiorna lo stato di un lavoro
  async updateJobStatus(jobId, status, error = null) {
    try {
      const response = await this.request('PATCH', `/api/printjobs/${jobId}/status`, {
        status,
        error
      });
      return response;
    } catch (error) {
      console.error('Errore aggiornamento stato:', error.message);
      throw error;
    }
  }

  // Verifica connessione al server
  async checkHealth() {
    try {
      const response = await this.request('GET', '/health');
      return response.status === 'ok';
    } catch (error) {
      return false;
    }
  }
}

module.exports = ApiClientSimple;
