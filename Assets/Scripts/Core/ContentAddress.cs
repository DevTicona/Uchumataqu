using System;

namespace Assets.Scripts.Core
{
    public static class ContentAddress
    {
        public const string TemaUi = "UITheme";
        public const string DatosPalabras = "json_palabras";
        public const string DatosOraciones = "json_oraciones";
        public const string DatosLecciones = "json_lecciones";
        public const string DatosEscenariosRa = "json_escenarios_ra";
        public const string PersonajeBase = "personaje_base";

        public const string EtiquetaUi = "ui";
        public const string EtiquetaIconosUi = "ui_icons";
        public const string EtiquetaAvatares = "avatars";
        public const string EtiquetaFondos = "backgrounds";
        public const string EtiquetaIslas = "islands";

        public static string Avatar(string avatarId) => $"avatar/{ValidarSegmento(avatarId)}";

        public static string Icono(string categoria, string nombre) =>
            $"ui/icon/{ValidarSegmento(categoria)}/{ValidarSegmento(nombre)}";

        public static string BaseUi(string nombre) => $"ui/base/{ValidarSegmento(nombre)}";

        public static string EfectoUi(string nombre) => $"ui/effect/{ValidarSegmento(nombre)}";

        public static string Fondo(string nombre) => $"imagen/fondo/{ValidarSegmento(nombre)}";

        public static string Isla(string nombre) => $"imagen/isla/{ValidarSegmento(nombre)}";

        public static string Marca(string nombre) => $"imagen/marca/{ValidarSegmento(nombre)}";

        private static string ValidarSegmento(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                throw new ArgumentException("El segmento de la dirección Addressable no puede estar vacío.");

            return valor.Trim().Replace(" ", "_").Replace("-", "_").ToLowerInvariant();
        }
    }
}
