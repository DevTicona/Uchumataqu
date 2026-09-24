using System;
using UnityEngine;

namespace Assets.Scripts.Core.UI.Theme
{
    public static class UIThemeController
    {
        private static UIThemeModel _theme;

        public static event Action OnThemeChanged;

        public static UIThemeModel Theme
        {
            get
            {
#if UNITY_EDITOR
                if (_theme == null)
                {
                    _theme = UnityEditor.AssetDatabase.LoadAssetAtPath<UIThemeModel>(
                        "Assets/Content/UI/UITheme.asset");
                }
#endif
                return _theme;
            }
        }

        public static void SetTheme(UIThemeModel nuevoTema)
        {
            _theme = nuevoTema;
            OnThemeChanged?.Invoke();
        }

        public static float EscalarTexto(float tamano)
        {
            float escala = Theme != null ? Theme.escalaTextoGlobal : 1f;
            return tamano * Mathf.Max(1f, escala);
        }
    }
}
