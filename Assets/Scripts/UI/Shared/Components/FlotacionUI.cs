using Assets.Scripts.Persistencia;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Core.UI.Components
{
    [DisallowMultipleComponent]
    public sealed class FlotacionUI : MonoBehaviour
    {
        [SerializeField] private RectTransform objetivo;
        [SerializeField, Range(1f, 10f)] private float amplitud = 4f;
        [SerializeField, Min(2f)] private float periodo = 5f;
        [SerializeField] private bool desplazar = true;
        private Vector2 origen;
        private Vector3 escala;
        private Quaternion rotacion;
        private ScrollRect scroll;
        private float fase;
        private bool preparado;

        private void OnEnable()
        {
            if (objetivo == null) return;
            origen = objetivo.anchoredPosition;
            rotacion = objetivo.localRotation;
            escala = objetivo.localScale;
            scroll = GetComponentInParent<ScrollRect>();
            fase = (transform.GetSiblingIndex() * 1.618f) % (Mathf.PI * 2f);
            preparado = true;
        }

        public void EstablecerActivo(bool valor) => enabled = valor;

        private void LateUpdate()
        {
            if (!Application.isPlaying || !preparado || objetivo == null) return;
            if (AyudaRepository.Obtener().reducir_movimiento ||
                (scroll != null && scroll.velocity.sqrMagnitude > 100f))
            {
                Restaurar();
                return;
            }
            float onda = Mathf.Sin(Time.unscaledTime * (Mathf.PI * 2f / periodo) + fase);
            if (desplazar) objetivo.anchoredPosition = origen + Vector2.up * (onda * amplitud);
            objetivo.localRotation = rotacion * Quaternion.Euler(0f, 0f, onda * 0.45f);
            objetivo.localScale = escala * (1f + onda * 0.012f);
        }

        private void OnDisable() => Restaurar();

        private void Restaurar()
        {
            if (!preparado || objetivo == null) return;
            if (desplazar) objetivo.anchoredPosition = origen;
            objetivo.localRotation = rotacion;
            objetivo.localScale = escala;
        }
    }
}
