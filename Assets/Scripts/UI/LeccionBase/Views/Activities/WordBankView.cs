using System;
using System.Collections.Generic;
using Assets.Scripts.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    public sealed class WordBankView : MonoBehaviour
    {
        [SerializeField] private TMP_Text txtInstruccion;
        [SerializeField] private TMP_Text txtPromptEs;
        [SerializeField] private TMP_Text txtRespuestaVacia;
        [SerializeField] private Button btnAudioPrompt;
        [SerializeField] private Transform contenedorDestino;
        [SerializeField] private Transform contenedorOrigen;
        [SerializeField] private GameObject prefabFichaToken;
        public bool PuedeEnviarRespuesta => !_enviada && _respuesta.Count > 0;
        public event Action OnRespuestaCambiada;

        public event Action<List<string>> OnTokensComprobados;
        public event Action OnAudioPromptClicked;

        private readonly List<GameObject> _banco = new List<GameObject>();
        private readonly List<GameObject> _respuesta = new List<GameObject>();
        private bool _enviada;
        private readonly UIEventSubscriptions _tokenSubscriptions = new UIEventSubscriptions();
        private float _ultimoAncho;

        private void Awake()
        {
            btnAudioPrompt.onClick.AddListener(Audio);
        }


        private void OnDestroy()
        {
            _tokenSubscriptions.Dispose();
            if (btnAudioPrompt != null) btnAudioPrompt.onClick.RemoveListener(Audio);
        }

        private void LateUpdate()
        {
            float ancho = ((RectTransform)contenedorOrigen).rect.width;
            if (Mathf.Abs(ancho - _ultimoAncho) > 1f) ReconstruirFilas();
        }

        public void Configurar(string tipo, string promptEs, List<string> tokensDisponibles, string oracionAudioId = null)
        {
            _tokenSubscriptions.Dispose();
            Limpiar(_banco); Limpiar(_respuesta);
            _enviada = false;
            txtInstruccion.text = tipo == "dictado_word_bank" ? "Escucha y construye la oración" :
                tipo == "traduccion_constructiva" ? "Construye la traducción al español" : "Ordena la oración en uchumataqu";
            txtPromptEs.text = promptEs ?? string.Empty;
            txtPromptEs.gameObject.SetActive(!string.IsNullOrEmpty(promptEs));
            btnAudioPrompt.gameObject.SetActive(tipo == "dictado_word_bank");
            txtPromptEs.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(promptEs) || tipo == "dictado_word_bank");
            foreach (string token in tokensDisponibles)
            {
                GameObject ficha = Instantiate(prefabFichaToken, contenedorOrigen, false);
                ficha.GetComponentInChildren<TMP_Text>().text = token;
                _tokenSubscriptions.Add(ficha.GetComponent<Button>(), () => Mover(ficha));
                _banco.Add(ficha);
            }
            ReconstruirFilas();
        }

        private void Mover(GameObject ficha)
        {
            if (_enviada) return;
            if (_banco.Remove(ficha)) _respuesta.Add(ficha);
            else { _respuesta.Remove(ficha); _banco.Add(ficha); }
            OnRespuestaCambiada?.Invoke();
            ReconstruirFilas();
        }

        private void ReconstruirFilas()
        {
            _ultimoAncho = ((RectTransform)contenedorOrigen).rect.width;
            // Saca todas las fichas de las filas anteriores antes de retirarlas.
            foreach (GameObject ficha in _banco) ficha.transform.SetParent(contenedorOrigen, false);
            foreach (GameObject ficha in _respuesta) ficha.transform.SetParent(contenedorDestino, false);
            Distribuir(contenedorOrigen, _banco);
            Distribuir(contenedorDestino, _respuesta);
            if (txtRespuestaVacia != null) txtRespuestaVacia.gameObject.SetActive(_respuesta.Count == 0);
            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        private static void Distribuir(Transform contenedor, List<GameObject> fichas)
        {
            // Move tokens before retiring the previous native rows, preserving instance identity.
            foreach (GameObject ficha in fichas) ficha.transform.SetParent(contenedor, false);
            for (int i = contenedor.childCount - 1; i >= 0; i--)
            {
                Transform child = contenedor.GetChild(i);
                if (child.GetComponent<HorizontalLayoutGroup>() == null || child.GetComponent<Button>() != null) continue;
                child.gameObject.SetActive(false);
                RuntimeActividadUIFactory.Retirar(child.gameObject);
            }
            VerticalLayoutGroup vertical = contenedor.GetComponent<VerticalLayoutGroup>();
            float ancho = Mathf.Max(120, ((RectTransform)contenedor).rect.width - vertical.padding.horizontal);
            Transform fila = null;
            float ocupado = 0;
            foreach (GameObject ficha in fichas)
            {
                TMP_Text texto = ficha.GetComponentInChildren<TMP_Text>();
                float preferido = Mathf.Clamp(texto.GetPreferredValues(texto.text).x + 48, 108, ancho);
                LayoutElement elemento = ficha.GetComponent<LayoutElement>();
                elemento.minWidth = preferido; elemento.preferredWidth = preferido; elemento.flexibleWidth = 0;
                elemento.minHeight = Mathf.Max(72f, texto.preferredHeight + 16f);
                elemento.preferredHeight = -1;
                if (fila == null || ocupado + preferido > ancho)
                {
                    GameObject row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                    row.layer = contenedor.gameObject.layer;
                    row.transform.SetParent(contenedor, false);
                    HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
                    layout.spacing = 12; layout.childControlWidth = layout.childControlHeight = true;
                    layout.childAlignment = TextAnchor.MiddleCenter;
                    layout.childForceExpandWidth = layout.childForceExpandHeight = false;
                    fila = row.transform; ocupado = 0;
                }
                ficha.transform.SetParent(fila, false);
                ocupado += preferido + 12;
            }
        }

        public void Comprobar()
        {
            if (_enviada || _respuesta.Count == 0) return;
            _enviada = true;
            var tokens = new List<string>();
            foreach (GameObject ficha in _respuesta) tokens.Add(ficha.GetComponentInChildren<TMP_Text>().text);
            foreach (GameObject ficha in _banco) ficha.GetComponent<Button>().interactable = false;
            foreach (GameObject ficha in _respuesta) ficha.GetComponent<Button>().interactable = false;
            OnTokensComprobados?.Invoke(tokens);
        }

        private static void Limpiar(List<GameObject> fichas)
        {
            foreach (GameObject ficha in fichas) { ficha.SetActive(false); RuntimeActividadUIFactory.Retirar(ficha); }
            fichas.Clear();
        }

        private void Audio() => OnAudioPromptClicked?.Invoke();
    }
}
