using System;
using System.Collections.Generic;
using System.Linq;

namespace RadioLoggerApp.MorseDecoder.SmartCW
{
    /// <summary>
    /// Il cuore innovativo del decoder: invece di classificare ogni durata con una
    /// soglia rigida (dit/dah, gap lettera/parola), mantiene un fascio (beam) di
    /// ipotesi in parallelo. Ogni evento di keying fa ramificare le ipotesi su tutte
    /// le interpretazioni plausibili, pesate con:
    ///  - verosimiglianza log-gaussiana della durata rispetto al modello di timing
    ///    dell'ipotesi (ogni ipotesi insegue la PROPRIA stima del dit → la velocità
    ///    WPM è stimata congiuntamente alla decodifica, non prima);
    ///  - modello linguistico ham (bigrammi + lessico + pattern indicativi).
    /// Il testo viene emesso solo quando tutte le ipotesi del beam concordano sul
    /// prefisso: ciò che esce è quindi il consenso, non la prima scelta avida.
    /// </summary>
    public class BayesianTimingDecoder
    {
        /// <summary>Tabella Morse internazionale (simboli → testo, prosigns inclusi).</summary>
        private static readonly Dictionary<string, string> MorseTable = new()
        {
            { ".-", "A" }, { "-...", "B" }, { "-.-.", "C" }, { "-..", "D" },
            { ".", "E" }, { "..-.", "F" }, { "--.", "G" }, { "....", "H" },
            { "..", "I" }, { ".---", "J" }, { "-.-", "K" }, { ".-..", "L" },
            { "--", "M" }, { "-.", "N" }, { "---", "O" }, { ".--.", "P" },
            { "--.-", "Q" }, { ".-.", "R" }, { "...", "S" }, { "-", "T" },
            { "..-", "U" }, { "...-", "V" }, { ".--", "W" }, { "-..-", "X" },
            { "-.--", "Y" }, { "--..", "Z" },
            { "-----", "0" }, { ".----", "1" }, { "..---", "2" }, { "...--", "3" },
            { "....-", "4" }, { ".....", "5" }, { "-....", "6" }, { "--...", "7" },
            { "---..", "8" }, { "----.", "9" },
            { ".-.-.-", "." }, { "--..--", "," }, { "..--..", "?" }, { ".----.", "'" },
            { "-.-.--", "!" }, { "-..-.", "/" }, { "-.--.", "(" }, { "-.--.-", ")" },
            { ".-...", "&" }, { "---...", ":" }, { "-.-.-.", ";" }, { "-...-", "=" },
            { ".-.-.", "+" }, { "-....-", "-" }, { "..--.-", "_" }, { ".-..-.", "\"" },
            { ".--.-.", "@" }, { "...-.-", "<SK>" }, { "........", "<ERR>" }
        };

        private readonly CwLanguageModel languageModel;

        /// <summary>Ampiezza del beam: più ipotesi = più robustezza, più CPU.</summary>
        public int BeamWidth { get; set; } = 24;

        // Deviazioni standard del modello log-gaussiano delle durate.
        // Valori tarati sul test sintetico: tolleranza al keying umano imperfetto.
        private const double MarkSigma = 0.30;
        private const double GapSigma = 0.40;

        // Peso del modello linguistico rispetto al modello acustico
        private const double LmWeight = 0.7;

        private List<Hypothesis> beam;

        // Testo già confermato (consenso di tutto il beam) e già emesso
        private string committedText = string.Empty;

        /// <summary>Testo confermato dal consenso del beam.</summary>
        public string CommittedText => committedText;

        /// <summary>Coda provvisoria della migliore ipotesi (può ancora cambiare).</summary>
        public string TentativeTail => beam.Count > 0 ? beam[0].Text : string.Empty;

        /// <summary>Stima corrente della velocità (dalla migliore ipotesi).</summary>
        public double EstimatedWpm => beam.Count > 0 && beam[0].DitMs > 0
            ? 1200.0 / beam[0].DitMs : 0;

