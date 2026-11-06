using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Core.UI.Components
{
    [DisallowMultipleComponent]
    public sealed class DesplazamientoAguaUI : BaseMeshEffect
    {
        [SerializeField] private float ciclosPorSegundo = 0.015f;
        private float desplazamiento;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (graphic != null && graphic.mainTexture != null)
                graphic.mainTexture.wrapMode = TextureWrapMode.Repeat;
        }

        private void Update()
        {
            desplazamiento = Mathf.Repeat(desplazamiento + Time.unscaledDeltaTime * ciclosPorSegundo, 1f);
            graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vertices)
        {
            if (!IsActive()) return;
            var vertice = new UIVertex();
            for (int i = 0; i < vertices.currentVertCount; i++)
            {
                vertices.PopulateUIVertex(ref vertice, i);
                vertice.uv0.x += desplazamiento;
                vertices.SetUIVertex(vertice, i);
            }
        }
    }
}
