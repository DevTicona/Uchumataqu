using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    /// <summary>
    /// Vista interactiva para actividades con banco de fichas (Word Bank).
    /// Soporta: ordenar_oracion, dictado_word_bank, traduccion_constructiva.
    /// Especificación: Docs/Specifications/01-modulo-pln-texto.md §8 y Docs/UI/04-escena-leccion-base.md §9
    /// </summary>
    public class WordBankView : MonoBehaviour
    {
        [Header("Encabezado / Prompt")]
        [SerializeField] private TMP_Text txtInstruccion;
        [SerializeField] private TMP_Text txtPromptEs;
        [SerializeField] private Button btnAudioPrompt;

        [Header("Contenedores de Fichas")]
        [SerializeField] private Transform contenedorDestino; // Área donde se arma la oración
        [SerializeField] private Transform contenedorOrigen;  // Banco de fichas disponibles

        [Header("Prefab de Ficha")]
        [SerializeField] private GameObject prefabFichaToken;

        [Header("Acción")]
        [SerializeField] private Button btnComprobar;

        public event Action<List<string>> OnTokensComprobados;
        public event Action OnAudioPromptClicked;

        private readonly List<GameObject> _fichasActivas = new List<GameObject>();

        public void InicializarRuntime()
        {
            Transform root = transform;
            txtInstruccion = RuntimeActividadUIFactory.CrearTexto(root, "Instruccion", 25f);
            txtPromptEs = RuntimeActividadUIFactory.CrearTexto(root, "Prompt", 30f);
            btnAudioPrompt = RuntimeActividadUIFactory.CrearBoton(root, "BtnAudio", "Escuchar oración");
            contenedorDestino = RuntimeActividadUIFactory.CrearSubcontenedor(root, "Respuesta");
            contenedorOrigen = RuntimeActividadUIFactory.CrearSubcontenedor(root, "Banco");
            btnComprobar = RuntimeActividadUIFactory.CrearBoton(root, "BtnComprobar", "Comprobar");
            RegistrarListeners();
        }

        private void Awake()
        {
            RegistrarListeners();
        }

        private void RegistrarListeners()
        {
            if (btnAudioPrompt != null)
            {
                btnAudioPrompt.onClick.RemoveAllListeners();
                btnAudioPrompt.onClick.AddListener(() => OnAudioPromptClicked?.Invoke());
            }

            if (btnComprobar != null)
            {
                btnComprobar.onClick.RemoveAllListeners();
                btnComprobar.onClick.AddListener(ConfirmarRespuesta);
            }
        }

        public void Configurar(string tipo, string promptEs, List<string> tokensDisponibles, string oracionAudioId = null)
        {
            LimpiarFichas();

            if (btnComprobar != null)
                btnComprobar.interactable = false;

            if (txtInstruccion != null)
                txtInstruccion.text = ObtenerInstruccion(tipo);

            if (txtPromptEs != null)
                txtPromptEs.text = promptEs ?? "";

            bool esDictado = tipo == "dictado_word_bank";
            if (btnAudioPrompt != null)
            {
                btnAudioPrompt.gameObject.SetActive(esDictado || !string.IsNullOrEmpty(oracionAudioId));
            }

            // Instanciar fichas en el contenedor de origen (Banco)
            if (contenedorOrigen != null)
            {
                foreach (var token in tokensDisponibles)
                {
                    CrearFicha(token, contenedorOrigen);
                }
            }
        }

        private void CrearFicha(string textoToken, Transform padre)
        {
            GameObject ficha = prefabFichaToken != null
                ? Instantiate(prefabFichaToken, padre)
                : RuntimeActividadUIFactory.CrearBoton(padre, $"Ficha_{textoToken}", textoToken).gameObject;
            _fichasActivas.Add(ficha);

            TMP_Text txt = ficha.GetComponentInChildren<TMP_Text>();
            if (txt != null) txt.text = textoToken;

            Button btn = ficha.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnFichaTapped(ficha, textoToken));
            }
        }

        private void OnFichaTapped(GameObject ficha, string textoToken)
        {
            if (ficha.transform.parent == contenedorOrigen)
            {
                // Mover al contenedor de destino (armar oración)
                ficha.transform.SetParent(contenedorDestino, false);
            }
            else
            {
                // Devolver al banco
                ficha.transform.SetParent(contenedorOrigen, false);
            }

            if (btnComprobar != null)
                btnComprobar.interactable = contenedorDestino != null && contenedorDestino.childCount > 0;
        }

        private void ConfirmarRespuesta()
        {
            List<string> tokensSeleccionados = ObtenerTokensSeleccionados();
            if (tokensSeleccionados.Count == 0) return;
            OnTokensComprobados?.Invoke(tokensSeleccionados);
        }

        private List<string> ObtenerTokensSeleccionados()
        {
            var tokens = new List<string>();
            if (contenedorDestino == null) return tokens;

            foreach (Transform ficha in contenedorDestino)
            {
                TMP_Text texto = ficha.GetComponentInChildren<TMP_Text>();
                if (texto != null) tokens.Add(texto.text);
            }
            return tokens;
        }

        private void LimpiarFichas()
        {
            foreach (var f in _fichasActivas)
            {
                if (f != null) Destroy(f);
            }
            _fichasActivas.Clear();
        }

        private string ObtenerInstruccion(string tipo)
        {
            switch (tipo)
            {
                case "ordenar_oracion":
                    return "Ordena las fichas para formar la oración:";
                case "traduccion_constructiva":
                    return "Traduce al Uchumataqu usando el banco de fichas:";
                case "dictado_word_bank":
                    return "Escucha el audio y arma la oración:";
                default:
                    return "Arma la respuesta con las fichas:";
            }
        }
    }
}
