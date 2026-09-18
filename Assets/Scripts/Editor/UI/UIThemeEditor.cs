using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    [CustomEditor(typeof(UIThemeModel))]
    public sealed class UIThemeEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            if (target is UIThemeModel theme)
                UIThemeController.SetTheme(theme);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();

            if (!EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                return;
            }

            serializedObject.ApplyModifiedProperties();
            AplicarTemaEnEditor((UIThemeModel)target);
        }

        private static void AplicarTemaEnEditor(UIThemeModel theme)
        {
            UIThemeController.SetTheme(theme);

            foreach (ThemedTextView texto in Object.FindObjectsByType<ThemedTextView>(
                         FindObjectsInactive.Include))
            {
                texto.AplicarTema();
                EditorUtility.SetDirty(texto);
            }

            foreach (ThemedButtonView boton in Object.FindObjectsByType<ThemedButtonView>(
                         FindObjectsInactive.Include))
            {
                boton.AplicarTema();
                EditorUtility.SetDirty(boton);
            }

            foreach (ThemedGraphicView graphic in Object.FindObjectsByType<ThemedGraphicView>(
                         FindObjectsInactive.Include))
            {
                graphic.AplicarTema();
                EditorUtility.SetDirty(graphic);
            }

            if (EditorSceneManager.GetActiveScene().IsValid())
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.SetDirty(theme);
            SceneView.RepaintAll();
        }
    }
}
