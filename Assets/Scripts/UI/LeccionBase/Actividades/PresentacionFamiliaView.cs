using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Core.Services;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    /// <summary>
    /// Vista para el paso pedagógico de presentación de palabra y familia morfológica.
    /// Especificación: Docs/UI/04-escena-leccion-base.md §8
    /// </summary>
    public class PresentacionFamiliaView : MonoBehaviour
    {
        [Header("Encabezado")]
        [SerializeField] private TMP_Text txtBadge; // "NUEVA PALABRA"
        [SerializeField] private TMP_Text txtRaizUch; // Ej: "Chhaq"
        [SerializeField] private TMP_Text txtSignificadoEs; // Ej: "Grande"

        [Header("Audio")]
        [SerializeField] private Button btnAudio;
        [SerializeField] private TMP_Text txtAudioBoton;

        [Header("Formas Documentadas")]
        [SerializeField] private GameObject contenedorFormas;
        [SerializeField] private TMP_Text txtTituloFormas;
        [SerializeField] private TMP_Text txtFormasLista;

        [Header("Acciones")]
        [SerializeField] private Button btnPracticarVoz;
        [SerializeField] private Button btnContinuar;

        public event Action OnContinuarClicked;
        public event Action OnAudioClicked;
        public event Action OnPracticarVozClicked;

        private PasoLeccionData _pasoActual;

        private void Awake()
        {
            if (btnContinuar != null) btnContinuar.onClick.AddListener(() => OnContinuarClicked?.Invoke());
            if (btnAudio != null) btnAudio.onClick.AddListener(() => OnAudioClicked?.Invoke());
            if (btnPracticarVoz != null) btnPracticarVoz.onClick.AddListener(() => OnPracticarVozClicked?.Invoke());
        }

        public void Configurar(PasoLeccionData paso, PalabraData palabra)
        {
            _pasoActual = paso;

            if (txtBadge != null) txtBadge.text = "NUEVA PALABRA";
            
            if (palabra != null)
            {
                if (txtRaizUch != null) txtRaizUch.text = palabra.uch;
                if (txtSignificadoEs != null) txtSignificadoEs.text = palabra.es;

                if (palabra.formas != null && palabra.formas.Count > 0)
                {
                    if (contenedorFormas != null) contenedorFormas.SetActive(true);
                    if (txtTituloFormas != null) txtTituloFormas.text = "Formas documentadas";

                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    foreach (var f in palabra.formas)
                    {
                        sb.AppendLine($"<b>{f.uch}</b>: {f.es}");
                    }
                    if (txtFormasLista != null) txtFormasLista.text = sb.ToString();
                }
                else
                {
                    if (contenedorFormas != null) contenedorFormas.SetActive(false);
                }

                // Reproducción automática de audio de presentación
                if (!string.IsNullOrEmpty(palabra.id))
                {
                    AudioService.Instance?.ReproducirPalabra(palabra.id);
                }
            }
            else
            {
                if (txtRaizUch != null) txtRaizUch.text = "";
                if (txtSignificadoEs != null) txtSignificadoEs.text = "";
                if (contenedorFormas != null) contenedorFormas.SetActive(false);
            }
        }
    }
}
