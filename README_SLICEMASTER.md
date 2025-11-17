# SliceMaster per SmartSDR 4.0

## Descrizione

SliceMaster è un software avanzato di controllo per radio FlexRadio con SmartSDR 4.0. Permette di gestire in modo efficiente le **slice** (ricevitori virtuali) della radio, offrendo un'interfaccia intuitiva e potente per operatori radioamatoriali.

## Caratteristiche Principali

### 🎛️ Gestione Slice Avanzata
- **Creazione e rimozione** dinamica delle slice
- **Controllo completo** di frequenza, modalità e filtri per ogni slice
- **Visualizzazione in tempo reale** di tutte le slice attive
- **Blocco slice** per prevenire modifiche accidentali

### 📡 Controlli Radio Completi
- **Selezione modalità**: LSB, USB, AM, CW, DIGL, DIGU, SAM, FM, NFM, DFM, RTTY
- **Filtri personalizzabili**: Regolazione precisa dei filtri passa-banda
- **Guadagno RF/AF**: Controllo tramite slider in tempo reale
- **Noise Blanker (NB)**: Riduzione rumori impulsivi
- **Noise Reduction (NR)**: Riduzione rumore di fondo
- **Auto Notch Filter (ANF)**: Eliminazione automatica portanti interferenti

### 📻 Selezione Banda Rapida
- Pulsanti rapidi per tutte le bande radioamatoriali:
  - 160m, 80m, 60m, 40m, 30m, 20m, 17m, 15m, 12m, 10m, 6m, 2m
- Impostazione automatica di frequenza e modalità per banda

### 💾 Sistema di Profili
- **Salvataggio profili**: Memorizza configurazioni preferite (frequenza, modo, filtri)
- **Caricamento rapido**: Applica profili salvati con un click
- **Profili predefiniti** inclusi:
  - 20m CW
  - 40m SSB
  - 20m SSB
  - 10m FM
  - FT8 20m/40m
  - RTTY 20m
  - 2m FM
- **Import/Export**: Condividi profili con altri operatori
- **Persistenza**: I profili vengono salvati automaticamente

## Architettura del Software

### Componenti Principali

1. **FlexRadioConnection.cs**
   - Gestisce la connessione TCP alla radio FlexRadio
   - Implementa discovery automatico delle radio sulla rete
   - Gestisce invio comandi e ricezione messaggi dalla radio

2. **SliceModels.cs**
   - Definisce il modello dati per le Slice
   - Implementa INotifyPropertyChanged per binding WPF
   - Contiene definizioni bande e modalità operative

3. **SliceController.cs**
   - Controller principale per operazioni sulle slice
   - Gestisce creazione, modifica e rimozione slice
   - Sincronizza stato locale con la radio

4. **SliceMasterWindow.xaml/.cs**
   - Interfaccia utente principale
   - Gestisce interazioni utente
   - Visualizza stato slice in tempo reale

5. **ProfileManager.cs**
   - Gestisce salvataggio/caricamento profili
   - Persistenza su file JSON
   - Profili predefiniti integrati

6. **WpfConverters.cs**
   - Converter per data binding WPF
   - Conversione bool/color, bool/visibility, frequenza/string

## Utilizzo

### Connessione alla Radio

1. **Inserire l'indirizzo IP** della radio FlexRadio nel campo "Radio IP"
   - Oppure usare il pulsante **"Scopri Radio"** per rilevamento automatico
2. Cliccare su **"Connetti"**
3. Lo stato della connessione viene mostrato nella barra superiore

### Gestione Slice

#### Creare una Nuova Slice
1. Cliccare su **"Nuova Slice"**
2. Inserire la frequenza in MHz (es: 14.200)
3. Selezionare la modalità operativa
4. Cliccare su **"OK"**

#### Modificare una Slice
- **Frequenza**: Modificare il valore nel campo e premere Invio
- **Modalità**: Selezionare dal menu a tendina
- **Filtri**: Modificare i valori Low/High e premere Invio
- **RF Gain**: Usare lo slider
- **AF Gain**: Usare lo slider
- **NB/NR/ANF**: Attivare/disattivare con le checkbox

#### Bloccare una Slice
- Cliccare sul pulsante 🔒
- **Verde** = sbloccata (modificabile)
- **Rosso** = bloccata (non modificabile)

#### Impostare Slice TX
- Cliccare su **"Set TX"** per impostare la slice come trasmettitrice
- Solo una slice alla volta può essere TX

### Uso dei Profili

#### Salvare un Profilo
1. Configurare una slice come desiderato
2. Cliccare su **"Salva Profilo"**
3. Inserire nome e descrizione
4. Cliccare su **"OK"**

#### Caricare un Profilo
1. Selezionare un profilo dalla lista
2. Cliccare su **"Carica Profilo"**
3. Le impostazioni vengono applicate alla prima slice attiva

