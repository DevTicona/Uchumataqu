using System.Globalization;

namespace Assets.Scripts.Util
{
    public static class FormatoTexto
    {
        public static string ConInicialMayuscula(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return texto ?? string.Empty;

            for (int indice = 0; indice < texto.Length; indice++)
            {
                if (!char.IsLetter(texto, indice)) continue;
                string inicial = StringInfo.GetNextTextElement(texto, indice);
                string mayuscula = inicial.ToUpperInvariant();
                if (inicial == mayuscula) return texto;
                return texto.Substring(0, indice) + mayuscula + texto.Substring(indice + inicial.Length);
            }
            return texto;
        }
    }
}
