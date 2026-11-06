using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Assets.Scripts.PLN
{
    /// <summary>
    /// Alinea grafemas de escritura como unidades lingüísticas (por ejemplo, chh o vocales largas)
    /// para señalar cambios sin partirlos en caracteres aislados.
    /// </summary>
    public static class AlineadorEscritura
    {
        private static readonly string[] Unidades =
        {
            "ch'", "chh",
            "ph", "th", "kh", "qh", "p'", "t'", "k'", "q'", "l'", "ch",
            "ts", "ll", "ly", "sh",
            "aa", "ee", "ii", "oo", "uu",
            "a:", "e:", "i:", "o:", "u:"
        };

        public static List<string> TokenizarUnidades(string texto)
        {
            string normalizado = NormalizadorTexto.NormalizarEscritura(texto);
            var unidades = new List<string>();
            for (int indice = 0; indice < normalizado.Length;)
            {
                if (indice + 3 <= normalizado.Length && string.Compare(normalizado, indice, "tsh", 0, 3,
                    StringComparison.OrdinalIgnoreCase) == 0)
                {
                    unidades.Add(normalizado.Substring(indice++, 1));
                    continue;
                }
                string unidad = Unidades.FirstOrDefault(candidata => indice + candidata.Length <= normalizado.Length &&
                    string.Compare(normalizado, indice, candidata, 0, candidata.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    ?? StringInfo.GetNextTextElement(normalizado, indice);
                unidades.Add(normalizado.Substring(indice, unidad.Length));
                indice += unidad.Length;
            }
            return unidades;
        }

        public static IReadOnlyList<CambioEscrituraModel> Comparar(string esperado, string recibido,
            bool conservarUnidades = true)
        {
            var letras = Alinear(SepararLetras(recibido), SepararLetras(esperado));
            if (!conservarUnidades) return letras;
            var unidades = Alinear(TokenizarUnidades(recibido), TokenizarUnidades(esperado));
            // Una unidad como «ii» no debe ocultar una sola letra omitida en «ini».
            return unidades.Count(cambio => cambio.Tipo != TipoCambioEscritura.Igual) <=
                letras.Count(cambio => cambio.Tipo != TipoCambioEscritura.Igual) ? unidades : letras;
        }

        public static IReadOnlyList<CambioEscrituraModel> CompararPalabras(string esperado, string recibido) =>
            Alinear(MotorEvaluacion.Tokenizar(recibido), MotorEvaluacion.Tokenizar(esperado));

        private static List<string> SepararLetras(string texto)
        {
            var letras = new List<string>();
            var elementos = StringInfo.GetTextElementEnumerator(NormalizadorTexto.NormalizarEscritura(texto));
            while (elementos.MoveNext()) letras.Add(elementos.GetTextElement());
            return letras;
        }

        private static List<CambioEscrituraModel> Alinear(IReadOnlyList<string> escritos, IReadOnlyList<string> esperados)
        {
            var distancias = new int[escritos.Count + 1, esperados.Count + 1];
            for (int i = 0; i <= escritos.Count; i++) distancias[i, 0] = i;
            for (int j = 0; j <= esperados.Count; j++) distancias[0, j] = j;
            for (int i = 1; i <= escritos.Count; i++)
                for (int j = 1; j <= esperados.Count; j++)
                    distancias[i, j] = Math.Min(Math.Min(distancias[i - 1, j] + 1, distancias[i, j - 1] + 1),
                        distancias[i - 1, j - 1] + (Iguales(escritos[i - 1], esperados[j - 1]) ? 0 : 1));

            // Reconstruye el camino de la matriz desde el final para localizar letras sobrantes, faltantes
            // y sustituidas. Este detalle alimenta la corrección visual; no decide la aprobación.
            var cambios = new List<CambioEscrituraModel>();
            int x = escritos.Count;
            int y = esperados.Count;
            while (x > 0 || y > 0)
            {
                if (x > 0 && distancias[x, y] == distancias[x - 1, y] + 1 &&
                    (y == 0 || Iguales(escritos[x - 1], esperados[y - 1])))
                {
                    cambios.Add(new CambioEscrituraModel(TipoCambioEscritura.Sobrante, escritos[--x], string.Empty));
                    continue;
                }
                if (x > 0 && y > 0)
                {
                    bool iguales = Iguales(escritos[x - 1], esperados[y - 1]);
                    if (distancias[x, y] == distancias[x - 1, y - 1] + (iguales ? 0 : 1))
                    {
                        cambios.Add(new CambioEscrituraModel(iguales ? TipoCambioEscritura.Igual : TipoCambioEscritura.Sustitucion,
                            escritos[--x], esperados[--y]));
                        continue;
                    }
                }
                if (x > 0 && distancias[x, y] == distancias[x - 1, y] + 1)
                    cambios.Add(new CambioEscrituraModel(TipoCambioEscritura.Sobrante, escritos[--x], string.Empty));
                else
                    cambios.Add(new CambioEscrituraModel(TipoCambioEscritura.Faltante, string.Empty, esperados[--y]));
            }
            cambios.Reverse();
            return cambios;
        }

        private static bool Iguales(string a, string b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}
