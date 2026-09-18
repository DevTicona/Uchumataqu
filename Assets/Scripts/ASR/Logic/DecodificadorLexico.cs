using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.ASR
{
    internal static class DecodificadorLexico
    {
        private sealed class Entrada
        {
            public string Palabra { get; set; }
            public string[] Fonemas { get; set; }
        }

        public static string Decodificar(IEnumerable<string> tokens, string textoSherpa, string lexicon)
        {
            string[] fonemas = tokens?
                .Where(token => !string.IsNullOrWhiteSpace(token) && token[0] != '<')
                .Select(token => token.Trim().ToUpperInvariant())
                .ToArray() ?? Array.Empty<string>();

            if (fonemas.Length == 0) return textoSherpa?.Trim() ?? string.Empty;
            IReadOnlyList<Entrada> entradas = CargarEntradas(lexicon);
            Entrada mejor = entradas
                .OrderBy(entrada => Distancia(fonemas, entrada.Fonemas))
                .ThenBy(entrada => Math.Abs(entrada.Fonemas.Length - fonemas.Length))
                .FirstOrDefault();
            return mejor?.Palabra ?? textoSherpa?.Trim() ?? string.Empty;
        }

        private static IReadOnlyList<Entrada> CargarEntradas(string lexicon)
        {
            if (string.IsNullOrEmpty(lexicon)) return Array.Empty<Entrada>();

            return lexicon
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(linea => linea.Split('\t'))
                .Where(partes => partes.Length == 2)
                .Select(partes => new Entrada
                {
                    Palabra = partes[0].Trim(),
                    Fonemas = partes[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                })
                .ToArray();
        }

        private static int Distancia(IReadOnlyList<string> izquierda, IReadOnlyList<string> derecha)
        {
            var anterior = new int[derecha.Count + 1];
            var actual = new int[derecha.Count + 1];
            for (int j = 0; j <= derecha.Count; j++) anterior[j] = j;

            for (int i = 1; i <= izquierda.Count; i++)
            {
                actual[0] = i;
                for (int j = 1; j <= derecha.Count; j++)
                {
                    int sustitucion = anterior[j - 1] +
                        (string.Equals(izquierda[i - 1], derecha[j - 1], StringComparison.Ordinal) ? 0 : 1);
                    actual[j] = Math.Min(Math.Min(anterior[j] + 1, actual[j - 1] + 1), sustitucion);
                }
                (anterior, actual) = (actual, anterior);
            }
            return anterior[derecha.Count];
        }
    }
}
