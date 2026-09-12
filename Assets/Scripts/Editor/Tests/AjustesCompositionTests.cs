using System.Linq;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
using Assets.Scripts.UI.Main.Paginas;
using NUnit.Framework;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Editor.Tests
{
    public class AjustesCompositionTests
    {
        [Test]
        public void PageAjustes_ConservaComposicionYRectTransformsValidos()
        {
            Scene scene = EditorSceneManager.OpenPreviewScene("Assets/Scenes/02_Main.unity");
            try
            {
                AjustesView view = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<AjustesView>(true))
                    .Single();
                view.gameObject.SetActive(true);
                RectTransform[] rects = view.GetComponentsInChildren<RectTransform>(true);
                RectTransform contenedor = Buscar(rects, "ContenedorAjustes");

                foreach (float volumen in new[] { 0f, 0.25f, 0.5f, 1f })
                {
                    view.Mostrar("Primo", "avatar_01", volumen, true, true);
                    Canvas.ForceUpdateCanvases();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(contenedor);
                    Canvas.ForceUpdateCanvases();
                    Assert.AreEqual(new Vector2(64f, 64f), Buscar(rects, "Handle").rect.size);
                }

                Toggle[] switches = view.GetComponentsInChildren<Toggle>(true);
                view.Mostrar("Primo", "avatar_01", 0.5f, true, true);
                foreach (Toggle item in switches)
                {
                    Assert.AreEqual(-5f, ObtenerKnob(item).anchoredPosition.x, 0.1f);
                    Assert.AreEqual(UIThemeManager.Theme.colorPrimario, ObtenerColorFondo(item));
                }

                view.Mostrar("Primo", "avatar_01", 0.5f, false, false);
                for (int i = 0; i < switches.Length; i++)
                {
                    RectTransform knob = ObtenerKnob(switches[i]);
                    RectTransform fondo = (RectTransform)knob.parent;
                    float posicionInicial = 5f + knob.rect.width - fondo.rect.width;
                    Assert.AreEqual(posicionInicial, knob.anchoredPosition.x, 0.1f);
                    Assert.AreEqual(UIThemeManager.Theme.colorBorde, ObtenerColorFondo(switches[i]));
                    Assert.AreEqual(1f, knob.GetComponent<Image>().canvasRenderer.GetAlpha(), 0.01f);
                    Assert.AreEqual(Color.white, knob.GetComponent<Image>().color);
                    Assert.IsNull(switches[i].graphic);
                }

                RectTransform hero = Buscar(rects, "HeroPerfil");
                RectTransform audio = Buscar(rects, "CardAudio");
                RectTransform experiencia = Buscar(rects, "CardExperiencia");
                RectTransform datos = Buscar(rects, "CardDatos");
                foreach (RectTransform card in new[] { hero, audio, experiencia, datos })
                {
                    LayoutElement layout = card.GetComponent<LayoutElement>();
                    Assert.AreEqual(0f, layout.minWidth);
                    Assert.AreEqual(0f, layout.preferredWidth);
                    Assert.AreEqual(1f, layout.flexibleWidth);
                }
                RectTransform botonReinicio = Buscar(rects, "BtnReiniciarProgreso");
                Assert.Greater(botonReinicio.sizeDelta.x, 0f);
                Assert.Less(botonReinicio.sizeDelta.x, 800f);
                Assert.IsFalse(datos.GetComponent<VerticalLayoutGroup>().childForceExpandWidth);
                Assert.IsFalse(view.GetComponentsInChildren<Transform>(true)
                    .Any(item => item.name == "TxtCreditos"));
                Assert.IsFalse(view.GetComponentsInChildren<TMP_Text>(true)
                    .Any(text => text.GetComponent<ThemedText>() == null));
                TMP_Text textoReinicio = view.GetComponentsInChildren<TMP_Text>(true)
                    .First(text => text.name == "TxtReinicio");
                textoReinicio.ForceMeshUpdate(true, true);
                Assert.IsFalse(textoReinicio.isTextOverflowing);

                foreach (RectTransform rect in rects)
                {
                    foreach (float value in new[] { rect.anchorMin.x, rect.anchorMin.y,
                                 rect.anchorMax.x, rect.anchorMax.y, rect.pivot.x, rect.pivot.y })
                        Assert.IsTrue(EsValorPermitido(value), rect.name);
                    Assert.AreEqual(Vector3.one, rect.localScale, rect.name);
                    Assert.Less(Quaternion.Angle(Quaternion.identity, rect.localRotation), 0.01f, rect.name);
                }
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        private static RectTransform Buscar(RectTransform[] rects, string nombre) =>
            rects.First(item => item.name == nombre);

        private static RectTransform ObtenerKnob(Toggle toggle) =>
            (RectTransform)toggle.transform.Find("SwitchBg/Knob");

        private static Color ObtenerColorFondo(Toggle toggle) =>
            toggle.transform.Find("SwitchBg").GetComponent<Image>().color;

        private static bool EsValorPermitido(float value) =>
            Mathf.Approximately(value, 0f) || Mathf.Approximately(value, 0.5f) ||
            Mathf.Approximately(value, 1f);
    }
}
