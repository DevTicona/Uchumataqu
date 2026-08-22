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
            if (btnVolver != null) btnVolver.onClick.AddListener(() => OnVolverClick?.Invoke());
            if (btnAudioObjeto != null) btnAudioObjeto.onClick.AddListener(() => OnAudioObjetoClick?.Invoke());
            if (btnPracticarVoz != null) btnPracticarVoz.onClick.AddListener(() => OnPracticarVozClick?.Invoke());
        }

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