        /// <summary>
        /// Confidenza: separazione normalizzata tra la prima e la seconda ipotesi.
        /// </summary>
        public double Confidence { get; private set; }

        /// <summary>Nuovo testo confermato (delta rispetto all'emissione precedente).</summary>
        public event EventHandler<string>? TextCommitted;

        /// <summary>Parola completata nel testo confermato.</summary>
        public event EventHandler<string>? WordCommitted;

        public BayesianTimingDecoder(CwLanguageModel? languageModel = null)
        {
            this.languageModel = languageModel ?? new CwLanguageModel();
            this.beam = new List<Hypothesis> { Hypothesis.Initial() };
        }

        /// <summary>Elabora un evento di keying facendo evolvere il beam.</summary>
        public void ProcessEvent(KeyingEvent evt)
        {
            if (evt.IsFlush)
            {
                Flush();
                return;
            }

            var candidates = new List<Hypothesis>();

            foreach (var hyp in beam)
            {
                if (evt.IsMark)
                {
                    BranchOnMark(hyp, evt.DurationMs, candidates);
                }
                else
                {
                    BranchOnGap(hyp, evt.DurationMs, candidates);
                }
            }

            if (candidates.Count == 0) return;

            PruneBeam(candidates);
            CommitConsensus();
            UpdateConfidence();
        }

        /// <summary>
        /// Ramifica un'ipotesi su un mark: può essere un dit (1 unità)
        /// o un dah (3 unità). Entrambe le interpretazioni proseguono nel beam,
        /// ciascuna aggiornando la propria stima del dit.
        /// </summary>
        private void BranchOnMark(Hypothesis hyp, double durationMs, List<Hypothesis> output)
        {
            // Simboli oltre gli 8 elementi non esistono nel codice: sotto rumore
            // forte si tronca con penalità invece di far crescere il simbolo all'infinito
            if (hyp.Symbol.Length >= 8)
            {
                var trunc = hyp.Clone();
                trunc.Score += -3.0;
                output.Add(trunc);
                return;
            }

            // Interpretazione dit
            var asDit = hyp.Clone();
            asDit.Symbol += ".";
            asDit.Score += LogGaussianDuration(durationMs, hyp.DitMs, MarkSigma);
            asDit.UpdateDit(durationMs);
            output.Add(asDit);

            // Interpretazione dah
            var asDah = hyp.Clone();
            asDah.Symbol += "-";
            asDah.Score += LogGaussianDuration(durationMs, hyp.DitMs * 3, MarkSigma);
            asDah.UpdateDit(durationMs / 3.0);
            output.Add(asDah);
        }

        /// <summary>
        /// Ramifica un'ipotesi su uno space: gap tra elementi (1 unità),
        /// tra lettere (3 unità) o tra parole (7+ unità). I gap di lettera e parola
        /// chiudono il simbolo corrente e interrogano il modello linguistico.
        /// </summary>
        private void BranchOnGap(Hypothesis hyp, double durationMs, List<Hypothesis> output)
        {
            // 1. Gap tra elementi: il simbolo continua
            if (hyp.Symbol.Length > 0)
            {
                var intra = hyp.Clone();
                intra.Score += LogGaussianDuration(durationMs, hyp.DitMs, GapSigma);
                output.Add(intra);
            }

            // 2. Gap tra lettere: chiudi il simbolo
            var letter = hyp.Clone();
            if (letter.CloseSymbol(languageModel, LmWeight))
            {
                letter.Score += LogGaussianDuration(durationMs, hyp.DitMs * 3, GapSigma);
                output.Add(letter);
            }

            // 3. Gap tra parole: chiudi simbolo + spazio.
            //    Oltre 7 unità la verosimiglianza resta piatta: una pausa può
            //    essere lunga quanto vuole (l'operatore pensa, il QSO respira).
            var word = hyp.Clone();
            if (word.CloseSymbol(languageModel, LmWeight))
            {
                double effective = Math.Min(durationMs, hyp.DitMs * 7);
                word.Score += LogGaussianDuration(effective, hyp.DitMs * 7, GapSigma);
                word.CloseWord(languageModel);
                output.Add(word);
            }
        }

