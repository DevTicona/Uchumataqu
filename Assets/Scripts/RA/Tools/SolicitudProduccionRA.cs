using UnityEngine;
#if UNITY_EDITOR
using System;
using UnityEditor;
#endif

namespace Assets.Scripts.RA
{
    /// <summary>Solicitud explícita de una herramienta de producción; sus assets viven bajo Editor.</summary>

    public sealed class SolicitudProduccionRA : MonoBehaviour
    {
#if UNITY_EDITOR
        public string Menu
        {
            get => string.Empty;
            set
            {
                if (string.IsNullOrEmpty(value) || !value.StartsWith("Uchumataqu/RA/", StringComparison.Ordinal))
                    throw new ArgumentException("La operación debe pertenecer al menú de producción RA.");
                string ruta = value;

                EditorApplication.CallbackFunction ejecutar = null;
                ejecutar = () =>
                {
                    EditorApplication.update -= ejecutar;
                    if (!EditorApplication.ExecuteMenuItem(ruta))
                        Debug.LogError("[RA_DESARROLLO] No se pudo ejecutar " + ruta);
                };
                EditorApplication.update += ejecutar;

            }
        }
#endif
    }
}
