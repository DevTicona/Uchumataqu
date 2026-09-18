using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.UI.LeccionBase.Componentes;
using Assets.Scripts.UI.LeccionBase.Actividades;

namespace Assets.Scripts.UI.LeccionBase
{
    /// <summary>
    /// Vista principal de la escena 03_LeccionBase.
    /// Especificado en Docs/UI/04-escena-leccion-base.md.
    /// </summary>
    public class LeccionBaseView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TMP_Text txtTituloLeccion;
        [SerializeField] private Slider sliderProgreso;
        [SerializeField] private TMP_Text txtProgreso;
        [SerializeField] private TMP_Text txtPorcentajeProgreso;
        [SerializeField] private Button btnAtras;

        [Header("Contenedor de Actividades")]
        [SerializeField] private RectTransform actividadContainer;

        [Header("Pages de escena")]
        [SerializeField] private GameObject pagePresentacion;
        [SerializeField] private GameObject pageSeleccionMultiple;
        [SerializeField] private GameObject pageEmparejamiento;
        [SerializeField] private GameObject pageMemoria;
        [SerializeField] private GameObject pageWordBank;

        [SerializeField] private GameObject pageASR;
        [Header("Acción principal")]
        [SerializeField] private Button btnPrincipal;
        [SerializeField] private TMP_Text txtAccionPrincipal;
        [SerializeField] private Image iconoAccionPrincipal;
        [SerializeField] private Sprite iconoContinuar;
        [SerializeField] private Sprite iconoComprobar;
        [SerializeField] private Sprite iconoFinalizar;
        [SerializeField] private Sprite iconoDetener;
        [SerializeField] private Button btnOmitirASR;

        public enum AccionPrincipal { Continuar, Comprobar, Finalizar }
        public event Action OnAccionPrincipalClick;

        [Header("Overlays")]
        [SerializeField] private FeedbackOverlayView feedbackOverlay;
        [SerializeField] private ModalResultadoView modalResultado;
        [SerializeField] private GameObject panelConfirmacionSalida;
        [SerializeField] private Button btnSeguirAprendiendo;
        [SerializeField] private Button btnConfirmarSalida;

        public event Action OnAtrasClick;
        public event Action OnVolverAlMapaClick;
        public event Action OnContinuarLeccionClick;
        public event Action OnConfirmarSalidaClick;
        public event Action OnCancelarSalidaClick;

        public FeedbackOverlayView Feedback => feedbackOverlay;
        public ModalResultadoView ModalResultado => modalResultado;

        private void Awake()
        {
            if (btnPrincipal != null) btnPrincipal.onClick.AddListener(NotificarAccionPrincipal);
            if (btnAtras != null) btnAtras.onClick.AddListener(NotificarAtras);

            if (modalResultado == null)
                modalResultado = GetComponentInChildren<ModalResultadoView>(true);

            if (modalResultado != null)
            {
                modalResultado.OnContinuarClicked += NotificarContinuarLeccion;
                modalResultado.OnVolverMapaClicked += NotificarVolverAlMapa;
                modalResultado.Ocultar();
            }

            if (btnSeguirAprendiendo != null)
                btnSeguirAprendiendo.onClick.AddListener(NotificarCancelarSalida);
            if (btnConfirmarSalida != null)
                btnConfirmarSalida.onClick.AddListener(NotificarConfirmarSalida);

            if (panelConfirmacionSalida != null)
                panelConfirmacionSalida.SetActive(false);
            feedbackOverlay?.Ocultar();
        }

        private void OnDestroy()
        {
            if (btnPrincipal != null) btnPrincipal.onClick.RemoveListener(NotificarAccionPrincipal);
            if (btnAtras != null) btnAtras.onClick.RemoveListener(NotificarAtras);
            if (modalResultado != null)
            {
                modalResultado.OnContinuarClicked -= NotificarContinuarLeccion;
                modalResultado.OnVolverMapaClicked -= NotificarVolverAlMapa;
            }
            if (btnSeguirAprendiendo != null)
                btnSeguirAprendiendo.onClick.RemoveListener(NotificarCancelarSalida);
            if (btnConfirmarSalida != null)
                btnConfirmarSalida.onClick.RemoveListener(NotificarConfirmarSalida);
        }

        public bool ValidarConfiguracion(out string error)
        {
            if (btnPrincipal == null || txtAccionPrincipal == null || iconoAccionPrincipal == null ||
                iconoContinuar == null || iconoComprobar == null || iconoFinalizar == null || iconoDetener == null || btnOmitirASR == null)
                return FallarValidacion("El footer tiene referencias incompletas.", out error);
            if (txtTituloLeccion == null || sliderProgreso == null || txtProgreso == null ||
                txtPorcentajeProgreso == null || btnAtras == null)
                return FallarValidacion("El header de la lección tiene referencias incompletas.", out error);
            if (actividadContainer == null)
                return FallarValidacion("Falta el contenedor de actividades.", out error);
            if (pagePresentacion == null || pageSeleccionMultiple == null || pageEmparejamiento == null ||
                pageMemoria == null || pageWordBank == null || pageASR == null)
                return FallarValidacion("Falta una o más Pages de actividad.", out error);
            if (feedbackOverlay == null || modalResultado == null || panelConfirmacionSalida == null ||
                btnSeguirAprendiendo == null || btnConfirmarSalida == null)
                return FallarValidacion("Los overlays de feedback, resultado o salida están incompletos.", out error);

            if (!feedbackOverlay.ValidarConfiguracion(out error)) return false;
            if (!modalResultado.ValidarConfiguracion(out error)) return false;

            error = null;
            return true;
        }

