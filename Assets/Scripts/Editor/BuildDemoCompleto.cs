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
        private const string IdentificadorOficial = "com.devticona.uchumataqu";
        private const string NombreProductoOficial = "Uchumataqu";

        [MenuItem("Uchumataqu/Build Demo Completo Android")]
        public static void CrearApkAndroid()
        {
            Compilar(demo: true);
        }

        [MenuItem("Uchumataqu/Build APK Oficial Android")]
        public static void CrearApkOficialAndroid()
        {
            Compilar(demo: false);
        }

        private static void Compilar(bool demo)
        {
            string rutaProyecto = Directory.GetParent(Application.dataPath)?.FullName
                ?? throw new InvalidOperationException("No se pudo resolver la raíz del proyecto.");
            string nombreBuild = demo ? "DemoCompleto" : "Oficial";
            string nombreProducto = demo ? "Uchumataqu Demo" : "Uchumataqu";
            string identificador = demo ? IdentificadorDemo : IdentificadorOficial;
            string directorioSalida = Path.Combine(rutaProyecto, "Builds", nombreBuild);
            string nombreApk = demo ? "Uchumataqu-Demo-1.0.11.apk" : "Uchumataqu-1.0.11.apk";
            string rutaApk = Path.Combine(directorioSalida, nombreApk);
            string[] escenas = EditorBuildSettings.scenes
                .Where(escena => escena.enabled)
                .Select(escena => escena.path)
                .ToArray();

            if (escenas.Length == 0 || escenas.Any(escena => !File.Exists(Path.Combine(rutaProyecto, escena))))
                throw new InvalidOperationException("La lista de escenas de Android está vacía o contiene rutas inexistentes.");

            Directory.CreateDirectory(directorioSalida);
            try
            {
                // Las APK locales usan la clave de depuración si no hay keystore configurado.
                PlayerSettings.Android.useCustomKeystore = false;
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, identificador);
                PlayerSettings.productName = nombreProducto;

                var opciones = new BuildPlayerOptions
                {
                    scenes = escenas,
                    locationPathName = rutaApk,
                    target = BuildTarget.Android,
                    options = BuildOptions.None,
                    extraScriptingDefines = demo ? new[] { DefineDemo } : Array.Empty<string>()
                };
                BuildReport reporte = BuildPipeline.BuildPlayer(opciones);
                if (reporte.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException(
                        $"La APK demo no se generó. Resultado: {reporte.summary.result}; errores: {reporte.summary.totalErrors}.");

                Debug.Log($"[BuildDemoCompleto] APK lista: {rutaApk} ({reporte.summary.totalSize / (1024f * 1024f):F1} MiB).");
            }
            finally
            {
                PlayerSettings.Android.useCustomKeystore = true;
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, IdentificadorOficial);
                PlayerSettings.productName = NombreProductoOficial;
            }
        }
    }
}
