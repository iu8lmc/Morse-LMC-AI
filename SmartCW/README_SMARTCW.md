# SmartCW — Motore innovativo di decodifica CW

SmartCW è il nuovo motore di decodifica del codice Morse di RadioLoggerApp.
La sua innovazione centrale: **non decide mai subito**. Invece di classificare
ogni tono con soglie rigide (dit/dah, gap lettera/parola), mantiene un fascio
di ipotesi in parallelo e pubblica solo il testo su cui tutte concordano.

## Architettura

```
audio (float, 44.1 kHz)
   │
   ▼
GoertzelToneDetector      potenza del tono in blocchi da 4 ms (algoritmo di
   │                      Goertzel, O(n) per bin — niente FFT completa) +
   │                      AFC: riaggancio automatico della frequenza 300–1200 Hz
   ▼
AdaptiveSignalGate        eventi mark/space con doppia soglia a isteresi;
   │                      il floor di rumore si stima SOLO sui blocchi di
   │                      spazio, il picco ha attacco rapido e rilascio lento
   │                      (sopravvive al QSB); squelch + filtro mediano
   ▼
BayesianTimingDecoder     beam search: ogni durata ramifica le ipotesi
   │                      (dit|dah, gap elemento|lettera|parola) pesate con
   │                      verosimiglianza log-gaussiana; OGNI ipotesi insegue
   │                      la propria stima del dit → il WPM è stimato
   │                      congiuntamente alla decodifica, non prima
   ▼
CwLanguageModel           bigrammi da corpus QSO incorporato + lessico ham
   │                      (Q-codes, RST, 73…) + riconoscimento del pattern
   │                      degli indicativi (prefisso-cifra-suffisso)
   ▼
testo confermato          emesso quando l'intero beam concorda sul prefisso
```

Facciata d'uso: `SmartCwDecoder.ProcessAudio(float[])` → eventi
`CharacterDecoded` / `WordDecoded` / `TextDecoded`, proprietà `CurrentWpm`,
`Frequency`, `SnrDb`, `Confidence` (distacco tra le prime due ipotesi del beam).

## Perché è diverso dai decoder classici

| Decoder classico (soglie fisse)        | SmartCW                                     |
|----------------------------------------|---------------------------------------------|
| soglia audio fissa                     | soglie adattive rumore/picco con isteresi   |
| FFT completa per trovare il tono       | Goertzel mirato + AFC                       |
| WPM stimato prima, poi si decodifica   | WPM stimato *insieme* alla decodifica       |
| una durata ambigua = un errore secco   | l'ambiguità resta nel beam finché il        |
|                                        | contesto (timing + lingua) non la risolve   |
| nessuna conoscenza del traffico ham    | bigrammi QSO, lessico, pattern indicativi   |

## Risultati misurati (CW sintetico, messaggio QSO di 69 caratteri)

Baseline = decoder a soglia fissa esistente, avvantaggiato con frequenza e
WPM esatti. CER = character error rate (Levenshtein).

| Scenario                        | SmartCW | Baseline |
|---------------------------------|--------:|---------:|
| Pulito, 20 wpm, 700 Hz          |   0,0 % |    0,0 % |
| Pulito, 30 wpm, 600 Hz (AFC)    |   0,0 % |    0,0 % |
| SNR 12 dB                       |   0,0 % |    0,0 % |
| SNR 6 dB                        |   0,0 % |    0,0 % |
| Jitter di manipolazione 20 %    |   0,0 % |    0,0 % |
| SNR 9 dB + jitter 15 %          |   0,0 % |    0,0 % |
| QSB + SNR 12 dB                 |   0,0 % |   48,5 % |
| SNR 8 dB + jitter 15 % + QSB    |   5,9 % |   44,1 % |
| **Media**                       | **0,7 %** | **11,6 %** |

Il vantaggio emerge dove serve: fading QSB e keying umano imperfetto, cioè le
condizioni reali delle bande HF.

## Note implementative

- Nessuna dipendenza da WPF/NAudio: i cinque file di questa cartella compilano
  anche su .NET 8 standard e sono testabili su qualunque piattaforma.
- Le verosimiglianze delle durate sono gaussiane nel dominio log: l'errore di
  keying umano è moltiplicativo, non additivo.
- Un simbolo Morse sconosciuto decodifica in `?` con forte penalità: sotto
  rumore estremo il decoder degrada con grazia invece di bloccarsi.
- Dopo un silenzio prolungato il beam fa flush: emette la migliore ipotesi e
  riparte conservando il timing appreso.
