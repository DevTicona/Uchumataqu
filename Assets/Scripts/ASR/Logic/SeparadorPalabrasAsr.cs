using System;
using System.Collections.Generic;

namespace Assets.Scripts.ASR
{
    internal static class SeparadorPalabrasAsr
    {
        public static ISet<int> ObtenerCortes(IList<string> reconocidos, string contexto)
        {
            var cortes = new HashSet<int>();
            if (string.IsNullOrWhiteSpace(contexto) || reconocidos.Count == 0) return cortes;
            string[] palabras = contexto.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
            if (palabras.Length < 2) return cortes;
            var esperados = new List<string>();
            var limites = new HashSet<int>();
            foreach (string palabra in palabras)
            {
                if (esperados.Count > 0) limites.Add(esperados.Count);
                esperados.AddRange(NormalizadorFoneticoAsr.TextoAFonemas(NormalizadorFoneticoAsr.ConvertirEsperado(palabra)));
            }
            if (esperados.Count == 0 || esperados.Count > 256 || reconocidos.Count > 2048) return cortes;

            var distancia = new int[esperados.Count + 1, reconocidos.Count + 1];
            for (int i = 0; i <= esperados.Count; i++) distancia[i, 0] = i;
            for (int j = 0; j <= reconocidos.Count; j++) distancia[0, j] = j;
            for (int i = 1; i <= esperados.Count; i++)
                for (int j = 1; j <= reconocidos.Count; j++)
                    distancia[i, j] = Math.Min(distancia[i - 1, j - 1] + (esperados[i - 1] == reconocidos[j - 1] ? 0 : 1),
                        Math.Min(distancia[i - 1, j] + 1, distancia[i, j - 1] + 1));

            // Con poco parecido no hay evidencia suficiente para asignar los límites de la frase.
            if (distancia[esperados.Count, reconocidos.Count] > Math.Max(esperados.Count, reconocidos.Count) * 0.6f)
                return cortes;

            int fila = esperados.Count;
            int columna = reconocidos.Count;
            while (fila > 0 || columna > 0)
            {
                if (limites.Remove(fila) && columna > 0 && columna < reconocidos.Count) cortes.Add(columna);
                if (fila > 0 && columna > 0 && distancia[fila, columna] == distancia[fila - 1, columna - 1]
                    + (esperados[fila - 1] == reconocidos[columna - 1] ? 0 : 1))
                {
                    fila--;
                    columna--;
                }
                else if (fila > 0 && distancia[fila, columna] == distancia[fila - 1, columna] + 1) fila--;
                else columna--;
            }
            return cortes;
        }
    }
}
