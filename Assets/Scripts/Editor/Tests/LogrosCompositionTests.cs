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
