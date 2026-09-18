using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class ActividadLayoutController : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollCentrado;

        private Coroutine _reconstruccionPendiente;
        private float _alturaMinimaDiseno = -1f;
        private Vector2 _ultimoTamano;

        private void OnEnable() => SolicitarReconstruccion();

        private void LateUpdate()
        {
            Vector2 tamano = ((RectTransform)transform).rect.size;
            if (tamano == _ultimoTamano) return;
            _ultimoTamano = tamano;
            SolicitarReconstruccion();
        }

        public void SolicitarReconstruccion()
        {
            AjustarAlturaAlViewport();
            ReconstruirAhora();
            if (!Application.isPlaying) return;

            if (_reconstruccionPendiente != null)
                StopCoroutine(_reconstruccionPendiente);
            _reconstruccionPendiente = StartCoroutine(ReconstruirAlFinalDelFrame());
        }

        private IEnumerator ReconstruirAlFinalDelFrame()
        {
            yield return null;
            ReconstruirAhora();
            _reconstruccionPendiente = null;
        }

        private void AjustarAlturaAlViewport()
        {
            LayoutElement elemento = GetComponent<LayoutElement>();
            ScrollRect scroll = GetComponentInParent<ScrollRect>();
            if (elemento == null || scroll == null || scroll.viewport == null) return;

            if (_alturaMinimaDiseno < 0f)
                _alturaMinimaDiseno = Mathf.Max(elemento.minHeight, elemento.preferredHeight);

            float rellenoVertical = 0f;
            VerticalLayoutGroup layoutPadre = transform.parent != null
                ? transform.parent.GetComponent<VerticalLayoutGroup>()
                : null;
            if (layoutPadre != null)
                rellenoVertical = layoutPadre.padding.top + layoutPadre.padding.bottom;

            float alturaDisponible = Mathf.Max(0f, scroll.viewport.rect.height - rellenoVertical);
            float alturaObjetivo = Mathf.Max(_alturaMinimaDiseno, alturaDisponible);
            elemento.minHeight = alturaObjetivo;
            elemento.preferredHeight = alturaObjetivo;
        }

        private void ReconstruirAhora()
        {
            if (!(transform is RectTransform root)) return;

            if (scrollCentrado != null && scrollCentrado.content != null && scrollCentrado.viewport != null)
            {
                LayoutElement contenido = scrollCentrado.content.GetComponent<LayoutElement>();
                if (contenido != null) contenido.minHeight = scrollCentrado.viewport.rect.height;
            }

            foreach (GridLayoutGroup grid in GetComponentsInChildren<GridLayoutGroup>())
            {
                float ancho = ((RectTransform)grid.transform).rect.width;
                if (ancho > 0) grid.cellSize = new Vector2(Mathf.Max(80, (ancho - grid.padding.horizontal - grid.spacing.x) / 2), 148);
            }

            RectTransform[] rects = GetComponentsInChildren<RectTransform>(true);
            for (int i = rects.Length - 1; i >= 0; i--)
                LayoutRebuilder.ForceRebuildLayoutImmediate(rects[i]);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(root);
            if (root.parent is RectTransform parent)
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
        }
    }
}
