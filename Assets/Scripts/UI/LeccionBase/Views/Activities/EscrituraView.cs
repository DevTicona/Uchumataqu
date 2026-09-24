using System;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
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
        [SerializeField] private GameObject panelRespuestaCorrecta;
        [SerializeField] private TMP_Text txtRespuestaCorrecta;

        private bool _enviada;

        public bool PuedeEnviarRespuesta => !_enviada &&
            inputRespuesta != null && !string.IsNullOrWhiteSpace(inputRespuesta.text);

        public event Action OnRespuestaCambiada;
        public event Action<string> OnRespuestaEnviada;
        public event Action OnAudioClicked;

        private void Awake()
        {
            CrearPanelRespuestaCorrectaSiFalta();
            OcultarRespuestaCorrecta();
            MobileInputFieldStyle.Aplicar(inputRespuesta);
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
            MobileInputFieldStyle.Aplicar(inputRespuesta);
            OcultarRespuestaCorrecta();
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

        public void MostrarRespuestaCorrecta(string respuesta)
        {
            CrearPanelRespuestaCorrectaSiFalta();
            if (panelRespuestaCorrecta == null || txtRespuestaCorrecta == null) return;

            txtRespuestaCorrecta.text = respuesta ?? string.Empty;
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
        private void NotificarAudio() => OnAudioClicked?.Invoke();

        private void OcultarRespuestaCorrecta()
        {
            if (panelRespuestaCorrecta != null) panelRespuestaCorrecta.SetActive(false);
        }

        private void CrearPanelRespuestaCorrectaSiFalta()
        {
            if (panelRespuestaCorrecta != null || inputRespuesta == null || inputRespuesta.transform.parent == null) return;

            GameObject panel = new GameObject("RespuestaCorrecta", typeof(RectTransform), typeof(Image),
                typeof(VerticalLayoutGroup), typeof(LayoutElement));
            panel.transform.SetParent(inputRespuesta.transform.parent, false);
            panel.transform.SetSiblingIndex(inputRespuesta.transform.GetSiblingIndex() + 1);
            panel.GetComponent<LayoutElement>().preferredHeight = 106f;

            var layout = panel.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(22, 22, 14, 14);
            layout.spacing = 2f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            ThemedGraphicView fondo = panel.AddComponent<ThemedGraphicView>();
            fondo.Rol = ThemedGraphicView.RolColor.ControlSuave;

            CrearTextoCorreccion(panel.transform, "Etiqueta", "RESPUESTA CORRECTA", 18f, FontStyles.Bold, 28f);
            txtRespuestaCorrecta = CrearTextoCorreccion(panel.transform, "Texto", string.Empty, 26f, FontStyles.Normal, 48f);
            panelRespuestaCorrecta = panel;
        }

        private TMP_Text CrearTextoCorreccion(Transform parent, string nombre, string texto, float tamanio,
            FontStyles estilo, float alto)
        {
            GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            objeto.transform.SetParent(parent, false);
            var etiqueta = objeto.GetComponent<TextMeshProUGUI>();
            etiqueta.text = texto;
            etiqueta.font = inputRespuesta.textComponent.font;
            etiqueta.fontStyle = estilo;
            etiqueta.fontSize = tamanio;
            etiqueta.alignment = TextAlignmentOptions.MidlineLeft;
            etiqueta.color = UIThemeController.Theme != null ? UIThemeController.Theme.colorTextoPrimario : Color.black;
            objeto.GetComponent<LayoutElement>().preferredHeight = alto;
            return etiqueta;
        }
    }
}
