using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.RA
{
    [Serializable]
    public sealed class ReaccionVisualRA
    {
        public string id;
        public string objetoObjetivo;
        public Animator animator;
        public ObjetoRAView resaltar;
        public string estado;
        [Min(0f)] public float duracion;
        public bool mantenerEstado;
        public int capa;
        [Min(0f)] public float demora;
        public Collider[] habilitarColliders = Array.Empty<Collider>();
        public Collider[] deshabilitarColliders = Array.Empty<Collider>();
        public GameObject[] activar = Array.Empty<GameObject>();
        public GameObject[] desactivar = Array.Empty<GameObject>();
        public Transform[] restaurarEscala = Array.Empty<Transform>();
    }

    public sealed class PresentacionReaccionesRA : MonoBehaviour
    {
        [SerializeField] private ReaccionVisualRA[] reacciones = Array.Empty<ReaccionVisualRA>();
        [SerializeField] private Transform referencias;
        [SerializeField, Min(0.01f)] private float velocidadRecorrido = 0.3f;

        public void Configurar(ReaccionVisualRA[] catalogo, Transform puntos, float velocidad)
        {
            reacciones = catalogo ?? Array.Empty<ReaccionVisualRA>();
            referencias = puntos;
            velocidadRecorrido = Mathf.Max(0.01f, velocidad);
        }

        public bool Contiene(string id) => Array.Exists(reacciones, r => r.id == id) ||
            GetComponent<IAccionEscenarioRA>()?.Contiene(id) == true;

        public IEnumerator Ejecutar(string id, Transform actor = null, IList<string> trayecto = null)
        {
            var accion = GetComponent<IAccionEscenarioRA>();
            if (accion?.Contiene(id) == true) yield return accion.Ejecutar(id);
            float duracion = 0f;
            foreach (var reaccion in reacciones)
            {
                if (!Corresponde(reaccion, id, actor)) continue;
                if (reaccion.demora > 0f)
                {
                    float espera = 0f;
                    while (espera < reaccion.demora)
                    {
                        if (gameObject.activeInHierarchy) espera += Time.deltaTime;
                        yield return null;
                    }
                }
                foreach (var collider in reaccion.habilitarColliders) if (collider != null) collider.enabled = true;
                foreach (var collider in reaccion.deshabilitarColliders) if (collider != null) collider.enabled = false;
                foreach (var objeto in reaccion.activar) if (objeto != null) objeto.SetActive(true);
                foreach (var objeto in reaccion.desactivar) if (objeto != null) objeto.SetActive(false);
                foreach (var pieza in reaccion.restaurarEscala) if (pieza != null) pieza.localScale = Vector3.one;
                if (reaccion.resaltar != null) reaccion.resaltar.Resaltar();
                duracion = Mathf.Max(duracion, reaccion.duracion);
                if (reaccion.animator == null) continue;
                reaccion.animator.CrossFadeInFixedTime(reaccion.estado, 0.12f, reaccion.capa);
            }
            if (actor != null && trayecto != null && trayecto.Count > 0)
            {
                foreach (string puntoId in trayecto)
                {
                    Transform punto = referencias != null ? referencias.Find(puntoId) : null;
                    if (punto == null) throw new InvalidOperationException("Anclaje RA ausente: " + puntoId);
                    while (Vector3.Distance(actor.position, punto.position) > 0.001f)
                    {
                        if (!gameObject.activeInHierarchy) { yield return null; continue; }

                        Vector3 direccion = punto.position - actor.position;
                        direccion.y = 0f;
                        if (direccion.sqrMagnitude > 0.0001f)
                            actor.rotation = Quaternion.LookRotation(direccion);
                        actor.position = Vector3.MoveTowards(actor.position, punto.position,
                            velocidadRecorrido * transform.lossyScale.x * Time.deltaTime);
                        yield return null;
                    }
                }
                actor.rotation = transform.rotation;
            }
            else
            {
                float tiempo = 0f;
                while (tiempo < duracion)
                {
                    if (gameObject.activeInHierarchy) tiempo += Time.deltaTime;
                    yield return null;
                }
            }
            foreach (var reaccion in reacciones)
                if (Corresponde(reaccion, id, actor) && reaccion.animator != null && !reaccion.mantenerEstado)
                    reaccion.animator.CrossFadeInFixedTime("Idle", 0.12f, reaccion.capa);
        }

        private static bool Corresponde(ReaccionVisualRA reaccion, string id, Transform actor)
            => reaccion.id == id && (string.IsNullOrEmpty(reaccion.objetoObjetivo) ||
                (actor != null && actor.GetComponent<ObjetoRAView>()?.ObjetoId == reaccion.objetoObjetivo));

        public void RestaurarIdle()
        {
            GetComponent<IAccionEscenarioRA>()?.Reiniciar();
            var restaurados = new HashSet<Animator>();
            foreach (var reaccion in reacciones)
            {
                if (reaccion.animator == null || !restaurados.Add(reaccion.animator)) continue;
                reaccion.animator.Rebind();
                // Los actores ocultos recuperan su estado al activarse; Update exige un objeto activo.
                if (reaccion.animator.isActiveAndEnabled) reaccion.animator.Update(0f);
            }
        }
    }
}
