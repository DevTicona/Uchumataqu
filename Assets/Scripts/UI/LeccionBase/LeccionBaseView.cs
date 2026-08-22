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
        [SerializeField] private GameObject prefabWordBank;

        [Header("Overlays")]
        [SerializeField] private FeedbackOverlayView feedbackOverlay;
        [SerializeField] private ModalResultadoView modalResultado;

        public event Action OnAtrasClick;
        public event Action OnVolverAlMapaClick;
        public event Action OnContinuarLeccionClick;

        public FeedbackOverlayView Feedback => feedbackOverlay;
        public ModalResultadoView ModalResultado => modalResultado;

        private GameObject _actividadInstanciada;

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
            }
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
            // Fallback dinámico si no hay prefab asignado en Inspector
            GameObject go = new GameObject("PresentacionFamiliaView", typeof(RectTransform), typeof(PresentacionFamiliaView));
            go.transform.SetParent(actividadContainer, false);
            _actividadInstanciada = go;
            return go.GetComponent<PresentacionFamiliaView>();
        }

        public SeleccionMultipleView InstanciarSeleccionMultiple()
        {
            LimpiarActividadActual();
            if (prefabSeleccionMultiple != null && actividadContainer != null)
            {
                _actividadInstanciada = Instantiate(prefabSeleccionMultiple, actividadContainer);
                return _actividadInstanciada.GetComponent<SeleccionMultipleView>();
            }
            GameObject go = new GameObject("SeleccionMultipleView", typeof(RectTransform), typeof(SeleccionMultipleView));
            go.transform.SetParent(actividadContainer, false);
            _actividadInstanciada = go;
            return go.GetComponent<SeleccionMultipleView>();
        }

        public EmparejamientoView InstanciarEmparejamiento()
        {
            LimpiarActividadActual();
            if (prefabEmparejamiento != null && actividadContainer != null)
            {
                _actividadInstanciada = Instantiate(prefabEmparejamiento, actividadContainer);
                return _actividadInstanciada.GetComponent<EmparejamientoView>();
            }
            GameObject go = new GameObject("EmparejamientoView", typeof(RectTransform), typeof(EmparejamientoView));
            go.transform.SetParent(actividadContainer, false);
            _actividadInstanciada = go;
            return go.GetComponent<EmparejamientoView>();
        }

        public WordBankView InstanciarWordBank()
        {
            LimpiarActividadActual();
            if (prefabWordBank != null && actividadContainer != null)
            {
                _actividadInstanciada = Instantiate(prefabWordBank, actividadContainer);
                return _actividadInstanciada.GetComponent<WordBankView>();
            }
            GameObject go = new GameObject("WordBankView", typeof(RectTransform), typeof(WordBankView));
            go.transform.SetParent(actividadContainer, false);
            _actividadInstanciada = go;
            return go.GetComponent<WordBankView>();
        }

        public void MostrarFeedback(bool correcto, string detalle = null, double similitud = 0, string correcta = null)
        {
            if (feedbackOverlay != null)
            {
                if (correcto)
                {
                    feedbackOverlay.MostrarCorrecto(detalle);
                }
                else if (similitud >= 60)
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
    }
}
