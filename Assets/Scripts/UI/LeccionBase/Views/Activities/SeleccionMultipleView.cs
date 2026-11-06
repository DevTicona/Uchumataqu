using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Data.Corpus;
using UnityEngine.Events;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    /// <summary>
    /// Vista para actividades de selección múltiple (UCH->ES, ES->UCH, Audio->UCH/ES, Forma->Raiz, etc.)
    /// Especificación: Docs/Specifications/04-flujo-leccion-app.md.
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

        public bool PuedeEnviarRespuesta => !_enviada && !string.IsNullOrEmpty(_opcionSeleccionada);
        public event Action OnSeleccionCambiada;

        public event Action<string> OnRespuestaEnviada;
        public event Action OnAudioPreguntaClicked;

        private PasoLeccionModel _pasoActual;
        private string _opcionSeleccionada;
        private int _indiceSeleccionado = -1;
        private readonly List<UnityAction> _accionesOpciones = new List<UnityAction>();
        private LayoutElement _encabezadoInstruccion;
        private float _altoMinimoEncabezado;
        private float _altoPreferidoEncabezado;

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

            DesregistrarListenersOpciones();

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
            DesregistrarListenersOpciones();
        }

        private void DesregistrarListenersOpciones()
        {
            for (int i = 0; i < _accionesOpciones.Count && i < botonesOpcion.Count; i++)
                if (botonesOpcion[i] != null)
                    botonesOpcion[i].onClick.RemoveListener(_accionesOpciones[i]);
            _accionesOpciones.Clear();
        }

        private void NotificarAudioPregunta() => OnAudioPreguntaClicked?.Invoke();

        public void Configurar(string tipo, string prompt, List<string> opcionesBarajadas,
            string audioPalabraId = null, string pistaEs = null)
        {
            _enviada = false;
            if (prefabOpcion != null)
            {
                LimpiarOpcionesDinamicas();

                for (int i = 0; i < opcionesBarajadas.Count; i++)
                {
                    Button boton = Instantiate(prefabOpcion, contenedorOpciones, false).GetComponent<Button>();
                    botonesOpcion.Add(boton);
                    textosOpcion.Add(boton.GetComponentInChildren<TMP_Text>());
                }
                RegistrarListeners();
            }
            bool esHueco = tipo == "completar_oracion";
            bool esAudio = tipo != null && tipo.StartsWith("audio_");
            if (panelTexto != null) panelTexto.SetActive(!esAudio && !esHueco);
            if (panelOracionHueco != null) panelOracionHueco.SetActive(esHueco);
            if (txtOracionHueco != null) txtOracionHueco.text = prompt ?? "";
            _opcionSeleccionada = null;
            _indiceSeleccionado = -1;

            if (txtInstruccion != null)
            {
                txtInstruccion.text = ObtenerInstruccionPorTipo(tipo, pistaEs);
                AjustarAltoEncabezado(esHueco);
            }

            if (txtTextoPregunta != null)
            {
                txtTextoPregunta.text = prompt ?? "";
            }

            if (btnAudioPregunta != null)
            {
                btnAudioPregunta.gameObject.SetActive(esAudio || (tipo != "uch_a_es" && !string.IsNullOrEmpty(audioPalabraId)));
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

                    botonesOpcion[i].GetComponent<TarjetaSeleccionView>()?.MostrarSeleccion(false);
                }
                else
                {
                    botonesOpcion[i].gameObject.SetActive(false);
                }
            }

            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        private void LimpiarOpcionesDinamicas()
        {
            DesregistrarListenersOpciones();
            if (contenedorOpciones != null)
            {
                for (int indice = contenedorOpciones.childCount - 1; indice >= 0; indice--)
                    RuntimeActividadUIFactory.Retirar(contenedorOpciones.GetChild(indice).gameObject);
            }

            botonesOpcion.Clear();
            textosOpcion.Clear();
        }

        private void AjustarAltoEncabezado(bool esHueco)
        {
            if (_encabezadoInstruccion == null)
            {
                _encabezadoInstruccion = txtInstruccion.transform.parent.GetComponent<LayoutElement>();
                if (_encabezadoInstruccion == null) return;
                _altoMinimoEncabezado = _encabezadoInstruccion.minHeight;
                _altoPreferidoEncabezado = _encabezadoInstruccion.preferredHeight;
            }

            _encabezadoInstruccion.minHeight = _altoMinimoEncabezado;
            _encabezadoInstruccion.preferredHeight = _altoPreferidoEncabezado;
            if (!esHueco) return;

            HorizontalLayoutGroup grupo = _encabezadoInstruccion.GetComponent<HorizontalLayoutGroup>();
            float margenVertical = grupo != null ? grupo.padding.top + grupo.padding.bottom : 0f;
            float anchoTexto = txtInstruccion.rectTransform.rect.width;
            float altoTexto = txtInstruccion.GetPreferredValues(txtInstruccion.text, anchoTexto, 0f).y;
            float altoNecesario = Mathf.Max(_altoMinimoEncabezado, Mathf.Ceil(altoTexto + margenVertical));
            _encabezadoInstruccion.minHeight = altoNecesario;
            _encabezadoInstruccion.preferredHeight = altoNecesario;
        }

        private void SeleccionarOpcion(int indice)
        {
            if (_enviada || indice < 0 || indice >= textosOpcion.Count) return;

            _indiceSeleccionado = indice;
            _opcionSeleccionada = textosOpcion[indice].text;

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
            botonesOpcion[_indiceSeleccionado].GetComponent<TarjetaSeleccionView>()?.MostrarResultado(correcto);
            if (!correcto && !string.IsNullOrEmpty(respuestaCorrecta))
                for (int i = 0; i < textosOpcion.Count; i++)
                    if (botonesOpcion[i].gameObject.activeSelf && textosOpcion[i].text == respuestaCorrecta)
                        botonesOpcion[i].GetComponent<TarjetaSeleccionView>()?.MostrarResultado(true);
            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        private string ObtenerInstruccionPorTipo(string tipo, string pistaEs)
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
                    return string.IsNullOrWhiteSpace(pistaEs)
                        ? "Completa la oración:"
                        : $"Completa la oración:\n«{pistaEs.Trim().Trim('«', '»')}»";
                case "audio_a_es_oracion":
                    return "Escucha y selecciona la traducción de la oración:";
                default:
                    return "Selecciona la respuesta correcta:";
            }
        }

    }
}
