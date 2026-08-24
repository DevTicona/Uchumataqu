using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Data.Corpus;

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

        public void InicializarRuntime()
        {
            Transform root = transform;
            txtBadge = RuntimeActividadUIFactory.CrearTexto(root, "Badge", 22f);
            txtRaizUch = RuntimeActividadUIFactory.CrearTexto(root, "RaizUch", 42f);
            txtSignificadoEs = RuntimeActividadUIFactory.CrearTexto(root, "SignificadoEs", 30f);
            btnAudio = RuntimeActividadUIFactory.CrearBoton(root, "BtnAudio", "Escuchar");
            txtAudioBoton = btnAudio.GetComponentInChildren<TextMeshProUGUI>();
            contenedorFormas = RuntimeActividadUIFactory.CrearSubcontenedor(root, "Formas").gameObject;
            txtTituloFormas = RuntimeActividadUIFactory.CrearTexto(contenedorFormas.transform, "TituloFormas", 24f);
            txtFormasLista = RuntimeActividadUIFactory.CrearTexto(contenedorFormas.transform, "ListaFormas", 24f);
            btnPracticarVoz = RuntimeActividadUIFactory.CrearBoton(root, "BtnPracticarVoz", "Practicar voz (opcional)");
            btnContinuar = RuntimeActividadUIFactory.CrearBoton(root, "BtnContinuar", "Continuar");
            RegistrarListeners();
        }

        private void Awake()
        {
            RegistrarListeners();
        }

        private void RegistrarListeners()
        {
            if (btnContinuar != null) btnContinuar.onClick.RemoveAllListeners();
            if (btnAudio != null) btnAudio.onClick.RemoveAllListeners();
            if (btnPracticarVoz != null) btnPracticarVoz.onClick.RemoveAllListeners();
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
