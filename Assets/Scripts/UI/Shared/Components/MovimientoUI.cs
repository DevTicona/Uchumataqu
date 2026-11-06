using System.Collections;
using Assets.Scripts.Persistencia;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Core.UI.Components
{
    [DisallowMultipleComponent]
    public sealed class MovimientoUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        [SerializeField] private bool animarEntrada = true;
        [SerializeField] private bool animarPulsacion;
        [SerializeField, Range(0.1f, 0.5f)] private float duracion = 0.22f;
        private Vector3 escalaOriginal;
        private CanvasGroup grupo;
        private Coroutine animacion;
        private bool inicializado;
        private bool pulsado;

        private void Awake()
        {
            escalaOriginal = transform.localScale;
            grupo = GetComponent<CanvasGroup>();
            inicializado = true;
        }

        private void OnEnable()
        {
            if (!Application.isPlaying || !animarEntrada) return;
            ReproducirEntrada();
        }

        public void ReproducirEntrada()
        {
            if (!Application.isPlaying || !isActiveAndEnabled) return;
            Cancelar();
            if (AyudaRepository.Obtener().reducir_movimiento) return;
            animacion = StartCoroutine(Interpolar(0.985f, 1f, true));
        }

        private void OnDisable() => Cancelar();

        private void Cancelar()
        {
            if (animacion != null) StopCoroutine(animacion);
            animacion = null;
            pulsado = false;
            if (!inicializado) return;
            transform.localScale = escalaOriginal;
            if (animarEntrada && grupo != null) grupo.alpha = 1f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var seleccionable = GetComponent<Selectable>();
            if (!animarPulsacion || (seleccionable != null && !seleccionable.IsInteractable()) ||
                AyudaRepository.Obtener().reducir_movimiento) return;
            Cancelar();
            pulsado = true;
            transform.localScale = escalaOriginal * 0.96f;
        }

        public void OnPointerUp(PointerEventData eventData) => RestaurarPulsacion();
        public void OnPointerExit(PointerEventData eventData) => RestaurarPulsacion();

        private void RestaurarPulsacion()
        {
            if (!animarPulsacion || !isActiveAndEnabled || !pulsado) return;
            float escala = escalaOriginal.x != 0f ? transform.localScale.x / escalaOriginal.x : 1f;
            Cancelar();
            if (AyudaRepository.Obtener().reducir_movimiento) return;
            animacion = StartCoroutine(Interpolar(escala, 1f, false));
        }

        private IEnumerator Interpolar(float desde, float hasta, bool fundido)
        {
            float inicio = Time.unscaledTime;
            float tiempo;
            do
            {
                tiempo = Mathf.Clamp01((Time.unscaledTime - inicio) / duracion);
                float suavizado = 1f - Mathf.Pow(1f - tiempo, 3f);
                transform.localScale = escalaOriginal * Mathf.Lerp(desde, hasta, suavizado);
                if (fundido && grupo != null) grupo.alpha = suavizado;
                yield return null;
            } while (tiempo < 1f);
            transform.localScale = escalaOriginal;
            if (fundido && grupo != null) grupo.alpha = 1f;
            animacion = null;
        }
    }
}
