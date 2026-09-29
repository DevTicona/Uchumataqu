using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Assets.Scripts.RA
{
    public sealed class ColocadorEscenarioRA : MonoBehaviour
    {
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private Transform reticula;
        private readonly List<ARRaycastHit> impactos = new List<ARRaycastHit>();

        public bool TienePoseValida { get; private set; }
        public Pose PoseActual { get; private set; }
        public bool EscenarioColocado { get; private set; }

        public void Configurar(ARRaycastManager raycast, ARPlaneManager planos, Transform indicador)
        {
            raycastManager = raycast;
            planeManager = planos;
            reticula = indicador;
        }

        private void Update()
        {
            if (EscenarioColocado || raycastManager == null) return;
            TienePoseValida = raycastManager.Raycast(
                new Vector2(Screen.width * 0.5f, Screen.height * 0.5f),
                impactos, TrackableType.PlaneWithinPolygon);
            if (TienePoseValida) PoseActual = impactos[0].pose;
            if (reticula != null)
            {
                reticula.gameObject.SetActive(TienePoseValida);
                if (TienePoseValida) reticula.SetPositionAndRotation(PoseActual.position, PoseActual.rotation);
            }
        }

        public bool IntentarColocar()
        {
            if (EscenarioColocado || !TienePoseValida) return false;
            EscenarioColocado = true;
            if (reticula != null) reticula.gameObject.SetActive(false);
            if (planeManager != null) planeManager.enabled = false;
            return true;
        }

        public void Recolocar()
        {
            EscenarioColocado = false;
            TienePoseValida = false;
            if (planeManager != null) planeManager.enabled = true;
            if (reticula != null) reticula.gameObject.SetActive(false);
        }
    }
}
