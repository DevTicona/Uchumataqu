using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public sealed class PulsoGuiaRAView : MonoBehaviour
    {
        [SerializeField] private Image borde;
        private bool activo;
        private float inicio;

        public bool Activo => activo;

        public void Activar(bool valor)
        {
            if (activo == valor) return;
            activo = valor;
            inicio = Time.unscaledTime;
            AplicarAlpha(1f);
        }

        private void LateUpdate()
        {
            if (activo)
                AplicarAlpha(Mathf.Lerp(0.35f, 1f,
                    (1f + Mathf.Cos((Time.unscaledTime - inicio) * Mathf.PI * 2f / 1.4f)) * 0.5f));
        }

        private void OnDisable() => Activar(false);

        private void AplicarAlpha(float alpha)
        {
            if (borde == null) return;
            Color color = borde.color;
            color.a = alpha;
            borde.color = color;
        }
    }
}
