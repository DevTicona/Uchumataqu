using System.Collections.Generic;

namespace Assets.Scripts.Features.Lecciones.Models
{
    /// <summary>
    /// Representa una lección individual dentro de la lista plana de lecciones.json.
    /// Campos: id, id_tema (referencia al tema), nombre, orden, palabras_base[], oraciones_ids[].
    /// </summary>
    [System.Serializable]
    public class LeccionItemData
    {
        public string id;
        public string id_tema;
        public string nombre;
        public int orden;
        public List<string> palabras_base;
        public List<string> oraciones_ids;
    }
}
