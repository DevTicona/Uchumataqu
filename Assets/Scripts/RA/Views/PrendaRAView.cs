using UnityEngine;

namespace Assets.Scripts.RA
{
    public sealed class PrendaRAView : MonoBehaviour
    {
        [SerializeField] private SkinnedMeshRenderer malla;
        [SerializeField] private string blendshape;
        [SerializeField, Min(0f)] private float elevacion;
        private bool colocada;
        private int indice = -1;
        private float pesoInicial;
        private Vector3 centroInicial, tamanioInicial;
        private bool colliderCapturado;
        public float Elevacion => elevacion;

        public void Configurar(SkinnedMeshRenderer renderer, string nombre, float altura)
        {
            malla = renderer;
            blendshape = nombre;
            elevacion = altura;
        }

        private void Awake()
        {
            CapturarCollider();
            indice = malla != null ? malla.sharedMesh.GetBlendShapeIndex(blendshape) : -1;
            if (indice >= 0) pesoInicial = malla.GetBlendShapeWeight(indice);
        }

        public void Equipar()
        {
            CapturarCollider();
            if (indice < 0 && malla != null)
            {
                indice = malla.sharedMesh.GetBlendShapeIndex(blendshape);
                if (indice >= 0) pesoInicial = malla.GetBlendShapeWeight(indice);
            }
            colocada = true;
            transform.localScale = Vector3.one;
            AplicarAjuste();
            AjustarSeleccion();
        }

        private void AjustarSeleccion()
        {
            var collider = GetComponent<BoxCollider>();
            if (collider == null) return;
            bool encontrado = false;
            Bounds limites = default;
            foreach (var piel in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var mesh = new Mesh();
                piel.BakeMesh(mesh, true);
                var conversion = transform.worldToLocalMatrix * piel.transform.localToWorldMatrix;
                foreach (var vertice in mesh.vertices)
                {
                    var punto = conversion.MultiplyPoint3x4(vertice);
                    if (!encontrado) { limites = new Bounds(punto, Vector3.zero); encontrado = true; }
                    else limites.Encapsulate(punto);
                }
                if (Application.isPlaying) Destroy(mesh);
                else DestroyImmediate(mesh);
            }
            if (encontrado) { collider.center = limites.center; collider.size = Vector3.Max(limites.size, Vector3.one * 0.1f); }
            collider.enabled = true;
        }

        private void CapturarCollider()
        {
            if (colliderCapturado) return;
            var collider = GetComponent<BoxCollider>();
            if (collider == null) return;
            centroInicial = collider.center;
            tamanioInicial = collider.size;
            colliderCapturado = true;
        }

        // El ajuste equipado prevalece sobre las curvas de la pose Idle del personaje.
        private void LateUpdate() => AplicarAjuste();

        private void AplicarAjuste()
        {
            if (colocada && malla != null && indice >= 0) malla.SetBlendShapeWeight(indice, 100f);
        }

        public void Reiniciar()
        {
            colocada = false;
            if (malla != null && indice >= 0) malla.SetBlendShapeWeight(indice, pesoInicial);
            var collider = GetComponent<Collider>();
            if (collider != null) collider.enabled = true;
            if (collider is BoxCollider caja) { caja.center = centroInicial; caja.size = tamanioInicial; }
        }
    }
}
