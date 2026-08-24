using System.Collections.Generic;

namespace Assets.Scripts.UI.Main
{
    public sealed class InicioPresentacionData
    {
        public string Nombre { get; set; }
        public string LeccionId { get; set; }
        public string TituloLeccion { get; set; }
        public int LeccionesCompletadas { get; set; }
        public int EscenariosVisitados { get; set; }
    }

    public sealed class IslaMapaPresentacionData
    {
        public string LeccionId { get; set; }
        public string EscenarioRaId { get; set; }
        public int Numero { get; set; }
        public string Titulo { get; set; }
        public Paginas.EstadoIsla Estado { get; set; }
        public bool EsHitoRa => !string.IsNullOrEmpty(EscenarioRaId);
    }

    public sealed class LogroPresentacionData
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Desbloqueado { get; set; }
    }
}
