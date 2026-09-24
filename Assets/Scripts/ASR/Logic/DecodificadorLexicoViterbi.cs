using System;
using System.Collections.Generic;

namespace Assets.Scripts.ASR
{
    /// <summary>
    /// Segmenta una secuencia fonetica continua usando el lexico pedagogico
    /// cerrado. Solo se usa despues de superar el filtro acustico inicial.
    /// </summary>
    public sealed class DecodificadorLexicoViterbi
    {
        private const float CostoFonemaSuelto = 1.2f;
        private readonly List<EntradaLexica> entradas = new List<EntradaLexica>();
        private int longitudMaximaPalabra;

        public bool EstaInicializado => entradas.Count > 0;
        public int TotalPalabras => entradas.Count;

        public void CargarLexiconDesdeTexto(string contenido)
        {
            entradas.Clear();
            longitudMaximaPalabra = 0;
            if (string.IsNullOrWhiteSpace(contenido)) return;

            string[] lineas = contenido.Replace("\r", string.Empty).Split('\n');
            foreach (string linea in lineas)
            {
                string[] partes = linea.Split('\t');
                if (partes.Length < 2 || string.IsNullOrWhiteSpace(partes[0])) continue;

                string[] fonemas = partes[1].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (fonemas.Length == 0) continue;

                var entrada = new EntradaLexica(partes[0].Trim(), new List<string>(fonemas));
                entradas.Add(entrada);
                longitudMaximaPalabra = Math.Max(longitudMaximaPalabra, fonemas.Length);
            }
        }

        public ResultadoDecodificacion DecodificarConRestricciones(
            List<string> tokensPredichos, float maxWordErrRate = 0.35f)
        {
            var resultado = new ResultadoDecodificacion();
            if (!EstaInicializado || tokensPredichos == null || tokensPredichos.Count == 0)
                return resultado;

            int cantidadTokens = tokensPredichos.Count;
            float[] costos = new float[cantidadTokens + 1];
            int[] padres = new int[cantidadTokens + 1];
            EntradaLexica[] palabrasPadre = new EntradaLexica[cantidadTokens + 1];

            for (int indice = 0; indice <= cantidadTokens; indice++)
            {
                costos[indice] = float.PositiveInfinity;
                padres[indice] = -1;
            }

            costos[0] = 0f;
            for (int fin = 1; fin <= cantidadTokens; fin++)
            {
                if (!float.IsPositiveInfinity(costos[fin - 1]))
                {
                    costos[fin] = costos[fin - 1] + CostoFonemaSuelto;
                    padres[fin] = fin - 1;
                }

                int inicioMinimo = Math.Max(0, fin - longitudMaximaPalabra - 3);
                for (int inicio = inicioMinimo; inicio < fin; inicio++)
                {
                    if (float.IsPositiveInfinity(costos[inicio])) continue;

                    int longitudVentana = fin - inicio;
                    foreach (EntradaLexica entrada in entradas)
                    {
                        if (Math.Abs(longitudVentana - entrada.Fonemas.Count) > 3) continue;

                        int distancia = Distancia(tokensPredichos, inicio, longitudVentana, entrada.Fonemas);
                        float errorNormalizado = (float)distancia / Math.Max(1, entrada.Fonemas.Count);
                        if (errorNormalizado > maxWordErrRate) continue;

                        float nuevoCosto = costos[inicio] + distancia;
                        if (nuevoCosto >= costos[fin]) continue;

                        costos[fin] = nuevoCosto;
                        padres[fin] = inicio;
                        palabrasPadre[fin] = entrada;
                    }
                }
            }

            if (padres[cantidadTokens] < 0) return resultado;

            var palabrasInversas = new List<EntradaLexica>();
            bool contieneFonemaSuelto = false;
            for (int actual = cantidadTokens; actual > 0; actual = padres[actual])
            {
                if (padres[actual] < 0) return resultado;
                EntradaLexica palabra = palabrasPadre[actual];
                if (palabra == null)
                    contieneFonemaSuelto = true;
                else
                    palabrasInversas.Add(palabra);
            }

            palabrasInversas.Reverse();
            foreach (EntradaLexica palabra in palabrasInversas)
            {
                resultado.PalabrasReconocidas.Add(palabra.Texto);
                resultado.FonemasReconstruidos.AddRange(palabra.Fonemas);
            }

            resultado.CostoPromedio = costos[cantidadTokens] / cantidadTokens;
            resultado.EsExitoso = !contieneFonemaSuelto && resultado.PalabrasReconocidas.Count > 0;
            return resultado;
        }

        private static int Distancia(List<string> predichos, int inicio, int longitud, List<string> esperados)
        {
            int[,] matriz = new int[longitud + 1, esperados.Count + 1];
            for (int fila = 0; fila <= longitud; fila++) matriz[fila, 0] = fila;
            for (int columna = 0; columna <= esperados.Count; columna++) matriz[0, columna] = columna;

            for (int fila = 1; fila <= longitud; fila++)
            {
                for (int columna = 1; columna <= esperados.Count; columna++)
                {
                    int sustitucion = predichos[inicio + fila - 1] == esperados[columna - 1] ? 0 : 1;
                    matriz[fila, columna] = Math.Min(
                        Math.Min(matriz[fila - 1, columna] + 1, matriz[fila, columna - 1] + 1),
                        matriz[fila - 1, columna - 1] + sustitucion);
                }
            }

            return matriz[longitud, esperados.Count];
        }

        public sealed class ResultadoDecodificacion
        {
            public List<string> PalabrasReconocidas = new List<string>();
            public List<string> FonemasReconstruidos = new List<string>();
            public float CostoPromedio;
            public bool EsExitoso;
        }

        private sealed class EntradaLexica
        {
            public readonly string Texto;
            public readonly List<string> Fonemas;

            public EntradaLexica(string texto, List<string> fonemas)
            {
                Texto = texto;
                Fonemas = fonemas;
            }
        }
    }
}
