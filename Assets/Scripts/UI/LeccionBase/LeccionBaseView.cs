using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI.LeccionBase
{
    /// <summary>
    /// Vista de la escena 03_LeccionBase.
    /// Especificado en Docs/UI/04-escena-leccion-base.md.
    /// </summary>
    public class LeccionBaseView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI txtTituloLeccion;
        [SerializeField] private Slider sliderProgreso;
        [SerializeField] private Button btnAtras;

        [Header("Contenedor de Actividades")]
        [SerializeField] private Transform actividadContainer;

        [Header("Feedback Overlay")]
        [SerializeField] private GameObject feedbackOverlay;
        [SerializeField] private TextMeshProUGUI txtFeedbackMensaje;
        [SerializeField] private Image imgFeedbackIcono;

        [Header("Resultado Final")]
        [SerializeField] private GameObject panelResultado;
        [SerializeField] private TextMeshProUGUI txtResultadoAciertos;
        [SerializeField] private TextMeshProUGUI txtResultadoPrecision;
        [SerializeField] private Button btnVolverAlMapa;

        public event Action OnAtrasClick;
        public event Action OnVolverAlMapaClick;

        private void Awake()
        {
            if (btnAtras != null) btnAtras.onClick.AddListener(() => OnAtrasClick?.Invoke());
            if (btnVolverAlMapa != null) btnVolverAlMapa.onClick.AddListener(() => OnVolverAlMapaClick?.Invoke());
        }

        public void ConfigurarHeader(string titulo, float progreso)
        {
            if (txtTituloLeccion != null) txtTituloLeccion.text = titulo;
            if (sliderProgreso != null) sliderProgreso.value = progreso;
        }

        public void ActualizarProgreso(float progreso)
        {
            if (sliderProgreso != null) sliderProgreso.value = progreso;
        }

        public Transform ObtenerContenedorActividad() => actividadContainer;

        public void MostrarFeedback(bool correcto, string mensaje)
        {
            if (feedbackOverlay != null)
            {
                feedbackOverlay.SetActive(true);
                if (txtFeedbackMensaje != null) txtFeedbackMensaje.text = mensaje;
            }
        }

        public void OcultarFeedback()
        {
            if (feedbackOverlay != null)
                feedbackOverlay.SetActive(false);
        }

        public void MostrarResultado(int aciertos, int total, float precision)
        {
            if (panelResultado != null)
            {
                panelResultado.SetActive(true);
                if (txtResultadoAciertos != null) txtResultadoAciertos.text = $"{aciertos} / {total} aciertos";
                if (txtResultadoPrecision != null) txtResultadoPrecision.text = $"{precision:F0}% de precisión";
            }
        }
    }
}
