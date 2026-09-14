using System.Linq;
using Assets.Scripts.UI.Main.Paginas;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Editor.Tests
{
    public class MapaLeccionesCompositionTests
    {
        [Test]
        public void Header_ActualizaCantidadYViewportRecibeArrastre()
        {
            var scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/02_Main.unity");
            try
            {
                var view = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<MapaLeccionesView>(true)).Single();
                var serialized = new SerializedObject(view);
                var cantidad = (TMP_Text)serialized.FindProperty("txtCantidad").objectReferenceValue;
                var etiqueta = (TMP_Text)serialized.FindProperty("txtLeccionesCompletadas").objectReferenceValue;
                Assert.IsNotNull(cantidad);
                Assert.IsNotNull(etiqueta);
                view.MostrarResumen(11, 27);
                Assert.AreEqual("11/27", cantidad.text);
                Assert.AreEqual("Completadas", etiqueta.text);
                view.MostrarResumen(27, 27);
                Assert.AreEqual("27/27", cantidad.text);
                view.MostrarResumen(0, 27);
                Assert.AreEqual("0/27", cantidad.text);
                var scroll = (ScrollRect)serialized.FindProperty("scrollRect").objectReferenceValue;
                Assert.IsNotNull(scroll.viewport);
                var superficie = scroll.viewport.GetComponent<Image>();
                Assert.IsNotNull(superficie);
                Assert.IsTrue(superficie.raycastTarget);
                Assert.AreEqual(0, superficie.color.a);
                Assert.IsTrue(scroll.vertical);
                Assert.IsFalse(scroll.horizontal);
                Assert.AreSame(scroll.viewport, scroll.content.parent);
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }
    }
}
