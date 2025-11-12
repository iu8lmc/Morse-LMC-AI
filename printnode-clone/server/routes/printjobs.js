const express = require('express');
const router = express.Router();
const multer = require('multer');
const PrintJob = require('../models/PrintJob');
const Printer = require('../models/Printer');
const { authenticateApiKey } = require('../middleware/auth');

// Configurazione multer per upload file
const upload = multer({
  storage: multer.memoryStorage(),
  limits: { fileSize: 10 * 1024 * 1024 } // 10MB max
});

// Crea nuovo lavoro di stampa
router.post('/', upload.single('file'), (req, res) => {
  const db = req.app.locals.db;
  const { printer_id, title, content_type } = req.body;

  if (!printer_id) {
    return res.status(400).json({ error: 'printer_id richiesto' });
  }

  if (!req.file) {
    return res.status(400).json({ error: 'File da stampare richiesto' });
  }

  // Verifica che la stampante esista
  Printer.getById(db, printer_id, (err, printer) => {
    if (err || !printer) {
      return res.status(404).json({ error: 'Stampante non trovata' });
    }

    const jobData = {
      printer_id,
      client_id: printer.client_id,
      title: title || req.file.originalname,
      content_type: content_type || req.file.mimetype,
      file_data: req.file.buffer
    };

    PrintJob.create(db, jobData, (err, job) => {
      if (err) {
        return res.status(500).json({ error: 'Errore creazione lavoro di stampa' });
      }

      // Notifica il client via WebSocket
      const io = req.app.locals.io;
      io.to(`client:${printer.client_id}`).emit('new-print-job', {
        job_id: job.id,
        printer_id,
        title: job.title
      });

      res.status(201).json({
        success: true,
        job_id: job.id,
        status: 'pending'
      });
    });
  });
});

// Recupera lavori pendenti per un client (chiamato dal client)
router.get('/pending', authenticateApiKey, (req, res) => {
  const db = req.app.locals.db;

  PrintJob.getPending(db, req.client.id, (err, jobs) => {
    if (err) {
      return res.status(500).json({ error: 'Errore recupero lavori' });
    }
    res.json(jobs);
  });
});

// Aggiorna stato lavoro (chiamato dal client)
router.patch('/:id/status', authenticateApiKey, (req, res) => {
  const db = req.app.locals.db;
  const { status, error } = req.body;

  if (!['pending', 'printing', 'completed', 'failed'].includes(status)) {
    return res.status(400).json({ error: 'Status non valido' });
  }

  PrintJob.updateStatus(db, req.params.id, status, error, (err) => {
    if (err) {
      return res.status(500).json({ error: 'Errore aggiornamento stato' });
    }
    res.json({ success: true, status });
  });
});

// Lista lavori recenti
router.get('/recent', (req, res) => {
  const db = req.app.locals.db;
  const limit = parseInt(req.query.limit) || 50;

  PrintJob.getRecent(db, limit, (err, jobs) => {
    if (err) {
      return res.status(500).json({ error: 'Errore recupero lavori' });
    }

    // Rimuovi i dati binari per la visualizzazione
    const jobsWithoutData = jobs.map(job => ({
      ...job,
      file_data: job.file_data ? `[${job.file_data.length} bytes]` : null
    }));

    res.json(jobsWithoutData);
  });
});

// Info lavoro specifico
router.get('/:id', (req, res) => {
  const db = req.app.locals.db;

  PrintJob.getById(db, req.params.id, (err, job) => {
    if (err) {
      return res.status(500).json({ error: 'Errore recupero lavoro' });
    }
    if (!job) {
      return res.status(404).json({ error: 'Lavoro non trovato' });
    }

    // Per sicurezza, rimuovi i dati del file
    const jobInfo = { ...job };
    delete jobInfo.file_data;

    res.json(jobInfo);
  });
});

module.exports = router;
