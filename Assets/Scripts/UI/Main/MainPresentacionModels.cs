using System.Collections.Generic;

namespace Assets.Scripts.UI.Main
{
    public sealed class InicioPresentacionData
    {
        public string Nombre { get; set; }
        public string AvatarId { get; set; }
        public string LeccionId { get; set; }
        public string TituloLeccion { get; set; }
        public bool EsContinuacion { get; set; }
        public bool CurriculoCompletado { get; set; }
        public int LeccionesCompletadas { get; set; }
        public int EscenariosVisitados { get; set; }
        public string ProximoRaTitulo { get; set; }
        public int ProximoRaCompletadas { get; set; }
        public int ProximoRaTotal { get; set; }
    }

    public sealed class IslaMapaPresentacionData
    {
        public string LeccionId { get; set; }
        public string EscenarioRaId { get; set; }
        public int Numero { get; set; }
        public string Titulo { get; set; }
        public string Region { get; set; }
        public bool EsActual { get; set; }
        public Paginas.EstadoIsla Estado { get; set; }
        public bool EsHitoRa => !string.IsNullOrEmpty(EscenarioRaId);
    }

    public sealed class LogroPresentacionData
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Desbloqueado { get; set; }
        public int ProgresoActual { get; set; }
        public int Objetivo { get; set; }
    }
}
