using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    /// <summary>Construye una APK separada con progreso completo para demostraciones.</summary>
    public static class BuildDemoCompleto
    {
        private const string DefineDemo = "UCHUMATAQU_DEMO_FULL_PROGRESS";
        private const string IdentificadorDemo = "com.devticona.uchumataqu.demo";

        [MenuItem("Uchumataqu/Build Demo Completo Android")]
        public static void CrearApkAndroid()
        {
            string rutaProyecto = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("No se pudo resolver la raíz del proyecto.");
            string directorioSalida = Path.Combine(rutaProyecto, "Builds", "DemoCompleto");
            string rutaApk = Path.Combine(directorioSalida, "Uchumataqu-Demo-1.0.11.apk");
            string identificadorOriginal = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);
            string nombreOriginal = PlayerSettings.productName;
            bool usarKeystorePersonalizadoOriginal = PlayerSettings.Android.useCustomKeystore;
            string[] escenas = EditorBuildSettings.scenes
                .Where(escena => escena.enabled)
                .Select(escena => escena.path)
                .ToArray();

            if (escenas.Length == 0 || escenas.Any(escena => !File.Exists(Path.Combine(rutaProyecto, escena))))
                throw new InvalidOperationException("La lista de escenas de Android está vacía o contiene rutas inexistentes.");

            Directory.CreateDirectory(directorioSalida);
            try
            {
                // La variante de demostración se firma con la clave de depuración local.
                PlayerSettings.Android.useCustomKeystore = false;
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, IdentificadorDemo);
                PlayerSettings.productName = "Uchumataqu Demo";

                var opciones = new BuildPlayerOptions
                {
                    scenes = escenas,
                    locationPathName = rutaApk,
                    target = BuildTarget.Android,
                    options = BuildOptions.None,
                    extraScriptingDefines = new[] { DefineDemo }
                };
                BuildReport reporte = BuildPipeline.BuildPlayer(opciones);
                if (reporte.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException(
                        $"La APK demo no se generó. Resultado: {reporte.summary.result}; errores: {reporte.summary.totalErrors}.");

                Debug.Log($"[BuildDemoCompleto] APK lista: {rutaApk} ({reporte.summary.totalSize / (1024f * 1024f):F1} MiB).");
            }
            finally
            {
                PlayerSettings.Android.useCustomKeystore = usarKeystorePersonalizadoOriginal;
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, identificadorOriginal);
                PlayerSettings.productName = nombreOriginal;
            }
        }
    }
}
