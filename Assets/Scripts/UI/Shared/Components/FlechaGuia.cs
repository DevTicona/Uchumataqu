using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Core.UI.Components
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class FlechaGuia : Graphic
    {
        private Vector2 inicio;
        private Vector2 fin;

        public void Apuntar(Vector2 desde, Vector2 hacia)
        {
            if (inicio == desde && fin == hacia) return;
            inicio = desde;
            fin = hacia;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            Vector2 direccion = (fin - inicio).normalized;
            if (direccion == Vector2.zero) return;
            Vector2 lado = new Vector2(-direccion.y, direccion.x);
            Vector2 basePunta = fin - direccion * Mathf.Min(18f, Vector2.Distance(inicio, fin) * .45f);
            foreach (var punto in new[] { inicio - lado * 3f, inicio + lado * 3f, basePunta + lado * 3f,
                basePunta - lado * 3f, basePunta - lado * 12f, basePunta + lado * 12f, fin })
                vh.AddVert(punto, color, Vector2.zero);
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(0, 2, 3);
            vh.AddTriangle(4, 5, 6);
        }
    }
}
