using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    /// <summary>Construye la APK oficial o una APK separada para demostraciones.</summary>
    public static class BuildDemoCompleto
    {
        [Serializable]
        private sealed class KeystoreCredentials
        {
            public string alias;
            public string password;
        }

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
            string nombreBuild = demo ? "DemoCompleto" : "Android";
            string nombreProducto = demo ? "Uchumataqu Demo" : NombreProductoOficial;
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

            bool usarKeystorePersonalizado = PlayerSettings.Android.useCustomKeystore;
            string contrasenaKeystoreAnterior = PlayerSettings.Android.keystorePass;
            string contrasenaClaveAnterior = PlayerSettings.Android.keyaliasPass;
            Directory.CreateDirectory(directorioSalida);
            try
            {
                if (demo) PlayerSettings.Android.useCustomKeystore = false;
                else AplicarCredencialesFirma(rutaProyecto);
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
                        $"La APK {(demo ? "demo" : "oficial")} no se generó. Resultado: {reporte.summary.result}; errores: {reporte.summary.totalErrors}.");

                Debug.Log($"[BuildDemoCompleto] APK lista: {rutaApk} ({reporte.summary.totalSize / (1024f * 1024f):F1} MiB).");
            }
            finally
            {
                PlayerSettings.Android.useCustomKeystore = usarKeystorePersonalizado;
                PlayerSettings.Android.keystorePass = contrasenaKeystoreAnterior;
                PlayerSettings.Android.keyaliasPass = contrasenaClaveAnterior;
                PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, IdentificadorOficial);
                PlayerSettings.productName = NombreProductoOficial;
            }
        }

        private static void AplicarCredencialesFirma(string rutaProyecto)
        {
            string rutaCredenciales = Path.Combine(rutaProyecto, "Builds", "Signing", "credentials.json");
            if (!File.Exists(rutaCredenciales))
                throw new FileNotFoundException("No se encontró el archivo local de credenciales del keystore.", rutaCredenciales);

            KeystoreCredentials credenciales = JsonUtility.FromJson<KeystoreCredentials>(File.ReadAllText(rutaCredenciales));
            if (credenciales == null || string.IsNullOrWhiteSpace(credenciales.alias) || string.IsNullOrEmpty(credenciales.password))
                throw new InvalidDataException("El archivo local de credenciales del keystore no contiene los campos requeridos.");

            PlayerSettings.Android.keyaliasName = credenciales.alias;
            PlayerSettings.Android.keystorePass = credenciales.password;
            PlayerSettings.Android.keyaliasPass = credenciales.password;
        }
    }
}
