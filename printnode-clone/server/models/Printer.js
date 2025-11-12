const { v4: uuidv4 } = require('uuid');

class Printer {
  static createOrUpdate(db, clientId, printerData, callback) {
    const id = printerData.id || uuidv4();

    db.run(
      `INSERT INTO printers (id, client_id, name, driver, is_default, capabilities)
       VALUES (?, ?, ?, ?, ?, ?)
       ON CONFLICT(id) DO UPDATE SET
         name = excluded.name,
         driver = excluded.driver,
         is_default = excluded.is_default,
         capabilities = excluded.capabilities`,
      [
        id,
        clientId,
        printerData.name,
        printerData.driver || '',
        printerData.isDefault ? 1 : 0,
        JSON.stringify(printerData.capabilities || {})
      ],
      function(err) {
        if (err) return callback(err);
        callback(null, { id, ...printerData });
      }
    );
  }

  static getByClient(db, clientId, callback) {
    db.all(
      'SELECT * FROM printers WHERE client_id = ?',
      [clientId],
      callback
    );
  }

  static getAll(db, callback) {
    db.all(
      `SELECT p.*, c.name as client_name, c.is_online as client_online
       FROM printers p
       JOIN clients c ON p.client_id = c.id`,
      callback
    );
  }

  static getById(db, id, callback) {
    db.get('SELECT * FROM printers WHERE id = ?', [id], callback);
  }

  static deleteByClient(db, clientId, callback) {
    db.run('DELETE FROM printers WHERE client_id = ?', [clientId], callback);
  }
}

module.exports = Printer;
