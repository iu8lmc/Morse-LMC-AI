# 🧪 Checklist Test - Finestra Impostazioni Avanzate

## ✅ Test di Base

### Apertura e Chiusura
- [ ] La finestra si apre cliccando "⚙ Impostazioni"
- [ ] Il pulsante "⚙ Impostazioni" è visibile e cliccabile
- [ ] La finestra ha il dark theme corretto
- [ ] La finestra è centrata rispetto alla finestra principale
- [ ] Il titolo "⚙ Impostazioni Avanzate - Morse Decoder AI" è visibile

### Controlli UI Generali
- [ ] Scroll bar funziona correttamente
- [ ] Tutti i GroupBox sono visibili
- [ ] Tutti i label sono leggibili (bianco su scuro)
- [ ] Gli slider sono draggabili
- [ ] I valori nei TextBox si aggiornano quando si muovono gli slider

---

## 📻 Sezione Parametri Morse

### Auto-calibrazione WPM
- [ ] Checkbox "Auto-calibrazione WPM" è visibile
- [ ] Default: Checked ✓
- [ ] Quando checked: slider WPM è disabilitato (grigio)
- [ ] Quando unchecked: slider WPM è abilitato

### Slider WPM
- [ ] Range: 5-60
- [ ] Default: 20
- [ ] Il valore nel TextBox si aggiorna in tempo reale
- [ ] Snap to tick funziona (step di 5)

### Auto Frequenza
- [ ] Checkbox "Rilevamento automatico frequenza" è visibile
- [ ] Default: Checked ✓
- [ ] Quando checked: slider Frequenza è disabilitato
- [ ] Quando unchecked: slider Frequenza è abilitato

### Slider Frequenza
- [ ] Range: 200-2000 Hz
- [ ] Default: 800 Hz
- [ ] Il valore si aggiorna in tempo reale
- [ ] Snap to tick funziona (step di 100)

### Slider Soglia
- [ ] Range: 0.1-0.9
- [ ] Default: 0.30
- [ ] Il valore mostra 2 decimali (es. 0.35)
- [ ] Slider è smooth (non snap)

---

## 🔊 Sezione DSP

### Slider Bandwidth
- [ ] Range: 50-500 Hz
- [ ] Default: 100 Hz
- [ ] Snap to tick funziona (step di 50)

### Checkbox AGC
- [ ] Visibile e cliccabile
- [ ] Default: Checked ✓

### Checkbox Noise Gate
- [ ] Visibile e cliccabile
- [ ] Default: Checked ✓

### ComboBox FFT Size
- [ ] 4 opzioni: 1024, 2048, 4096, 8192
- [ ] Default: 4096 selezionato
- [ ] Dropdown funziona
- [ ] Cambio selezione funziona

---

## 🧠 Sezione AI/ML

### Checkbox Abilita AI
- [ ] Visibile e cliccabile
- [ ] Default: Checked ✓
- [ ] Quando checked: tutti i controlli AI sono abilitati
- [ ] Quando unchecked: tutti i controlli AI sono disabilitati (grigi)

### Slider Re-training
- [ ] Range: 10-200
- [ ] Default: 50
- [ ] Disabilitato quando AI è disabilitata

### Slider Min Dataset
- [ ] Range: 50-500
- [ ] Default: 100
- [ ] Disabilitato quando AI è disabilitata

### Checkbox Correzione Errori
- [ ] Visibile e cliccabile
- [ ] Default: Checked ✓
- [ ] Disabilitata quando AI è disabilitata

### Pulsanti Gestione Modello
- [ ] "💾 Salva Modello" visibile e cliccabile
- [ ] "📂 Carica Modello" visibile e cliccabile
- [ ] "🔄 Reset Modello" visibile con colore rosso
- [ ] Tutti disabilitati quando AI è disabilitata

---

## 🎤 Sezione Audio

### Slider Buffer Size
- [ ] Range: 10-200 ms
- [ ] Default: 50 ms
- [ ] Snap to tick funziona (step di 10)

### ComboBox Sample Rate
- [ ] 3 opzioni: 22050, 44100, 48000
- [ ] Default: 44100 selezionato
- [ ] Dropdown funziona

