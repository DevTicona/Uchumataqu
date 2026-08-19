namespace Assets.Scripts.Features.Lecciones.Models
{
    /// <summary>
    /// Representa un tema individual dentro del catálogo de temas en temas.json.
    /// Campos: id, nombre, orden, ra_id (referencia a la experiencia AR).
    /// </summary>
    [System.Serializable]
    public class TemaItemData
    {
        public string id;
        public string nombre;
        public int orden;
        public string ra_id;
    }
}
