namespace Assets.Scripts.Core
{
    public enum PaginaMain
    {
        Inicio = 0,
        Lecciones = 1,
        Logros = 2,
        Ajustes = 3,
        Acerca = 4
    }

    /// <summary>
    /// Almacena el estado transitorio de navegación entre escenas sin acoplar componentes.
    /// Especificado en Docs/UI/07-navegacion-y-estado-ui.md.
    /// </summary>
    public static class ContextoNavegacion
    {
        public static string LeccionId { get; set; } = "L01";
        public static string EscenarioRaId { get; set; } = "ra_encuentro_comunicacion";
        public static PaginaMain PaginaDestino { get; set; } = PaginaMain.Inicio;

        public static void Limpiar()
        {
            LeccionId = "L01";
            EscenarioRaId = "ra_encuentro_comunicacion";
            PaginaDestino = PaginaMain.Inicio;
        }
    }
}
