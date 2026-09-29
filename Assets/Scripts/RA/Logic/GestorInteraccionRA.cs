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

        public Camera Camara => camara;

        public event Action<Vector2> PulsacionLibre;
        public event Action<ObjetoRAView> ObjetoTocado;
        public event Action<DestinoRA> DestinoTocado;

        public void Configurar(Camera origen, LayerMask capas)
        {
            camara = origen;
            capasInteractivas = capas;
        }

        private void Update()
        {
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame || camara == null) return;
            Vector2 posicion = pointer.position.ReadValue();
            if (EventSystem.current != null)
            {
                resultadosUI.Clear();
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = posicion }, resultadosUI);
                if (resultadosUI.Count > 0) return;
            }
            if (Physics.Raycast(camara.ScreenPointToRay(posicion), out var impacto, distanciaMaxima, capasInteractivas))
            {
                var destino = impacto.collider.GetComponentInParent<DestinoRA>();
                if (destino != null)
                {
                    DestinoTocado?.Invoke(destino);
                    return;
                }

                var objeto = impacto.collider.GetComponentInParent<ObjetoRAView>();
                if (objeto != null)
                {
                    ObjetoTocado?.Invoke(objeto);
                    return;
                }
            }
            PulsacionLibre?.Invoke(posicion);
        }
    }
}
