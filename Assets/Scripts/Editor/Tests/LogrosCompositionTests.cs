using System.IO;
using System.Linq;
using Assets.Scripts.UI.Main;
using Assets.Scripts.UI.Main.Paginas;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Editor.Tests
{
    public class LogrosCompositionTests
    {
        [Test]
        public void Galeria_ConservaComposicionYRespetaLimites() => Validar(false);

        public static void Validar(bool capturar)
        {
            Scene scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/02_Main.unity");
            try
            {
                LogrosView source = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<LogrosView>(true)).Single();
                var root = new GameObject("ValidacionLogros", typeof(RectTransform), typeof(Canvas));
                SceneManager.MoveGameObjectToScene(root, scene);
                var canvas = root.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                var view = Object.Instantiate(source, root.transform);
                foreach (GameObject original in scene.GetRootGameObjects())
                    if (original != root) original.SetActive(false);
                view.gameObject.SetActive(true);
                var page = (RectTransform)view.transform;
                page.anchorMin = Vector2.zero;
                page.anchorMax = Vector2.one;
                page.offsetMin = page.offsetMax = Vector2.zero;
                var serialized = new SerializedObject(view);
                var gallery = (Transform)serialized.FindProperty("galeriaObtenidos").objectReferenceValue;
                Assert.IsNotNull(gallery);
                RectTransform[] originales = gallery.GetComponentsInChildren<RectTransform>(true);
                var data = MainService.CrearLogros(null);

                foreach (Vector2 size in new[] { new Vector2(900, 1600), new Vector2(1080, 2340) })
                {
                    ((RectTransform)root.transform).sizeDelta = size;
                    for (int count = 0; count <= data.Count; count++)
                    {
                        for (int i = 0; i < data.Count; i++)
                        {
                            data[i].Desbloqueado = i < count;
                            data[i].ProgresoActual = i < count
                                ? data[i].Objetivo
                                : i == count && data[i].Objetivo > 1 ? data[i].Objetivo / 2 : 0;
                        }
                        view.Mostrar(data);
                        Canvas.ForceUpdateCanvases();
                        LayoutRebuilder.ForceRebuildLayoutImmediate(page);
                        Canvas.ForceUpdateCanvases();
                        CollectionAssert.AreEqual(originales, gallery.GetComponentsInChildren<RectTransform>(true),
                            "La galería debe reutilizar la composición de escena.");
                        foreach (Image relleno in view.GetComponentsInChildren<Image>(true)
                                     .Where(image => image.name == "Relleno"))
                        {
                            RectTransform rect = relleno.rectTransform;
                            RectTransform fondo = (RectTransform)rect.parent;
                            Assert.AreEqual(Image.Type.Sliced, relleno.type);
                            Assert.Greater(relleno.sprite.border.x, 0f);
                            Assert.AreEqual(Vector2.zero, rect.anchorMin);
                            Assert.AreEqual(new Vector2(0f, 1f), rect.anchorMax);
                            Assert.AreEqual(new Vector2(0f, 0.5f), rect.pivot);
                            Assert.AreEqual(fondo.rect.width * relleno.fillAmount, rect.rect.width, 0.5f);
                            Assert.AreEqual(relleno.fillAmount > 0f, relleno.enabled);
                        }
                        int visible = 0;
                        foreach (Transform row in gallery)
                        foreach (Transform cell in row)
                        {
                            var card = (RectTransform)cell.GetChild(0);
                            if (!card.gameObject.activeInHierarchy) continue;
                            visible++;
                            DentroDe(card, (RectTransform)gallery);
                            foreach (RectTransform child in card.GetComponentsInChildren<RectTransform>())
                                if (child != card) DentroDe(child, card);
                            foreach (TMP_Text text in card.GetComponentsInChildren<TMP_Text>())
                            {
                                text.ForceMeshUpdate();
                                Assert.IsFalse(text.isTextOverflowing, text.text);
                            }
                        }
                        Assert.AreEqual(count, visible);
                        foreach (RectTransform rect in view.GetComponentsInChildren<RectTransform>(true))
                        {
                            foreach (float value in new[] { rect.anchorMin.x, rect.anchorMin.y, rect.anchorMax.x,
                                         rect.anchorMax.y, rect.pivot.x, rect.pivot.y })
                                Assert.IsTrue(value == 0f || value == 0.5f || value == 1f, rect.name);
                            Assert.AreEqual(Vector3.one, rect.localScale, rect.name);
                            Assert.Less(Quaternion.Angle(Quaternion.identity, rect.localRotation), 0.01f, rect.name);
                        }
                        if (capturar && count == 2) Capturar(root, scene, size);
                    }
                }
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static void DentroDe(RectTransform child, RectTransform parent)
        {
            var corners = new Vector3[4];
            child.GetWorldCorners(corners);
            foreach (Vector3 corner in corners)
            {
                Vector3 local = parent.InverseTransformPoint(corner);
                Assert.GreaterOrEqual(local.x, parent.rect.xMin - 1f, child.name);
                Assert.LessOrEqual(local.x, parent.rect.xMax + 1f, child.name);
                Assert.GreaterOrEqual(local.y, parent.rect.yMin - 1f, child.name);
                Assert.LessOrEqual(local.y, parent.rect.yMax + 1f, child.name);
            }
        }

        private static void Capturar(GameObject root, Scene scene, Vector2 size)
        {
            var cameraObject = new GameObject("CamaraValidacion", typeof(Camera));
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.overrideSceneCullingMask = EditorSceneManager.GetSceneCullingMask(scene);
            camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            camera.orthographicSize = size.y / 2f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(1f, 0.72f, 0.24f);
            var texture = new RenderTexture((int)size.x / 2, (int)size.y / 2, 24);
            var image = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            RenderTexture previous = RenderTexture.active;
            try
            {
                camera.targetTexture = texture;
                root.GetComponent<Canvas>().worldCamera = camera;
                camera.Render();
                RenderTexture.active = texture;
                image.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
                image.Apply();
                Directory.CreateDirectory("Temp/LogrosQA");
                File.WriteAllBytes($"Temp/LogrosQA/logros_{size.x}x{size.y}.png", image.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = previous;
                camera.targetTexture = null;
                Object.DestroyImmediate(image);
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
