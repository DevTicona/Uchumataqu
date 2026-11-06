using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.UI.LeccionBase.Componentes;
using Assets.Scripts.UI.LeccionBase.Actividades;

namespace Assets.Scripts.UI.LeccionBase
{
    /// <summary>
    /// Vista principal de la escena 03_LeccionBase.
    /// Especificado en Docs/Specifications/04-flujo-leccion-app.md.
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
        [SerializeField] private GameObject pageEscritura;

        [SerializeField] private GameObject pageASR;
        [Header("Acción principal")]
        [SerializeField] private Button btnPrincipal;
        [SerializeField] private TMP_Text txtAccionPrincipal;
        [SerializeField] private Image iconoAccionPrincipal;
        [SerializeField] private Sprite iconoContinuar;
        [SerializeField] private Sprite iconoComprobar;
        [SerializeField] private Sprite iconoFinalizar;
        [SerializeField] private Sprite iconoDetener;

        public enum AccionPrincipal { Continuar, Comprobar, Finalizar, Siguiente }
        public event Action OnAccionPrincipalClick;

        [Header("Overlays")]
        [SerializeField] private FeedbackOverlayView feedbackOverlay;
        [SerializeField] private ModalResultadoView modalResultado;
        [SerializeField] private GameObject panelConfirmacionSalida;
        [SerializeField] private Button btnSeguirAprendiendo;
        [SerializeField] private Button btnConfirmarSalida;
        private Vector2 ultimoTamanoFeedback;

        public event Action OnAtrasClick;
        public event Action OnVolverAlMapaClick;
        public event Action OnConfirmarSalidaClick;
        public event Action OnCancelarSalidaClick;

