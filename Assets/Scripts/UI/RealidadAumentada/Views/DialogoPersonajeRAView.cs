using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public sealed class DialogoPersonajeRAView : MonoBehaviour
    {
        [SerializeField] private RectTransform areaSegura;
        [SerializeField] private RectTransform tarjeta;
        [SerializeField] private RectTransform footer;
        [SerializeField] private CanvasGroup visibilidad;
        [SerializeField] private TMP_Text frase;
        [SerializeField] private TMP_Text traduccion;
        [SerializeField] private Button repetir;
        [SerializeField] private ColaDialogoRA cola;

        private Transform ancla;
        private Camera camara;
        private bool solicitado;
        private bool pausado;

        public event Action Repetir;
        public bool Visible => visibilidad != null && visibilidad.alpha > 0f;
        public bool TieneContenido => solicitado;
        public Transform Ancla => ancla;
        public string Texto => frase.text;
        public string Traduccion => traduccion.text;

        private void Awake()
        {
            if (repetir != null) repetir.onClick.AddListener(NotificarRepetir);
        }

        private void OnDestroy()
        {
            if (repetir != null) repetir.onClick.RemoveListener(NotificarRepetir);
        }
        private void NotificarRepetir() => Repetir?.Invoke();

        public void Mostrar(Transform punto, Camera origen, string texto, bool conAudio, string espanol = "")
        {
            ancla = punto;
            camara = origen;
            solicitado = punto != null && !string.IsNullOrWhiteSpace(texto);
            pausado = false;
            frase.text = texto;
            traduccion.text = espanol;
            traduccion.gameObject.SetActive(!string.IsNullOrWhiteSpace(espanol));
            repetir.interactable = conAudio;
            const float lateral = 32f;
            const float vertical = 24f;
            const float separacion = 8f;
            float maximoAncho = Mathf.Min(550f, areaSegura.rect.width - 40f);
            float contenidoAncho = Mathf.Max(frase.GetPreferredValues(texto).x,
                traduccion.GetPreferredValues(espanol).x);
            float ancho = Mathf.Clamp(contenidoAncho + lateral * 2f, Mathf.Min(180f, maximoAncho), maximoAncho);
            tarjeta.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ancho);
            float altoFrase = frase.GetPreferredValues(texto, ancho - lateral * 2f, 1000f).y;
            float altoTraduccion = traduccion.gameObject.activeSelf
                ? traduccion.GetPreferredValues(espanol, ancho - lateral * 2f, 1000f).y : 0f;
            float altura = vertical * 2f + altoFrase + (altoTraduccion > 0f ? separacion + altoTraduccion : 0f);
            tarjeta.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, altura);
            AjustarTexto(frase.rectTransform, vertical, altoFrase, lateral);
            AjustarTexto(traduccion.rectTransform, vertical + altoFrase + separacion, altoTraduccion, lateral);
            ActualizarPosicion();
        }

        private static void AjustarTexto(RectTransform rect, float superior, float alto, float lateral)
        {
            rect.anchorMin = Vector2.up;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -superior);
            rect.sizeDelta = new Vector2(-lateral * 2f, alto);
        }

        public void Ocultar()
        {
            solicitado = false;
            ancla = null;
            AplicarVisibilidad(false);
        }

        public void Suspender(bool valor)
        {
            pausado = valor;
            if (valor) AplicarVisibilidad(false);
        }

        private void LateUpdate() => ActualizarPosicion();

        private void ActualizarPosicion()
        {
            if (!solicitado || pausado || ancla == null || camara == null ||
                !ancla.gameObject.activeInHierarchy)
            {
                AplicarVisibilidad(false);
                return;
            }
            Vector3 pantalla = camara.WorldToScreenPoint(ancla.position);
            Vector3 viewport = camara.WorldToViewportPoint(ancla.position);
            bool dentro = pantalla.z > 0f && viewport.x >= 0f && viewport.x <= 1f &&
                viewport.y >= 0f && viewport.y <= 1f;
            if (!dentro) { AplicarVisibilidad(false); return; }
            Canvas canvas = tarjeta.GetComponentInParent<Canvas>();
            Camera camaraUI = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(areaSegura, pantalla, camaraUI, out Vector2 punto);
            Rect limites = areaSegura.rect;
            float anchoMedio = tarjeta.rect.width * 0.5f;
            // El borde inferior sigue al ancla: limitar Y al footer desplazaba la tarjeta sobre el cuerpo.
            tarjeta.anchoredPosition = new Vector2(
                Mathf.Clamp(punto.x, limites.xMin + anchoMedio + 16f, limites.xMax - anchoMedio - 16f),
                punto.y + 16f);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(tarjeta, pantalla, camaraUI, out Vector2 punta);
            cola.Apuntar(punta);
            AplicarVisibilidad(true);
        }

        private void AplicarVisibilidad(bool visible)
        {
            if (visibilidad == null) return;
            visibilidad.alpha = visible ? 1f : 0f;
            visibilidad.blocksRaycasts = visible;
            visibilidad.interactable = visible;
        }
    }
}
