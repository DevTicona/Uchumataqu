using UnityEngine;

namespace Assets.Scripts.RA
{
    [System.Serializable]
    public sealed class PuntoDestinoObjetoRA
    {
        public string objetoId;
        public Transform punto;
    }

    public sealed class DestinoRA : MonoBehaviour
    {
        [SerializeField] private string destinoId;
        [SerializeField] private Transform puntoColocacion;
        [SerializeField] private GameObject indicador;
        [SerializeField] private bool ocultarAlColocar = true;
        [SerializeField] private ObjetoRAView objetoReceptor;
        public ObjetoRAView ObjetoReceptor => objetoReceptor;
        [SerializeField] private PuntoDestinoObjetoRA[] puntosPorObjeto = System.Array.Empty<PuntoDestinoObjetoRA>();
        public string DestinoId => destinoId;
        public Transform PuntoColocacion => puntoColocacion != null ? puntoColocacion : transform;
        public bool OcultarAlColocar => ocultarAlColocar;
        public bool IndicadorVisible => indicador != null ? indicador.activeInHierarchy : gameObject.activeInHierarchy;
        public void Configurar(string id) => destinoId = id;

        public Transform PuntoPara(string objetoId)
        {
            foreach (var punto in puntosPorObjeto)
                if (punto.objetoId == objetoId && punto.punto != null) return punto.punto;
            return PuntoColocacion;
        }

        public void ConfigurarPuntos(PuntoDestinoObjetoRA[] puntos) => puntosPorObjeto = puntos;

        public bool ContieneEnPantalla(Vector2 posicion, Camera camara)
        {
            if (ObtenerAreaEnPantalla(camara, out var area) && area.Contains(posicion)) return true;
            if (!IndicadorVisible || objetoReceptor == null) return false;
            foreach (var renderer in objetoReceptor.GetComponentsInChildren<Renderer>())
                if (renderer.enabled && ProyectarLimites(renderer.bounds, camara, out area) && area.Contains(posicion)) return true;
            return false;
        }

        public bool SolapaTarjetaEnPantalla(Rect tarjeta, Camera camara)
        {
            if (tarjeta.width <= 0f || tarjeta.height <= 0f || !IndicadorVisible) return false;
            if (ObtenerAreaEnPantalla(camara, out var area) && Solapa(tarjeta, area)) return true;
            if (objetoReceptor == null) return false;
            foreach (var renderer in objetoReceptor.GetComponentsInChildren<Renderer>())
                if (renderer.enabled && ProyectarLimites(renderer.bounds, camara, out area) && Solapa(tarjeta, area)) return true;
            return false;
        }

        private static bool Solapa(Rect tarjeta, Rect area)
        {
            float ancho = Mathf.Max(0f, Mathf.Min(tarjeta.xMax, area.xMax) - Mathf.Max(tarjeta.xMin, area.xMin));
            float alto = Mathf.Max(0f, Mathf.Min(tarjeta.yMax, area.yMax) - Mathf.Max(tarjeta.yMin, area.yMin));
            return ancho * alto >= Mathf.Min(tarjeta.width * tarjeta.height, area.width * area.height) * 0.25f;
        }

        private bool ObtenerAreaEnPantalla(Camera camara, out Rect area)
        {
            area = default;
            if (!IndicadorVisible || camara == null || indicador == null) return false;
            var collider = indicador.GetComponent<Collider>();
            if (collider == null || !collider.enabled) return false;
            return ProyectarLimites(collider.bounds, camara, out area);
        }

        private static bool ProyectarLimites(Bounds limites, Camera camara, out Rect area)
        {
            area = default;
            if (camara == null) return false;
            Vector2 minimo = Vector2.one * float.PositiveInfinity;
            Vector2 maximo = Vector2.one * float.NegativeInfinity;
            for (int i = 0; i < 8; i++)
            {
                Vector3 esquina = limites.center + Vector3.Scale(limites.extents, new Vector3(
                    (i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
                Vector3 pantalla = camara.WorldToScreenPoint(esquina);
                if (pantalla.z <= 0f) return false;
                minimo = Vector2.Min(minimo, pantalla);
                maximo = Vector2.Max(maximo, pantalla);
            }
            float margen = Mathf.Max(12f, camara.pixelHeight * 0.015f);
            area = Rect.MinMaxRect(minimo.x - margen, minimo.y - margen, maximo.x + margen, maximo.y + margen);
            return true;
        }

        public void MostrarIndicador(bool visible)
        {
            if (indicador != null) indicador.SetActive(visible);
            else gameObject.SetActive(visible);
        }
    }
}
