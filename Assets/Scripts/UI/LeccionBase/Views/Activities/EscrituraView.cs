using System;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Lecciones;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    public sealed class EscrituraView : MonoBehaviour
    {
        [SerializeField] private TMP_Text txtTituloEscritura;
        [SerializeField] private TMP_Text txtInstruccion;
        [SerializeField] private TMP_InputField inputRespuesta;
        [SerializeField] private ThemedGraphicView bordeInputRespuesta;
        [SerializeField] private GameObject panelRespuestaCorrecta;
        [SerializeField] private TMP_Text txtRespuestaCorrecta;
        [SerializeField] private ThemedGraphicView bordeRespuestaCorrecta;

        private bool _enviada;

        public bool PuedeEnviarRespuesta => !_enviada &&
            inputRespuesta != null && !string.IsNullOrWhiteSpace(inputRespuesta.text);

        public event Action OnRespuestaCambiada;
        public event Action<string> OnRespuestaEnviada;

        private void Awake()
        {
            OcultarRespuestaCorrecta();
            MobileInputFieldStyle.Aplicar(inputRespuesta);
            if (inputRespuesta != null)
                inputRespuesta.onValueChanged.AddListener(NotificarCambio);
        }

        private void OnDestroy()
        {
            if (inputRespuesta != null)
                inputRespuesta.onValueChanged.RemoveListener(NotificarCambio);
        }

        private void OnDisable()
        {
            if (inputRespuesta != null)
                inputRespuesta.DeactivateInputField();
        }

        public void Configurar(string tipo, string estimulo)
        {
            _enviada = false;
            txtTituloEscritura.text = ObtenerTitulo(tipo);
            txtInstruccion.text = estimulo ?? string.Empty;
            inputRespuesta.SetTextWithoutNotify(string.Empty);
            inputRespuesta.interactable = true;
            MobileInputFieldStyle.Aplicar(inputRespuesta);
            if (bordeInputRespuesta != null)
                bordeInputRespuesta.Rol = ThemedGraphicView.RolColor.Borde;
            OcultarRespuestaCorrecta();
            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        private static string ObtenerTitulo(string tipo)
        {
            if (tipo == TipoPasoLeccion.EscribirPalabra)
                return "Escribe en uchumataqu la siguiente palabra:";
            if (tipo == TipoPasoLeccion.EscribirForma)
                return "Escribe en uchumataqu la siguiente forma:";
            return "Escribe en uchumataqu la siguiente oración:";
        }

        public void ConfirmarRespuesta()
        {
            if (!PuedeEnviarRespuesta) return;
            _enviada = true;
            inputRespuesta.interactable = false;
            OnRespuestaEnviada?.Invoke(inputRespuesta.text);
        }

        public void MostrarResultado(string semaforo, string respuestaCorrecta)
        {
            if (bordeInputRespuesta != null)
                bordeInputRespuesta.Rol = semaforo == "verde"
                    ? ThemedGraphicView.RolColor.Exito
                    : semaforo == "amarillo"
                        ? ThemedGraphicView.RolColor.Advertencia
                        : ThemedGraphicView.RolColor.Error;

            if (semaforo == "verde")
            {
                OcultarRespuestaCorrecta();
                return;
            }

            if (panelRespuestaCorrecta == null || txtRespuestaCorrecta == null) return;
            txtRespuestaCorrecta.text = respuestaCorrecta ?? string.Empty;
            if (bordeRespuestaCorrecta != null)
                bordeRespuestaCorrecta.Rol = ThemedGraphicView.RolColor.Exito;
            panelRespuestaCorrecta.SetActive(true);
            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        public void EnfocarEntrada()
        {
            if (inputRespuesta == null || !inputRespuesta.interactable) return;
            inputRespuesta.Select();
            inputRespuesta.ActivateInputField();
        }

        private void NotificarCambio(string _) => OnRespuestaCambiada?.Invoke();
        private void OcultarRespuestaCorrecta()
        {
            if (panelRespuestaCorrecta != null) panelRespuestaCorrecta.SetActive(false);
        }

    }
}
