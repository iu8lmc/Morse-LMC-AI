const API_BASE = window.location.origin;

// Gestione Tab
document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        const tabName = btn.dataset.tab;

        // Rimuovi active da tutti
        document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
        document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));

        // Aggiungi active al selezionato
        btn.classList.add('active');
        document.getElementById(tabName).classList.add('active');

        // Carica dati per la tab
        loadTabData(tabName);
    });
});

// Carica dati iniziali
loadDashboard();
setInterval(loadDashboard, 5000); // Aggiorna ogni 5 secondi

async function loadDashboard() {
    try {
        const health = await fetch(`${API_BASE}/health`).then(r => r.json());
        document.getElementById('connection-status').textContent = 'Connesso';
        document.getElementById('connection-status').className = 'status-badge online';
        document.getElementById('server-time').textContent = new Date(health.timestamp).toLocaleString('it-IT');
    } catch (error) {
        document.getElementById('connection-status').textContent = 'Disconnesso';
        document.getElementById('connection-status').className = 'status-badge offline';
    }

    loadStats();
    loadRecentJobs();
}

async function loadStats() {
    try {
        const [clients, printers, jobs] = await Promise.all([
            fetch(`${API_BASE}/api/clients`).then(r => r.json()),
            fetch(`${API_BASE}/api/printers`).then(r => r.json()),
            fetch(`${API_BASE}/api/printjobs/recent?limit=100`).then(r => r.json())
        ]);

        const onlineClients = clients.filter(c => c.is_online).length;
        const pendingJobs = jobs.filter(j => j.status === 'pending').length;
        const completedJobs = jobs.filter(j => j.status === 'completed').length;

        document.getElementById('stats-clients').textContent = onlineClients;
        document.getElementById('stats-printers').textContent = printers.length;
        document.getElementById('stats-jobs-pending').textContent = pendingJobs;
        document.getElementById('stats-jobs-completed').textContent = completedJobs;

    } catch (error) {
        console.error('Errore caricamento statistiche:', error);
    }
}

async function loadRecentJobs() {
    try {
        const jobs = await fetch(`${API_BASE}/api/printjobs/recent?limit=10`).then(r => r.json());
        const container = document.getElementById('recent-jobs-list');

        if (jobs.length === 0) {
            container.innerHTML = '<p class="empty-state">Nessun lavoro recente</p>';
            return;
        }

        container.innerHTML = jobs.map(job => `
            <div class="job-card">
                <h3>${job.title}</h3>
                <div class="info">
                    <div class="info-item">
                        <strong>Stampante:</strong> ${job.printer_name || 'N/A'}
                    </div>
                    <div class="info-item">
                        <strong>Client:</strong> ${job.client_name || 'N/A'}
                    </div>
                    <div class="info-item">
                        <strong>Stato:</strong> <span class="badge ${job.status}">${translateStatus(job.status)}</span>
                    </div>
                    <div class="info-item">
                        <strong>Data:</strong> ${new Date(job.created_at).toLocaleString('it-IT')}
                    </div>
                </div>
                ${job.error ? `<p style="color: #dc2626; margin-top: 10px;">Errore: ${job.error}</p>` : ''}
            </div>
        `).join('');

    } catch (error) {
        console.error('Errore caricamento lavori recenti:', error);
    }
}

async function loadTabData(tabName) {
    switch(tabName) {
        case 'clients':
            await loadClients();
            break;
        case 'printers':
            await loadPrinters();
            break;
        case 'jobs':
            await loadAllJobs();
            break;
        case 'print':
            await loadPrintersForPrint();
            break;
    }
}

async function loadClients() {
    try {
        const clients = await fetch(`${API_BASE}/api/clients`).then(r => r.json());
        const container = document.getElementById('clients-list');

        if (clients.length === 0) {
            container.innerHTML = '<p class="empty-state">Nessun client registrato</p>';
            return;
        }

        container.innerHTML = clients.map(client => `
            <div class="client-card">
                <h3>${client.name}</h3>
                <div class="info">
                    <div class="info-item">
                        <strong>ID:</strong> ${client.id}
                    </div>
                    <div class="info-item">
                        <strong>Stato:</strong> <span class="badge ${client.is_online ? 'online' : 'offline'}">${client.is_online ? 'Online' : 'Offline'}</span>
                    </div>
                    <div class="info-item">
                        <strong>Registrato:</strong> ${new Date(client.created_at).toLocaleString('it-IT')}
                    </div>
                    <div class="info-item">
                        <strong>Ultima connessione:</strong> ${client.last_seen ? new Date(client.last_seen).toLocaleString('it-IT') : 'Mai'}
                    </div>
                </div>
            </div>
        `).join('');

    } catch (error) {
        console.error('Errore caricamento client:', error);
    }
}

