using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public sealed class ColaDialogoRA : MaskableGraphic
    {
        private Vector2 punta;

        public void Apuntar(Vector2 posicion)
        {
            if ((punta - posicion).sqrMagnitude < 0.01f) return;
            punta = posicion;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            float baseX = Mathf.Clamp(punta.x, -rectTransform.rect.width * 0.35f,
                rectTransform.rect.width * 0.35f);
            float baseY = rectTransform.rect.yMin + 3f;
            vh.AddVert(new Vector3(baseX - 15f, baseY), color, Vector2.zero);
            vh.AddVert(new Vector3(baseX + 15f, baseY), color, Vector2.zero);
            vh.AddVert(punta, color, Vector2.zero);
            vh.AddTriangle(0, 1, 2);
        }
    }
}