        /// <summary>
        /// Verosimiglianza log-gaussiana nel dominio log-durata: modella il fatto
        /// che l'errore di keying umano è proporzionale (moltiplicativo), non additivo.
        /// </summary>
        private static double LogGaussianDuration(double duration, double expected, double sigma)
        {
            if (duration <= 0 || expected <= 0) return -20;
            double z = Math.Log(duration / expected) / sigma;
            return -0.5 * z * z;
        }

        /// <summary>Deduplica e pota il fascio alle migliori BeamWidth ipotesi.</summary>
        private void PruneBeam(List<Hypothesis> candidates)
        {
            var unique = new Dictionary<string, Hypothesis>();
            foreach (var h in candidates)
            {
                // Ipotesi con stesso testo, stesso simbolo aperto e dit simile
                // sono ridondanti: sopravvive la migliore
                string key = h.Text + "|" + h.Symbol + "|" + Math.Round(h.DitMs / 4);
                if (!unique.TryGetValue(key, out var existing) || h.Score > existing.Score)
                {
                    unique[key] = h;
                }
            }

            beam = unique.Values.OrderByDescending(h => h.Score).Take(BeamWidth).ToList();

            // Rinormalizza gli score per evitare derive numeriche su sessioni lunghe
            double best = beam[0].Score;
            foreach (var h in beam) h.Score -= best;
        }

        /// <summary>
        /// Emette il prefisso su cui TUTTE le ipotesi del beam concordano:
        /// il testo pubblicato è il consenso del fascio, non una scelta avida.
        /// </summary>
        private void CommitConsensus()
        {
            if (beam.Count == 0) return;

            string prefix = beam[0].Text;
            foreach (var h in beam.Skip(1))
            {
                int len = 0;
                int max = Math.Min(prefix.Length, h.Text.Length);
                while (len < max && prefix[len] == h.Text[len]) len++;
                prefix = prefix.Substring(0, len);
                if (prefix.Length == 0) return;
            }

            EmitAndTrim(prefix.Length);
        }

        /// <summary>Emette i primi <paramref name="length"/> caratteri condivisi.</summary>
        private void EmitAndTrim(int length)
        {
            if (length <= 0) return;

            string emitted = beam[0].Text.Substring(0, length);
            committedText += emitted;
            TextCommitted?.Invoke(this, emitted);

            foreach (var word in ExtractCompletedWords(emitted))
            {
                WordCommitted?.Invoke(this, word);
            }

            foreach (var h in beam)
            {
                h.Text = h.Text.Substring(length);
            }
        }

        // Accumula i caratteri dell'ultima parola parziale tra le emissioni
        private string partialWord = string.Empty;

        private IEnumerable<string> ExtractCompletedWords(string emitted)
        {
            foreach (char c in emitted)
            {
                if (c == ' ')
                {
                    if (partialWord.Length > 0)
                    {
                        var w = partialWord;
                        partialWord = string.Empty;
                        yield return w;
                    }
                }
                else
                {
                    partialWord += c;
                }
            }
        }

        /// <summary>
        /// Confidenza dal distacco tra le prime due ipotesi: se il beam è
        /// unanime la confidenza è alta, se le ipotesi si equivalgono è bassa.
        /// </summary>
        private void UpdateConfidence()
        {
            if (beam.Count < 2)
            {
                Confidence = 1.0;
                return;
            }
            // Score rinormalizzati: best = 0, seconda ≤ 0.
            // Ipotesi equivalenti → 0.5; seconda molto distante → 1.0.
            Confidence = Math.Clamp(1.0 - 0.5 * Math.Exp(beam[1].Score), 0.0, 1.0);
        }

