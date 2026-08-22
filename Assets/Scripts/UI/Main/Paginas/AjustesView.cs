using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Persistencia;

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
            var perfil = PerfilRepository.CargarPerfil();
            if (inputNombre != null && perfil != null)
            {
                inputNombre.text = perfil.nombre ?? "";
            }

            if (sliderVolumenGeneral != null)
            {
                sliderVolumenGeneral.value = AudioListener.volume;
            }

            MostrarDialogoReinicio(false);
        }

        private void GuardarNombre()
        {
            if (inputNombre == null) return;
            string nuevoNombre = inputNombre.text.Trim();
            if (string.IsNullOrEmpty(nuevoNombre)) return;

            var perfil = PerfilRepository.CargarPerfil();
            perfil.nombre = nuevoNombre;
            PerfilRepository.GuardarPerfil(perfil);

            AudioService.Instance?.ReproducirEfecto("click_boton");
        }

        private void OnVolumenChanged(float vol)
        {
            AudioListener.volume = vol;
        }

        private void MostrarDialogoReinicio(bool mostrar)
        {
            if (dialogoConfirmacionReinicio != null)
                dialogoConfirmacionReinicio.SetActive(mostrar);
        }

        private void EjecutarReinicio()
        {
            MostrarDialogoReinicio(false);
            ProgresoRepository.ReiniciarProgreso();
            NavigationService.IrABootstrap();
        }
    }
}
