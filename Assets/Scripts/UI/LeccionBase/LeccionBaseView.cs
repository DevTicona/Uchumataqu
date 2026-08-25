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
        [SerializeField] private TMP_Text txtPasoContador;
        [SerializeField] private Slider sliderProgreso;
        [SerializeField] private Button btnAtras;

        [Header("Contenedor de Actividades")]
        [SerializeField] private Transform actividadContainer;

        [Header("Prefabs de Actividades")]
        [SerializeField] private GameObject prefabPresentacion;
        [SerializeField] private GameObject prefabSeleccionMultiple;
        [SerializeField] private GameObject prefabEmparejamiento;
        [SerializeField] private GameObject prefabMemoria;
        [SerializeField] private GameObject prefabWordBank;

        [Header("Overlays")]
        [SerializeField] private FeedbackOverlayView feedbackOverlay;
        [SerializeField] private ModalResultadoView modalResultado;

        public event Action OnAtrasClick;
        public event Action OnVolverAlMapaClick;
        public event Action OnContinuarLeccionClick;
        public event Action OnConfirmarSalidaClick;
        public event Action OnCancelarSalidaClick;

        public FeedbackOverlayView Feedback => feedbackOverlay;
        public ModalResultadoView ModalResultado => modalResultado;

        private GameObject _actividadInstanciada;
        private GameObject _panelConfirmacionSalida;

        private void Awake()
        {
            if (btnAtras != null) btnAtras.onClick.AddListener(() => OnAtrasClick?.Invoke());

            if (feedbackOverlay == null)
                feedbackOverlay = GetComponentInChildren<FeedbackOverlayView>(true);

            if (modalResultado == null)
                modalResultado = GetComponentInChildren<ModalResultadoView>(true);

            if (modalResultado != null)
            {
                modalResultado.OnContinuarClicked += () => OnContinuarLeccionClick?.Invoke();
                modalResultado.OnVolverMapaClicked += () => OnVolverAlMapaClick?.Invoke();
                modalResultado.Ocultar();
            }
            feedbackOverlay?.Ocultar();
        }

        public void ConfigurarHeader(string titulo, int pasoActual, int totalPasos, float progreso)
        {
            if (txtTituloLeccion != null) txtTituloLeccion.text = titulo;
            if (txtPasoContador != null) txtPasoContador.text = $"{pasoActual}/{totalPasos}";
            if (sliderProgreso != null) sliderProgreso.value = progreso;
        }

        public void ActualizarProgreso(int pasoActual, int totalPasos, float progreso)
        {
            if (txtPasoContador != null) txtPasoContador.text = $"{pasoActual}/{totalPasos}";
            if (sliderProgreso != null) sliderProgreso.value = progreso;
        }

        public void LimpiarActividadActual()
        {
            if (_actividadInstanciada != null)
            {
                Destroy(_actividadInstanciada);
                _actividadInstanciada = null;
            }

            if (actividadContainer != null)
            {
                foreach (Transform child in actividadContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public PresentacionFamiliaView InstanciarPresentacion()
        {
            LimpiarActividadActual();
            if (prefabPresentacion != null && actividadContainer != null)
            {
                _actividadInstanciada = Instantiate(prefabPresentacion, actividadContainer);
                return _actividadInstanciada.GetComponent<PresentacionFamiliaView>();
            }
            if (actividadContainer == null) return null;
            GameObject go = RuntimeActividadUIFactory.CrearContenedor("PresentacionFamiliaView", actividadContainer);
            _actividadInstanciada = go;
            var actividad = go.AddComponent<PresentacionFamiliaView>();
            actividad.InicializarRuntime();
            return actividad;
        }

        public SeleccionMultipleView InstanciarSeleccionMultiple()
        {
            LimpiarActividadActual();
            if (prefabSeleccionMultiple != null && actividadContainer != null)
            {
                _actividadInstanciada = Instantiate(prefabSeleccionMultiple, actividadContainer);
                return _actividadInstanciada.GetComponent<SeleccionMultipleView>();
            }
            if (actividadContainer == null) return null;
            GameObject go = RuntimeActividadUIFactory.CrearContenedor("SeleccionMultipleView", actividadContainer);
            _actividadInstanciada = go;
            var actividad = go.AddComponent<SeleccionMultipleView>();
            actividad.InicializarRuntime();
            return actividad;
        }

        public EmparejamientoView InstanciarEmparejamiento()
        {
            LimpiarActividadActual();
            if (prefabEmparejamiento != null && actividadContainer != null)
            {
                _actividadInstanciada = Instantiate(prefabEmparejamiento, actividadContainer);
                return _actividadInstanciada.GetComponent<EmparejamientoView>();
            }
            if (actividadContainer == null) return null;
            GameObject go = RuntimeActividadUIFactory.CrearContenedor("EmparejamientoView", actividadContainer);
            _actividadInstanciada = go;
            var actividad = go.AddComponent<EmparejamientoView>();
            actividad.InicializarRuntime();
            return actividad;
        }

        public WordBankView InstanciarWordBank()
        {
            LimpiarActividadActual();
            if (prefabWordBank != null && actividadContainer != null)
            {
                _actividadInstanciada = Instantiate(prefabWordBank, actividadContainer);
                return _actividadInstanciada.GetComponent<WordBankView>();
            }
            if (actividadContainer == null) return null;
            GameObject go = RuntimeActividadUIFactory.CrearContenedor("WordBankView", actividadContainer);
            _actividadInstanciada = go;
            var actividad = go.AddComponent<WordBankView>();
            actividad.InicializarRuntime();
            return actividad;
        }

        public MemoriaView InstanciarMemoria()
        {
            LimpiarActividadActual();
            if (prefabMemoria != null && actividadContainer != null)
            {
                _actividadInstanciada = Instantiate(prefabMemoria, actividadContainer);
                return _actividadInstanciada.GetComponent<MemoriaView>();
            }
            if (actividadContainer == null) return null;
            GameObject go = RuntimeActividadUIFactory.CrearContenedor("MemoriaView", actividadContainer);
            _actividadInstanciada = go;
            var actividad = go.AddComponent<MemoriaView>();
            actividad.InicializarRuntime();
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
                else
                {
                    feedbackOverlay.MostrarIncorrecto(correcta);
                }
            }
        }

        public void OcultarFeedback()
        {
            if (feedbackOverlay != null)
                feedbackOverlay.Ocultar();
        }

        public void MostrarResultado(int aciertos, int total, double precision)
        {
            if (modalResultado != null)
            {
                modalResultado.Mostrar(aciertos, total, precision);
            }
        }

        public void MostrarConfirmacionSalida(bool mostrar)
        {
            if (_panelConfirmacionSalida == null) CrearConfirmacionSalida();
            if (_panelConfirmacionSalida != null) _panelConfirmacionSalida.SetActive(mostrar);
        }

        private void CrearConfirmacionSalida()
        {
            Transform padre = feedbackOverlay != null
                ? feedbackOverlay.transform.parent
                : actividadContainer != null ? actividadContainer.parent : null;
            if (padre == null) return;

            _panelConfirmacionSalida = RuntimeActividadUIFactory.CrearContenedor("ConfirmacionSalida", padre);
            RuntimeActividadUIFactory.CrearTexto(_panelConfirmacionSalida.transform, "Titulo", 30f).text =
                "¿Quieres salir de la lección?";
            RuntimeActividadUIFactory.CrearTexto(_panelConfirmacionSalida.transform, "Detalle", 23f).text =
                "Tu avance hasta este punto se guardará.";
            Button seguir = RuntimeActividadUIFactory.CrearBoton(
                _panelConfirmacionSalida.transform, "BtnSeguir", "Seguir aprendiendo");
            Button salir = RuntimeActividadUIFactory.CrearBoton(
                _panelConfirmacionSalida.transform, "BtnSalir", "Salir");
            seguir.onClick.AddListener(() => OnCancelarSalidaClick?.Invoke());
            salir.onClick.AddListener(() => OnConfirmarSalidaClick?.Invoke());
            _panelConfirmacionSalida.SetActive(false);
        }
    }
}
