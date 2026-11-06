using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.RA
{
    public static class EncuadreEscenarioRA
    {
        public static bool ObtenerLimites(Transform raiz, out Bounds limites)
        {
            limites = default;
            var puntos = ObtenerPuntosVisibles(raiz);
            if (puntos.Count == 0) return false;
            limites = new Bounds(puntos[0], Vector3.zero);
            foreach (Vector3 punto in puntos) limites.Encapsulate(punto);
            return true;
        }

        public static List<Vector3> ObtenerPuntosVisibles(Transform raiz)
        {
            var puntos = new List<Vector3>();
            foreach (var renderer in raiz.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled || !(renderer is MeshRenderer || renderer is SkinnedMeshRenderer) ||
                    renderer.GetComponentInParent<DestinoRA>() != null) continue;
                Bounds malla;
                Matrix4x4 aRaiz;
                if (renderer is SkinnedMeshRenderer piel)
                {
                    var instantanea = new Mesh();
                    try { piel.BakeMesh(instantanea); malla = instantanea.bounds; }
                    finally
                    {
                        if (Application.isPlaying) Object.Destroy(instantanea);
                        else Object.DestroyImmediate(instantanea);
                    }
                    aRaiz = raiz.worldToLocalMatrix * Matrix4x4.TRS(piel.transform.position, piel.transform.rotation, Vector3.one);
                }
                else
                {
                    var filtro = renderer.GetComponent<MeshFilter>();
                    if (filtro == null || filtro.sharedMesh == null) continue;
                    malla = filtro.sharedMesh.bounds;
                    aRaiz = raiz.worldToLocalMatrix * renderer.localToWorldMatrix;
                }
                for (int i = 0; i < 8; i++)
                {
                    var esquina = malla.center + Vector3.Scale(malla.extents,
                        new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    puntos.Add(aRaiz.MultiplyPoint3x4(esquina));
                }
            }
            return puntos;
        }

        public static void Ajustar(Transform raiz, Bounds limites, Camera camara, float centroVertical,
            float ancho = 0.88f, float alto = 0.52f, bool geometriaVisible = false)
        {
            var puntos = geometriaVisible ? ObtenerPuntosVisibles(raiz) : new List<Vector3>();
            if (puntos.Count == 0)
                for (int i = 0; i < 8; i++)
                    puntos.Add(limites.center + Vector3.Scale(limites.extents,
                        new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1)));
            Vector3 escala = raiz.localScale;
            float profundidad = Mathf.Max(camara.nearClipPlane * 4f,
                camara.WorldToViewportPoint(raiz.TransformPoint(limites.center)).z);
            Vector3 centro = camara.ViewportToWorldPoint(new Vector3(0.5f, centroVertical, profundidad));
            var area = new Rect((1f - ancho) * 0.5f, centroVertical - alto * 0.5f, ancho, alto);

            void Aplicar(float factor)
            {
                raiz.localScale = escala * factor;
                raiz.position += centro - raiz.TransformPoint(limites.center);
            }

            bool Cabe()
            {
                foreach (Vector3 esquina in puntos)
                {
                    Vector3 vista = camara.WorldToViewportPoint(raiz.TransformPoint(esquina));
                    if (vista.z <= camara.nearClipPlane || !area.Contains(new Vector2(vista.x, vista.y))) return false;
                }
                return true;
            }

            Aplicar(1f);
            if (Cabe()) return;
            float minimo = 0f, maximo = 1f;
            for (int i = 0; i < 24; i++)
            {
                float factor = (minimo + maximo) * 0.5f;
                Aplicar(factor);
                if (Cabe()) minimo = factor;
                else maximo = factor;
            }
            Aplicar(minimo);
        }
    }
}
