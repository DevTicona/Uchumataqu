using System;
using Assets.Scripts.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Core.UI.Theme;

namespace Assets.Scripts.UI.Bootstrap
{
    public sealed class BootstrapView : MonoBehaviour
    {
        [SerializeField] private TMP_Text textoEstado;
        [SerializeField] private TMP_Text textoPorcentaje;
        [SerializeField] private Slider sliderProgreso;
        [SerializeField] private Sprite spriteBotonError;

        public event Action OnReintentar;
        public event Action OnCerrar;

        private CanvasGroup _grupoEstado;
        private GameObject _panelError;
        private float _tiempoAnimacion;
        private bool _animando;
        private readonly UIEventSubscriptions _subscriptions = new UIEventSubscriptions();

        private void OnDestroy() => _subscriptions.Dispose();

        private void Awake()
        {
            if (textoEstado != null)
                _grupoEstado = textoEstado.GetComponent<CanvasGroup>() ?? textoEstado.gameObject.AddComponent<CanvasGroup>();

            ActualizarProgreso(0f);
        }

        private void Update()
        {
            if (_animando && _grupoEstado != null)
            {
                _tiempoAnimacion += Time.unscaledDeltaTime;
                _grupoEstado.alpha = Mathf.Lerp(0.55f, 1f, (Mathf.Sin(_tiempoAnimacion * 3f) + 1f) * 0.5f);
            }
        }

        public void MostrarProgreso(float progreso)
        {
            ActualizarProgreso(progreso);
        }

        private void ActualizarProgreso(float progreso)
        {
            float progresoNormalizado = Mathf.Clamp01(progreso);
            if (textoPorcentaje != null)
                textoPorcentaje.text = $"{Mathf.RoundToInt(progresoNormalizado * 100f)}%";

            if (sliderProgreso != null)
                sliderProgreso.value = progresoNormalizado;
        }

        public void MostrarCarga(string mensaje)
        {
            _animando = true;
            if (_grupoEstado != null) _grupoEstado.alpha = 1f;
            if (textoEstado != null) textoEstado.text = mensaje;
            if (_panelError != null) _panelError.SetActive(false);
        }

        public void MostrarError(string mensaje)
        {
            _animando = false;
            if (_grupoEstado != null) _grupoEstado.alpha = 1f;
            if (textoEstado != null) textoEstado.text = mensaje;
            AsegurarPanelError();
            _panelError.SetActive(true);
        }

        private void AsegurarPanelError()
        {
            if (_panelError != null) return;

            _panelError = new GameObject("PanelError", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            _panelError.transform.SetParent(transform, false);

            var rect = (RectTransform)_panelError.transform;
            rect.anchorMin = new Vector2(0.15f, 0.08f);
            rect.anchorMax = new Vector2(0.85f, 0.18f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var layout = _panelError.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;

            Button reintentar = CrearBoton("BtnReintentar", "Reintentar");
            Button cerrar = CrearBoton("BtnCerrar", "Cerrar");
            _subscriptions.Add(reintentar, () => OnReintentar?.Invoke());
            _subscriptions.Add(cerrar, () => OnCerrar?.Invoke());
        }

        private Button CrearBoton(string nombre, string etiqueta)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            objeto.transform.SetParent(_panelError.transform, false);
            objeto.GetComponent<Image>().color = new Color(0.18f, 0.45f, 0.55f, 1f);
            objeto.GetComponent<LayoutElement>().preferredHeight = 64f;

            var textoObjeto = new GameObject("Texto", typeof(RectTransform), typeof(TextMeshProUGUI));
            textoObjeto.transform.SetParent(objeto.transform, false);
            var rect = (RectTransform)textoObjeto.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var texto = textoObjeto.GetComponent<TextMeshProUGUI>();
            texto.text = etiqueta;
            texto.fontSize = UIThemeController.EscalarTexto(24f);
            texto.alignment = TextAlignmentOptions.Center;
            texto.color = Color.white;
            Button boton = objeto.GetComponent<Button>();
            Image imagen = objeto.GetComponent<Image>();
            imagen.sprite = spriteBotonError;
            imagen.type = Image.Type.Sliced;
            return boton;
        }
    }
}