        private static bool FallarValidacion(string mensaje, out string error)
        {
            error = mensaje;
            return false;
        }

        public void ConfigurarHeader(string titulo, float progreso)
        {
            if (txtTituloLeccion != null) txtTituloLeccion.text = titulo;
            ActualizarProgreso(progreso);
        }

        public void ActualizarProgreso(float progreso)
        {
            float valorNormalizado = Mathf.Clamp01(progreso);
            if (sliderProgreso != null) sliderProgreso.value = valorNormalizado;
            if (txtProgreso != null) txtProgreso.text = "Progreso";
            if (txtPorcentajeProgreso != null)
                txtPorcentajeProgreso.text = $"{Mathf.RoundToInt(valorNormalizado * 100f)}%";
        }

        public void LimpiarActividadActual()
        {
            foreach (GameObject page in new[] { pagePresentacion, pageSeleccionMultiple, pageEmparejamiento, pageMemoria, pageWordBank, pageASR })
                if (page != null) page.SetActive(false);
        }

        public ASRView InstanciarASR() => InstanciarActividad<ASRView>(pageASR);
        public PresentacionFamiliaView InstanciarPresentacion()
        {
            return InstanciarActividad<PresentacionFamiliaView>(pagePresentacion);
        }

        public SeleccionMultipleView InstanciarSeleccionMultiple()
        {
            return InstanciarActividad<SeleccionMultipleView>(pageSeleccionMultiple);
        }

        public EmparejamientoView InstanciarEmparejamiento()
        {
            return InstanciarActividad<EmparejamientoView>(pageEmparejamiento);
        }

        public WordBankView InstanciarWordBank()
        {
            return InstanciarActividad<WordBankView>(pageWordBank);
        }

        public MemoriaView InstanciarMemoria()
        {
            return InstanciarActividad<MemoriaView>(pageMemoria);
        }

        private T InstanciarActividad<T>(GameObject page) where T : Component
        {
            LimpiarActividadActual();
            if (page == null || !page.TryGetComponent(out T actividad))
                throw new InvalidOperationException($"Falta la Page de {typeof(T).Name}.");
            page.SetActive(true);
            foreach (ScrollRect scroll in page.GetComponentsInChildren<ScrollRect>())
            {
                scroll.StopMovement();
                scroll.verticalNormalizedPosition = 1f;
            }
            page.GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
            return actividad;
        }
        public void MostrarFeedback(string semaforo, string detalle = null, double similitud = 0, string correcta = null)
        {
            if (feedbackOverlay != null)
            {
                if (semaforo == "verde")
                {
                    feedbackOverlay.MostrarCorrecto(detalle);
                }
                else if (semaforo == "amarillo")
                {
                    feedbackOverlay.MostrarCasi(detalle, similitud);
                }
                else if (semaforo == "informacion")
                {
                    feedbackOverlay.MostrarInformacion(detalle);
                }
                else
                {
                    feedbackOverlay.MostrarIncorrecto(detalle ?? correcta);
                }
            }
        }

        public void OcultarFeedback()
        {
            if (feedbackOverlay != null)
                feedbackOverlay.Ocultar();
        }

        public void MostrarResultado(int aciertos, int total, double precision, int estrellas)
        {
            if (modalResultado != null)
            {
                modalResultado.Mostrar(aciertos, total, precision, estrellas);
            }
        }

        public void MostrarConfirmacionSalida(bool mostrar)
        {
            if (panelConfirmacionSalida != null) panelConfirmacionSalida.SetActive(mostrar);
        }

        public void PresentarAccionPrincipal(AccionPrincipal accion, bool interactuable, bool visible = true)
        {
            txtAccionPrincipal.text = accion == AccionPrincipal.Comprobar ? "COMPROBAR" :
                accion == AccionPrincipal.Finalizar ? "FINALIZAR" : "CONTINUAR";
            iconoAccionPrincipal.sprite = accion == AccionPrincipal.Comprobar ? iconoComprobar :
                accion == AccionPrincipal.Finalizar ? iconoFinalizar : iconoContinuar;
            btnPrincipal.interactable = interactuable;
            btnPrincipal.gameObject.SetActive(visible);
        }

        public void MostrarDetenerGrabacion()
        {
            txtAccionPrincipal.text = "DETENER";
            iconoAccionPrincipal.sprite = iconoDetener;
            btnPrincipal.interactable = true;
            btnPrincipal.gameObject.SetActive(true);
        }

        public void EstablecerInteraccionDuranteGrabacion(bool grabando)
        {
            if (btnAtras != null) btnAtras.interactable = !grabando;
            if (btnOmitirASR != null) btnOmitirASR.interactable = !grabando;
        }

        public void MostrarOmitirASR(bool visible) => btnOmitirASR.gameObject.SetActive(visible);

        public void ConfigurarTextoOmitirASR(string texto)
        {
            TMP_Text etiqueta = btnOmitirASR.GetComponentInChildren<TMP_Text>();
            if (etiqueta != null) etiqueta.text = texto;
        }

        private void NotificarAccionPrincipal() => OnAccionPrincipalClick?.Invoke();
        private void NotificarAtras() => OnAtrasClick?.Invoke();
        private void NotificarVolverAlMapa() => OnVolverAlMapaClick?.Invoke();
        private void NotificarContinuarLeccion() => OnContinuarLeccionClick?.Invoke();
        private void NotificarConfirmarSalida() => OnConfirmarSalidaClick?.Invoke();
        private void NotificarCancelarSalida() => OnCancelarSalidaClick?.Invoke();
    }
}
