require('dotenv').config();
const express = require('express');
const http = require('http');
const socketIO = require('socket.io');
const cors = require('cors');
const path = require('path');

const db = require('./config/database');
const Client = require('./models/Client');

const clientsRouter = require('./routes/clients');
const printersRouter = require('./routes/printers');
const printJobsRouter = require('./routes/printjobs');

const app = express();
const server = http.createServer(app);
const io = socketIO(server, {
  cors: {
    origin: '*',
    methods: ['GET', 'POST']
  }
});

const PORT = process.env.PORT || 3000;

// Middleware
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

// Rendi db e io disponibili nelle route
app.locals.db = db;
app.locals.io = io;

// Serve static files dalla dashboard
app.use(express.static(path.join(__dirname, '../web-dashboard/public')));

// API Routes
app.use('/api/clients', clientsRouter);
app.use('/api/printers', printersRouter);
app.use('/api/printjobs', printJobsRouter);

// Route principale - serve la dashboard
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, '../web-dashboard/views/index.html'));
});

// Health check
app.get('/health', (req, res) => {
  res.json({ status: 'ok', timestamp: new Date().toISOString() });
});

// WebSocket per comunicazione real-time con i client
io.on('connection', (socket) => {
  console.log('🔌 Nuovo socket connesso:', socket.id);

  // Autenticazione client
  socket.on('authenticate', (data) => {
    const { api_key } = data;

    if (!api_key) {
      socket.emit('auth-error', { error: 'API key mancante' });
      return;
    }

    Client.getByApiKey(db, api_key, (err, client) => {
      if (err || !client) {
        socket.emit('auth-error', { error: 'API key non valida' });
        return;
      }

      // Associa il socket al client
      socket.clientId = client.id;
      socket.join(`client:${client.id}`);

      // Aggiorna stato online
      Client.updateOnlineStatus(db, client.id, true, () => {
        socket.emit('authenticated', { client_id: client.id, name: client.name });
        console.log(`✓ Client autenticato: ${client.name} (${client.id})`);
      });
    });
  });

  // Aggiornamento stampanti
  socket.on('update-printers', (printers) => {
    if (!socket.clientId) {
      return socket.emit('error', { error: 'Non autenticato' });
    }

    console.log(`📥 Aggiornamento stampanti da client ${socket.clientId}:`, printers.length);

    // Notifica la dashboard dell'aggiornamento
    io.emit('printers-updated', { client_id: socket.clientId });
  });

  // Aggiornamento stato lavoro
  socket.on('job-status-update', (data) => {
    const { job_id, status, error } = data;
    console.log(`📄 Lavoro ${job_id}: ${status}`);

    // Notifica la dashboard
    io.emit('job-updated', { job_id, status, error });
  });

  // Disconnessione
  socket.on('disconnect', () => {
    if (socket.clientId) {
      Client.updateOnlineStatus(db, socket.clientId, false, () => {
        console.log(`✗ Client disconnesso: ${socket.clientId}`);
      });
    }
  });
});

// Gestione errori
app.use((err, req, res, next) => {
  console.error('Errore:', err.stack);
  res.status(500).json({ error: 'Errore interno del server' });
});

// Avvio server
server.listen(PORT, () => {
  console.log('\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
  console.log('🖨️  PrintNode Clone Server');
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
  console.log(`🚀 Server avviato su http://localhost:${PORT}`);
  console.log(`🌐 Dashboard disponibile su http://localhost:${PORT}`);
  console.log(`📡 WebSocket in ascolto per connessioni client`);
  console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');
});

// Graceful shutdown
process.on('SIGINT', () => {
  console.log('\n🛑 Arresto server...');
  server.close(() => {
    db.close(() => {
      console.log('✓ Server chiuso');
      process.exit(0);
    });
  });
});
