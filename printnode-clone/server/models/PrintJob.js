const { v4: uuidv4 } = require('uuid');

class PrintJob {
  static create(db, jobData, callback) {
    const id = uuidv4();

    db.run(
      `INSERT INTO print_jobs (id, printer_id, client_id, title, content_type, file_data, status)
       VALUES (?, ?, ?, ?, ?, ?, ?)`,
      [
        id,
        jobData.printer_id,
        jobData.client_id,
        jobData.title || 'Untitled',
        jobData.content_type || 'application/pdf',
        jobData.file_data,
        'pending'
      ],
      function(err) {
        if (err) return callback(err);
        callback(null, { id, ...jobData, status: 'pending' });
      }
    );
  }

  static getById(db, id, callback) {
    db.get('SELECT * FROM print_jobs WHERE id = ?', [id], callback);
  }

  static getPending(db, clientId, callback) {
    db.all(
      `SELECT * FROM print_jobs
       WHERE client_id = ? AND status = 'pending'
       ORDER BY created_at ASC`,
      [clientId],
      callback
    );
  }

  static updateStatus(db, id, status, error, callback) {
    const printedAt = (status === 'completed' || status === 'failed') ? 'CURRENT_TIMESTAMP' : null;

    db.run(
      `UPDATE print_jobs
       SET status = ?, error = ?, printed_at = ${printedAt ? printedAt : 'printed_at'}
       WHERE id = ?`,
      [status, error || null, id],
      callback
    );
  }

  static getRecent(db, limit, callback) {
    db.all(
      `SELECT pj.*, p.name as printer_name, c.name as client_name
       FROM print_jobs pj
       JOIN printers p ON pj.printer_id = p.id
       JOIN clients c ON pj.client_id = c.id
       ORDER BY pj.created_at DESC
       LIMIT ?`,
      [limit || 50],
      callback
    );
  }
}

module.exports = PrintJob;
