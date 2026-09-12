using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    [CustomEditor(typeof(UITheme))]
    public sealed class UIThemeEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            if (target is UITheme theme)
                UIThemeManager.SetTheme(theme);
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
            AplicarTemaEnEditor((UITheme)target);
        }

        private static void AplicarTemaEnEditor(UITheme theme)
        {
            UIThemeManager.SetTheme(theme);

            foreach (ThemedText texto in Object.FindObjectsByType<ThemedText>(
                         FindObjectsInactive.Include))
            {
                texto.AplicarTema();
                EditorUtility.SetDirty(texto);
            }

            foreach (ThemedButton boton in Object.FindObjectsByType<ThemedButton>(
                         FindObjectsInactive.Include))
            {
                boton.AplicarTema();
                EditorUtility.SetDirty(boton);
            }

            foreach (ThemedGraphic graphic in Object.FindObjectsByType<ThemedGraphic>(
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
