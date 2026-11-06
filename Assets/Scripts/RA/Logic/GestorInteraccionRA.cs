using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Scripts.RA
{
    public sealed class GestorInteraccionRA : MonoBehaviour
    {
        [SerializeField] private Camera camara;
        [SerializeField] private LayerMask capasInteractivas;
        [SerializeField] private float distanciaMaxima = 15f;

        private readonly List<RaycastResult> resultadosUI = new List<RaycastResult>();
        private ObjetoRAView objetoArrastrado;
        private Vector2 inicioArrastre;
        private Vector3 posicionAntesArrastre;
        private Vector3 offsetArrastre;
        private Plane planoArrastre;
        private bool desplazado;
        public bool PermitirArrastre { get; set; }
        public Func<ObjetoRAView, bool> PuedeArrastrarObjeto { get; set; }
        public Func<ObjetoRAView, Vector2, DestinoRA> ResolverDestinoArrastre { get; set; }
        public bool EstaArrastrando => objetoArrastrado != null;

        public Camera Camara => camara;

        public event Action<Vector2> PulsacionLibre;
        public event Action<ObjetoRAView> ObjetoTocado;
        public event Action<DestinoRA> DestinoTocado;
        public event Action<ActorNarrativoRA> ActorTocado;
        public event Action<ObjetoRAView, DestinoRA> ArrastreCompletado;

        public void Configurar(Camera origen, LayerMask capas)
        {
            camara = origen;
            capasInteractivas = capas;
        }

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null || camara == null) { CancelarArrastre(); return; }
            Vector2 posicion = pointer.position.ReadValue();
            if (objetoArrastrado != null)
            {
                if (!PermitirArrastre || !objetoArrastrado.gameObject.activeInHierarchy)
                { CancelarArrastre(); return; }
                if (pointer.press.isPressed || pointer.press.wasReleasedThisFrame)
                    SeguirPuntero(posicion);
                if (pointer.press.wasReleasedThisFrame)
                {
                    var origen = objetoArrastrado;
                    DestinoRA destino = null;
                    if (desplazado && !SobreUI(posicion))
                    {
                        if (ResolverDestinoArrastre != null) destino = ResolverDestinoArrastre(origen, posicion);
                        else if (ResolverImpacto(camara.ScreenPointToRay(posicion), out var final))
                            destino = final.collider.GetComponentInParent<DestinoRA>();
                    }
                    bool notificar = desplazado;
                    if (destino == null) origen.transform.position = posicionAntesArrastre;
                    objetoArrastrado = null;
                    if (notificar) ArrastreCompletado?.Invoke(origen, destino);
                }
                else if (!pointer.press.isPressed) CancelarArrastre();
                return;
            }
            if (!pointer.press.wasPressedThisFrame) return;
            objetoArrastrado = null;
            if (SobreUI(posicion)) return;
            inicioArrastre = posicion;
            if (ResolverImpacto(camara.ScreenPointToRay(posicion), out var impacto))
            {
                var destino = impacto.collider.GetComponentInParent<DestinoRA>();
                if (destino != null) { DestinoTocado?.Invoke(destino); return; }
                var objeto = impacto.collider.GetComponentInParent<ObjetoRAView>();
                if (objeto != null)
                {
                    objeto.SeleccionarAncla(impacto.point);
                    bool arrastrable = PermitirArrastre && (PuedeArrastrarObjeto?.Invoke(objeto) ?? true);
                    ObjetoTocado?.Invoke(objeto);
                    if (arrastrable)
                    {
                        objeto.EstablecerResaltado(false);
                        objetoArrastrado = objeto;
                        posicionAntesArrastre = objeto.transform.position;
                        planoArrastre = new Plane(camara.transform.forward, impacto.point);
                        offsetArrastre = objeto.transform.position - impacto.point;
                        desplazado = false;
                    }
                    return;
                }
                var actor = impacto.collider.GetComponentInParent<ActorNarrativoRA>();
                if (actor != null) { ActorTocado?.Invoke(actor); return; }
            }
            PulsacionLibre?.Invoke(posicion);
        }

        private void SeguirPuntero(Vector2 posicion)
        {
            desplazado |= Vector2.Distance(inicioArrastre, posicion) >= 2f;
            if (desplazado && planoArrastre.Raycast(camara.ScreenPointToRay(posicion), out float distancia))
                objetoArrastrado.transform.position = camara.ScreenPointToRay(posicion).GetPoint(distancia) + offsetArrastre;
        }

        public void CancelarArrastre()
        {
            if (objetoArrastrado != null) objetoArrastrado.transform.position = posicionAntesArrastre;
            objetoArrastrado = null;
            desplazado = false;
        }

        public bool ResolverImpacto(Ray rayo, out RaycastHit impacto)
        {
            var impactos = Physics.RaycastAll(rayo, distanciaMaxima, capasInteractivas);
            Array.Sort(impactos, (a, b) => a.distance.CompareTo(b.distance));
            impacto = default;
            bool encontrado = false;
            foreach (var candidato in impactos)
            {
                if (objetoArrastrado != null && candidato.collider.transform.IsChildOf(objetoArrastrado.transform)) continue;
                var destino = candidato.collider.GetComponentInParent<DestinoRA>();
                if (destino != null && !destino.IndicadorVisible) continue;
                if (!encontrado) { impacto = candidato; encontrado = true; }
                // El collider del cuerpo no debe ocultar su propio destino de vestimenta.
                if (destino != null && (impacto.collider == candidato.collider ||
                    (impacto.collider.GetComponentInParent<ObjetoRAView>() is ObjetoRAView cuerpo &&
                     destino.transform.IsChildOf(cuerpo.transform))))
                {
                    impacto = candidato;
                    break;
                }
                var prenda = candidato.collider.GetComponentInParent<ObjetoRAView>();
                var personaje = impacto.collider.GetComponentInParent<ObjetoRAView>();
                if (prenda != null && personaje != null && prenda != personaje &&
                    prenda.GetComponent<PrendaRAView>() != null && prenda.transform.IsChildOf(personaje.transform))
                    impacto = candidato;
            }
            return encontrado;
        }

        private bool SobreUI(Vector2 posicion)
        {
            if (EventSystem.current != null)
            {
                resultadosUI.Clear();
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = posicion }, resultadosUI);
                return resultadosUI.Count > 0;
            }
            return false;
        }

        private void OnDisable() => CancelarArrastre();
    }
}
