using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Assets.Scripts.RA
{
    public sealed class ColocadorEscenarioRA : MonoBehaviour
    {
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private ARAnchorManager anchorManager;
        [SerializeField] private SesionRAController sesion;
        [SerializeField] private Camera camara;
        [SerializeField] private GameObject guiaCentro;
        [SerializeField, Min(0.1f)] private float radioVisualizacion = 0.6f;
        [SerializeField, Min(0f)] private float tiempoConfirmacion = 0.3f;
        [SerializeField, Min(0.1f)] private float distanciaMinima = 0.25f;
        [SerializeField, Min(0.1f)] private float distanciaMaxima = 4f;
        private readonly List<ARRaycastHit> impactos = new List<ARRaycastHit>();
        private MaterialPropertyBlock propiedadesPlano;
        private static readonly int PuntoGuiaId = Shader.PropertyToID("_FocusPoint");
        private static readonly int RadioGuiaId = Shader.PropertyToID("_FocusRadius");
        private ARPlane planoCentral;
        private Vector3 inicioConfirmacion;
        private float tiempoEstable;
        private ARAnchor anclaje;
        private bool creandoAnclaje;
        private int versionAnclaje;
        private bool superficieNotificada;
        private ARPlaneManager planosObservados;

        public event Action SuperficieDetectada;

        public Camera Camara { get => camara; set => camara = value; }
        public GameObject GuiaCentro { get => guiaCentro; set => guiaCentro = value; }
        public bool TienePoseValida { get; private set; }
        public Pose PoseActual { get; private set; }
        public bool EscenarioColocado { get; private set; }
        public Transform SuperficieColocada { get; private set; }
        public bool TieneSeguimiento => ARSession.state == ARSessionState.SessionTracking &&
            (!EscenarioColocado || (anclaje != null && anclaje.trackingState == TrackingState.Tracking));
        public string MensajeEscaneo => !string.IsNullOrEmpty(sesion?.ErrorInicio) ? sesion.ErrorInicio : ARSession.state switch
        {
            ARSessionState.Unsupported => "Este dispositivo no admite realidad aumentada.",
            ARSessionState.NeedsInstall => "Es necesario instalar los servicios de realidad aumentada del dispositivo.",
            ARSessionState.Installing => "Instalando servicios de realidad aumentada...",
            ARSessionState.SessionInitializing => "Buscando seguimiento. Mueve despacio el dispositivo en una zona bien iluminada.",
            ARSessionState.SessionTracking => TienePoseValida
                ? "Superficie detectada en el centro. Toca para colocar."
                : "Apunta el centro a una mesa o al suelo y mueve lentamente el dispositivo.",
            _ => "Preparando la cámara y los servicios de realidad aumentada..."
        };

        public void Configurar(ARRaycastManager raycast, ARPlaneManager planos)
        {
            raycastManager = raycast;
            planeManager = planos;
            if (isActiveAndEnabled) ObservarPlanos();
            camara = raycast != null ? raycast.GetComponent<Unity.XR.CoreUtils.XROrigin>()?.Camera : null;
        }

        public void ConfigurarSesion(ARAnchorManager anclas, SesionRAController controlador)
        {
            anchorManager = anclas;
            sesion = controlador;
        }

        private void LateUpdate()
        {
            if (EscenarioColocado) return;

            if (guiaCentro != null) guiaCentro.SetActive(true);
            bool centroSobrePlano = BuscarPoseCentral(out Pose poseCentral, out ARPlane plano);
            if (!centroSobrePlano || plano != planoCentral ||
                Vector3.Distance(inicioConfirmacion, poseCentral.position) > 0.04f)
            {
                tiempoEstable = 0f;
                inicioConfirmacion = poseCentral.position;
            }
            else tiempoEstable += Time.deltaTime;
            planoCentral = centroSobrePlano ? plano : null;
            if (centroSobrePlano) PoseActual = poseCentral;
            TienePoseValida = centroSobrePlano && tiempoEstable >= tiempoConfirmacion;
            MostrarPlanos(centroSobrePlano);
        }

        // Colocación RA: exige una pose estable sobre un plano y confirma el seguimiento del anclaje
        // antes de permitir que el presentador sitúe el escenario en la superficie física.
        public async Task<bool> IntentarColocarAsync()
        {
            if (EscenarioColocado || creandoAnclaje || !TienePoseValida || anchorManager == null ||
                !anchorManager.isActiveAndEnabled || anchorManager.subsystem == null ||
                !BuscarPoseCentral(out Pose pose, out ARPlane plano) || plano != planoCentral ||
                Vector3.Distance(PoseActual.position, pose.position) > 0.04f) return false;
            int version = ++versionAnclaje;
            creandoAnclaje = true;
            ARAnchor candidato = null;
            var altasConfirmadas = new HashSet<TrackableId>();
            UnityAction<ARTrackablesChangedEventArgs<ARAnchor>> confirmarAltas = cambios =>
            {
                foreach (var alta in cambios.added) altasConfirmadas.Add(alta.trackableId);
            };
            anchorManager.trackablesChanged.AddListener(confirmarAltas);
            try
            {
                if (anchorManager.subsystem.subsystemDescriptor.supportsTrackableAttachments)
                    candidato = anchorManager.AttachAnchor(plano, pose);
                if (candidato == null)
                {
                    var resultado = await anchorManager.TryAddAnchorAsync(pose);
                    if (resultado.status.IsSuccess()) candidato = resultado.value;
                }
                // El proveedor debe notificar el alta antes de usar o retirar el anclaje.
                float limite = Time.realtimeSinceStartup + 5f;
                while (candidato != null && (!altasConfirmadas.Contains(candidato.trackableId) || candidato.trackingState != TrackingState.Tracking) &&
                    anchorManager != null && anchorManager.isActiveAndEnabled && Time.realtimeSinceStartup < limite)
                    await Awaitable.NextFrameAsync();
                if (this == null || version != versionAnclaje || !isActiveAndEnabled || !TieneSeguimiento)
                {
                    EliminarAnclaje(candidato);
                    return false;
                }
                if (candidato == null) return false;
                if (!altasConfirmadas.Contains(candidato.trackableId) || candidato.trackingState != TrackingState.Tracking)
                {
                    EliminarAnclaje(candidato);
                    return false;
                }
                anclaje = candidato;
            }
            catch (Exception ex)
            {
                EliminarAnclaje(candidato);
                Debug.LogWarning("[RA_ANCLAJE] No se pudo fijar el contenido: " + ex.Message);
                return false;
            }
            finally
            {
                if (anchorManager != null) anchorManager.trackablesChanged.RemoveListener(confirmarAltas);
                if (version == versionAnclaje) creandoAnclaje = false;
            }
            PoseActual = pose;
            SuperficieColocada = anclaje.transform;
            EscenarioColocado = true;
            TienePoseValida = false;
            if (guiaCentro != null) guiaCentro.SetActive(false);
            MostrarPlanos(false);
            return true;
        }

        public void Recolocar()
        {
            ++versionAnclaje;
            creandoAnclaje = false;
            EliminarAnclaje(anclaje);
            anclaje = null;
            EscenarioColocado = false;
            SuperficieColocada = null;
            TienePoseValida = false;
            planoCentral = null;
            tiempoEstable = 0f;
            superficieNotificada = false;
            if (planeManager != null) planeManager.enabled = true;
            MostrarPlanos(false);
            if (guiaCentro != null) guiaCentro.SetActive(true);
        }

        private void EliminarAnclaje(ARAnchor anchor)
        {
            if (anchor == null) return;
            if (anchorManager == null || !anchorManager.isActiveAndEnabled ||
                anchorManager.subsystem == null || !anchorManager.TryRemoveAnchor(anchor))
                Destroy(anchor.gameObject);
        }

        private void OnDestroy()
        {
            ++versionAnclaje;
            EliminarAnclaje(anclaje);
        }

        private bool BuscarPoseCentral(out Pose pose, out ARPlane superficie)
        {
            pose = default;
            superficie = null;
            if (!TieneSeguimiento || camara == null || raycastManager == null || planeManager == null ||
                !raycastManager.isActiveAndEnabled || !planeManager.isActiveAndEnabled ||
                !raycastManager.Raycast(camara.pixelRect.center, impactos, TrackableType.PlaneWithinPolygon))
                return false;

            foreach (var impacto in impactos)
            {
                ARPlane plano = planeManager.GetPlane(impacto.trackableId);
                if (plano == null || plano.trackingState != TrackingState.Tracking ||
                    plano.subsumedBy != null || plano.alignment != PlaneAlignment.HorizontalUp ||
                    impacto.distance < distanciaMinima || impacto.distance > distanciaMaxima) continue;
                pose = impacto.pose;
                superficie = plano;
                return true;
            }
            return false;
        }

        private void MostrarPlanos(bool visibles)
        {
            if (planeManager == null) return;
            bool mallaVisible = false;
            foreach (var plano in planeManager.trackables)
            {
                if (plano == null) continue;
                ARPlaneMeshVisualizer visualizador = plano.GetComponent<ARPlaneMeshVisualizer>();
                bool mostrar = visibles && TieneSeguimiento && plano == planoCentral &&
                    plano.trackingState == TrackingState.Tracking &&
                    plano.alignment == PlaneAlignment.HorizontalUp && plano.subsumedBy == null;
                if (visualizador != null) visualizador.enabled = mostrar;
                if (plano.TryGetComponent(out MeshRenderer renderer))
                {
                    renderer.enabled = mostrar;
                    if (mostrar)
                    {
                        propiedadesPlano ??= new MaterialPropertyBlock();
                        renderer.GetPropertyBlock(propiedadesPlano);
                        propiedadesPlano.SetVector(PuntoGuiaId, PoseActual.position);
                        propiedadesPlano.SetFloat(RadioGuiaId, radioVisualizacion);
                        renderer.SetPropertyBlock(propiedadesPlano);
                        if (plano.TryGetComponent(out MeshFilter filtro) && filtro.sharedMesh != null &&
                            filtro.sharedMesh.vertexCount > 0) mallaVisible = true;
                    }
                }
            }
            if (mallaVisible && !superficieNotificada)
            {
                superficieNotificada = true;
                SuperficieDetectada?.Invoke();
            }
        }

        private void OnDisable()
        {
            if (planosObservados != null)
                planosObservados.trackablesChanged.RemoveListener(OcultarPlanosNuevos);
            planosObservados = null;
            ++versionAnclaje;
            creandoAnclaje = false;
            TienePoseValida = false;
            planoCentral = null;
            tiempoEstable = 0f;
            superficieNotificada = false;
            MostrarPlanos(false);
            if (guiaCentro != null) guiaCentro.SetActive(false);
        }

        private void OnEnable() => ObservarPlanos();

        private void ObservarPlanos()
        {
            if (planosObservados == planeManager) return;
            if (planosObservados != null)
                planosObservados.trackablesChanged.RemoveListener(OcultarPlanosNuevos);
            planosObservados = planeManager;
            if (planosObservados != null)
                planosObservados.trackablesChanged.AddListener(OcultarPlanosNuevos);
        }

        private void OcultarPlanosNuevos(ARTrackablesChangedEventArgs<ARPlane> cambios)
        {
            if (!EscenarioColocado) return;
            foreach (var plano in cambios.added)
            {
                if (plano == null) continue;
                if (plano.TryGetComponent(out ARPlaneMeshVisualizer visualizador)) visualizador.enabled = false;
                if (plano.TryGetComponent(out MeshRenderer renderer)) renderer.enabled = false;
            }
        }
    }
}
