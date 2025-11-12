const express = require('express');
const router = express.Router();
const Client = require('../models/Client');

// Crea nuovo client (registrazione)
router.post('/register', (req, res) => {
  const { name } = req.body;

  if (!name) {
    return res.status(400).json({ error: 'Nome client richiesto' });
  }

  const db = req.app.locals.db;

  Client.create(db, name, (err, client) => {
    if (err) {
      return res.status(500).json({ error: 'Errore creazione client' });
    }
    res.status(201).json(client);
  });
});

// Lista tutti i client
router.get('/', (req, res) => {
  const db = req.app.locals.db;

  Client.getAll(db, (err, clients) => {
    if (err) {
      return res.status(500).json({ error: 'Errore recupero client' });
    }
    res.json(clients);
  });
});

// Info client specifico
router.get('/:id', (req, res) => {
  const db = req.app.locals.db;

  Client.getById(db, req.params.id, (err, client) => {
    if (err) {
      return res.status(500).json({ error: 'Errore recupero client' });
    }
    if (!client) {
      return res.status(404).json({ error: 'Client non trovato' });
    }
    res.json(client);
  });
});

module.exports = router;
