using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Data.Corpus;
using UnityEngine.Events;
using Assets.Scripts.Core.UI.Theme;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    /// <summary>
    /// Vista para actividades de selección múltiple (UCH->ES, ES->UCH, Audio->UCH/ES, Forma->Raiz, etc.)
    /// Especificación: Docs/UI/04-escena-leccion-base.md §9
    /// </summary>
    public class SeleccionMultipleView : MonoBehaviour
    {
        [Header("Pregunta / Prompt")]
        [SerializeField] private TMP_Text txtInstruccion;
        [SerializeField] private TMP_Text txtTextoPregunta;
        [SerializeField] private Button btnAudioPregunta;

        [SerializeField] private TMP_Text txtOracionHueco;
        [SerializeField] private GameObject panelTexto;
        [SerializeField] private GameObject panelOracionHueco;
        [SerializeField] private Transform contenedorOpciones;
        [SerializeField] private GameObject prefabOpcion;
        private bool _enviada;

        private readonly List<Button> botonesOpcion = new List<Button>();
        private readonly List<TMP_Text> textosOpcion = new List<TMP_Text>();
        private readonly List<Image> fondosOpcion = new List<Image>();

        [Header("Colores de Selección")]
        [SerializeField] private Color colorNormal = new Color(0.96f, 0.96f, 0.96f, 1f);
        [SerializeField] private Color colorSeleccionado = new Color(0.92f, 0.76f, 0.35f, 1f); // Tierra cálido
        [SerializeField] private Color colorCorrecto = new Color(0.40f, 0.73f, 0.42f, 1f);
        [SerializeField] private Color colorIncorrecto = new Color(0.90f, 0.35f, 0.35f, 1f);

        public bool PuedeEnviarRespuesta => !_enviada && !string.IsNullOrEmpty(_opcionSeleccionada);
        public event Action OnSeleccionCambiada;

        public event Action<string> OnRespuestaEnviada;
        public event Action OnAudioPreguntaClicked;

        private PasoLeccionModel _pasoActual;
        private string _opcionSeleccionada;
        private int _indiceSeleccionado = -1;
        private readonly List<UnityAction> _accionesOpciones = new List<UnityAction>();

        private void OnDisable()
        {
            StopAllCoroutines();
            CancelInvoke();
        }

        private void Awake()
        {
            RegistrarListeners();
        }

        private void RegistrarListeners()
        {
            if (btnAudioPregunta != null)
            {
                btnAudioPregunta.onClick.RemoveListener(NotificarAudioPregunta);
                btnAudioPregunta.onClick.AddListener(NotificarAudioPregunta);
            }

            for (int i = 0; i < _accionesOpciones.Count && i < botonesOpcion.Count; i++)
                if (botonesOpcion[i] != null)
                    botonesOpcion[i].onClick.RemoveListener(_accionesOpciones[i]);
            _accionesOpciones.Clear();

            for (int i = 0; i < botonesOpcion.Count; i++)
            {
                int index = i;
                UnityAction accion = () => SeleccionarOpcion(index);
                _accionesOpciones.Add(accion);
                if (botonesOpcion[i] != null)
                    botonesOpcion[i].onClick.AddListener(accion);
            }
        }

        private void OnDestroy()
        {
            if (btnAudioPregunta != null) btnAudioPregunta.onClick.RemoveListener(NotificarAudioPregunta);
            for (int i = 0; i < _accionesOpciones.Count && i < botonesOpcion.Count; i++)
                if (botonesOpcion[i] != null)
                    botonesOpcion[i].onClick.RemoveListener(_accionesOpciones[i]);
        }

        private void NotificarAudioPregunta() => OnAudioPreguntaClicked?.Invoke();

        public void Configurar(string tipo, string prompt, List<string> opcionesBarajadas, string audioPalabraId = null)
        {
            _enviada = false;
            if (prefabOpcion != null)
            {
                while (botonesOpcion.Count < opcionesBarajadas.Count)
                {
                    Button boton = Instantiate(prefabOpcion, contenedorOpciones, false).GetComponent<Button>();
                    botonesOpcion.Add(boton);
                    textosOpcion.Add(boton.GetComponentInChildren<TMP_Text>());
                    fondosOpcion.Add(boton.GetComponent<Image>());
                }
                RegistrarListeners();
            }
            bool esHueco = tipo == "completar_oracion";
            bool esAudio = tipo != null && tipo.StartsWith("audio_");
            if (panelTexto != null) panelTexto.SetActive(!esAudio && !esHueco);
            if (panelOracionHueco != null) panelOracionHueco.SetActive(esHueco);
            if (panelTexto != null) panelTexto.transform.parent.gameObject.SetActive(!esAudio);
            if (txtOracionHueco != null) txtOracionHueco.text = prompt ?? "";
            _opcionSeleccionada = null;
            _indiceSeleccionado = -1;

            if (txtInstruccion != null)
            {
                txtInstruccion.text = ObtenerInstruccionPorTipo(tipo);
            }

            if (txtTextoPregunta != null)
            {
                txtTextoPregunta.text = prompt ?? "";
            }

            if (btnAudioPregunta != null)
            {
                btnAudioPregunta.gameObject.SetActive(esAudio || !string.IsNullOrEmpty(audioPalabraId));
                TMP_Text etiquetaAudio = btnAudioPregunta.GetComponentInChildren<TMP_Text>(true);
                if (etiquetaAudio != null)
                    etiquetaAudio.text = tipo == "audio_a_es_oracion" ? "ESCUCHAR ORACIÓN" : "ESCUCHAR PALABRA";
            }

            // Configurar botones de opciones
            for (int i = 0; i < botonesOpcion.Count; i++)
            {
                if (i < opcionesBarajadas.Count)
                {
                    botonesOpcion[i].gameObject.SetActive(true);
                    botonesOpcion[i].interactable = true;
                    if (i < textosOpcion.Count && textosOpcion[i] != null)
                        textosOpcion[i].text = opcionesBarajadas[i];

                    if (i < fondosOpcion.Count && fondosOpcion[i] != null)
                        fondosOpcion[i].color = ObtenerColorNormal();
                    botonesOpcion[i].GetComponent<TarjetaSeleccionView>()?.MostrarSeleccion(false);
                }
                else
                {
                    botonesOpcion[i].gameObject.SetActive(false);
                }
            }

            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        private void SeleccionarOpcion(int indice)
        {
            if (_enviada || indice < 0 || indice >= textosOpcion.Count) return;

            _indiceSeleccionado = indice;
            _opcionSeleccionada = textosOpcion[indice].text;

            for (int i = 0; i < fondosOpcion.Count; i++)
            {
                if (fondosOpcion[i] != null)
                {
                    fondosOpcion[i].color = (i == _indiceSeleccionado)
                        ? ObtenerColorSeleccionado()
                        : ObtenerColorNormal();
                }
            }

            for (int i = 0; i < botonesOpcion.Count; i++)
                botonesOpcion[i].GetComponent<TarjetaSeleccionView>()?.MostrarSeleccion(i == _indiceSeleccionado);

            OnSeleccionCambiada?.Invoke();
            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        public void ConfirmarRespuesta()
        {
            if (_enviada || string.IsNullOrEmpty(_opcionSeleccionada)) return;
            _enviada = true;
            foreach (Button boton in botonesOpcion) boton.interactable = false;
            OnRespuestaEnviada?.Invoke(_opcionSeleccionada);
        }

        public void MostrarResultado(bool correcto, string respuestaCorrecta = null)
        {
            if (_indiceSeleccionado < 0) return;
            Image fondo = fondosOpcion[_indiceSeleccionado];
            Color color = correcto ? colorCorrecto : colorIncorrecto;
            if (UIThemeController.Theme != null)
                color = correcto ? UIThemeController.Theme.colorExito : UIThemeController.Theme.colorError;
            fondo.color = Color.Lerp(Color.white, color, .5f);
            botonesOpcion[_indiceSeleccionado].GetComponent<TarjetaSeleccionView>()?.MostrarResultado(correcto);
            if (!correcto && !string.IsNullOrEmpty(respuestaCorrecta))
                for (int i = 0; i < textosOpcion.Count; i++)
                    if (botonesOpcion[i].gameObject.activeSelf && textosOpcion[i].text == respuestaCorrecta)
                        botonesOpcion[i].GetComponent<TarjetaSeleccionView>()?.MostrarResultado(true);
            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        private string ObtenerInstruccionPorTipo(string tipo)
        {
            switch (tipo)
            {
                case "seleccion_simple_uch_es":
                case "seleccion_multiple_uch_es":
                case "uch_a_es":
                    return "¿Cuál es la traducción correcta?";
                case "seleccion_simple_es_uch":
                case "seleccion_multiple_es_uch":
                case "es_a_uch":
                    return "¿Cómo se dice en Uchumataqu?";
                case "audio_a_uch":
                case "audio_a_es":
                    return "Escucha el audio y selecciona la opción:";
                case "forma_a_raiz":
                    return "¿A qué palabra raíz pertenece esta forma?";
                case "forma_a_es":
                    return "¿Qué significado tiene esta forma expresiva?";
                case "es_a_forma":
                    return "Selecciona la forma documentada correcta:";
                case "audio_a_forma":
                    return "Escucha y selecciona la forma correcta:";
                case "completar_oracion":
                    return "Completa la oración:";
                case "audio_a_es_oracion":
                    return "Escucha y selecciona la traducción de la oración:";
                default:
                    return "Selecciona la respuesta correcta:";
            }
        }

        private Color ObtenerColorNormal() => UIThemeController.Theme != null
            ? UIThemeController.Theme.colorSuperficie
            : colorNormal;

        private Color ObtenerColorSeleccionado() => UIThemeController.Theme != null
            ? Color.Lerp(UIThemeController.Theme.colorControlSuave, UIThemeController.Theme.colorPrimario, 0.28f)
            : colorSeleccionado;
    }
}
