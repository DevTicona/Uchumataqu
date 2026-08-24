using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista de la pestaña de Ajustes en 02_Main.
    /// Especificado en Docs/UI/08-perfil-logros-ajustes-acerca.md §4.
    /// </summary>
    public class AjustesView : MonoBehaviour
    {
        [Header("Datos de Perfil")]
        [SerializeField] private TMP_InputField inputNombre;
        [SerializeField] private Button btnGuardarPerfil;

        [Header("Audio")]
        [SerializeField] private Slider sliderVolumenGeneral;

        [Header("Zona de Peligro")]
        [SerializeField] private Button btnReiniciarProgreso;
        [SerializeField] private GameObject dialogoConfirmacionReinicio;
        [SerializeField] private Button btnConfirmarReinicio;
        [SerializeField] private Button btnCancelarReinicio;

        public event Action<string> OnGuardarNombre;
        public event Action<float> OnCambiarVolumen;
        public event Action OnConfirmarReinicio;
        public event Action OnSolicitarActualizacion;

        private void Awake()
        {
            if (btnGuardarPerfil != null)
                btnGuardarPerfil.onClick.AddListener(GuardarNombre);

            if (sliderVolumenGeneral != null)
                sliderVolumenGeneral.onValueChanged.AddListener(OnVolumenChanged);

            if (btnReiniciarProgreso != null)
                btnReiniciarProgreso.onClick.AddListener(() => MostrarDialogoReinicio(true));

            if (btnCancelarReinicio != null)
                btnCancelarReinicio.onClick.AddListener(() => MostrarDialogoReinicio(false));

            if (btnConfirmarReinicio != null)
                btnConfirmarReinicio.onClick.AddListener(EjecutarReinicio);
        }

        private void OnEnable()
        {
            MostrarDialogoReinicio(false);
            OnSolicitarActualizacion?.Invoke();
        }

        public void Mostrar(string nombre, float volumen)
        {
            if (inputNombre != null) inputNombre.text = nombre ?? string.Empty;
            if (sliderVolumenGeneral != null) sliderVolumenGeneral.SetValueWithoutNotify(volumen);
        }

        private void GuardarNombre()
        {
            if (inputNombre == null) return;
            string nuevoNombre = inputNombre.text.Trim();
            if (string.IsNullOrEmpty(nuevoNombre)) return;

            OnGuardarNombre?.Invoke(nuevoNombre);
        }

        private void OnVolumenChanged(float vol)
        {
            OnCambiarVolumen?.Invoke(vol);
        }

        private void MostrarDialogoReinicio(bool mostrar)
        {
            if (dialogoConfirmacionReinicio != null)
                dialogoConfirmacionReinicio.SetActive(mostrar);
        }

        private void EjecutarReinicio()
        {
            MostrarDialogoReinicio(false);
            OnConfirmarReinicio?.Invoke();
        }
    }
}
