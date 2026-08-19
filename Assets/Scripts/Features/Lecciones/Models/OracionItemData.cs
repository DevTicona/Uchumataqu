using System.Collections.Generic;

namespace Assets.Scripts.Features.Lecciones.Models
{
    /// <summary>
    /// Representa una oración en Uchumataqu con su estructura de palabras y traducción.
    /// Mapea los objetos dentro del array "items" en oraciones.json.
    /// </summary>
    [System.Serializable]
    public class OracionItemData
    {
        public string id;
        public List<string> estructura;
        public string es;
    }
}
