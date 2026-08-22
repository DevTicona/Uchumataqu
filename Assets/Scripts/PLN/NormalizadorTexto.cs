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
        public static string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
            texto = texto.Trim().ToLowerInvariant();
            texto = Regex.Replace(texto, @"\s+", " ");
            return texto;
        }
    }
}
