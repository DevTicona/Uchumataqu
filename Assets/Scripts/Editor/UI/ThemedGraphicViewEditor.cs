using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Editor
{
    /// <summary>
    /// Aplica el color del tema solo al cambiar el rol del componente.
    /// Cambiar Image.color directamente queda como una decision estatica de la escena.
    /// </summary>
    [CustomEditor(typeof(ThemedGraphicView))]
    public sealed class ThemedGraphicViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            bool cambio = EditorGUI.EndChangeCheck();
            serializedObject.ApplyModifiedProperties();

            if (!cambio) return;

            foreach (Object objetivo in targets)
            {
                var graphic = (ThemedGraphicView)objetivo;
                if (UIThemeController.Theme == null)
                {
                    UIThemeModel tema = AssetDatabase.LoadAssetAtPath<UIThemeModel>(
                        "Assets/Content/UI/UITheme.asset");
                    if (tema != null) UIThemeController.SetTheme(tema);
                }

                graphic.AplicarTema();
                EditorUtility.SetDirty(graphic);
                Image imagen = graphic.GetComponent<Image>();
                if (imagen != null) EditorUtility.SetDirty(imagen);
                if (graphic.gameObject.scene.IsValid())
                    EditorSceneManager.MarkSceneDirty(graphic.gameObject.scene);
            }
        }
    }
}
