using System.Collections.Generic;

namespace Assets.Scripts.Features.Lecciones.Models
{
    /// <summary>
    /// Representa una actividad del catálogo definido en actividades.json.
    /// Campos nuevos: tipo (estructural|pedagogica), fase (exposicion|bloque|produccion|tema), categoria (palabras|oraciones|ra).
    /// </summary>
    [System.Serializable]
    public class ActividadCatalogoItemData
    {
        public string id;
        public string nombre;
        public string tipo;        // "estructural" | "pedagogica"
        public string fase;        // "exposicion" | "bloque" | "produccion" | "tema"
        public string categoria;   // "palabras" | "oraciones" | "ra"
        public bool requiereAudio;
        public bool requierePLN;
        public List<string> instrucciones;
        public List<string> recordatorios;
        public List<string> feedback;
    }
}
