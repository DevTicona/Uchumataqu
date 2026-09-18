using UnityEngine;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    internal static class RuntimeActividadUIFactory
    {
        public static void Retirar(GameObject objeto)
        {
            if (objeto == null) return;
            objeto.SetActive(false);
            if (Application.isPlaying) Object.Destroy(objeto);
            else Object.DestroyImmediate(objeto);
        }
    }
}
