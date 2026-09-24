using System;
using System.Collections.Generic;
using Assets.Scripts.Persistencia;
using Assets.Scripts.PLN;

namespace Assets.Scripts.ASR
{
    /// <summary>
    /// Traduce la secuencia fonética CTC a la entrada legible más cercana del corpus.
    /// La secuencia fonética se evalúa antes de convertirla en texto de presentación.
    /// </summary>
    public static class DecodificadorFoneticoAsr
    {
        private const float SimilitudMinimaLegible = 45f;

        public static string Decodificar(string prediccionFonetica, string objetivoActual)
        {
            if (string.IsNullOrWhiteSpace(prediccionFonetica))
                return "No se identificó una palabra con claridad";

            string mejorTexto = null;
            float mejorSimilitud = float.MinValue;
            var candidatos = new HashSet<string>(StringComparer.Ordinal);
            AgregarCandidato(candidatos, objetivoActual);

            if (CorpusRepository.EstaInicializado)
            {
                foreach (var palabra in CorpusRepository.ObtenerTodasLasPalabras())
                {
                    AgregarCandidato(candidatos, palabra?.uch);
                    if (palabra?.formas == null) continue;
                    foreach (var forma in palabra.formas)
                        AgregarCandidato(candidatos, forma?.uch);
                }

                foreach (var oracion in CorpusRepository.ObtenerTodasLasOraciones())
                    AgregarCandidato(candidatos, oracion?.uch);
            }

            foreach (string candidato in candidatos)
            {
                string foneticaCandidata = NormalizadorFoneticoAsr.ConvertirEsperado(candidato);
                float similitud = MotorEvaluacion.PorcentajeSimilitud(foneticaCandidata, prediccionFonetica);
                if (similitud <= mejorSimilitud) continue;

                mejorSimilitud = similitud;
                mejorTexto = candidato;
            }

            return mejorSimilitud >= SimilitudMinimaLegible
                ? mejorTexto
                : "No se identificó una palabra con claridad";
        }

        private static void AgregarCandidato(ISet<string> candidatos, string texto)
        {
            if (!string.IsNullOrWhiteSpace(texto)) candidatos.Add(texto.Trim());
        }
    }
}
