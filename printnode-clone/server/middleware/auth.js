const jwt = require('jsonwebtoken');

const authenticateApiKey = (req, res, next) => {
  const apiKey = req.headers['x-api-key'] || req.query.api_key;

  if (!apiKey) {
    return res.status(401).json({ error: 'API key mancante' });
  }

  const db = req.app.locals.db;

  db.get('SELECT * FROM clients WHERE api_key = ?', [apiKey], (err, client) => {
    if (err) {
      return res.status(500).json({ error: 'Errore server' });
    }

    if (!client) {
      return res.status(401).json({ error: 'API key non valida' });
    }

    req.client = client;
    next();
  });
};

module.exports = { authenticateApiKey };