#### Eliminare un Profilo
1. Selezionare un profilo dalla lista
2. Cliccare su **"Elimina Profilo"**
3. Confermare l'eliminazione

### Selezione Banda Rapida

Cliccare su uno dei pulsanti banda (es: "20m") per:
- Impostare la frequenza centrale della banda
- Impostare la modalità appropriata per quella banda
- Se non ci sono slice attive, ne viene creata una automaticamente

## API SmartSDR 4.0

SliceMaster comunica con la radio tramite protocollo TCP sulla porta **4992** (porta predefinita SmartSDR).

### Comandi Principali

```
# Connessione
client program SliceMaster

# Gestione Slice
slice create <frequency> <mode>
slice remove <slice_id>
slice set <slice_id> freq=<frequency>
slice set <slice_id> mode=<mode>
slice set <slice_id> filter_lo=<value> filter_hi=<value>
slice set <slice_id> rfgain=<value>
slice set <slice_id> afgain=<value>
slice set <slice_id> nb=<0|1>
slice set <slice_id> nr=<0|1>
slice set <slice_id> anf=<0|1>

# Trasmissione
xmit slice <slice_id>

# Refresh
slice list
```

### Formato Messaggi Ricevuti

```
S<slice_id>|<param>=<value>|<param>=<value>|...

Esempi:
S0|freq=14.200000|mode=USB|filter_lo=100|filter_hi=2400
S1|rfgain=50|afgain=75|nb=1
```

## Requisiti di Sistema

- **OS**: Windows 7 o superiore
- **.NET**: .NET 7.0 o superiore
- **Radio**: FlexRadio con SmartSDR 4.0
- **Rete**: Connessione di rete alla radio (Ethernet o WiFi)

## Dipendenze

Il progetto utilizza le seguenti librerie (già incluse):
- **WPF** (Windows Presentation Foundation)
- **System.Text.Json** (per gestione profili)
- **System.Net.Sockets** (per comunicazione TCP)

## Configurazione Avanzata

### Porta Personalizzata

Per modificare la porta di connessione (default: 4992), modificare la costante in `FlexRadioConnection.cs`:

```csharp
private int _radioPort = 4992; // Cambia qui
```

### Profili Custom

I profili vengono salvati in:
```
%APPDATA%\SliceMaster\slicemaster_profiles.json
```

È possibile modificare manualmente questo file JSON per aggiungere profili personalizzati.

### Modalità Operative Supportate

| Modalità | Descrizione | Filtro Default Low | Filtro Default High |
|----------|-------------|-------------------|---------------------|
| LSB | Lower Side Band | -2400 | -100 |
| USB | Upper Side Band | 100 | 2400 |
| AM | Amplitude Modulation | -4000 | 4000 |
| CW | Continuous Wave | -250 | 250 |
| DIGL | Digital Lower | -2400 | -100 |
| DIGU | Digital Upper | 100 | 2400 |
| SAM | Synchronous AM | -4000 | 4000 |
| FM | Frequency Modulation | -8000 | 8000 |
| NFM | Narrow FM | -4000 | 4000 |
| DFM | Digital FM | -4000 | 4000 |
| RTTY | Radioteletype | 100 | 2400 |

## Troubleshooting

### Non riesce a connettersi alla radio

1. Verificare che la radio sia accesa e connessa alla rete
2. Controllare l'indirizzo IP della radio nelle impostazioni SmartSDR
3. Verificare che non ci siano firewall che bloccano la porta 4992
4. Provare il pulsante "Scopri Radio" per rilevamento automatico

### Le slice non rispondono ai comandi

1. Verificare che la connessione sia attiva (indicatore verde)
2. Controllare la barra di stato per messaggi di errore
3. Cliccare su "Aggiorna" per sincronizzare lo stato
4. Provare a disconnettere e riconnettere

### I profili non vengono salvati

1. Verificare i permessi di scrittura su `%APPDATA%\SliceMaster`
2. Controllare che ci sia spazio disco disponibile
3. Verificare che il nome del profilo non contenga caratteri non validi

## Sviluppi Futuri

Possibili miglioramenti:
- 🎨 Tema personalizzabile (scuro/chiaro)
- 📊 Visualizzazione spettro waterfall integrata
- 🔊 Controllo audio avanzato con equalizzatore
- 📝 Logging QSO integrato
- 🌐 Controllo remoto via web
- 📡 Supporto per CAT control
- 🎯 Presets per contest
- 🔄 Sincronizzazione profili cloud

## Licenza

Questo software è stato creato per uso radioamatoriale.

## Supporto

Per problemi, suggerimenti o contributi:
- Aprire una issue su GitHub
- Contattare lo sviluppatore

## Crediti

Sviluppato con ❤️ per la comunità radioamatoriale.

---

**73 de SliceMaster Team!**