### Info Latenza
- [ ] Testo "💡 Latenza stimata: ~70ms" è visibile
- [ ] Colore grigio (#888888) applicato

---

## 🎯 Pulsanti di Azione

### Pulsante Salva
- [ ] "✅ Salva" visibile
- [ ] Colore blu (#007ACC)
- [ ] Click salva le impostazioni
- [ ] Mostra messaggio "Impostazioni salvate con successo!"
- [ ] Finestra si chiude dopo il salvataggio

### Pulsante Annulla
- [ ] "❌ Annulla" visibile
- [ ] Colore grigio (#505050)
- [ ] Click chiude la finestra senza salvare

### Pulsante Ripristina Default
- [ ] "🔄 Ripristina Default" visibile
- [ ] Colore grigio (#505050)
- [ ] Click mostra dialog di conferma
- [ ] Se "Yes": tutti i valori tornano ai default
- [ ] Se "No": nessuna modifica

---

## 💾 Test Persistenza

### Salvataggio Impostazioni
1. Modifica alcuni parametri (es. WPM = 25, Frequenza = 1000)
2. Click "✅ Salva"
3. Chiudi la finestra impostazioni
4. Riapri la finestra impostazioni
   - [ ] I valori modificati sono mantenuti
   - [ ] WPM = 25
   - [ ] Frequenza = 1000

### File JSON
- [ ] Verifica che esista il file:
  - Windows: `%APPDATA%\RadioLoggerApp\MorseDecoder\settings.json`
  - Linux: `~/.config/RadioLoggerApp/MorseDecoder/settings.json`
- [ ] Apri il file e verifica che contenga JSON valido
- [ ] Verifica che i valori corrispondano alle impostazioni salvate

### Backup
- [ ] Dopo il primo salvataggio, verifica la presenza di file backup
- [ ] Nome file backup: `settings_backup_YYYYMMDD_HHMMSS.json`

---

## 🔧 Test Integrazione con Decoder

### Applicazione Impostazioni
1. Finestra Decoder chiusa (non decodificando)
2. Modifica impostazioni (es. WPM = 30)
3. Salva
4. Verifica nel log: "Componenti reinizializzati con le nuove impostazioni"
   - [ ] Messaggio appare nel log

### Durante Decodifica
1. Avvia la decodifica audio
2. Click su "⚙ Impostazioni"
3. Mostra dialog: "La decodifica è in corso..."
   - [ ] Dialog appare correttamente
4. Click "Yes" per continuare
5. Modifica parametri e salva
6. Verifica messaggio: "Impostazioni aggiornate. Riavviare la decodifica..."
   - [ ] Messaggio appare nel log
7. Ferma e riavvia la decodifica
   - [ ] Nuove impostazioni applicate

---

## 🧠 Test Gestione Modello AI

### Salva Modello
1. Click "💾 Salva Modello"
2. Dialog "Save File" appare
   - [ ] Nome default: `morse_model_YYYYMMDD_HHMMSS.mdl`
   - [ ] Filtro: "ML Model (*.mdl)"
3. Seleziona percorso e salva
4. Verifica messaggio: "Percorso modello impostato"
   - [ ] Messaggio appare

### Carica Modello
1. Click "📂 Carica Modello"
2. Dialog "Open File" appare
   - [ ] Filtro: "ML Model (*.mdl)"
3. Seleziona un file .mdl
4. Verifica messaggio: "Percorso modello impostato"
   - [ ] Messaggio appare

### Reset Modello
1. Click "🔄 Reset Modello"
2. Dialog conferma appare: "Resettare il modello AI?"
   - [ ] Dialog ha icona Warning
3. Click "Yes"
4. Verifica messaggio: "Il modello verrà resettato al prossimo avvio"
   - [ ] Messaggio appare
5. Click "No"
   - [ ] Nessun cambiamento

---

## 🎨 Test Visuale

### Dark Theme
- [ ] Background finestra: #1E1E1E (grigio scuro)
- [ ] Testo label: #D4D4D4 (grigio chiaro)
- [ ] GroupBox border: #3E3E42 (grigio medio)
- [ ] TextBox background: #2D2D30 (grigio scuro)
- [ ] Button background: #007ACC (blu)
- [ ] Button hover: #005A9E (blu scuro)
- [ ] Reset button: #D14C4C (rosso)

### Layout
- [ ] Tutti i controlli sono allineati
- [ ] Spacing consistente (margins 5-10px)
- [ ] Nessun overlap di controlli
- [ ] ScrollViewer funziona se finestra è ridimensionata

---

## ⚠️ Test Validazione

### Valori Fuori Range
1. Modifica manualmente il JSON con valori invalidi:
   - WPM = 100 (> 60)
   - Frequenza = 5000 (> 2000)
2. Riavvia l'applicazione
3. Verifica che carichi i valori default
   - [ ] Log: "Impostazioni non valide, uso valori predefiniti"

### File JSON Corrotto
1. Corrompi il file settings.json (rimuovi parentesi)
2. Riavvia l'applicazione
3. Verifica che carichi i valori default
   - [ ] Log: "Errore nel caricamento, uso valori predefiniti"

---

## 🐛 Test Edge Cases

### Apertura Multipla
- [ ] Non è possibile aprire 2 finestre impostazioni contemporaneamente
- [ ] La finestra è modale (blocca la finestra principale)

### Chiusura con X
- [ ] Click sulla X della finestra = equivalente a "Annulla"
- [ ] Impostazioni non salvate vengono perse

### Modifica Rapida Slider
- [ ] Muovi velocemente gli slider avanti e indietro
- [ ] Nessun crash o freeze
- [ ] I valori si aggiornano correttamente

---

## ✅ Criteri di Successo

Per considerare il test **SUPERATO**, tutti questi devono essere ✓:

1. ✅ La finestra si apre e chiude correttamente
2. ✅ Tutti i controlli funzionano come previsto
3. ✅ Le impostazioni vengono salvate in JSON
4. ✅ Le impostazioni salvate vengono ricaricate correttamente
5. ✅ L'integrazione con MorseDecoderWindow funziona
6. ✅ Nessun crash o errore durante l'uso normale
7. ✅ Il dark theme è applicato correttamente
8. ✅ I pulsanti di gestione modello funzionano
9. ✅ La validazione degli input funziona
10. ✅ I valori default sono corretti

---

## 📊 Report Bug

Se trovi bug, annota:

**Bug #**: ___
**Descrizione**: _________
**Passi per riprodurre**:
1. ___
2. ___
3. ___

**Comportamento atteso**: ___
**Comportamento attuale**: ___
**Severity**: [ ] Critical [ ] High [ ] Medium [ ] Low
**Screenshot**: (se applicabile)

---

## 🎯 Test di Performance

### Tempo di Apertura
- [ ] La finestra si apre in < 500ms

### Responsività
- [ ] Gli slider rispondono immediatamente (< 50ms)
- [ ] Il salvataggio completa in < 1s

### Memory Leak
1. Apri e chiudi la finestra 10 volte
2. Monitora l'uso della memoria
   - [ ] Nessun aumento significativo di memoria

---

**Test completato da**: _________
**Data**: _________
**Versione**: v1.1
**Esito**: [ ] ✅ PASS [ ] ❌ FAIL

**Note aggiuntive**:
_________________________
_________________________