async function loadPrinters() {
    try {
        const printers = await fetch(`${API_BASE}/api/printers`).then(r => r.json());
        const container = document.getElementById('printers-list');

        if (printers.length === 0) {
            container.innerHTML = '<p class="empty-state">Nessuna stampante disponibile</p>';
            return;
        }

        container.innerHTML = printers.map(printer => `
            <div class="printer-card">
                <h3>🖨️ ${printer.name} ${printer.is_default ? '(Predefinita)' : ''}</h3>
                <div class="info">
                    <div class="info-item">
                        <strong>Client:</strong> ${printer.client_name || 'N/A'}
                        <span class="badge ${printer.client_online ? 'online' : 'offline'}">
                            ${printer.client_online ? 'Online' : 'Offline'}
                        </span>
                    </div>
                    <div class="info-item">
                        <strong>Driver:</strong> ${printer.driver || 'N/A'}
                    </div>
                    <div class="info-item">
                        <strong>Stato:</strong> <span class="badge">${printer.status || 'idle'}</span>
                    </div>
                    <div class="info-item">
                        <strong>ID:</strong> ${printer.id}
                    </div>
                </div>
            </div>
        `).join('');

    } catch (error) {
        console.error('Errore caricamento stampanti:', error);
    }
}

async function loadAllJobs() {
    try {
        const jobs = await fetch(`${API_BASE}/api/printjobs/recent?limit=50`).then(r => r.json());
        const container = document.getElementById('jobs-list');

        if (jobs.length === 0) {
            container.innerHTML = '<p class="empty-state">Nessun lavoro disponibile</p>';
            return;
        }

        container.innerHTML = jobs.map(job => `
            <div class="job-card">
                <h3>📄 ${job.title}</h3>
                <div class="info">
                    <div class="info-item">
                        <strong>ID:</strong> ${job.id}
                    </div>
                    <div class="info-item">
                        <strong>Stampante:</strong> ${job.printer_name || 'N/A'}
                    </div>
                    <div class="info-item">
                        <strong>Client:</strong> ${job.client_name || 'N/A'}
                    </div>
                    <div class="info-item">
                        <strong>Stato:</strong> <span class="badge ${job.status}">${translateStatus(job.status)}</span>
                    </div>
                    <div class="info-item">
                        <strong>Creato:</strong> ${new Date(job.created_at).toLocaleString('it-IT')}
                    </div>
                    ${job.printed_at ? `
                    <div class="info-item">
                        <strong>Completato:</strong> ${new Date(job.printed_at).toLocaleString('it-IT')}
                    </div>
                    ` : ''}
                </div>
                ${job.error ? `<p style="color: #dc2626; margin-top: 10px;">❌ Errore: ${job.error}</p>` : ''}
            </div>
        `).join('');

    } catch (error) {
        console.error('Errore caricamento lavori:', error);
    }
}

async function loadPrintersForPrint() {
    try {
        const printers = await fetch(`${API_BASE}/api/printers`).then(r => r.json());
        const select = document.getElementById('printer-select');

        select.innerHTML = '<option value="">Seleziona una stampante...</option>';

        printers.forEach(printer => {
            if (printer.client_online) {
                select.innerHTML += `<option value="${printer.id}">${printer.name} - ${printer.client_name}</option>`;
            }
        });

    } catch (error) {
        console.error('Errore caricamento stampanti:', error);
    }
}

// Form: Stampa
document.getElementById('print-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    const printerId = document.getElementById('printer-select').value;
    const title = document.getElementById('job-title').value;
    const file = document.getElementById('file-upload').files[0];

    if (!printerId || !file) {
        showAlert('print-result', 'error', 'Seleziona una stampante e un file');
        return;
    }

    const formData = new FormData();
    formData.append('printer_id', printerId);
    formData.append('title', title || file.name);
    formData.append('file', file);

    try {
        const response = await fetch(`${API_BASE}/api/printjobs`, {
            method: 'POST',
            body: formData
        });

        const result = await response.json();

        if (result.success) {
            showAlert('print-result', 'success', `✓ Lavoro di stampa inviato! ID: ${result.job_id}`);
            document.getElementById('print-form').reset();
        } else {
            showAlert('print-result', 'error', result.error || 'Errore invio lavoro');
        }

    } catch (error) {
        showAlert('print-result', 'error', 'Errore di connessione al server');
    }
});

// Modal: Registra Client
function showRegisterClientModal() {
    document.getElementById('register-modal').classList.add('show');
    document.getElementById('register-result').style.display = 'none';
    document.getElementById('register-form').style.display = 'block';
    document.getElementById('register-form').reset();
}

function closeRegisterClientModal() {
    document.getElementById('register-modal').classList.remove('show');
}

document.getElementById('register-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    const name = document.getElementById('client-name').value;

    try {
        const response = await fetch(`${API_BASE}/api/clients/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ name })
        });

        const result = await response.json();

        if (result.api_key) {
            document.getElementById('new-api-key').textContent = result.api_key;
            document.getElementById('register-form').style.display = 'none';
            document.getElementById('register-result').style.display = 'block';
            loadClients();
        } else {
            alert('Errore registrazione client');
        }

    } catch (error) {
        alert('Errore di connessione al server');
    }
});

// Utility functions
function translateStatus(status) {
    const translations = {
        'pending': 'In Attesa',
        'printing': 'In Stampa',
        'completed': 'Completato',
        'failed': 'Fallito'
    };
    return translations[status] || status;
}

function showAlert(elementId, type, message) {
    const el = document.getElementById(elementId);
    el.className = `alert alert-${type}`;
    el.textContent = message;
    el.style.display = 'block';

    setTimeout(() => {
        el.style.display = 'none';
    }, 5000);
}

// Chiudi modal cliccando fuori
window.onclick = function(event) {
    const modal = document.getElementById('register-modal');
    if (event.target == modal) {
        closeRegisterClientModal();
    }
}
