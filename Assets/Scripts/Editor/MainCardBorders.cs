#if UNITY_EDITOR
using System;
using System.Linq;
using Assets.Scripts.Core.UI.Components;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Editor
{
    /// <summary>Añade marcos decorativos sin intervenir en el layout de las tarjetas.</summary>
    public static class MainCardBorders
    {
        private const string SpritePath = "Assets/Content/UI/Base/aro_cuadrado_solido.png";

        public static int Aplicar(Scene scene)
        {
            if (Application.isPlaying || scene.path != "Assets/Scenes/02_Main.unity")
                throw new InvalidOperationException("Los bordes se actualizan en 02_Main fuera de Play Mode.");
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (sprite == null) throw new InvalidOperationException("Falta el sprite de borde.");
            Transform[] cards = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .Where(t => t.name.StartsWith("Card", StringComparison.Ordinal) || t.name == "HeroCard")
                .ToArray();
            int cantidad = 0;
            foreach (Transform card in cards)
            {
                Transform superficie = card.GetComponent<Image>() != null ? card : card.Find("FondoPanel");
                if (superficie == null) continue;
                Transform marco = superficie.Find("Marco");
                // Los marcos existentes pertenecen al diseño configurado en el editor.
                if (marco != null) continue;
                var objeto = new GameObject("Marco", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                Undo.RegisterCreatedObjectUndo(objeto, "Añadir borde a tarjeta");
                objeto.layer = superficie.gameObject.layer;
                objeto.transform.SetParent(superficie, false);
                marco = objeto.transform;
                Undo.RecordObject(marco.GetComponent<RectTransform>(), "Configurar borde");
                RectTransform rect = (RectTransform)marco;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
                marco.SetAsLastSibling();
                LayoutElement layout = marco.GetComponent<LayoutElement>() ?? Undo.AddComponent<LayoutElement>(marco.gameObject);
                layout.ignoreLayout = true;
                Image imagen = marco.GetComponent<Image>();
                Undo.RecordObject(imagen, "Configurar borde");
                imagen.sprite = sprite;
                imagen.type = Image.Type.Sliced;
                imagen.fillCenter = false;
                imagen.raycastTarget = false;
                imagen.pixelsPerUnitMultiplier = 1f;
                ThemedGraphic themed = marco.GetComponent<ThemedGraphic>() ?? Undo.AddComponent<ThemedGraphic>(marco.gameObject);
                themed.Rol = ThemedGraphic.RolColor.Borde;
                cantidad++;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            return cantidad;
        }
    }
}
#endif
