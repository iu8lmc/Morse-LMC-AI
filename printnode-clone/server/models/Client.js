const { v4: uuidv4 } = require('uuid');
const crypto = require('crypto');

class Client {
  static create(db, name, callback) {
    const id = uuidv4();
    const apiKey = crypto.randomBytes(32).toString('hex');

    db.run(
      'INSERT INTO clients (id, name, api_key) VALUES (?, ?, ?)',
      [id, name, apiKey],
      function(err) {
        if (err) return callback(err);
        callback(null, { id, name, api_key: apiKey });
      }
    );
  }

  static getById(db, id, callback) {
    db.get('SELECT * FROM clients WHERE id = ?', [id], callback);
  }

  static getByApiKey(db, apiKey, callback) {
    db.get('SELECT * FROM clients WHERE api_key = ?', [apiKey], callback);
  }

  static updateOnlineStatus(db, clientId, isOnline, callback) {
    db.run(
      'UPDATE clients SET is_online = ?, last_seen = CURRENT_TIMESTAMP WHERE id = ?',
      [isOnline ? 1 : 0, clientId],
      callback
    );
  }

  static getAll(db, callback) {
    db.all('SELECT id, name, created_at, last_seen, is_online FROM clients', callback);
  }
}

module.exports = Client;
