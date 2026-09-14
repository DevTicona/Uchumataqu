using System.Linq;
using Assets.Scripts.UI.Main;
using Assets.Scripts.UI.Main.Paginas;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Editor.Tests
{
    public class LogrosCompositionTests
    {
        [Test]
        public void UIFija_TieneReferenciasSerializadasYNoEntradasAddressables()
        {
            var settings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
            foreach (var entrada in settings.groups.Where(g => g != null).SelectMany(g => g.entries))
                Assert.IsFalse(AssetDatabase.GUIDToAssetPath(entrada.guid).StartsWith("Assets/Content/UI/"),
                    "La UI fija debe estar asignada por Inspector: " + entrada.address);
            var catalogo = AssetDatabase.LoadAssetAtPath<CatalogoIconosLogros>("Assets/Content/UI/CatalogoIconosLogros.asset");
            Assert.IsNotNull(catalogo);
            foreach (var logro in MainService.CrearLogros(null))
                Assert.IsNotNull(catalogo.ObtenerSprite(logro.IconoId), logro.IconoId);
            foreach (string nombre in new[] { "LogroObtenido", "LogroHorizontal" })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Content/Prefabs/Logros/{nombre}.prefab");
                var serialized = new SerializedObject(prefab.GetComponent<LogroItemView>());
                Assert.AreSame(catalogo, serialized.FindProperty("catalogoIconos").objectReferenceValue);
            }
            var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/00_Bootstrap.unity");
            try
            {
                var initializer = scene.GetRootGameObjects().SelectMany(r =>
                    r.GetComponentsInChildren<Assets.Scripts.Core.Sistema.AppInitializer>(true)).Single();
                Assert.IsNotNull(new SerializedObject(initializer).FindProperty("temaUI").objectReferenceValue);
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        [Test]
        public void Bordes_RespetanElGrosorDelPrefabAlActualizarDatos()
        {
            foreach (string nombre in new[] { "LogroObtenido", "LogroHorizontal" })
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Content/Prefabs/Logros/{nombre}.prefab");
                GameObject instancia = Object.Instantiate(prefab);
                try
                {
                    var tarjeta = instancia.GetComponent<LogroItemView>();
                    var marco = instancia.transform.Find("Marco").GetComponent<UnityEngine.UI.Image>();
                    Assert.IsNotNull(marco.sprite);
                    Assert.IsFalse(marco.raycastTarget);
                    marco.pixelsPerUnitMultiplier = 1.5f;
                    foreach (LogroItemView.EstadoLogro estado in System.Enum.GetValues(typeof(LogroItemView.EstadoLogro)))
                    {
                        tarjeta.Configurar(MainService.CrearLogros(null)[0], estado);
                        Assert.AreEqual(1.5f, marco.pixelsPerUnitMultiplier, "Los datos no deben sustituir el diseño del prefab.");
                    }
                }
                finally { Object.DestroyImmediate(instancia); }
            }
        }

        [Test]
        public void Logros_UsaUnaTarjetaReutilizableParaTodasLasSecciones()
        {
            Validar(false);
        }

        public static void Validar(bool capturar)
        {
            Scene scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/02_Main.unity");
            try
            {
                LogrosView source = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<LogrosView>(true)).Single();
                var root = new GameObject("ValidacionLogros", typeof(RectTransform), typeof(Canvas));
                SceneManager.MoveGameObjectToScene(root, scene);
                LogrosView view = Object.Instantiate(source, root.transform);
                foreach (GameObject original in scene.GetRootGameObjects()) if (original != root) original.SetActive(false);
                view.gameObject.SetActive(true);

                SerializedObject serialized = new(view);
                Assert.IsNotNull(serialized.FindProperty("prefabLogroObtenido").objectReferenceValue);
                Assert.IsNotNull(serialized.FindProperty("prefabLogroHorizontal").objectReferenceValue);
                var datos = MainService.CrearLogros(null);
                view.Mostrar(datos);
                Assert.AreEqual(datos.Count, view.GetComponentsInChildren<LogroItemView>(true)
                    .Count(tarjeta => tarjeta.gameObject.activeInHierarchy));
                Transform contenedor = (Transform)serialized.FindProperty("contenedorLogros").objectReferenceValue;
                Assert.IsNull(contenedor.Find("GaleriaObtenidos"));
                Assert.IsFalse(contenedor.Cast<Transform>().Any(hijo => hijo.name.StartsWith("Logro_")));
                LogroItemView[] tarjetas = view.GetComponentsInChildren<LogroItemView>(true);
                view.Mostrar(datos);
                CollectionAssert.AreEqual(tarjetas, view.GetComponentsInChildren<LogroItemView>(true),
                    "Una actualización debe reutilizar las tarjetas existentes.");
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}
