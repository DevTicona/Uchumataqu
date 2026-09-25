using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    internal static class RuntimeActividadUIFactory
    {
        public static GameObject Instanciar(GameObject prefab, Transform parent)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying && PrefabUtility.IsPartOfPrefabAsset(prefab))
                return (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
#endif
            return Object.Instantiate(prefab, parent, false);
        }

        public static void Retirar(GameObject objeto)
        {
            if (objeto == null) return;
            objeto.SetActive(false);
            if (Application.isPlaying) Object.Destroy(objeto);
            else Object.DestroyImmediate(objeto);
        }
    }
}
