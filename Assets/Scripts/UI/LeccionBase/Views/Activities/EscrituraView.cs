using System;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Lecciones;
using Assets.Scripts.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    public sealed class EscrituraView : MonoBehaviour
    {
        [SerializeField] private TMP_Text txtTituloEscritura;
        [SerializeField] private TMP_Text txtInstruccion;
        [SerializeField] private GameObject panelEstimulo;
        [SerializeField] private TMP_InputField inputRespuesta;
        [SerializeField] private ThemedGraphicView bordeInputRespuesta;
        [SerializeField] private GameObject panelRespuestaCorrecta;
        [SerializeField] private TMP_Text txtRespuestaCorrecta;
        [SerializeField] private ThemedGraphicView bordeRespuestaCorrecta;
        [SerializeField] private Button btnAudio;

        private bool _enviada;
        private TMP_Text _textoResultadoUsuario;
        private EnfoqueEntradaMovilView _enfoqueEntrada;

        public bool PuedeEnviarRespuesta => !_enviada &&
            inputRespuesta != null && !string.IsNullOrWhiteSpace(inputRespuesta.text);

        public event Action OnRespuestaCambiada;
        public event Action<string> OnRespuestaEnviada;
        public event Action OnAudioClicked;

        private void Awake()
        {
            if (btnAudio != null) btnAudio.onClick.AddListener(NotificarAudio);
            OcultarRespuestaCorrecta();
            MobileInputFieldStyle.Aplicar(inputRespuesta);
            if (inputRespuesta != null && inputRespuesta.textComponent != null)
            {
                TMP_Text textoOriginal = inputRespuesta.textComponent;
                _textoResultadoUsuario = Instantiate(textoOriginal, textoOriginal.transform.parent, false);
                _textoResultadoUsuario.name = "TextoResultadoUsuario";
                _textoResultadoUsuario.raycastTarget = false;
                _textoResultadoUsuario.overrideColorTags = false;
                _textoResultadoUsuario.gameObject.SetActive(false);
            }
            if (inputRespuesta != null)
            {
                inputRespuesta.onValueChanged.AddListener(NotificarCambio);
                inputRespuesta.onSelect.AddListener(PrepararEdicion);
                _enfoqueEntrada = inputRespuesta.GetComponent<EnfoqueEntradaMovilView>();
                if (_enfoqueEntrada != null) _enfoqueEntrada.EdicionSolicitada += MostrarTextoEditable;
            }
        }

        private void OnDestroy()
        {
            if (btnAudio != null) btnAudio.onClick.RemoveListener(NotificarAudio);
            if (inputRespuesta != null)
            {
                inputRespuesta.onValueChanged.RemoveListener(NotificarCambio);
                inputRespuesta.onSelect.RemoveListener(PrepararEdicion);
            }
            if (_enfoqueEntrada != null) _enfoqueEntrada.EdicionSolicitada -= MostrarTextoEditable;
        }

        private void OnDisable()
        {
            if (inputRespuesta != null)
                inputRespuesta.DeactivateInputField();
        }

        public void Configurar(string tipo, string estimulo, bool esAudio = false)
        {
            _enviada = false;
            txtTituloEscritura.text = esAudio ? "Escucha y escribe en uchumataqu:" : ObtenerTitulo(tipo);
            txtInstruccion.text = estimulo ?? string.Empty;
            if (panelEstimulo != null) panelEstimulo.SetActive(!esAudio);
            if (btnAudio != null) btnAudio.gameObject.SetActive(esAudio);
            if (inputRespuesta.textComponent != null)
                inputRespuesta.textComponent.gameObject.SetActive(true);
            if (_textoResultadoUsuario != null)
                _textoResultadoUsuario.gameObject.SetActive(false);
            inputRespuesta.SetTextWithoutNotify(string.Empty);
            inputRespuesta.interactable = true;
            inputRespuesta.transition = Selectable.Transition.None;
            ThemedGraphicView superficie = inputRespuesta.GetComponent<ThemedGraphicView>();
            if (superficie != null) superficie.Rol = ThemedGraphicView.RolColor.Superficie;
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
            inputRespuesta.SetTextWithoutNotify(FormatoTexto.ConInicialMayuscula(inputRespuesta.text));
            OnRespuestaEnviada?.Invoke(inputRespuesta.text);
        }

        public void MostrarResultado(string semaforo, string respuestaCorrecta)
        {
            ThemedGraphicView superficie = inputRespuesta.GetComponent<ThemedGraphicView>();
            if (superficie != null) superficie.Rol = ThemedGraphicView.RolColor.Superficie;
            var diferencia = DiffGrafemas.Comparar(inputRespuesta.text, respuestaCorrecta);
            if (_textoResultadoUsuario != null)
            {
                _textoResultadoUsuario.richText = true;
                _textoResultadoUsuario.text = diferencia.usuario;
                _textoResultadoUsuario.gameObject.SetActive(true);
                inputRespuesta.textComponent.gameObject.SetActive(false);
            }

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

            inputRespuesta.interactable = true;

            if (panelRespuestaCorrecta == null || txtRespuestaCorrecta == null) return;
            txtRespuestaCorrecta.richText = true;
            txtRespuestaCorrecta.text = diferencia.correcta;
            if (bordeRespuestaCorrecta != null)
                bordeRespuestaCorrecta.Rol = ThemedGraphicView.RolColor.Exito;
            panelRespuestaCorrecta.SetActive(false);
            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        public void EnfocarEntrada()
        {
            if (inputRespuesta == null || !inputRespuesta.interactable) return;
            MostrarTextoEditable();
            inputRespuesta.Select();
            inputRespuesta.ActivateInputField();
        }

        private void NotificarCambio(string _)
        {
            if (_enviada && inputRespuesta.interactable)
            {
                _enviada = false;
                MostrarTextoEditable();
            }
            OnRespuestaCambiada?.Invoke();
        }
        private void PrepararEdicion(string _) => MostrarTextoEditable();

        private void MostrarTextoEditable()
        {
            if (_textoResultadoUsuario != null) _textoResultadoUsuario.gameObject.SetActive(false);
            if (inputRespuesta.textComponent != null) inputRespuesta.textComponent.gameObject.SetActive(true);
            inputRespuesta.ForceLabelUpdate();
        }

        private void NotificarAudio() => OnAudioClicked?.Invoke();
        private void OcultarRespuestaCorrecta()
        {
            if (panelRespuestaCorrecta != null) panelRespuestaCorrecta.SetActive(false);
        }

    }
}
