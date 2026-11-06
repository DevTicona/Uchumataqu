using System;
using System.Collections.Generic;
using System.Text;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.ASR
{
    /// <summary>
    /// Reconstruye la grafía de la secuencia CTC para el feedback.
    /// Solo usa una forma del corpus cuando sus fonemas coinciden exactamente;
    /// la secuencia cruda se conserva aparte para la evaluación.
    /// </summary>
    public static class DecodificadorFoneticoAsr
    {
        public static string Decodificar(string prediccionFonetica, string objetivoActual)
        {
            if (string.IsNullOrWhiteSpace(prediccionFonetica))
                return "No se identificó una palabra con claridad";

            string secuencia = NormalizadorFoneticoAsr.NormalizarPrediccionSherpa(prediccionFonetica);
            if (!string.IsNullOrWhiteSpace(objetivoActual) &&
                NormalizadorFoneticoAsr.ConvertirEsperado(objetivoActual) == secuencia)
                return objetivoActual.Trim();

            var candidatos = new HashSet<string>(StringComparer.Ordinal);
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
                if (NormalizadorFoneticoAsr.ConvertirEsperado(candidato) == secuencia)
                    return candidato;
            }

            var fonemas = NormalizadorFoneticoAsr.TextoAFonemas(secuencia);
            var cortes = SeparadorPalabrasAsr.ObtenerCortes(fonemas, objetivoActual);
            var grafia = new StringBuilder();
            for (int i = 0; i < fonemas.Count; i++)
            {
                if (cortes.Contains(i)) grafia.Append(' ');
                grafia.Append(Grafia(fonemas[i]));
            }

            if (grafia.Length == 0) return "No se identificó una palabra con claridad";
            grafia[0] = char.ToUpperInvariant(grafia[0]);
            return grafia.ToString();
        }

        private static string Grafia(string fonema)
        {
            switch (fonema)
            {
                case "AA": return "ä";
                case "EE": return "ë";
                case "II": return "ï";
                case "OO": return "ö";
                case "UU": return "ü";
                case "NY": return "ñ";
                case "P_EJ": return "pʼ";
                case "T_EJ": return "tʼ";
                case "K_EJ": return "kʼ";
                case "Q_EJ": return "qʼ";
                case "CH_EJ": return "chʼ";
                default: return fonema.ToLowerInvariant();
            }
        }

        private static void AgregarCandidato(ISet<string> candidatos, string texto)
        {
            if (!string.IsNullOrWhiteSpace(texto)) candidatos.Add(texto.Trim());
        }
    }
}
