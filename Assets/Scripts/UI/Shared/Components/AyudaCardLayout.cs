using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Core.UI.Components
{
    [DisallowMultipleComponent]
    public sealed class AyudaCardLayout : MonoBehaviour
    {
        [SerializeField] private LayoutElement lectura;
        [SerializeField] private TMP_Text descripcion;
        public bool ComposicionAutomatica => GetComponent<VerticalLayoutGroup>() != null;

        public T Buscar<T>(string nombre) where T : Component
        {
            foreach (var elemento in GetComponentsInChildren<T>(true))
                if (elemento.name == nombre) return elemento;
            return null;
        }

        public void Reconstruir() => Ajustar(float.PositiveInfinity);
        public void OrganizarMenu(bool mostrar) => Reconstruir();
        public void OrganizarGuia() => Reconstruir();

        public void Ajustar(float alturaMaxima)
        {
            if (!ComposicionAutomatica) return;
            lectura ??= Buscar<LayoutElement>("AreaTexto");
            descripcion ??= Buscar<TMP_Text>("Descripcion");
            if (lectura == null || descripcion == null) return;
            var rect = (RectTransform)transform;
            var grupo = GetComponent<VerticalLayoutGroup>();
            float ancho = Mathf.Max(1f, rect.rect.width - grupo.padding.horizontal - 14f);
            float altoTexto = descripcion.GetPreferredValues(descripcion.text, ancho, Mathf.Infinity).y;
            lectura.preferredHeight = Mathf.Max(1f, altoTexto);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            if (!float.IsInfinity(alturaMaxima))
            {
                float resto = LayoutUtility.GetPreferredHeight(rect) - lectura.preferredHeight;
                lectura.preferredHeight = Mathf.Clamp(alturaMaxima - resto, 1f, Mathf.Max(1f, altoTexto));
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
            descripcion.ForceMeshUpdate(true, true);
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)lectura.transform);
        }
    }
}