        [SerializeField] private Assets.Scripts.Core.UI.Ayuda.AyudaContextualView ayudaContextual;
        public Assets.Scripts.Core.UI.Ayuda.AyudaContextualView Ayuda => ayudaContextual;

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
            ActualizarEspacioActividad();
        }

        private void OnDestroy()
        {
            if (btnPrincipal != null) btnPrincipal.onClick.RemoveListener(NotificarAccionPrincipal);
            if (btnAtras != null) btnAtras.onClick.RemoveListener(NotificarAtras);
            if (modalResultado != null)
            {
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
                iconoContinuar == null || iconoComprobar == null || iconoFinalizar == null || iconoDetener == null)
                return FallarValidacion("El footer tiene referencias incompletas.", out error);
            if (txtTituloLeccion == null || sliderProgreso == null || txtProgreso == null ||
                txtPorcentajeProgreso == null || btnAtras == null)
                return FallarValidacion("El header de la lección tiene referencias incompletas.", out error);
            if (actividadContainer == null)
                return FallarValidacion("Falta el contenedor de actividades.", out error);
            if (pagePresentacion == null || pageSeleccionMultiple == null || pageEmparejamiento == null ||
                pageMemoria == null || pageWordBank == null || pageEscritura == null || pageASR == null)
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
            if (txtPorcentajeProgreso != null)
                txtPorcentajeProgreso.text = $"{Mathf.RoundToInt(valorNormalizado * 100f)}%";
        }

        public void LimpiarActividadActual()
        {
            foreach (GameObject page in new[] { pagePresentacion, pageSeleccionMultiple, pageEmparejamiento, pageMemoria, pageWordBank, pageEscritura, pageASR })
                if (page != null) page.SetActive(false);
        }

        public ASRView InstanciarASR() => InstanciarActividad<ASRView>(pageASR);
        public EscrituraView InstanciarEscritura() => InstanciarActividad<EscrituraView>(pageEscritura);
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
                feedbackOverlay.ReproducirSonidoRespuesta(semaforo);
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
                ActualizarEspacioActividad();
                if (pageWordBank != null && pageWordBank.activeSelf)
                    pageWordBank.GetComponent<WordBankView>()?.SolicitarMostrarRespuesta();
            }
        }

        public void OcultarFeedback()
        {
            if (feedbackOverlay != null)
                feedbackOverlay.Ocultar();
            ActualizarEspacioActividad();
        }

        public void MostrarResultado(int aciertos, int total, double precision, int estrellas)
        {
            ayudaContextual?.PermitirAyuda(false);

            if (modalResultado != null)
            {
                modalResultado.Mostrar(aciertos, total, precision, estrellas);
            }
        }

        public void MostrarConfirmacionSalida(bool mostrar)
        {
            ayudaContextual?.PermitirAyuda(!mostrar);

            if (panelConfirmacionSalida != null) panelConfirmacionSalida.SetActive(mostrar);
        }

        public void MostrarFeedbackRespuesta(FeedbackRespuesta respuesta)
        {
            if (feedbackOverlay == null) return;
            feedbackOverlay.ReproducirSonidoRespuesta(respuesta.Semaforo);
            feedbackOverlay.MostrarRespuesta(respuesta);
            ActualizarEspacioActividad();
        }

        public void PresentarAccionPrincipal(AccionPrincipal accion, bool interactuable, bool visible = true)
        {
            txtAccionPrincipal.text = accion == AccionPrincipal.Comprobar ? "COMPROBAR" :
                accion == AccionPrincipal.Finalizar ? "FINALIZAR" : "SIGUIENTE";
            iconoAccionPrincipal.sprite = accion == AccionPrincipal.Comprobar ? iconoComprobar :
                accion == AccionPrincipal.Finalizar ? iconoFinalizar : iconoContinuar;
            btnPrincipal.gameObject.SetActive(visible);
            btnPrincipal.interactable = interactuable;
            btnPrincipal.GetComponent<ThemedButtonView>()?.AplicarTema();
            ActualizarEspacioActividad();
        }

        public void MostrarDetenerGrabacion()
        {
            txtAccionPrincipal.text = "DETENER";
            iconoAccionPrincipal.sprite = iconoDetener;
            btnPrincipal.gameObject.SetActive(true);
            btnPrincipal.interactable = true;
            btnPrincipal.GetComponent<ThemedButtonView>()?.AplicarTema();
            ActualizarEspacioActividad();
        }

        public void EstablecerInteraccionDuranteGrabacion(bool grabando)
        {
            if (btnAtras != null) btnAtras.interactable = !grabando;
            ayudaContextual?.PermitirAyuda(!grabando);

        }

        private void LateUpdate()
        {
            if (feedbackOverlay != null && feedbackOverlay.isActiveAndEnabled &&
                feedbackOverlay.transform is RectTransform rect && rect.rect.size != ultimoTamanoFeedback)
                ActualizarEspacioActividad();
        }

        private void ActualizarEspacioActividad()
        {
            if (actividadContainer == null || !(actividadContainer.parent is RectTransform safeArea)) return;
            if (feedbackOverlay != null && feedbackOverlay.isActiveAndEnabled &&
                feedbackOverlay.transform is RectTransform rect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                ultimoTamanoFeedback = rect.rect.size;
            }
            float margen = 28f;
            foreach (RectTransform elemento in new[] {
                feedbackOverlay != null ? feedbackOverlay.transform as RectTransform : null,
                btnPrincipal != null ? btnPrincipal.transform as RectTransform : null })
            {
                if (elemento == null || !elemento.gameObject.activeSelf) continue;
                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(safeArea, elemento);
                margen = Mathf.Max(margen, bounds.max.y - safeArea.rect.yMin + 24f);
            }
            Vector2 offset = actividadContainer.offsetMin;
            if (Mathf.Abs(offset.y - margen) < 1f) return;
            offset.y = margen;
            actividadContainer.offsetMin = offset;
            foreach (ActividadLayoutController layout in actividadContainer.GetComponentsInChildren<ActividadLayoutController>())
                layout.SolicitarReconstruccion();
        }

        public void MostrarSeccion(string seccion)
        {
            if (txtProgreso == null) return;

            txtProgreso.text = "Progreso";
            txtProgreso.gameObject.SetActive(true);
        }

        private void NotificarAccionPrincipal() => OnAccionPrincipalClick?.Invoke();
        private void NotificarAtras() => OnAtrasClick?.Invoke();
        private void NotificarVolverAlMapa() => OnVolverAlMapaClick?.Invoke();
        private void NotificarConfirmarSalida() => OnConfirmarSalidaClick?.Invoke();
        private void NotificarCancelarSalida() => OnCancelarSalidaClick?.Invoke();
    }
}
