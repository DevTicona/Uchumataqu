using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.Corpus;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.RA
{
    public sealed class PresentacionCantidadesRA : IDisposable
    {
        private sealed class Unidad
        {
            public Transform Raiz;
            public Renderer[] Renderers;
            public bool[] Visibilidad;
            public Collider[] Colliders;
            public bool[] Colisiones;

            public void Mostrar(bool visible)
            {
                for (int i = 0; i < Renderers.Length; i++)
                    if (Renderers[i] != null) Renderers[i].enabled = visible && Visibilidad[i];
                for (int i = 0; i < Colliders.Length; i++)
                    if (Colliders[i] != null) Colliders[i].enabled = visible && Colisiones[i];
            }
        }

        private sealed class Grupo
        {
            public ObjetoRAView Actor;
            public Unidad[] Unidades;
            public BoxCollider Superficie;
        }

        private readonly Dictionary<string, Grupo> grupos = new Dictionary<string, Grupo>();
        private readonly List<Mesh> mallasTemporales = new List<Mesh>();
        private readonly Dictionary<SkinnedMeshRenderer, Mesh> mallasHorneadas = new Dictionary<SkinnedMeshRenderer, Mesh>();
        private readonly Dictionary<string, GameObject[]> decorativos = new Dictionary<string, GameObject[]>();
        private readonly Dictionary<GameObject, bool> visibilidadDecorativos = new Dictionary<GameObject, bool>();
        private readonly HashSet<GameObject> decorativosPracticados = new HashSet<GameObject>();
        private readonly Transform escenario;
        private GameObject replicas;

        public PresentacionCantidadesRA(Transform escenario, EscenarioRAModel datos, IDictionary<string, ObjetoRAView> objetos)
        {
            this.escenario = escenario;
            // RA2 conserva su composición y su comportamiento actual.
            if (datos.id == "ra_familia_vestimenta") return;
            if (datos.id == "ra_animales_ecosistema")
            {
                var decoracion = escenario.Find("Decoracion");
                var ids = objetos.Keys.OrderByDescending(id => id.Length).ToArray();
                foreach (string id in objetos.Keys)
                {
                    var adicionales = decoracion.GetComponentsInChildren<Transform>(true)
                        .Where(t => ids.FirstOrDefault(clave => t.name.StartsWith(clave + "_", StringComparison.Ordinal)) == id)
                        .Select(t => t.gameObject).ToArray();
                    decorativos.Add(id, adicionales);
                    foreach (var adicional in adicionales) visibilidadDecorativos[adicional] = adicional.activeSelf;
                }
            }
            foreach (var paso in datos.mision.fases.SelectMany(f => f.pasos).Where(p => p.cantidad > 1))
            {
                if (grupos.ContainsKey(paso.objetivo_objeto_id)) continue;
                var actor = objetos[paso.objetivo_objeto_id];
                var raices = paso.unidades_cantidad?.Count > 0
                    ? paso.unidades_cantidad.Select(ruta => actor.transform.Find(ruta) ??
                        throw new InvalidOperationException("Unidad RA ausente: " + paso.id + "/" + ruta)).ToArray()
                    : new[] { actor.transform };
                var unidades = raices.Select(raiz =>
                {
                    var renderers = raiz.GetComponentsInChildren<Renderer>(true).Where(r =>
                        r.GetComponentInParent<ObjetoRAView>(true) == actor && r.GetComponentInParent<DestinoRA>(true) == null).ToArray();
                    var colliders = raiz.GetComponentsInChildren<Collider>(true).Where(c =>
                        c.GetComponentInParent<ObjetoRAView>(true) == actor && c.GetComponentInParent<DestinoRA>(true) == null).ToArray();
                    return new Unidad { Raiz = raiz, Renderers = renderers, Visibilidad = renderers.Select(r => r.enabled).ToArray(),
                        Colliders = colliders, Colisiones = colliders.Select(c => c.enabled).ToArray() };
                }).ToArray();
                BoxCollider superficie = null;
                if (datos.id == "ra_cocina_alimentos" && objetos.TryGetValue("obj_mesa", out var mesa))
                    superficie = mesa.transform.Find("ColliderTablero")?.GetComponent<BoxCollider>();
                grupos.Add(actor.ObjetoId, new Grupo { Actor = actor, Unidades = unidades, Superficie = superficie });
            }
            Restablecer();
        }

        public void Restablecer()
        {
            if (replicas != null) { replicas.SetActive(false); Destruir(replicas); replicas = null; }
            foreach (var malla in mallasTemporales) Destruir(malla);
            mallasTemporales.Clear();
            mallasHorneadas.Clear();
            foreach (var decorativo in visibilidadDecorativos)
                if (decorativo.Key != null) decorativo.Key.SetActive(decorativo.Value && !decorativosPracticados.Contains(decorativo.Key));
            foreach (var grupo in grupos.Values)
            {
                for (int i = 0; i < grupo.Unidades.Length; i++) grupo.Unidades[i].Mostrar(i == 0);
                if (grupo.Unidades.Length > 1 && grupo.Actor != null)
                    grupo.Actor.SeleccionarAncla(grupo.Unidades[0].Raiz.position);
            }
        }

        public void PrepararActividad(PasoRAModel paso)
        {
            if (paso?.objetivo_objeto_id != null && decorativos.TryGetValue(paso.objetivo_objeto_id, out var adicionales))
                foreach (var adicional in adicionales)
                {
                    decorativosPracticados.Add(adicional);
                    adicional.SetActive(false);
                }
        }

        public void Reiniciar()
        {
            decorativosPracticados.Clear();
            Restablecer();
        }

        public void Mostrar(PasoRAModel paso)
        {
            Restablecer();
            PrepararActividad(paso);
            int cantidad = Mathf.Max(1, paso.cantidad);
            if (cantidad == 1 || !grupos.TryGetValue(paso.objetivo_objeto_id, out var grupo)) return;
            bool originalVisible = grupo.Unidades[0].Renderers.Any(r => r.enabled && r.gameObject.activeInHierarchy);
            if (grupo.Unidades.Length >= cantidad && originalVisible)
            {
                for (int i = 0; i < grupo.Unidades.Length; i++) grupo.Unidades[i].Mostrar(i < cantidad);
                return;
            }

            replicas = new GameObject("CantidadOracion_" + paso.id);
            replicas.transform.SetParent(escenario, false);
            var unidad = grupo.Unidades[0];
            var visibles = unidad.Renderers.Where((r, i) => unidad.Visibilidad[i]).ToArray();
            if (visibles.Length == 0) return;
            var fauna = escenario.GetComponent<DistribucionFaunaRA>();
            if (fauna != null && fauna.ObtenerZona(grupo.Actor.transform, out var espacio, out var area))
            {
                MostrarFauna(paso, grupo, visibles, espacio, area, cantidad);
                return;
            }
            var limites = LimitesEnEscenario(visibles[0].localBounds, visibles[0].transform);
            foreach (var renderer in visibles) limites.Encapsulate(LimitesEnEscenario(renderer.localBounds, renderer.transform));
            if (paso.posiciones_cantidad?.Count > 0)
            {
                if (paso.posiciones_cantidad.Count != cantidad || paso.escalas_cantidad.Count != cantidad)
                    throw new InvalidOperationException("Distribución de cantidad incompleta: " + paso.id);
                unidad.Mostrar(false);
                for (int i = 0; i < cantidad; i++)
                {
                    Vector3 destino = grupo.Actor.transform.parent.TransformPoint(paso.posiciones_cantidad[i]);
                    var copia = new GameObject("Unidad_" + (i + 1));
                    copia.transform.SetParent(replicas.transform, false);
                    copia.transform.position = destino;
                    foreach (var renderer in visibles) CopiarVisual(renderer, copia.transform, Vector3.zero);
                    var mallas = copia.GetComponentsInChildren<Renderer>();
                    var geometria = mallas[0].bounds;
                    foreach (var malla in mallas) geometria.Encapsulate(malla.bounds);
                    Vector3 ajuste = destino - new Vector3(geometria.center.x, geometria.min.y, geometria.center.z);
                    foreach (Transform nodo in copia.transform) nodo.position += ajuste;
                    copia.transform.localScale *= paso.escalas_cantidad[i];
                }
                return;
            }
            float margen = 0.04f * grupo.Actor.transform.lossyScale.x / escenario.lossyScale.x;
            float ancho = Mathf.Max(limites.size.x, margen) + margen;
            float fondo = Mathf.Max(limites.size.z, margen) + margen;
            int columnas = Mathf.CeilToInt(Mathf.Sqrt(cantidad));
            Vector3 origen = originalVisible ? Vector3.zero : escenario.InverseTransformPoint(grupo.Actor.AnclaDialogo.position) - limites.center;
            bool incluirOriginal = !originalVisible || grupo.Superficie != null;
            if (grupo.Superficie != null)
            {
                var mesa = LimitesEnEscenario(new Bounds(grupo.Superficie.center, grupo.Superficie.size), grupo.Superficie.transform);
                columnas = Mathf.CeilToInt(Mathf.Sqrt(cantidad * mesa.size.x / mesa.size.z));
                int filas = Mathf.CeilToInt((float)cantidad / columnas);
                origen = new Vector3(mesa.center.x - limites.center.x - (columnas - 1) * ancho / 2f,
                    0f, mesa.center.z - limites.center.z - (filas - 1) * fondo / 2f);
                grupo.Unidades[0].Mostrar(false);
            }
            for (int i = incluirOriginal ? 0 : 1; i < cantidad; i++)
            {
                Vector3 offset = escenario.TransformVector(origen + Vector3.right * (i % columnas * ancho) + Vector3.forward * (i / columnas * fondo));
                var copia = new GameObject("Unidad_" + (i + 1));
                copia.transform.SetParent(replicas.transform, false);
                foreach (var renderer in visibles) CopiarVisual(renderer, copia.transform, offset);
            }
        }

        private Bounds LimitesEnEscenario(Bounds limites, Transform espacio)
        {
            var resultado = new Bounds(escenario.InverseTransformPoint(espacio.TransformPoint(limites.center)), Vector3.zero);
            for (int i = 0; i < 8; i++)
            {
                var esquina = limites.center + Vector3.Scale(limites.extents,
                    new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                resultado.Encapsulate(escenario.InverseTransformPoint(espacio.TransformPoint(esquina)));
            }
            return resultado;
        }

        private void CopiarVisual(Renderer fuente, Transform padre, Vector3 offset)
        {
            Mesh malla;
            if (fuente is SkinnedMeshRenderer piel)
            {
                // Todas las réplicas de esta presentación muestran la misma pose.
                if (!mallasHorneadas.TryGetValue(piel, out malla))
                {
                    malla = new Mesh();
                    piel.BakeMesh(malla);
                    mallasTemporales.Add(malla);
                    mallasHorneadas.Add(piel, malla);
                }
            }
            else
            {
                malla = fuente.GetComponent<MeshFilter>()?.sharedMesh;
                if (malla == null) return;
            }
            var nodo = new GameObject(fuente.name);
            nodo.transform.SetPositionAndRotation(fuente.transform.position + offset, fuente.transform.rotation);
            // BakeMesh ya incorpora la escala del rig en sus vértices.
            nodo.transform.localScale = fuente is SkinnedMeshRenderer ? Vector3.one : fuente.transform.lossyScale;
            nodo.transform.SetParent(padre, true);
            nodo.AddComponent<MeshFilter>().sharedMesh = malla;
            var renderer = nodo.AddComponent<MeshRenderer>();
            renderer.sharedMaterials = fuente.sharedMaterials;
            var propiedades = new MaterialPropertyBlock();
            fuente.GetPropertyBlock(propiedades);
            renderer.SetPropertyBlock(propiedades);
        }

        private static void Destruir(Object objeto)
        {
            if (Application.isPlaying) Object.Destroy(objeto);
            else Object.DestroyImmediate(objeto);
        }

        public void Dispose() => Reiniciar();


        private void MostrarFauna(PasoRAModel paso, Grupo grupo, Renderer[] visibles,
            Transform espacio, Rect area, int cantidad)
        {
            Bounds original = DistribucionFaunaRA.Medir(visibles, espacio);
            var ocupados = new List<Rect>();
            grupo.Unidades[0].Mostrar(false);
            for (int i = 0; i < cantidad; i++)
            {
                var copia = new GameObject("Unidad_" + (i + 1)).transform;
                copia.SetParent(replicas.transform, false);
                foreach (var renderer in visibles) CopiarVisual(renderer, copia, Vector3.zero);
                var mallas = copia.GetComponentsInChildren<Renderer>();
                Bounds geometria = DistribucionFaunaRA.Medir(mallas, copia);
                Vector3 apoyo = new Vector3(geometria.center.x, geometria.min.y, geometria.center.z);
                foreach (Transform nodo in copia) nodo.localPosition -= apoyo;
                copia.localRotation = Quaternion.Euler(0f, DistribucionFaunaRA.Giro(i), 0f);
                float escala = paso.escalas_cantidad?.Count == cantidad ? paso.escalas_cantidad[i] : 1f;
                copia.localScale = Vector3.one * escala;
                Bounds huella = DistribucionFaunaRA.Medir(mallas, espacio);
                huella.center -= espacio.InverseTransformPoint(copia.position);
                int columnas = Mathf.CeilToInt(Mathf.Sqrt(cantidad * area.width / area.height));
                int filas = Mathf.CeilToInt((float)cantidad / columnas);
                Vector3 preferido = new Vector3(
                    Mathf.Lerp(area.xMin, area.xMax, (i % columnas + 0.5f) / columnas),
                    original.min.y,
                    Mathf.Lerp(area.yMin, area.yMax, (i / columnas + 0.5f) / filas));
                if (paso.posiciones_cantidad?.Count == cantidad) preferido = paso.posiciones_cantidad[i];
                Vector3 destino = DistribucionFaunaRA.BuscarApoyo(area, huella, preferido, ocupados);
                copia.position = espacio.TransformPoint(destino);
                const float separacion = 0.035f;
                ocupados.Add(new Rect(destino.x + huella.min.x - separacion,
                    destino.z + huella.min.z - separacion,
                    huella.size.x + 2f * separacion, huella.size.z + 2f * separacion));
            }
        }
}
}
