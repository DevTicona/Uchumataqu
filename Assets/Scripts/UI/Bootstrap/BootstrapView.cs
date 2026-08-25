using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Bootstrap
{
    public sealed class BootstrapView : MonoBehaviour
    {
        [SerializeField] private TMP_Text textoEstado;

        public event Action OnReintentar;
        public event Action OnCerrar;

        private CanvasGroup _grupoEstado;
        private GameObject _panelError;
        private float _tiempoAnimacion;
        private bool _animando;

        private void Awake()
        {
            if (textoEstado != null)
                _grupoEstado = textoEstado.GetComponent<CanvasGroup>() ?? textoEstado.gameObject.AddComponent<CanvasGroup>();
        }

        private void Update()
        {
            if (!_animando || _grupoEstado == null) return;
            _tiempoAnimacion += Time.unscaledDeltaTime;
            _grupoEstado.alpha = Mathf.Lerp(0.55f, 1f, (Mathf.Sin(_tiempoAnimacion * 3f) + 1f) * 0.5f);
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
            reintentar.onClick.AddListener(() => OnReintentar?.Invoke());
            cerrar.onClick.AddListener(() => OnCerrar?.Invoke());
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
            texto.fontSize = 24f;
            texto.alignment = TextAlignmentOptions.Center;
            texto.color = Color.white;
            return objeto.GetComponent<Button>();
        }
    }
}
