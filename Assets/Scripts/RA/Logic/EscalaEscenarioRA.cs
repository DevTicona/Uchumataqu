using UnityEngine;

namespace Assets.Scripts.RA
{
    /// <summary>Un mismo ancho físico para todos los dioramas, conservando sus proporciones.</summary>
    public static class EscalaEscenarioRA
    {
        public const float FactorBase = 0.9f;
        public const float MultiplicadorReferencia = 1.62f;
        public const float Incremento = 1.1f;
        public const float FactorMesa = 0.49f;
        public const float MultiplicadorComun = MultiplicadorReferencia * Incremento * FactorMesa;

        public static float ObtenerAncho(Transform raiz, Bounds limites) => Mathf.Max(
            limites.size.x * Mathf.Abs(raiz.lossyScale.x),
            limites.size.z * Mathf.Abs(raiz.lossyScale.z));

        public static bool AjustarAncho(Transform raiz, Bounds limites, float anchoObjetivo)
        {
            float actual = ObtenerAncho(raiz, limites);
            if (actual <= Mathf.Epsilon || anchoObjetivo <= 0f ||
                float.IsNaN(actual) || float.IsInfinity(actual) ||
                float.IsNaN(anchoObjetivo) || float.IsInfinity(anchoObjetivo)) return false;
            if (!Mathf.Approximately(actual, anchoObjetivo))
                raiz.localScale *= anchoObjetivo / actual;
            return true;
        }
    }
}
