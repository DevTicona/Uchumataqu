using System;

namespace Assets.Scripts.Core
{
    public static class ContentAddress
    {
        public const string DatosPalabras = "json_palabras";
        public const string DatosOraciones = "json_oraciones";
        public const string DatosLecciones = "json_lecciones";
        public const string DatosEscenariosRa = "json_escenarios_ra";
        public const string PersonajeBase = "personaje_base";

        public const string EtiquetaIslas = "islands";

        public static string Isla(string nombre) => $"imagen/isla/{ValidarSegmento(nombre)}";

        private static string ValidarSegmento(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("El segmento de la dirección Addressable no puede estar vacío.");

            return valor.Trim().Replace(" ", "_").Replace("-", "_").ToLowerInvariant();
        }
    }
}
