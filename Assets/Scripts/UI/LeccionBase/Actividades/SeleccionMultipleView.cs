using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Data.Corpus;

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

        [Header("Opciones")]
        [SerializeField] private List<Button> botonesOpcion = new List<Button>();
        [SerializeField] private List<TMP_Text> textosOpcion = new List<TMP_Text>();
        [SerializeField] private List<Image> fondosOpcion = new List<Image>();

        [Header("Colores de Selección")]
        [SerializeField] private Color colorNormal = new Color(0.96f, 0.96f, 0.96f, 1f);
        [SerializeField] private Color colorSeleccionado = new Color(0.92f, 0.76f, 0.35f, 1f); // Tierra cálido
        [SerializeField] private Color colorCorrecto = new Color(0.40f, 0.73f, 0.42f, 1f);
        [SerializeField] private Color colorIncorrecto = new Color(0.90f, 0.35f, 0.35f, 1f);

        [Header("Acción")]
        [SerializeField] private Button btnComprobar;

        public event Action<string> OnRespuestaEnviada;
        public event Action OnAudioPreguntaClicked;

        private PasoLeccionData _pasoActual;
        private string _opcionSeleccionada;
        private int _indiceSeleccionado = -1;

        public void InicializarRuntime()
        {
            Transform root = transform;
            txtInstruccion = RuntimeActividadUIFactory.CrearTexto(root, "Instruccion", 25f);
            txtTextoPregunta = RuntimeActividadUIFactory.CrearTexto(root, "Pregunta", 36f);
            btnAudioPregunta = RuntimeActividadUIFactory.CrearBoton(root, "BtnAudio", "Escuchar audio");
            for (int i = 0; i < 4; i++)
            {
                Button boton = RuntimeActividadUIFactory.CrearBoton(root, $"Opcion_{i + 1}", string.Empty);
                botonesOpcion.Add(boton);
                textosOpcion.Add(boton.GetComponentInChildren<TextMeshProUGUI>());
                fondosOpcion.Add(boton.GetComponent<Image>());
            }
            btnComprobar = RuntimeActividadUIFactory.CrearBoton(root, "BtnComprobar", "Comprobar");
            RegistrarListeners();
        }

        private void Awake()
        {
            RegistrarListeners();
        }

        private void RegistrarListeners()
        {
            if (btnAudioPregunta != null)
            {
                btnAudioPregunta.onClick.RemoveAllListeners();
                btnAudioPregunta.onClick.AddListener(() => OnAudioPreguntaClicked?.Invoke());
            }

            if (btnComprobar != null)
            {
                btnComprobar.onClick.RemoveAllListeners();
                btnComprobar.onClick.AddListener(ConfirmarRespuesta);
            }

            for (int i = 0; i < botonesOpcion.Count; i++)
            {
                int index = i;
                botonesOpcion[i].onClick.RemoveAllListeners();
                botonesOpcion[i].onClick.AddListener(() => SeleccionarOpcion(index));
            }
        }

        public void Configurar(string tipo, string prompt, List<string> opcionesBarajadas, string audioPalabraId = null)
        {
            _opcionSeleccionada = null;
            _indiceSeleccionado = -1;

            if (btnComprobar != null)
                btnComprobar.interactable = false;

            if (txtInstruccion != null)
            {
                txtInstruccion.text = ObtenerInstruccionPorTipo(tipo);
            }

            if (txtTextoPregunta != null)
            {
                txtTextoPregunta.text = prompt ?? "";
            }

            bool esTipoAudio = tipo != null && tipo.StartsWith("audio_");
            if (btnAudioPregunta != null)
            {
                btnAudioPregunta.gameObject.SetActive(esTipoAudio || !string.IsNullOrEmpty(audioPalabraId));
            }

            // Configurar botones de opciones
            for (int i = 0; i < botonesOpcion.Count; i++)
            {
                if (i < opcionesBarajadas.Count)
                {
                    botonesOpcion[i].gameObject.SetActive(true);
                    if (i < textosOpcion.Count && textosOpcion[i] != null)
                        textosOpcion[i].text = opcionesBarajadas[i];

                    if (i < fondosOpcion.Count && fondosOpcion[i] != null)
                        fondosOpcion[i].color = colorNormal;
                }
                else
                {
                    botonesOpcion[i].gameObject.SetActive(false);
                }
            }
        }

        private void SeleccionarOpcion(int indice)
        {
            if (indice < 0 || indice >= textosOpcion.Count) return;

            _indiceSeleccionado = indice;
            _opcionSeleccionada = textosOpcion[indice].text;

            for (int i = 0; i < fondosOpcion.Count; i++)
            {
                if (fondosOpcion[i] != null)
                {
                    fondosOpcion[i].color = (i == _indiceSeleccionado) ? colorSeleccionado : colorNormal;
                }
            }

            if (btnComprobar != null)
                btnComprobar.interactable = true;
        }

        private void ConfirmarRespuesta()
        {
            if (string.IsNullOrEmpty(_opcionSeleccionada)) return;
            OnRespuestaEnviada?.Invoke(_opcionSeleccionada);
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
    }
}
