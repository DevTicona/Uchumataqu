using System;

namespace Assets.Scripts.UI.Main
{
    public class InicioModel
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
}
