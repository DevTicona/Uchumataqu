using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.UI.Components;

namespace Assets.Scripts.Core.UI.Theme
{
    public static class UIThemeManager
    {
        private const float EscalaTextoPredeterminada = 1.15f;

        private static UITheme _theme;
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

        public static UITheme Theme => _theme;

        /// <summary>
        /// Carga el tema usando AddressablesService
        /// </summary>
        public static async Task CargarTemaAsync(string key = ContentAddress.TemaUi)
        {
            try
            {
                var tema = await AddressablesService.CargarAssetAsync<UITheme>(key);
                _theme = tema;
                ActivarAplicacionAutomatica();
                AplicarTipografia(SceneManager.GetActiveScene());
                OnThemeChanged?.Invoke();
                Debug.Log("[UIThemeManager] Tema cargado correctamente.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UIThemeManager] Error cargando UITheme: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Permite cambiar el tema en runtime
        /// </summary>
        public static void SetTheme(UITheme nuevoTema)
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

        public static void AplicarSpriteBoton(Button boton)
        {
            if (boton == null || _theme?.spriteBoton == null) return;
            if (!(boton.targetGraphic is Image imagen)) return;

            imagen.sprite = _theme.spriteBoton;
            imagen.type = Image.Type.Sliced;
        }

        public static void AplicarSpriteCircular(Image imagen, Sprite sprite = null)
        {
            if (imagen == null) return;

            Sprite spriteCircular = sprite != null ? sprite : _theme?.spriteCircular;
            if (spriteCircular == null) return;

            imagen.sprite = spriteCircular;
            imagen.type = Image.Type.Simple;
            imagen.preserveAspect = true;
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
            ThemedText textoTematizado = texto.GetComponent<ThemedText>();
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
            if (_theme?.fuentePrincipal != null)
                texto.font = _theme.fuentePrincipal;
        }

        private static float ObtenerEscalaTexto() =>
            _theme != null ? Mathf.Max(1f, _theme.escalaTextoGlobal) : EscalaTextoPredeterminada;
    }
}
