using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections.Generic;
using Assets.Scripts.Core.UI.Components;

namespace Assets.Scripts.Core.UI.Theme
{
    public static class UIThemeController
    {
        private const float EscalaTextoPredeterminada = 1.15f;

        private static UIThemeModel _theme;
        private static readonly Dictionary<TMP_Text, DatosTexto> TextosRegistrados = new Dictionary<TMP_Text, DatosTexto>();
        private static bool _escuchaEscenasActiva;

        private readonly struct DatosTexto
        {
            public readonly TMP_Text Texto;
            public readonly float Tamano;
            public readonly float Minimo;
            public readonly float Maximo;

            public DatosTexto(TMP_Text texto, float divisor = 1f)
            {
                Texto = texto;
                Tamano = texto.fontSize / divisor;
                Minimo = texto.fontSizeMin / divisor;
                Maximo = texto.fontSizeMax / divisor;
            }
        }

        // Evento para notificar cambios
        public static event Action OnThemeChanged;

        public static UIThemeModel Theme
        {
            get
            {
#if UNITY_EDITOR
                if (_theme == null)
                {
                    _theme = UnityEditor.AssetDatabase.LoadAssetAtPath<UIThemeModel>("Assets/Content/UI/UITheme.asset");
                }
#endif
                return _theme;
            }
        }

        /// <summary>
        /// Permite cambiar el tema en runtime
        /// </summary>
        public static void SetTheme(UIThemeModel nuevoTema)
        {
            _theme = nuevoTema;
            ActivarAplicacionAutomatica();
            AplicarTipografia(SceneManager.GetActiveScene());
            OnThemeChanged?.Invoke();
        }

        public static float EscalarTexto(float tamano) => tamano * ObtenerEscalaTexto();

        public static void RegistrarTextoEscalado(TMP_Text texto)
        {
            if (texto == null) return;
            float escala = ObtenerEscalaTexto();
            TextosRegistrados[texto] = new DatosTexto(texto, escala);
            AplicarFuente(texto);
        }

        private static void ActivarAplicacionAutomatica()
        {
            if (_escuchaEscenasActiva) return;
            SceneManager.sceneLoaded += AlCargarEscena;
            _escuchaEscenasActiva = true;
        }

        private static void AlCargarEscena(Scene escena, LoadSceneMode _) => AplicarTipografia(escena);

        private static void AplicarTipografia(Scene escena)
        {
            if (!escena.IsValid() || !escena.isLoaded || _theme == null) return;

            foreach (GameObject raiz in escena.GetRootGameObjects())
            {
                foreach (TMP_Text texto in raiz.GetComponentsInChildren<TMP_Text>(true))
                    AplicarTipografia(texto);
            }
        }

        private static void AplicarTipografia(TMP_Text texto)
        {
            ThemedTextView textoTematizado = texto.GetComponent<ThemedTextView>();
            if (textoTematizado != null)
            {
                textoTematizado.AplicarTema();
                return;
            }

            if (!TextosRegistrados.TryGetValue(texto, out DatosTexto datos) || datos.Texto != texto)
            {
                datos = new DatosTexto(texto);
                TextosRegistrados[texto] = datos;
            }

            float escala = ObtenerEscalaTexto();
            texto.fontSize = datos.Tamano * escala;
            if (texto.enableAutoSizing)
            {
                texto.fontSizeMin = datos.Minimo * escala;
                texto.fontSizeMax = datos.Maximo * escala;
            }
            AplicarFuente(texto);
        }

        private static void AplicarFuente(TMP_Text texto)
        {
            if (Theme?.fuentePrincipal != null)
                texto.font = Theme.fuentePrincipal;
        }

        private static float ObtenerEscalaTexto() =>
            Theme != null ? Mathf.Max(1f, Theme.escalaTextoGlobal) : EscalaTextoPredeterminada;
    }
}
