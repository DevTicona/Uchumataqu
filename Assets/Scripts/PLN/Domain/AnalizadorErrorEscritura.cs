using System;
using System.Linq;

namespace Assets.Scripts.PLN
{
    /// <summary>
    /// Describe el primer cambio útil para que la persona pueda corregir su respuesta escrita.
    /// </summary>
    public static class AnalizadorErrorEscritura
    {
        public static string Describir(string esperado, string recibido)
        {
            string respuestaEsperada = NormalizadorTexto.Normalizar(esperado);
            string respuestaRecibida = NormalizadorTexto.Normalizar(recibido);
            if (string.IsNullOrEmpty(respuestaRecibida)) return "No se registró una respuesta.";

            string[] palabrasEsperadas = SepararPalabras(respuestaEsperada);
            string[] palabrasRecibidas = SepararPalabras(respuestaRecibida);
            int indice = PrimerIndiceDiferente(palabrasEsperadas, palabrasRecibidas);

            if (indice < palabrasEsperadas.Length && indice == palabrasRecibidas.Length)
                return $"Te faltó la palabra “{palabrasEsperadas[indice]}”.";
            if (indice < palabrasRecibidas.Length && indice == palabrasEsperadas.Length)
                return $"La palabra “{palabrasRecibidas[indice]}” está de más.";
            if (indice < palabrasEsperadas.Length && indice < palabrasRecibidas.Length)
                return DescribirPalabra(palabrasEsperadas[indice], palabrasRecibidas[indice]);

            return "La respuesta no coincide completamente.";
        }

        private static string DescribirPalabra(string esperada, string recibida)
        {
            var cambios = AlineadorEscritura.Comparar(esperada, recibida, conservarUnidades: false)
                .Where(cambio => cambio.Tipo != TipoCambioEscritura.Igual).ToArray();
            if (cambios.Length == 1)
            {
                var cambio = cambios[0];
                switch (cambio.Tipo)
                {
                    case TipoCambioEscritura.Faltante:
                        return $"Te faltó la letra “{cambio.Esperado}” en “{esperada}”.";
                    case TipoCambioEscritura.Sobrante:
                        return $"La letra “{cambio.Recibido}” está de más en “{recibida}”.";
                    case TipoCambioEscritura.Sustitucion:
                        return $"La letra “{cambio.Recibido}” debe ser “{cambio.Esperado}” en “{esperada}”.";
                }
            }

            return $"La palabra “{recibida}” debe ser “{esperada}”.";
        }

        private static string[] SepararPalabras(string texto) =>
            texto.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        private static int PrimerIndiceDiferente(string[] esperada, string[] recibida)
        {
            int limite = Math.Min(esperada.Length, recibida.Length);
            for (int i = 0; i < limite; i++)
                if (!string.Equals(esperada[i], recibida[i], StringComparison.Ordinal)) return i;
            return limite;
        }

    }
}
