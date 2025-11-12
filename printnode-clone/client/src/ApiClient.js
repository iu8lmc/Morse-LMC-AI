const axios = require('axios');

class ApiClient {
  constructor(serverUrl, apiKey) {
    this.serverUrl = serverUrl;
    this.apiKey = apiKey;

    this.client = axios.create({
      baseURL: serverUrl,
      headers: {
        'X-API-Key': apiKey
      }
    });
  }

  // Sincronizza le stampanti con il server
  async syncPrinters(printers) {
    try {
      const response = await this.client.post('/api/printers/sync', {
        printers
      });
      return response.data;
    } catch (error) {
      console.error('Errore sincronizzazione stampanti:', error.message);
      throw error;
    }
  }

  // Recupera lavori di stampa pendenti
  async getPendingJobs() {
    try {
      const response = await this.client.get('/api/printjobs/pending');
      return response.data;
    } catch (error) {
      console.error('Errore recupero lavori:', error.message);
      return [];
    }
  }

  // Aggiorna lo stato di un lavoro
  async updateJobStatus(jobId, status, error = null) {
    try {
      const response = await this.client.patch(`/api/printjobs/${jobId}/status`, {
        status,
        error
      });
      return response.data;
    } catch (error) {
      console.error('Errore aggiornamento stato:', error.message);
      throw error;
    }
  }

  // Verifica connessione al server
  async checkHealth() {
    try {
      const response = await this.client.get('/health');
      return response.data.status === 'ok';
    } catch (error) {
      return false;
    }
  }
}

module.exports = ApiClient;
