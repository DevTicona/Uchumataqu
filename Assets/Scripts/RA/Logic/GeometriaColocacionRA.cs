using UnityEngine;

namespace Assets.Scripts.RA
{
    public static class GeometriaColocacionRA
    {
        public static Quaternion RotacionVertical(Transform objeto, Transform destino, Vector3 arriba)
        {
            foreach (var renderer in objeto.GetComponentsInChildren<MeshRenderer>())
            {
                if (!renderer.enabled) continue;
                var filtro = renderer.GetComponent<MeshFilter>();
                if (filtro == null || filtro.sharedMesh == null) continue;
                var tamano = filtro.sharedMesh.bounds.size;
                Vector3 eje = EjeLargo(tamano);
                eje = objeto.InverseTransformDirection(renderer.transform.TransformDirection(eje));
                return Quaternion.FromToRotation(destino.rotation * eje, arriba) * destino.rotation;
            }
            return destino.rotation;
        }

        public static void AlinearPiezasParaColocacionVertical(Transform objeto, Quaternion giroFinal, Vector3 arriba)
        {
            Vector3 ejeObjetivo = Quaternion.Inverse(giroFinal) * arriba;
            foreach (var renderer in objeto.GetComponentsInChildren<MeshRenderer>())
            {
                if (!renderer.enabled) continue;
                var filtro = renderer.GetComponent<MeshFilter>();
                if (filtro == null || filtro.sharedMesh == null) continue;
                Bounds limites = filtro.sharedMesh.bounds;
                Vector3 tamano = limites.size;
                Vector3 eje = EjeLargo(tamano);
                Vector3 ejeEnGrupo = objeto.InverseTransformDirection(renderer.transform.TransformDirection(eje));
                Quaternion ajuste = Quaternion.FromToRotation(ejeEnGrupo, ejeObjetivo);
                Vector3 centro = renderer.transform.TransformPoint(limites.center);
                renderer.transform.rotation = objeto.rotation * ajuste * Quaternion.Inverse(objeto.rotation) * renderer.transform.rotation;
                renderer.transform.position += centro - renderer.transform.TransformPoint(limites.center);
            }
        }

        private static Vector3 EjeLargo(Vector3 tamano) => tamano.x >= tamano.y && tamano.x >= tamano.z
            ? Vector3.right : tamano.y >= tamano.z ? Vector3.up : Vector3.forward;

        public static Quaternion OrientarHaciaCamara(Vector3 punto, Vector3 camara, Vector3 frenteCamara)
        {
            Vector3 frente = Vector3.ProjectOnPlane(camara - punto, Vector3.up);
            if (frente.sqrMagnitude < 0.001f)
                frente = Vector3.ProjectOnPlane(-frenteCamara, Vector3.up);
            return frente.sqrMagnitude > 0.001f ? Quaternion.LookRotation(frente) : Quaternion.identity;
        }

        public static bool ObtenerLimitesLocales(Transform raiz, out Bounds limites, bool usarMalla = false)
        {
            limites = default;
            bool encontrado = false;
            foreach (var renderer in raiz.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled || !(renderer is MeshRenderer || renderer is SkinnedMeshRenderer)) continue;
                Vector3 escala = renderer.transform.lossyScale;
                if (Mathf.Abs(escala.x * escala.y * escala.z) < 0.00000001f) continue;
                Bounds local = renderer.localBounds;
                if (usarMalla && renderer is MeshRenderer && renderer.TryGetComponent<MeshFilter>(out var filtro) && filtro.sharedMesh != null)
                    local = filtro.sharedMesh.bounds;
                Matrix4x4 aRaiz = raiz.worldToLocalMatrix * renderer.localToWorldMatrix;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 esquina = local.center + Vector3.Scale(local.extents, new Vector3(
                        (i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
                    Vector3 punto = aRaiz.MultiplyPoint3x4(esquina);
                    if (encontrado) limites.Encapsulate(punto);
                    else { limites = new Bounds(punto, Vector3.zero); encontrado = true; }
                }
            }
            return encontrado;
        }

        public static void ApoyarEnSuperficie(Transform raiz, Bounds limites, Vector3 punto, float anchoMaximo)
        {
            // Los límites se miden en el espacio del prefab: girar el teléfono no
            // debe cambiar la escala del mismo diorama.
            float ancho = Mathf.Max(limites.size.x * Mathf.Abs(raiz.lossyScale.x),
                limites.size.z * Mathf.Abs(raiz.lossyScale.z));
            if (ancho > anchoMaximo && anchoMaximo > 0f)
                raiz.localScale *= anchoMaximo / ancho;
            Vector3 apoyo = new Vector3(limites.center.x, limites.min.y, limites.center.z);
            raiz.position += punto - raiz.TransformPoint(apoyo);
        }
    }
}
