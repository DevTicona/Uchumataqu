using System;

namespace Assets.Scripts.UI.Main
{
    public class InicioData
    {
        public string Nombre { get; set; }
        public string AvatarId { get; set; }
        public string LeccionId { get; set; }
        public string TituloLeccion { get; set; }
        public int PorcentajeLeccionActual { get; set; }
        public bool EsContinuacion { get; set; }
        public bool CurriculoCompletado { get; set; }
        public int LeccionesCompletadas { get; set; }
        public int EscenariosVisitados { get; set; }
        public string ProximoRaTitulo { get; set; }
        public int ProximoRaCompletadas { get; set; }
        public int ProximoRaTotal { get; set; }
    }

    public class IslaMapaData
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

    public class LogroData
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string IconoId { get; set; }
        public bool Desbloqueado { get; set; }
        public int ProgresoActual { get; set; }
        public int Objetivo { get; set; }
    }

    // Alias retrocompatibles para migraciones y tests antiguos
    [Obsolete("Usar InicioData en su lugar.")]
    public class InicioPresentacionData : InicioData { }

    [Obsolete("Usar IslaMapaData en su lugar.")]
    public class IslaMapaPresentacionData : IslaMapaData { }

    [Obsolete("Usar LogroData en su lugar.")]
    public class LogroPresentacionData : LogroData { }
}
