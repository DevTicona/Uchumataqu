using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Assets.Scripts.UI.RealidadAumentada
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class TarjetaArrastrableRAView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private TMP_Text texto;
        private RectTransform rect;
        private CanvasGroup grupo;
        private Vector2 posicionInicial;
        private bool arrastrando;
        private Vector2 desplazamientoPuntero;
        public Rect UltimaAreaSoltada { get; private set; }

        public event Action ArrastreIniciado;
        public event Action<Vector2> Soltada;

        private void Awake()
        {
            rect = (RectTransform)transform;
            grupo = GetComponent<CanvasGroup>();
        }

        public void Mostrar(string palabra, bool disponible)
        {
            texto.text = palabra;
            gameObject.SetActive(true);
            grupo.interactable = disponible;
            grupo.alpha = disponible ? 1f : 0.6f;
        }

        public void Ocultar()
        {
            Restaurar();
            gameObject.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData evento)
        {
            if (!grupo.interactable) return;
            posicionInicial = rect.anchoredPosition;
            UltimaAreaSoltada = default;
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rect.parent,
                evento.position, evento.pressEventCamera, out var puntero);
            desplazamientoPuntero = puntero - rect.anchoredPosition;
            arrastrando = true;
            grupo.blocksRaycasts = false;
            ArrastreIniciado?.Invoke();
        }

        public void OnDrag(PointerEventData evento)
        {
            if (arrastrando && RectTransformUtility.ScreenPointToLocalPointInRectangle(
                (RectTransform)rect.parent, evento.position, evento.pressEventCamera, out Vector2 posicion))
                rect.anchoredPosition = posicion - desplazamientoPuntero;
        }

        public void OnEndDrag(PointerEventData evento)
        {
            if (!arrastrando) return;
            var canvas = GetComponentInParent<Canvas>();
            var camara = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var esquinas = new Vector3[4];
            rect.GetWorldCorners(esquinas);
            Vector2 minimo = Vector2.one * float.PositiveInfinity, maximo = Vector2.one * float.NegativeInfinity;
            foreach (var esquina in esquinas)
            {
                var punto = RectTransformUtility.WorldToScreenPoint(camara, esquina);
                minimo = Vector2.Min(minimo, punto);
                maximo = Vector2.Max(maximo, punto);
            }
            UltimaAreaSoltada = Rect.MinMaxRect(minimo.x, minimo.y, maximo.x, maximo.y);
            Restaurar();
            Soltada?.Invoke(evento.position);
        }

        private void Restaurar()
        {
            if (arrastrando && rect != null) rect.anchoredPosition = posicionInicial;
            arrastrando = false;
            if (grupo != null) grupo.blocksRaycasts = true;
        }

        private void OnDisable() => Restaurar();
    }
}
