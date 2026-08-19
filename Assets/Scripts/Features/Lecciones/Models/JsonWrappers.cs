using System.Collections.Generic;

namespace Assets.Scripts.Features.Lecciones.Models
{
    // =========================================
    // PALABRAS
    // =========================================

    /// <summary>
    /// Wrapper raíz para deserializar palabras.json.
    /// Estructura: { "items": [...] }
    /// </summary>
    [System.Serializable]
    public class PalabrasWrapperData
    {
        public List<PalabraItemData> items;
    }

    // =========================================
    // ORACIONES
    // =========================================

    /// <summary>
    /// Wrapper raíz para deserializar oraciones.json.
    /// Estructura: { "items": [...] }
    /// </summary>
    [System.Serializable]
    public class OracionesWrapperData
    {
        public List<OracionItemData> items;
    }

    // =========================================
    // LECCIONES
    // =========================================

    /// <summary>
    /// Wrapper raíz para deserializar lecciones.json.
    /// Estructura: { "items": [...] }
    /// </summary>
    [System.Serializable]
    public class LeccionesWrapperData
    {
        public List<LeccionItemData> items;
    }

    // =========================================
    // TEMAS
    // =========================================

    /// <summary>
    /// Wrapper raíz para deserializar temas.json.
    /// Estructura: { "items": [...] }
    /// </summary>
    [System.Serializable]
    public class TemasWrapperData
    {
        public List<TemaItemData> items;
    }

    // =========================================
    // ACTIVIDADES
    // =========================================

    /// <summary>
    /// Wrapper raíz para deserializar actividades.json.
    /// Estructura: { "items": [...] }
    /// </summary>
    [System.Serializable]
    public class ActividadesWrapperData
    {
        public List<ActividadCatalogoItemData> items;
    }
}
