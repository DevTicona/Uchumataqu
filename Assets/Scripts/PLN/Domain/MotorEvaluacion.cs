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

        // Programación dinámica: dp[i,j] es el mínimo de ediciones entre los prefijos a[0..i) y b[0..j).
        // Tiempo y memoria O(m*n); esta versión compara caracteres, no fonemas ni grafemas compuestos.
        public static int DistanciaLevenshtein(string a, string b)
        {
            if (a == null) a = string.Empty;
            if (b == null) b = string.Empty;

            // Comparar un prefijo con la cadena vacía requiere tantas inserciones o borrados como su longitud.
            int[,] dp = new int[a.Length + 1, b.Length + 1];
            for (int i = 0; i <= a.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    // Cada celda elige entre borrar, insertar o sustituir; conservar un carácter cuesta cero.
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

            // Rama de caja blanca CB03/CB04: evita dividir entre cero cuando ambos textos quedan vacíos.
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

        /// <summary>
        /// Aprueba solo la igualdad ortográfica normalizada. La distancia orienta el feedback;
        /// el análisis morfológico agrega contexto sin cambiar el resultado.
        /// </summary>
        public static ResultadoEvaluacion EvaluarEscritura(string esperado, string respuesta,
            AnalizadorMorfologico analizador = null)
        {
            string textoEsperado = NormalizadorTexto.NormalizarEscritura(esperado);
            string textoIngresado = NormalizadorTexto.NormalizarEscritura(respuesta);
            // En escritura, una similitud alta no basta: se exige igualdad tras normalizar, sin distinguir mayúsculas.
            bool aprobado = string.Equals(textoEsperado, textoIngresado, StringComparison.OrdinalIgnoreCase);
            int largoMaximo = Math.Max(textoEsperado.Length, textoIngresado.Length);
            float similitud = largoMaximo == 0 ? 100f : Math.Max(0f,
                (1f - (float)DistanciaLevenshtein(
                    textoEsperado.ToLowerInvariant(), textoIngresado.ToLowerInvariant()) / largoMaximo) * 100f);

            var resultado = new ResultadoEvaluacion
            {
                aprobado = aprobado,
                porcentajeSimilitud = similitud,
                colorSemaforo = aprobado ? "verde" : similitud >= UMBRAL_AMARILLO ? "amarillo" : "rojo",
                textoEsperado = esperado,
                textoIngresado = respuesta,
                origenEntrada = "texto"
            };
            // La consulta morfológica explica formas documentadas; no modifica la aprobación ortográfica.
            analizador?.CompletarEvaluacion(resultado);
            return resultado;
        }

        public static List<string> Tokenizar(string oracion, bool conservarMayusculas = false)
        {
            // Rama de caja blanca CB05/CB06: retorno vacío frente a preparación y separación de palabras.
            if (string.IsNullOrWhiteSpace(oracion)) return new List<string>();
            return NormalizadorTexto.PrepararTokens(oracion, conservarMayusculas)
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }
    }
}
