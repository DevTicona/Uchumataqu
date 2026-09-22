using System;
using System.Text.RegularExpressions;

namespace Assets.Scripts.ASR
{
    /// <summary>
    /// Convierte la ortografía de uchumataqu a la secuencia ASCII emitida por
    /// el modelo CTC de Sherpa. La representación no contiene separadores de
    /// palabras porque el vocabulario acústico tampoco los contiene.
    /// </summary>
    public static class NormalizadorFoneticoAsr
    {
        private static readonly Regla[] Reglas =
        {
            new Regla("ch['’ʼ]", "CH_EJ"), new Regla("chh", "CHH"),
            new Regla("ph", "PH"), new Regla("th", "TH"), new Regla("kh", "KH"), new Regla("qh", "QH"),
            new Regla("p['’ʼ]", "P_EJ"), new Regla("t['’ʼ]", "T_EJ"), new Regla("k['’ʼ]", "K_EJ"), new Regla("q['’ʼ]", "Q_EJ"),
            new Regla("ch", "CH"), new Regla("tk", "TK"), new Regla("tsh", "TSH"),
            new Regla("ts", "TS"), new Regla("ll|ly|l['’ʼ]", "LL"), new Regla("ñ", "NY"), new Regla("sh", "SH"),
            new Regla("ä|aa|ā|â|a:", "AA"), new Regla("ë|ee|ē|ê|e:", "EE"),
            new Regla("ï|ii|ī|î|i:", "II"), new Regla("ö|oo|ō|ô|o:", "OO"), new Regla("ü|uu|ū|û|u:", "UU"),
            new Regla("c", "K"), new Regla("d", "T"), new Regla("h", string.Empty),
            new Regla("á", "A"), new Regla("é", "E"), new Regla("í", "I"), new Regla("ó", "O"), new Regla("ú", "U"),
            new Regla("p", "P"), new Regla("t", "T"), new Regla("k", "K"), new Regla("q", "Q"),
            new Regla("s|z", "S"), new Regla("x", "X"), new Regla("j", "J"), new Regla("m", "M"),
            new Regla("n", "N"), new Regla("r", "R"), new Regla("l", "L"), new Regla("y", "Y"),
            new Regla("w", "W"), new Regla("a", "A"), new Regla("e", "E"), new Regla("i", "I"),
            new Regla("o", "O"), new Regla("u", "U")
        };

        public static string ConvertirEsperado(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            string resultado = texto.Trim().ToLowerInvariant();
            resultado = Regex.Replace(resultado, @"[¡!¿?,.;:()\""«»\-—\s]", string.Empty);
            foreach (Regla regla in Reglas)
                resultado = regla.Patron.Replace(resultado, regla.Reemplazo);

            return resultado;
        }

        public static string NormalizarPrediccionSherpa(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            return Regex.Replace(texto.ToUpperInvariant(), @"\s+", string.Empty);
        }

        private readonly struct Regla
        {
            public readonly Regex Patron;
            public readonly string Reemplazo;

            public Regla(string patron, string reemplazo)
            {
                Patron = new Regex(patron, RegexOptions.CultureInvariant);
                Reemplazo = reemplazo;
            }
        }
    }
}
