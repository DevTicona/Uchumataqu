using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI.RealidadAumentada
{
    /// <summary>
    /// Vista de UI para la escena 04_RealidadAumentada.
    /// Especificado en Docs/UI/05-escena-realidad-aumentada.md.
    /// </summary>
    public class RealidadAumentadaView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI txtTituloEscenario;
        [SerializeField] private Button btnVolver;

        [Header("Paneles de Estado")]
        [SerializeField] private GameObject panelEscaneo;
        [SerializeField] private GameObject reticulaColocacion;
        [SerializeField] private GameObject panelObjeto;
        [SerializeField] private GameObject panelMision;
        [SerializeField] private GameObject panelResultado;

        [Header("Detalle Objeto")]
        [SerializeField] private TextMeshProUGUI txtObjetoUch;
        [SerializeField] private TextMeshProUGUI txtObjetoEs;
        [SerializeField] private Button btnAudioObjeto;
        [SerializeField] private Button btnPracticarVoz;
        [SerializeField] private TextMeshProUGUI txtEstadoRA;
        [SerializeField] private TextMeshProUGUI txtMision;
        [SerializeField] private Button btnRepetirAudio;
        [SerializeField] private Button btnRecolocar;
        [SerializeField] private Button btnReiniciar;
        [SerializeField] private Button btnExplorar;

        public event Action OnVolverClick;
        public event Action OnAudioObjetoClick;
        public event Action OnPracticarVozClick;
        public event Action OnRepetirAudioClick;
        public event Action OnRecolocarClick;
        public event Action OnReiniciarClick;
        public event Action OnExplorarClick;

        private void Awake()
        {
            if (btnVolver != null) btnVolver.onClick.AddListener(Volver);
            if (btnAudioObjeto != null) btnAudioObjeto.onClick.AddListener(ReproducirAudio);
            if (btnPracticarVoz != null) btnPracticarVoz.onClick.AddListener(PracticarVoz);
            if (btnRepetirAudio != null) btnRepetirAudio.onClick.AddListener(RepetirAudio);
            if (btnRecolocar != null) btnRecolocar.onClick.AddListener(Recolocar);
            if (btnReiniciar != null) btnReiniciar.onClick.AddListener(Reiniciar);
            if (btnExplorar != null) btnExplorar.onClick.AddListener(Explorar);

        }

        private void OnDestroy()
        {
            if (btnVolver != null) btnVolver.onClick.RemoveListener(Volver);
            if (btnAudioObjeto != null) btnAudioObjeto.onClick.RemoveListener(ReproducirAudio);
            if (btnPracticarVoz != null) btnPracticarVoz.onClick.RemoveListener(PracticarVoz);
            if (btnRepetirAudio != null) btnRepetirAudio.onClick.RemoveListener(RepetirAudio);
            if (btnRecolocar != null) btnRecolocar.onClick.RemoveListener(Recolocar);
            if (btnReiniciar != null) btnReiniciar.onClick.RemoveListener(Reiniciar);
            if (btnExplorar != null) btnExplorar.onClick.RemoveListener(Explorar);

        }

        private void Volver() => OnVolverClick?.Invoke();
        private void ReproducirAudio() => OnAudioObjetoClick?.Invoke();
        private void PracticarVoz() => OnPracticarVozClick?.Invoke();

        private void RepetirAudio() => OnRepetirAudioClick?.Invoke();
        private void Recolocar() => OnRecolocarClick?.Invoke();
        private void Reiniciar() => OnReiniciarClick?.Invoke();
        private void Explorar() => OnExplorarClick?.Invoke();

        public void ConfigurarHeader(string titulo)
        {
            if (txtTituloEscenario != null) txtTituloEscenario.text = titulo;
        }

        public void MostrarPanelObjeto(string uch, string es)
        {
            if (panelObjeto != null) panelObjeto.SetActive(true);
            if (txtObjetoUch != null) txtObjetoUch.text = uch;
            if (txtObjetoEs != null) txtObjetoEs.text = es;
        }

        public void OcultarPanelObjeto()
        {
            if (panelObjeto != null) panelObjeto.SetActive(false);
        }

        public void MostrarEscaneo(bool poseValida)
        {
            if (panelEscaneo != null) panelEscaneo.SetActive(true);
            if (txtEstadoRA != null) txtEstadoRA.text = poseValida
                ? "Superficie detectada. Toca para colocar."
                : "Mueve lentamente el dispositivo para detectar una superficie.";
            if (panelMision != null) panelMision.SetActive(false);
            if (panelResultado != null) panelResultado.SetActive(false);
        }

        public void MostrarMision(string mensaje)
        {
            if (panelEscaneo != null) panelEscaneo.SetActive(false);
            if (panelMision != null) panelMision.SetActive(true);
            if (panelResultado != null) panelResultado.SetActive(false);
            if (txtMision != null) txtMision.text = mensaje;
        }

        public void MostrarResultado()
        {
            if (panelMision != null) panelMision.SetActive(false);
            if (panelResultado != null) panelResultado.SetActive(true);
        }

        public void MostrarExploracionFinal()
        {
            if (panelResultado != null) panelResultado.SetActive(false);
            MostrarMision("Explora libremente los objetos de la casa.");
        }

        public void MostrarError(string mensaje)
        {
            if (panelEscaneo != null) panelEscaneo.SetActive(true);
            if (txtEstadoRA != null) txtEstadoRA.text = mensaje;
        }

    }
}
