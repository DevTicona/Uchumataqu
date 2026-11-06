using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.RA
{
    public sealed class DistribucionFaunaRA : MonoBehaviour
    {
        private static readonly float[] Giros = { -18f, 24f, 154f, -142f, 8f, 172f, 42f, -32f, 128f, -166f };
        [SerializeField] private Transform terrestre;
        [SerializeField] private Transform acuatico;
        [SerializeField] private Rect areaTerrestre;
        [SerializeField] private Rect areaAgua;

        public void Configurar(Transform tierra, Rect suelo, Transform agua, Rect rio)
        {
            terrestre = tierra;
            areaTerrestre = suelo;
            acuatico = agua;
            areaAgua = rio;
        }

        public bool ObtenerZona(Transform actor, out Transform espacio, out Rect area)
        {
            bool enAgua = acuatico != null && actor.IsChildOf(acuatico);
            espacio = enAgua ? acuatico : terrestre;
            area = enAgua ? areaAgua : areaTerrestre;
            return espacio != null && actor.IsChildOf(espacio) && area.width > 0f && area.height > 0f;
        }

        public static float Giro(int indice) => Giros[indice % Giros.Length];

        public static Bounds Medir(Renderer[] mallas, Transform espacio)
        {
            bool iniciado = false;
            Bounds resultado = default;
            foreach (var malla in mallas)
            {
                Bounds limites = malla.localBounds;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 punto = espacio.InverseTransformPoint(malla.transform.TransformPoint(limites.center +
                        Vector3.Scale(limites.extents, new Vector3((i & 1) == 0 ? -1 : 1,
                            (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1))));
                    if (iniciado) resultado.Encapsulate(punto);
                    else { resultado = new Bounds(punto, Vector3.zero); iniciado = true; }
                }
            }
            return resultado;
        }

        public static Vector3 BuscarApoyo(Rect area, Bounds limites, Vector3 preferido, IList<Rect> ocupados)
        {
            float minX = area.xMin - limites.min.x;
            float maxX = area.xMax - limites.max.x;
            float minZ = area.yMin - limites.min.z;
            float maxZ = area.yMax - limites.max.z;
            Vector3 mejor = preferido;
            float mejorCoste = float.PositiveInfinity;
            for (int i = -1; i < 525; i++)
            {
                float x = i < 0 ? Mathf.Clamp(preferido.x, minX, maxX)
                    : Mathf.Lerp(minX, maxX, ((i % 25) + 0.16f * Mathf.Sin(i * 2.4f)) / 24f);
                float z = i < 0 ? Mathf.Clamp(preferido.z, minZ, maxZ)
                    : Mathf.Lerp(minZ, maxZ, ((i / 25) + 0.16f * Mathf.Cos(i * 1.7f)) / 20f);
                x = Mathf.Clamp(x, minX, maxX);
                z = Mathf.Clamp(z, minZ, maxZ);
                var huella = new Rect(x + limites.min.x, z + limites.min.z, limites.size.x, limites.size.z);
                float coste = 0f;
                foreach (Rect ocupado in ocupados)
                {
                    float ancho = Mathf.Max(0f, Mathf.Min(huella.xMax, ocupado.xMax) - Mathf.Max(huella.xMin, ocupado.xMin));
                    float fondo = Mathf.Max(0f, Mathf.Min(huella.yMax, ocupado.yMax) - Mathf.Max(huella.yMin, ocupado.yMin));
                    if (ancho > 0f && fondo > 0f) coste += 1000000f + ancho * fondo;
                }
                coste += (new Vector2(x, z) - new Vector2(preferido.x, preferido.z)).sqrMagnitude;
                if (coste >= mejorCoste) continue;
                mejorCoste = coste;
                mejor = new Vector3(x, preferido.y, z);
            }
            return mejor;
        }
    }
}