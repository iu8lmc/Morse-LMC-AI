const express = require('express');
const router = express.Router();
const Printer = require('../models/Printer');
const { authenticateApiKey } = require('../middleware/auth');

// Lista tutte le stampanti
router.get('/', (req, res) => {
  const db = req.app.locals.db;

  Printer.getAll(db, (err, printers) => {
    if (err) {
      return res.status(500).json({ error: 'Errore recupero stampanti' });
    }
    res.json(printers);
  });
});

// Stampanti di un client specifico
router.get('/client/:clientId', (req, res) => {
  const db = req.app.locals.db;

  Printer.getByClient(db, req.params.clientId, (err, printers) => {
    if (err) {
      return res.status(500).json({ error: 'Errore recupero stampanti' });
    }
    res.json(printers);
  });
});

// Info stampante specifica
router.get('/:id', (req, res) => {
  const db = req.app.locals.db;

  Printer.getById(db, req.params.id, (err, printer) => {
    if (err) {
      return res.status(500).json({ error: 'Errore recupero stampante' });
    }
    if (!printer) {
      return res.status(404).json({ error: 'Stampante non trovata' });
    }
    res.json(printer);
  });
});

// Aggiorna stampanti (usato dal client)
router.post('/sync', authenticateApiKey, (req, res) => {
  const db = req.app.locals.db;
  const printers = req.body.printers;

  if (!Array.isArray(printers)) {
    return res.status(400).json({ error: 'Array di stampanti richiesto' });
  }

  // PRIMA elimina tutte le stampanti vecchie del client per evitare duplicati
  Printer.deleteByClient(db, req.client.id, (err) => {
    if (err) {
      console.error('Errore eliminazione stampanti vecchie:', err);
      return res.status(500).json({ error: 'Errore eliminazione stampanti vecchie' });
    }

    if (printers.length === 0) {
      return res.json({ success: true, count: 0 });
    }

    // POI inserisci le stampanti nuove
    let completed = 0;
    const results = [];

    printers.forEach(printer => {
      Printer.createOrUpdate(db, req.client.id, printer, (err, result) => {
        completed++;
        if (!err) results.push(result);

        if (completed === printers.length) {
          res.json({ success: true, count: results.length, printers: results });
        }
      });
    });
  });
});

module.exports = router;
