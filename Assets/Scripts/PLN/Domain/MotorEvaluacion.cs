using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.PLN
{
    /// <summary>
    /// Motor central de evaluación de similitud y tokenización.
    /// Especificado en Docs/Specifications/01-modulo-pln-texto.md (Secciones 3 y 4).
    /// </summary>
    public static class MotorEvaluacion
    {
        public const float UMBRAL_VERDE = 85.0f;
        public const float UMBRAL_AMARILLO = 60.0f;

        public static int DistanciaLevenshtein(string a, string b)
        {
            if (a == null) a = string.Empty;
            if (b == null) b = string.Empty;

            int[,] dp = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    int costo = a[i - 1] == b[j - 1] ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + costo
                    );
                }
            }

            return dp[a.Length, b.Length];
        }

        public static float PorcentajeSimilitud(string esperado, string respuesta)
        {
            string a = NormalizadorTexto.Normalizar(esperado);
            string b = NormalizadorTexto.Normalizar(respuesta);

            int distancia = DistanciaLevenshtein(a, b);
            int largoMax = Math.Max(a.Length, b.Length);

            if (largoMax == 0) return 100f;
            return Math.Max(0f, (1f - (float)distancia / largoMax) * 100f);
        }

        public static ResultadoEvaluacion EvaluarTexto(string esperado, string respuesta, string origenEntrada = "texto",
            string textoEsperadoOriginal = null)
        {
            float similitud = PorcentajeSimilitud(esperado, respuesta);
            bool aprobado = similitud >= UMBRAL_VERDE;
            string semaforo = similitud >= UMBRAL_VERDE ? "verde" : (similitud >= UMBRAL_AMARILLO ? "amarillo" : "rojo");

            return new ResultadoEvaluacion
            {
                aprobado = aprobado,
                porcentajeSimilitud = similitud,
                colorSemaforo = semaforo,
                textoEsperado = textoEsperadoOriginal ?? esperado,
                textoIngresado = respuesta,
                origenEntrada = origenEntrada
            };
        }

        public static ResultadoEvaluacion EvaluarSeleccionExacta(string esperado, string respuesta)
        {
            bool correcto = NormalizadorTexto.Normalizar(esperado) == NormalizadorTexto.Normalizar(respuesta);
            return new ResultadoEvaluacion
            {
                aprobado = correcto,
                porcentajeSimilitud = correcto ? 100f : 0f,
                colorSemaforo = correcto ? "verde" : "rojo",
                textoEsperado = esperado,
                textoIngresado = respuesta,
                origenEntrada = "seleccion"
            };
        }

        public static ResultadoEvaluacion EvaluarEscritura(string esperado, string respuesta)
        {
            string textoEsperado = NormalizadorTexto.NormalizarEscritura(esperado);
            string textoIngresado = NormalizadorTexto.NormalizarEscritura(respuesta);
            bool aprobado = string.Equals(textoEsperado, textoIngresado, StringComparison.OrdinalIgnoreCase);
            int largoMaximo = Math.Max(textoEsperado.Length, textoIngresado.Length);
            float similitud = largoMaximo == 0 ? 100f : Math.Max(0f,
                (1f - (float)DistanciaLevenshtein(
                    textoEsperado.ToLowerInvariant(), textoIngresado.ToLowerInvariant()) / largoMaximo) * 100f);

            return new ResultadoEvaluacion
            {
                aprobado = aprobado,
                porcentajeSimilitud = similitud,
                colorSemaforo = aprobado ? "verde" : similitud >= UMBRAL_AMARILLO ? "amarillo" : "rojo",
                textoEsperado = esperado,
                textoIngresado = respuesta,
                origenEntrada = "texto"
            };
        }

        public static List<string> Tokenizar(string oracion)
        {
            if (string.IsNullOrWhiteSpace(oracion)) return new List<string>();
            return NormalizadorTexto.Normalizar(oracion)
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }
    }
}
