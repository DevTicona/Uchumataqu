using System.Text;
using System.Text.RegularExpressions;

namespace Assets.Scripts.PLN
{
    /// <summary>
    /// Normalización de texto en Uchumataqu y Castellano.
    /// Especificado en Docs/Specifications/01-modulo-pln-texto.md (Sección 2).
    /// Preserva grafías nativas (chh, ph, qh, vocales largas, eyectivas, ñ, etc.).
    /// </summary>
    public static class NormalizadorTexto
    {
        public static string NormalizarEscritura(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            string normalizado = texto.Normalize(NormalizationForm.FormC)
                .Replace('’', '\'')
                .Replace('ʼ', '\'')
                .Replace("<", string.Empty)
                .Replace(">", string.Empty)
                .Trim();
            return Regex.Replace(normalizado, @"\s+", " ");
        }

        public static string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            texto = texto.Trim().ToLowerInvariant();
            // Remover signos de puntuación comunes preservando caracteres de palabras y espacios
            texto = Regex.Replace(texto, @"[¡!¿?,.;:()""«»\-—]", " ");
            texto = Regex.Replace(texto, @"\s+", " ");
            return texto.Trim();
        }
    }
}
