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
            ConfigurarImportadoresDeSprites();

            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
                throw new InvalidOperationException("No se encontró la configuración de Addressables.");
            settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;

            AddressableAssetGroup datos = ObtenerOCrearGrupo(settings, "Global_Data");
            AddressableAssetGroup audios = ObtenerOCrearGrupo(settings, "Audios");
            AddressableAssetGroup ui = ObtenerOCrearGrupo(settings, "UI");
            AddressableAssetGroup imagenes = ObtenerOCrearGrupo(settings, "Imagenes");
            AddressableAssetGroup prefabs = ObtenerOCrearGrupo(settings, "Prefabs");
            AddressableAssetGroup ra = ObtenerOCrearGrupo(settings, "RA");

            int cantidad = 0;
            cantidad += SincronizarCarpeta(settings, datos, "Assets/Content/Data", CrearDireccionDato, "global_data");
            cantidad += SincronizarCarpeta(settings, audios, "Assets/Content/Audio/Palabras", p => Direccion(p, "audio_palabra"), "audio_palabras");
            cantidad += SincronizarCarpeta(settings, audios, "Assets/Content/Audio/Oraciones", p => Direccion(p, "audio_oracion", conservarMayusculas: true), "audio_oraciones");
            cantidad += SincronizarCarpeta(settings, ui, "Assets/Content/UI/Base", p => Direccion(p, "ui/base"), "ui", "ui_base");
            cantidad += SincronizarCarpeta(settings, ui, "Assets/Content/UI/Effects", p => Direccion(p, "ui/effect"), "ui", "ui_effects");
            cantidad += SincronizarCarpeta(settings, ui, "Assets/Content/UI/Fonts", p => Direccion(p, "ui/font"), "ui", "ui_fonts");
            cantidad += SincronizarCarpeta(settings, ui, "Assets/Content/UI/Icons", CrearDireccionIcono, "ui", "ui_icons");
            cantidad += SincronizarArchivo(settings, ui, "Assets/Content/UI/UITheme.asset", "UITheme", "ui_theme");
            cantidad += SincronizarCarpeta(settings, imagenes, "Assets/Content/Imagenes/Avatares", p => Direccion(p, "avatar"), "avatars");
            cantidad += SincronizarCarpeta(settings, imagenes, "Assets/Content/Imagenes/Fondos", p => Direccion(p, "imagen/fondo"), "backgrounds");
            cantidad += SincronizarCarpeta(settings, imagenes, "Assets/Content/Imagenes/Marca", p => Direccion(p, "imagen/marca"), "branding");
            cantidad += SincronizarCarpeta(settings, imagenes, "Assets/Content/Imagenes/Islas", p => Direccion(p, "imagen/isla"), "islands");
            cantidad += SincronizarCarpeta(settings, prefabs, "Assets/Content/Prefabs", CrearDireccionPrefab, "prefab");
            cantidad += SincronizarCarpeta(settings, ra, "Assets/Content/Modelos3D", CrearDireccionModelo, "ra");

            EliminarEntradasInexistentes(settings);
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.BatchModification, null, true, true);
            AssetDatabase.SaveAssets();
            return cantidad;
        }

        private static void SincronizarSilenciosamente()
        {
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

        private static void ConfigurarImportadoresDeSprites()
        {
            var bordes = new Dictionary<string, Vector4>(StringComparer.OrdinalIgnoreCase)
            {
                ["ui_panel_rounded_16"] = new Vector4(24, 24, 24, 24),
                ["ui_button_rounded_14"] = new Vector4(20, 20, 20, 20),
                ["ui_input_rounded_12"] = new Vector4(18, 18, 18, 18),
                ["ui_pill"] = new Vector4(32, 32, 32, 32),
                ["ui_progress_rounded"] = new Vector4(15, 15, 15, 15),
                ["ui_card_shadow"] = new Vector4(40, 44, 40, 40),
                ["ui_modal_shadow"] = new Vector4(48, 52, 48, 44)
            };

            ConfigurarCarpetaDeSprites("Assets/Content/UI", bordes, 512);
            ConfigurarCarpetaDeSprites("Assets/Content/Imagenes", bordes, 2048);
        }

        private static void ConfigurarCarpetaDeSprites(
            string carpeta,
            IReadOnlyDictionary<string, Vector4> bordes,
            int tamanoMaximo)
        {
            if (!AssetDatabase.IsValidFolder(carpeta)) return;

            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { carpeta }))
            {
                string ruta = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.Equals(Path.GetExtension(ruta), ".png", StringComparison.OrdinalIgnoreCase)) continue;
                if (!(AssetImporter.GetAtPath(ruta) is TextureImporter importador)) continue;

                string nombre = Path.GetFileNameWithoutExtension(ruta);
                Vector4 borde = bordes.TryGetValue(nombre, out Vector4 valor) ? valor : Vector4.zero;
                SpriteImportMode modoDestino = importador.spriteImportMode == SpriteImportMode.Multiple
                    ? SpriteImportMode.Multiple
                    : SpriteImportMode.Single;
                var ajustes = new TextureImporterSettings();
                importador.ReadTextureSettings(ajustes);
                bool cambio = importador.textureType != TextureImporterType.Sprite
                    || importador.spriteImportMode != modoDestino
                    || importador.mipmapEnabled
                    || !importador.alphaIsTransparency
                    || importador.wrapMode != TextureWrapMode.Clamp
                    || importador.npotScale != TextureImporterNPOTScale.None
                    || ajustes.spriteMeshType != SpriteMeshType.FullRect
                    || ajustes.spriteGenerateFallbackPhysicsShape
                    || importador.maxTextureSize != tamanoMaximo
                    || importador.spriteBorder != borde;

                if (!cambio) continue;
                ajustes.spriteMeshType = SpriteMeshType.FullRect;
                ajustes.spriteGenerateFallbackPhysicsShape = false;
                importador.SetTextureSettings(ajustes);
                importador.textureType = TextureImporterType.Sprite;
                importador.spriteImportMode = modoDestino;
                importador.mipmapEnabled = false;
                importador.alphaIsTransparency = true;
                importador.wrapMode = TextureWrapMode.Clamp;
                importador.npotScale = TextureImporterNPOTScale.None;
                importador.maxTextureSize = tamanoMaximo;
                importador.spriteBorder = borde;
                importador.SaveAndReimport();
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

        private static string CrearDireccionIcono(string ruta)
        {
            string categoria = Normalizar(new DirectoryInfo(Path.GetDirectoryName(ruta) ?? string.Empty).Name);
            return $"ui/icon/{categoria}/{Normalizar(Path.GetFileNameWithoutExtension(ruta))}";
        }

        private static string CrearDireccionPrefab(string ruta)
        {
            string nombre = Normalizar(Path.GetFileNameWithoutExtension(ruta));
            return nombre switch
            {
                "boton" => "prefab_boton",
                "islaleccion" => "prefab_isla_leccion",
                "wordcard" => "prefab_word_card",
                "actividademparejamiento" => "actividad_emparejamiento",
                "actividadseleccion" => "actividad_seleccion",
                "pln_palabra" => "pln_palabra",
                "nubedialogo" => "prefab_nube_dialogo",
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
