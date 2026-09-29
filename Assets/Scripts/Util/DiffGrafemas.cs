using System;
using System.Collections.Generic;
using Assets.Scripts.PLN;

namespace Assets.Scripts.Util
{
    public static class DiffGrafemas
    {
        private const string Correcto = "#2E8B57";
        private const string Incorrecto = "#D32F2F";
        private const string Faltante = "#E08A00";

        private static readonly string[] Unidades =
        {
            "ch'", "chh",
            "ph", "th", "kh", "qh", "p'", "t'", "k'", "q'", "l'", "ch",
            "ts", "ll", "ly", "sh",
            "aa", "ee", "ii", "oo", "uu",
            "a:", "e:", "i:", "o:", "u:"
        };

        public static List<string> Tokenizar(string texto)
        {
            string normalizado = NormalizadorTexto.NormalizarEscritura(texto);
            var grafemas = new List<string>();
            for (int indice = 0; indice < normalizado.Length;)
            {
                if (indice + 3 <= normalizado.Length &&
                    string.Compare(normalizado, indice, "tsh", 0, 3,
                        StringComparison.OrdinalIgnoreCase) == 0)
                {
                    grafemas.Add(normalizado.Substring(indice++, 1));
                    continue;
                }

                int longitud = 1;
                foreach (string unidad in Unidades)
                {
                    if (indice + unidad.Length > normalizado.Length ||
                        string.Compare(normalizado, indice, unidad, 0, unidad.Length,
                            StringComparison.OrdinalIgnoreCase) != 0)
                        continue;

                    longitud = unidad.Length;
                    break;
                }

                grafemas.Add(normalizado.Substring(indice, longitud));
                indice += longitud;
            }

            return grafemas;
        }

        public static (string usuario, string correcta) Comparar(string usuario, string correcta)
        {
            List<string> escritos = Tokenizar(usuario);
            List<string> esperados = Tokenizar(correcta);
            int[,] distancias = new int[escritos.Count + 1, esperados.Count + 1];
            for (int i = 0; i <= escritos.Count; i++) distancias[i, 0] = i;
            for (int j = 0; j <= esperados.Count; j++) distancias[0, j] = j;

            for (int i = 1; i <= escritos.Count; i++)
            {
                for (int j = 1; j <= esperados.Count; j++)
                {
                    int sustitucion = Iguales(escritos[i - 1], esperados[j - 1]) ? 0 : 1;
                    distancias[i, j] = Math.Min(
                        Math.Min(distancias[i - 1, j] + 1, distancias[i, j - 1] + 1),
                        distancias[i - 1, j - 1] + sustitucion);
                }
            }

            var resultadoUsuario = new List<string>();
            var resultadoCorrecto = new List<string>();
            int x = escritos.Count;
            int y = esperados.Count;
            while (x > 0 || y > 0)
            {
                if (x > 0 && distancias[x, y] == distancias[x - 1, y] + 1 &&
                    (y == 0 || Iguales(escritos[x - 1], esperados[y - 1])))
                {
                    resultadoUsuario.Add($"<color={Incorrecto}><s>{escritos[x - 1]}</s></color>");
                    x--;
                    continue;
                }

                if (x > 0 && y > 0)
                {
                    bool iguales = Iguales(escritos[x - 1], esperados[y - 1]);
                    int costo = iguales ? 0 : 1;
                    if (distancias[x, y] == distancias[x - 1, y - 1] + costo)
                    {
                        resultadoUsuario.Add(Color(iguales ? Correcto : Incorrecto, escritos[x - 1]));
                        resultadoCorrecto.Add(iguales
                            ? Color(Correcto, esperados[y - 1])
                            : ResaltarFaltante(esperados[y - 1]));
                        x--;
                        y--;
                        continue;
                    }
                }

                if (x > 0 && distancias[x, y] == distancias[x - 1, y] + 1)
                {
                    resultadoUsuario.Add($"<color={Incorrecto}><s>{escritos[x - 1]}</s></color>");
                    x--;
                }
                else
                {
                    resultadoUsuario.Add(Color(Faltante, "_"));
                    resultadoCorrecto.Add(ResaltarFaltante(esperados[y - 1]));
                    y--;
                }
            }

            resultadoUsuario.Reverse();
            resultadoCorrecto.Reverse();
            return (string.Concat(resultadoUsuario), string.Concat(resultadoCorrecto));
        }

        private static bool Iguales(string a, string b) =>
            string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        private static string Color(string color, string texto) => $"<color={color}>{texto}</color>";

        private static string ResaltarFaltante(string texto) =>
            $"<color={Faltante}><b><u>{texto}</u></b></color>";
    }
}
