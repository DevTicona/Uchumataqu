using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Core;
using Assets.Scripts.Core.UI;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Main
{
    /// <summary>
    /// Gestiona la navegación y aplica los estados visuales del tema global.
    /// </summary>
    public class BottomNavigationView : MonoBehaviour
    {
        [Header("Botones de navegación")]
        [SerializeField] private Button btnInicio;
        [SerializeField] private Button btnLecciones;
        [SerializeField] private Button btnLogros;
        [SerializeField] private Button btnAjustes;
        [SerializeField] private Button btnAcerca;

        public event Action<PaginaMain> OnTabSelected;

        private readonly Dictionary<PaginaMain, Button> _botones = new Dictionary<PaginaMain, Button>();
        private readonly Dictionary<Button, GameObject> _indicadores = new Dictionary<Button, GameObject>();
        private readonly Dictionary<Button, Transform> _iconos = new Dictionary<Button, Transform>();
        private readonly Dictionary<Button, Coroutine> _animaciones = new Dictionary<Button, Coroutine>();
        private PaginaMain _paginaActiva = PaginaMain.Inicio;
        private readonly UIEventSubscriptions _subscriptions = new UIEventSubscriptions();

        private readonly Dictionary<Button, Color> _coloresOriginalesIconos = new Dictionary<Button, Color>();

        private void OnDestroy() => _subscriptions.Dispose();

        private void OnEnable()
        {
            UIThemeController.OnThemeChanged += AplicarColores;
            AplicarColores();
        }

        private void OnDisable() => UIThemeController.OnThemeChanged -= AplicarColores;

        private void Awake()
        {
            ConfigurarBoton(btnInicio, PaginaMain.Inicio);
            ConfigurarBoton(btnLecciones, PaginaMain.Lecciones);
            ConfigurarBoton(btnLogros, PaginaMain.Logros);
            ConfigurarBoton(btnAjustes, PaginaMain.Ajustes);
            ConfigurarBoton(btnAcerca, PaginaMain.Acerca);
        }

        public void ResaltarPestana(PaginaMain pagina)
        {
            _paginaActiva = pagina;
            AplicarColores();
            foreach (KeyValuePair<PaginaMain, Button> item in _botones)
            {
                bool seleccionado = item.Key == pagina;

                if (_indicadores.TryGetValue(item.Value, out GameObject indicador))
                    indicador.SetActive(seleccionado);

                if (!_iconos.TryGetValue(item.Value, out Transform icono)) continue;

                if (_animaciones.TryGetValue(item.Value, out Coroutine animacion) && animacion != null)
                    StopCoroutine(animacion);

                if (seleccionado)
                    _animaciones[item.Value] = StartCoroutine(AnimarSeleccion(icono));
                else
                    icono.localScale = Vector3.one;
            }
        }

        private void AplicarColores()
        {
            if (UIThemeController.Theme == null) return;
            foreach (KeyValuePair<PaginaMain, Button> item in _botones)
            {
                bool activo = item.Key == _paginaActiva;
                if (_iconos.TryGetValue(item.Value, out Transform icono))
                {
                    Image img = icono.GetComponent<Image>();
                    if (img != null)
                    {
                        var themed = img.GetComponent<ThemedGraphicView>();
                        if (themed != null) Destroy(themed);
                        img.color = _coloresOriginalesIconos[item.Value];
                    }
                }

                if (_indicadores.TryGetValue(item.Value, out GameObject indicador))
                {
                    var themed = indicador.GetComponent<ThemedGraphicView>();
                    if (themed != null) Destroy(themed);
                }

                ThemedButtonView themedBtn = item.Value.GetComponent<ThemedButtonView>();
                if (themedBtn != null) themedBtn.enabled = false;
                item.Value.transition = Selectable.Transition.None;
                item.Value.targetGraphic?.canvasRenderer.SetColor(Color.white);
            }
        }

        private void ConfigurarBoton(Button boton, PaginaMain pagina)
        {
            if (boton == null) return;

            _botones[pagina] = boton;
            _subscriptions.Add(boton, () => OnTabSelected?.Invoke(pagina));

            TMP_Text etiqueta = boton.GetComponentInChildren<TMP_Text>(true);
            if (etiqueta != null) etiqueta.gameObject.SetActive(false);

            Transform indicador = boton.transform.Find("IndicadorActivo");
            if (indicador != null) _indicadores[boton] = indicador.gameObject;

            Transform icono = boton.transform.Find("Icono");
            if (icono != null)
            {
                _iconos[boton] = icono;
                Image img = icono.GetComponent<Image>();
                if (img != null) _coloresOriginalesIconos[boton] = img.color;
            }
        }

        private static IEnumerator AnimarSeleccion(Transform icono)
        {
            const float duracionSubida = 0.1f;
            const float duracionAsentamiento = 0.08f;
            Vector3 escalaInicial = Vector3.one;
            Vector3 escalaMaxima = Vector3.one * 1.16f;
            Vector3 escalaFinal = Vector3.one * 1.08f;

            for (float tiempo = 0f; tiempo < duracionSubida; tiempo += Time.unscaledDeltaTime)
            {
                if (icono == null) yield break;
                icono.localScale = Vector3.Lerp(escalaInicial, escalaMaxima, tiempo / duracionSubida);
                yield return null;
            }

            for (float tiempo = 0f; tiempo < duracionAsentamiento; tiempo += Time.unscaledDeltaTime)
            {
                if (icono == null) yield break;
                icono.localScale = Vector3.Lerp(escalaMaxima, escalaFinal, tiempo / duracionAsentamiento);
                yield return null;
            }

            if (icono != null)
                icono.localScale = escalaFinal;
        }
    }
}
