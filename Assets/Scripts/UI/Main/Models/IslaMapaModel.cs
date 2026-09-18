using System;

namespace Assets.Scripts.UI.Main
{
    public class IslaMapaModel
    {
        public string LeccionId { get; set; }
        public string EscenarioRaId { get; set; }
        public int Numero { get; set; }
        public string Titulo { get; set; }
        public string Region { get; set; }
        public string TituloSeccion { get; set; }
        public bool EsActual { get; set; }
        public EstadoIsla Estado { get; set; }
        public bool EsHitoRa => !string.IsNullOrEmpty(EscenarioRaId);
    }
}
