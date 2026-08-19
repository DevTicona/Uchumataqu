using UnityEngine;
using System;
using Assets.Scripts.Core.Services;

namespace Assets.Scripts.Core.UI.Theme
{
    public static class UIThemeManager
    {
        private static UITheme _theme;

        // Evento para notificar cambios
        public static event Action OnThemeChanged;

        public static UITheme Theme => _theme;

        /// <summary>
        /// Carga el tema usando AddressablesService
        /// </summary>
        public static async void CargarTema(string key = "UITheme")
        {
            try
            {
                var tema = await AddressablesService.CargarAssetAsync<UITheme>(key);
                _theme = tema;
                OnThemeChanged?.Invoke();
                Debug.Log("[UIThemeManager] Tema cargado correctamente.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIThemeManager] Error cargando UITheme: {ex.Message}");
            }
        }

        /// <summary>
        /// Permite cambiar el tema en runtime
        /// </summary>
        public static void SetTheme(UITheme nuevoTema)
        {
            _theme = nuevoTema;
            OnThemeChanged?.Invoke();
        }
    }
}