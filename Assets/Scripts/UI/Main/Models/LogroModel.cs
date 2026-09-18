using System;

namespace Assets.Scripts.UI.Main
{
    public class LogroModel
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public string IconoId { get; set; }
        public bool Desbloqueado { get; set; }
        public int ProgresoActual { get; set; }
        public int Objetivo { get; set; }
    }
}
