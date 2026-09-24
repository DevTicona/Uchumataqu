using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class ActividadLayoutController : MonoBehaviour
    {
        private Coroutine _reconstruccionPendiente;

        private void OnEnable() => SolicitarReconstruccion();

        private void OnDisable()
        {
            if (_reconstruccionPendiente != null) StopCoroutine(_reconstruccionPendiente);
            _reconstruccionPendiente = null;
        }

        public void SolicitarReconstruccion()
        {
            // Solo recalcula el layout existente: sus medidas pertenecen al prefab o al Inspector.
            ReconstruirAhora();
            if (!Application.isPlaying || !isActiveAndEnabled) return;

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

        private void ReconstruirAhora()
        {
            if (!(transform is RectTransform root)) return;

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
