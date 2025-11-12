const sqlite3 = require('sqlite3').verbose();
const path = require('path');

const dbPath = process.env.DATABASE_PATH || path.join(__dirname, '../database.sqlite');

const db = new sqlite3.Database(dbPath, (err) => {
  if (err) {
    console.error('Errore connessione database:', err.message);
  } else {
    console.log('✓ Database SQLite connesso');
    initDatabase();
  }
});

function initDatabase() {
  // Tabella utenti/client
  db.run(`CREATE TABLE IF NOT EXISTS clients (
    id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    api_key TEXT UNIQUE NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    last_seen DATETIME,
    is_online INTEGER DEFAULT 0
  )`);

  // Tabella stampanti
  db.run(`CREATE TABLE IF NOT EXISTS printers (
    id TEXT PRIMARY KEY,
    client_id TEXT NOT NULL,
    name TEXT NOT NULL,
    driver TEXT,
    status TEXT DEFAULT 'idle',
    is_default INTEGER DEFAULT 0,
    capabilities TEXT,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (client_id) REFERENCES clients(id) ON DELETE CASCADE
  )`);

  // Tabella lavori di stampa
  db.run(`CREATE TABLE IF NOT EXISTS print_jobs (
    id TEXT PRIMARY KEY,
    printer_id TEXT NOT NULL,
    client_id TEXT NOT NULL,
    title TEXT,
    content_type TEXT,
    file_data BLOB,
    status TEXT DEFAULT 'pending',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    printed_at DATETIME,
    error TEXT,
    FOREIGN KEY (printer_id) REFERENCES printers(id),
    FOREIGN KEY (client_id) REFERENCES clients(id)
  )`);

  console.log('✓ Tabelle database inizializzate');
}

module.exports = db;
