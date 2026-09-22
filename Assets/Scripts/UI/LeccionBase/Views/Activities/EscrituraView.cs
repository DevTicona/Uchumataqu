using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    public sealed class EscrituraView : MonoBehaviour
    {
        [SerializeField] private TMP_Text txtInstruccion;
        [SerializeField] private TMP_InputField inputRespuesta;
        [SerializeField] private Button btnAudioReferencia;

        private bool _enviada;

        public bool PuedeEnviarRespuesta => !_enviada &&
            inputRespuesta != null && !string.IsNullOrWhiteSpace(inputRespuesta.text);

        public event Action OnRespuestaCambiada;
        public event Action<string> OnRespuestaEnviada;
        public event Action OnAudioClicked;

        private void Awake()
        {
            if (inputRespuesta != null)
                inputRespuesta.onValueChanged.AddListener(NotificarCambio);
            if (btnAudioReferencia != null)
                btnAudioReferencia.onClick.AddListener(NotificarAudio);
        }

        private void OnDestroy()
        {
            if (inputRespuesta != null)
                inputRespuesta.onValueChanged.RemoveListener(NotificarCambio);
            if (btnAudioReferencia != null)
                btnAudioReferencia.onClick.RemoveListener(NotificarAudio);
        }

        private void OnDisable()
        {
            if (inputRespuesta != null)
                inputRespuesta.DeactivateInputField();
        }

        public void Configurar(string consigna, bool mostrarAudio)
        {
            _enviada = false;
            txtInstruccion.text = consigna ?? string.Empty;
            inputRespuesta.SetTextWithoutNotify(string.Empty);
            inputRespuesta.interactable = true;
            inputRespuesta.contentType = TMP_InputField.ContentType.Standard;
            inputRespuesta.lineType = TMP_InputField.LineType.SingleLine;
            inputRespuesta.characterLimit = 200;
            btnAudioReferencia.gameObject.SetActive(mostrarAudio);
            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        public void ConfirmarRespuesta()
        {
            if (!PuedeEnviarRespuesta) return;
            _enviada = true;
            inputRespuesta.interactable = false;
            OnRespuestaEnviada?.Invoke(inputRespuesta.text);
        }

        public void EnfocarEntrada()
        {
            if (inputRespuesta == null || !inputRespuesta.interactable) return;
            inputRespuesta.Select();
            inputRespuesta.ActivateInputField();
        }

        private void NotificarCambio(string _) => OnRespuestaCambiada?.Invoke();
        private void NotificarAudio() => OnAudioClicked?.Invoke();
    }
}