        /// <summary>
        /// Flush per silenzio prolungato: chiude i simboli aperti in tutte le
        /// ipotesi, emette la migliore per intero e riparte da capo mantenendo
        /// la stima di velocità acquisita.
        /// </summary>
        public void Flush()
        {
            var closed = new List<Hypothesis>();
            foreach (var hyp in beam)
            {
                var h = hyp.Clone();
                if (h.Symbol.Length > 0)
                {
                    if (!h.CloseSymbol(languageModel, LmWeight)) continue;
                }
                h.CloseWord(languageModel);
                closed.Add(h);
            }

            if (closed.Count > 0)
            {
                // Nel flush sopravvive solo la migliore ipotesi: emettila per intero
                beam = new List<Hypothesis> { closed.OrderByDescending(h => h.Score).First() };
                EmitAndTrim(beam[0].Text.Length);
            }

            // Riparti pulito, conservando il timing imparato
            double ditMs = beam.Count > 0 ? beam[0].DitMs : 60;
            beam = new List<Hypothesis> { Hypothesis.Initial(ditMs) };
        }

        public void Reset()
        {
            beam = new List<Hypothesis> { Hypothesis.Initial() };
            committedText = string.Empty;
            partialWord = string.Empty;
            Confidence = 0;
        }

        /// <summary>
        /// Un'ipotesi del beam: testo decodificato, simbolo Morse aperto,
        /// stima personale della durata del dit e punteggio log-verosimiglianza.
        /// </summary>
        private class Hypothesis
        {
            public string Text = string.Empty;
            public string Symbol = string.Empty;
            public double DitMs = 60; // 20 WPM di default
            public double Score;

            public static Hypothesis Initial(double ditMs = 60) => new() { DitMs = ditMs };

            public Hypothesis Clone() => new()
            {
                Text = Text,
                Symbol = Symbol,
                DitMs = DitMs,
                Score = Score
            };

            /// <summary>
            /// Aggiorna la stima del dit con media mobile esponenziale:
            /// così ogni ipotesi insegue la velocità effettiva dell'operatore.
            /// </summary>
            public void UpdateDit(double observedDitMs)
            {
                // Limiti di sanità: 5–60 WPM
                observedDitMs = Math.Clamp(observedDitMs, 20, 240);

                // Aggiornamento robusto: un singolo evento anomalo (glitch di
                // rumore) non può strattonare la stima; la velocità di un
                // operatore reale cambia gradualmente
                double ratio = Math.Clamp(observedDitMs / DitMs, 0.6, 1.6);
                DitMs += (DitMs * ratio - DitMs) * 0.25;
            }

            /// <summary>
            /// Chiude il simbolo Morse corrente convertendolo in testo.
            /// Un simbolo sconosciuto decodifica in '?' con forte penalità:
            /// l'ipotesi sopravvive (utile sotto rumore estremo) ma di norma
            /// una sorella con interpretazione valida la supera nel beam.
            /// </summary>
            public bool CloseSymbol(CwLanguageModel lm, double lmWeight)
            {
                if (Symbol.Length == 0) return true;

                if (!MorseTable.TryGetValue(Symbol, out var decoded))
                {
                    Score += -6.0;
                    Text += "?";
                    Symbol = string.Empty;
                    return true;
                }

                // Punteggio linguistico sulla transizione tra caratteri
                char prev = Text.Length > 0 ? Text[^1] : ' ';
                foreach (char c in decoded)
                {
                    Score += lmWeight * lm.TransitionLogProb(prev, c);
                    prev = c;
                }

                Text += decoded;
                Symbol = string.Empty;
                return true;
            }

            /// <summary>Chiude la parola corrente applicando il bonus del lessico.</summary>
            public void CloseWord(CwLanguageModel lm)
            {
                int lastSpace = Text.LastIndexOf(' ');
                string word = lastSpace >= 0 ? Text.Substring(lastSpace + 1) : Text;
                Score += lm.WordBonus(word);
                if (Text.Length > 0 && !Text.EndsWith(' '))
                {
                    Text += " ";
                }
            }
        }
    }
}
