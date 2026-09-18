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

        public event Action OnVolverClick;
        public event Action OnAudioObjetoClick;
        public event Action OnPracticarVozClick;

        private void Awake()
        {
            if (btnVolver != null) btnVolver.onClick.AddListener(Volver);
            if (btnAudioObjeto != null) btnAudioObjeto.onClick.AddListener(ReproducirAudio);
            if (btnPracticarVoz != null) btnPracticarVoz.onClick.AddListener(PracticarVoz);
        }

        private void OnDestroy()
        {
            if (btnVolver != null) btnVolver.onClick.RemoveListener(Volver);
            if (btnAudioObjeto != null) btnAudioObjeto.onClick.RemoveListener(ReproducirAudio);
            if (btnPracticarVoz != null) btnPracticarVoz.onClick.RemoveListener(PracticarVoz);
        }

        private void Volver() => OnVolverClick?.Invoke();
        private void ReproducirAudio() => OnAudioObjetoClick?.Invoke();
        private void PracticarVoz() => OnPracticarVozClick?.Invoke();

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
    }
}
