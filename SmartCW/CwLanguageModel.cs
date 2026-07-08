using System;
using System.Collections.Generic;

namespace RadioLoggerApp.MorseDecoder.SmartCW
{
    /// <summary>
    /// Modello linguistico leggero specializzato nel traffico radioamatoriale.
    /// Fornisce due segnali al beam search:
    ///  1. probabilità bigramma carattere→carattere (addestrato su un corpus QSO
    ///     incorporato: CQ, Q-codes, rapporti RST, indicativi, inglese operativo);
    ///  2. bonus per parole complete presenti nel lessico ham.
    /// Tutto è calcolato a runtime dal corpus incorporato: nessun file esterno.
    /// </summary>
    public class CwLanguageModel
    {
        // Corpus rappresentativo del traffico CW (QSO tipici, contest, Q-codes)
        private const string Corpus =
            "CQ CQ CQ DE IU8LMC IU8LMC K " +
            "CQ CQ CQ DE IK2ABC IK2ABC PSE K " +
            "CQ DX CQ DX DE W1AW W1AW K " +
            "R R DE IZ0XYZ TNX FER CALL UR RST 599 599 BT " +
            "NAME IS MARCO MARCO QTH ROMA ROMA HW CPY BK " +
            "TNX FER RPRT UR RST 579 579 NAME IS JOHN JOHN QTH BOSTON BT " +
            "RIG IS FT991 PWR 100 W ANT DIPOLE UP 10 M BT " +
            "WX SUNNY TEMP 25 C BT TNX FER NICE QSO 73 73 GL SK " +
            "QRZ QRZ DE F5ABC K QRL QRL QSY UP 1 " +
            "TEST DE DL1XX 599 001 599 002 TU " +
            "5NN TU 5NN 73 GL DX CUL " +
            "PSE QRS QRS UR SIGS FB HR OM " +
            "GM GA GE OM TNX RPRT UR 559 QSB QRM QRN " +
            "MNI TNX FER QSO HPE CUAGN 73 ES GL DE IU8LMC SK E E " +
            "THE QUICK BROWN FOX JUMPS OVER THE LAZY DOG " +
            "SOS SOS SOS DE VESSEL POSITION 40 N 14 E ";

        // Lessico ham: parole che ricevono un bonus quando completate
        private static readonly HashSet<string> HamLexicon = new(StringComparer.Ordinal)
        {
            "CQ", "DE", "DX", "K", "KN", "R", "BK", "TU", "TNX", "PSE", "FER",
            "UR", "RST", "599", "5NN", "579", "559", "73", "88", "GL", "GM",
            "GA", "GE", "OM", "YL", "HR", "HW", "CPY", "NAME", "QTH", "RIG",
            "ANT", "PWR", "WX", "TEMP", "ES", "CUL", "CUAGN", "HPE", "MNI",
            "FB", "SIGS", "QRZ", "QRL", "QRM", "QRN", "QSB", "QSY", "QRS",
            "QSO", "QSL", "TEST", "SOS", "THE", "IS", "AND", "SK", "AR"
        };

        // Log-probabilità bigramma: indice = (prev, next) su charset compatto
        private readonly Dictionary<(char, char), double> bigramLogProb = new();
        private readonly Dictionary<char, double> unigramLogProb = new();
        private readonly double floorLogProb;

        public CwLanguageModel()
        {
            // Conta i bigrammi sul corpus (spazio incluso: modella inizio/fine parola)
            var bigramCounts = new Dictionary<(char, char), int>();
            var unigramCounts = new Dictionary<char, int>();
            int total = 0;

            for (int i = 0; i < Corpus.Length; i++)
            {
                char c = Corpus[i];
                unigramCounts[c] = unigramCounts.GetValueOrDefault(c) + 1;
                total++;
                if (i > 0)
                {
                    var key = (Corpus[i - 1], c);
                    bigramCounts[key] = bigramCounts.GetValueOrDefault(key) + 1;
                }
            }

            // Smoothing add-one implicito tramite floor
            floorLogProb = Math.Log(0.5 / total);

            foreach (var (c, n) in unigramCounts)
            {
                unigramLogProb[c] = Math.Log((double)n / total);
            }

            foreach (var ((prev, next), n) in bigramCounts)
            {
                int prevCount = unigramCounts[prev];
                bigramLogProb[(prev, next)] = Math.Log((double)n / prevCount);
            }
        }

        /// <summary>
        /// Log-probabilità della transizione prev→next.
        /// Interpola bigramma e unigramma, con floor per coppie mai viste
        /// (nessun carattere è impossibile: il Morse trasporta anche testo arbitrario).
        /// </summary>
        public double TransitionLogProb(char prev, char next)
        {
            if (bigramLogProb.TryGetValue((prev, next), out double big))
            {
                return big;
            }
            if (unigramLogProb.TryGetValue(next, out double uni))
            {
                // Mai visto il bigramma ma il carattere esiste nel corpus
                return uni + Math.Log(0.3);
            }
            return floorLogProb;
        }

        /// <summary>
        /// Bonus (log-scala) assegnato al completamento di una parola.
        /// Parole del lessico ham ricevono un premio; le altre nessuna penalità
        /// (gli indicativi di chiamata sono "parole" arbitrarie e legittime).
        /// </summary>
        public double WordBonus(string word)
        {
            if (string.IsNullOrEmpty(word)) return 0;
            if (HamLexicon.Contains(word)) return 1.5;
            if (LooksLikeCallsign(word)) return 1.0;
            return 0;
        }

        /// <summary>
        /// Riconosce il pattern di un indicativo di chiamata:
        /// prefisso lettere(+cifra opzionale), una cifra, suffisso di lettere.
        /// Es.: IU8LMC, W1AW, DL1XX, F5ABC.
        /// </summary>
        private static bool LooksLikeCallsign(string word)
        {
            if (word.Length < 3 || word.Length > 8) return false;

            int digitIndex = -1;
            for (int i = 1; i < word.Length - 1; i++)
            {
                if (char.IsDigit(word[i])) digitIndex = i;
            }
            if (digitIndex < 1) return false;

            // Il suffisso dopo l'ultima cifra deve essere composto da lettere
            for (int i = digitIndex + 1; i < word.Length; i++)
            {
                if (!char.IsLetter(word[i])) return false;
            }
            // E deve iniziare con una lettera
            return char.IsLetter(word[0]);
        }
    }
}
