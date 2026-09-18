#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    [InitializeOnLoad]
    public static class AddressablesCatalogSync
    {
        private const string MenuPath = "Uchumataqu/Contenido/Sincronizar Addressables";

        private static readonly HashSet<string> ExtensionesPermitidas = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".asset", ".fbx", ".json", ".mat", ".otf", ".png", ".prefab", ".ttf", ".wav"
        };

        static AddressablesCatalogSync()
        {
            EditorApplication.delayCall += SincronizarSilenciosamente;
        }

        [MenuItem(MenuPath)]
        public static void SincronizarDesdeMenu()
        {
            int cantidad = Sincronizar();
            Debug.Log($"[Addressables] Catálogo sincronizado: {cantidad} recursos administrados.");
        }

        public static int Sincronizar()
        {

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                throw new InvalidOperationException("No se encontró la configuración de Addressables.");
            settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;

            AddressableAssetGroup datos = ObtenerOCrearGrupo(settings, "Global_Data");
            AddressableAssetGroup audios = ObtenerOCrearGrupo(settings, "Audios");
            AddressableAssetGroup imagenes = ObtenerOCrearGrupo(settings, "Imagenes");
            AddressableAssetGroup prefabs = ObtenerOCrearGrupo(settings, "Prefabs");
            AddressableAssetGroup ra = ObtenerOCrearGrupo(settings, "RA");
            AddressableAssetGroup compartidos = ObtenerOCrearGrupo(settings, "Shared_Dependencies");

            int cantidad = 0;
            cantidad += SincronizarCarpeta(settings, datos, "Assets/Content/Data", CrearDireccionDato, "global_data");
            cantidad += SincronizarCarpeta(settings, audios, "Assets/Content/Audio/Palabras", p => Direccion(p, "audio_palabra"), "audio_palabras");
            cantidad += SincronizarCarpeta(settings, audios, "Assets/Content/Audio/Oraciones", p => Direccion(p, "audio_oracion", conservarMayusculas: true), "audio_oraciones");
            // Los avatares fijos se gestionan mediante CatalogoAvatares.asset (ScriptableObject serializado por Inspector)
            cantidad += SincronizarCarpeta(settings, imagenes, "Assets/Content/Imagenes/Islas", p => Direccion(p, "imagen/isla"), "islands");
            cantidad += SincronizarCarpeta(settings, prefabs, "Assets/Content/Prefabs", CrearDireccionPrefab, "prefab");
            cantidad += SincronizarCarpeta(settings, ra, "Assets/Content/Modelos3D", CrearDireccionModelo, "ra");
            cantidad += SincronizarArchivo(
                settings,
                compartidos,
                "Assets/TextMesh Pro/Shaders/TMP_SDF-Mobile.shader",
                "shared/tmp_sdf_mobile",
                "shared_dependency");

            EliminarEntradasInexistentes(settings);
            EliminarUIFija(settings);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            AssetDatabase.SaveAssets();
            return cantidad;
        }

        private static void SincronizarSilenciosamente()
        {
            // Un reload de dominio también ocurre al entrar en Play Mode. No se deben
            // reimportar ni guardar assets mientras la aplicación está arrancando.
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                return;

            try
            {
                Sincronizar();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Addressables] No se pudo sincronizar el catálogo: {ex.Message}");
            }
        }

        private static AddressableAssetGroup ObtenerOCrearGrupo(AddressableAssetSettings settings, string nombre)
        {
            AddressableAssetGroup grupo = settings.FindGroup(nombre);
            return grupo ?? settings.CreateGroup(
                nombre,
                false,
                false,
                false,
                null,
                typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema));
        }

        private static int SincronizarCarpeta(
            AddressableAssetSettings settings,
            AddressableAssetGroup grupo,
            string carpeta,
            Func<string, string> crearDireccion,
            params string[] etiquetas)
        {
            if (!AssetDatabase.IsValidFolder(carpeta)) return 0;

            int cantidad = 0;
            foreach (string guid in AssetDatabase.FindAssets(string.Empty, new[] { carpeta }))
            {
                string ruta = AssetDatabase.GUIDToAssetPath(guid);
                if (!EsRecursoValido(ruta)) continue;

                ConfigurarEntrada(settings, grupo, guid, crearDireccion(ruta), etiquetas);
                cantidad++;
            }
            return cantidad;
        }

        private static int SincronizarArchivo(
            AddressableAssetSettings settings,
            AddressableAssetGroup grupo,
            string ruta,
            string direccion,
            params string[] etiquetas)
        {
            string guid = AssetDatabase.AssetPathToGUID(ruta);
            if (string.IsNullOrEmpty(guid)) return 0;
            ConfigurarEntrada(settings, grupo, guid, direccion, etiquetas);
            return 1;
        }

        private static void ConfigurarEntrada(
            AddressableAssetSettings settings,
            AddressableAssetGroup grupo,
            string guid,
            string direccion,
            IReadOnlyCollection<string> etiquetas)
        {
            AddressableAssetEntry entrada = settings.CreateOrMoveEntry(guid, grupo, false, false);
            entrada.address = direccion;
            foreach (string existente in entrada.labels.ToArray())
                entrada.SetLabel(existente, false, false, false);
            foreach (string etiqueta in etiquetas)
            {
                settings.AddLabel(etiqueta, false);
                entrada.SetLabel(etiqueta, true, true, false);
            }
        }

        private static void EliminarEntradasInexistentes(AddressableAssetSettings settings)
        {
            var guids = settings.groups
                .Where(g => g != null)
                .SelectMany(g => g.entries)
                .Where(e => e != null && string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(e.guid)))
                .Select(e => e.guid)
                .Distinct()
                .ToArray();

            foreach (string guid in guids)
                settings.RemoveAssetEntry(guid, false);
        }

        private static void EliminarUIFija(AddressableAssetSettings settings)
        {
            var guids = settings.groups.Where(g => g != null).SelectMany(g => g.entries)
                .Where(e => e != null && EsUIFija(AssetDatabase.GUIDToAssetPath(e.guid)))
                .Select(e => e.guid).ToArray();
            foreach (string guid in guids) settings.RemoveAssetEntry(guid, false);
        }

        private static bool EsUIFija(string ruta) =>
            ruta.StartsWith("Assets/Content/UI/", StringComparison.OrdinalIgnoreCase) ||
            ruta.StartsWith("Assets/Content/Imagenes/Fondos/", StringComparison.OrdinalIgnoreCase) ||
            ruta.StartsWith("Assets/Content/Imagenes/Marca/", StringComparison.OrdinalIgnoreCase) ||
            ruta.StartsWith("Assets/Content/Imagenes/Avatares/", StringComparison.OrdinalIgnoreCase);

        private static bool EsRecursoValido(string ruta)
        {
            if (string.IsNullOrEmpty(ruta) || AssetDatabase.IsValidFolder(ruta)) return false;
            return ExtensionesPermitidas.Contains(Path.GetExtension(ruta));
        }

        private static string CrearDireccionDato(string ruta)
        {
            string nombre = Normalizar(Path.GetFileNameWithoutExtension(ruta));
            return nombre == "lecciones_compiladas" ? "json_lecciones" : $"json_{nombre}";
        }

        private static string CrearDireccionPrefab(string ruta)
        {
            string nombre = Normalizar(Path.GetFileNameWithoutExtension(ruta));
            return nombre switch
            {
                "islaleccion" => "prefab_isla_leccion",
                _ => $"prefab/{nombre}"
            };
        }

        private static string CrearDireccionModelo(string ruta)
        {
            string nombre = Normalizar(Path.GetFileNameWithoutExtension(ruta));
            return nombre == "ra_saludos_personaje_base" ? "personaje_base" : $"ra/modelo/{nombre}";
        }

        private static string Direccion(string ruta, string prefijo, bool conservarMayusculas = false)
        {
            string nombre = Path.GetFileNameWithoutExtension(ruta);
            return $"{prefijo}/{(conservarMayusculas ? nombre : Normalizar(nombre))}";
        }

        private static string Normalizar(string valor) =>
            valor.Trim().Replace(" ", "_").Replace("-", "_").ToLowerInvariant();
    }
}
#endif
