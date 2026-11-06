using System;
using System.Collections.Generic;
using Assets.Scripts.Core.UI;
using Assets.Scripts.Core.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    public sealed class WordBankView : MonoBehaviour
    {
        [SerializeField] private TMP_Text txtInstruccion;
        [SerializeField] private TMP_Text txtPromptEs;
        [SerializeField] private Button btnAudioPrompt;
        [SerializeField] private Transform contenedorDestino;
        [SerializeField] private Transform contenedorOrigen;
        [SerializeField] private GameObject prefabFichaToken;
        [Header("Distribucion dinamica")]
        [SerializeField, Min(0f)] private float separacionFichas = 12f;
        [SerializeField, Min(1)] private int maximoFichasPorFila = 3;
        [SerializeField] private TextAnchor alineacionFilas = TextAnchor.MiddleCenter;
        public bool PuedeEnviarRespuesta => !_enviada && _respuesta.Count > 0;
        public event Action OnRespuestaCambiada;

        public event Action<List<string>> OnTokensComprobados;
        public event Action OnAudioPromptClicked;

        private readonly List<GameObject> _banco = new List<GameObject>();
        private readonly List<GameObject> _respuesta = new List<GameObject>();
        private bool _enviada;
        private readonly UIEventSubscriptions _tokenSubscriptions = new UIEventSubscriptions();
        private float _ultimoAncho;
        private float _ultimoAnchoDestino;
        private readonly List<RectTransform> _filasBanco = new List<RectTransform>();
        private readonly List<RectTransform> _filasRespuesta = new List<RectTransform>();
        private bool _mostrarRespuestaPendiente;

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
            float anchoDestino = ((RectTransform)contenedorDestino).rect.width;
            if (Mathf.Abs(ancho - _ultimoAncho) > 1f ||
                Mathf.Abs(anchoDestino - _ultimoAnchoDestino) > 1f) ReconstruirFilas();
            if (_mostrarRespuestaPendiente)
            {
                MostrarRespuestaEnViewport();
                _mostrarRespuestaPendiente = false;
            }
        }

        public void Configurar(string tipo, string promptEs, List<string> tokensDisponibles, string oracionAudioId = null)
        {
            _tokenSubscriptions.Dispose();
            Limpiar(_banco); Limpiar(_respuesta);
            LimpiarFilasGeneradas(contenedorOrigen, _filasBanco);
            LimpiarFilasGeneradas(contenedorDestino, _filasRespuesta);
            _enviada = false;
            _mostrarRespuestaPendiente = false;
            txtInstruccion.text = tipo == "dictado_word_bank" ? "Escucha y construye la oración" :
                tipo == "traduccion_constructiva" ? "Construye la traducción al español" : "Ordena la oración en uchumataqu";
            txtPromptEs.text = promptEs ?? string.Empty;
            txtPromptEs.gameObject.SetActive(!string.IsNullOrEmpty(promptEs));
            btnAudioPrompt.gameObject.SetActive(tipo == "dictado_word_bank");
            txtPromptEs.transform.parent.gameObject.SetActive(!string.IsNullOrEmpty(promptEs));
            foreach (string token in tokensDisponibles)
            {
                GameObject ficha = RuntimeActividadUIFactory.Instanciar(prefabFichaToken, contenedorOrigen);
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
            ReconstruirFilas();
            _mostrarRespuestaPendiente = true;
            OnRespuestaCambiada?.Invoke();
        }

        private void MostrarRespuestaEnViewport()
        {
            ScrollRect scroll = GetComponentInChildren<ScrollRect>();
            if (scroll == null || scroll.content == null || scroll.viewport == null) return;
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(
                scroll.viewport, (RectTransform)contenedorDestino);
            Rect viewport = scroll.viewport.rect;
            float desplazamiento = bounds.size.y > viewport.height || bounds.max.y > viewport.yMax
                ? viewport.yMax - bounds.max.y
                : bounds.min.y < viewport.yMin ? viewport.yMin - bounds.min.y : 0f;
            if (Mathf.Abs(desplazamiento) < 1f) return;
            scroll.StopMovement();
            Vector2 posicion = scroll.content.anchoredPosition;
            posicion.y = Mathf.Clamp(posicion.y + desplazamiento, 0f,
                Mathf.Max(0f, scroll.content.rect.height - viewport.height));
            scroll.content.anchoredPosition = posicion;
        }

        public void SolicitarMostrarRespuesta() => _mostrarRespuestaPendiente = _respuesta.Count > 0;

        private void ReconstruirFilas()
        {
            _banco.RemoveAll(ficha => ficha == null);
            _respuesta.RemoveAll(ficha => ficha == null);
            _ultimoAncho = ((RectTransform)contenedorOrigen).rect.width;
            _ultimoAnchoDestino = ((RectTransform)contenedorDestino).rect.width;
            // Conserva las fichas y reutiliza las filas al cambiar la respuesta.
            foreach (GameObject ficha in _banco) ficha.transform.SetParent(contenedorOrigen, false);
            foreach (GameObject ficha in _respuesta) ficha.transform.SetParent(contenedorDestino, false);
            float anchoFicha = Mathf.Min(AnchoDisponible(contenedorOrigen), AnchoDisponible(contenedorDestino));
            foreach (GameObject ficha in _banco) AjustarAnchoFicha(ficha, anchoFicha);
            foreach (GameObject ficha in _respuesta) AjustarAnchoFicha(ficha, anchoFicha);
            Distribuir(contenedorOrigen, _banco, _filasBanco);
            Distribuir(contenedorDestino, _respuesta, _filasRespuesta);
            AjustarAlturaCajaOracion();
            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        private static float AnchoDisponible(Transform contenedor)
        {
            VerticalLayoutGroup layout = contenedor.GetComponent<VerticalLayoutGroup>();
            return Mathf.Max(0f, ((RectTransform)contenedor).rect.width -
                (layout != null ? layout.padding.horizontal : 0f));
        }

        private static void AjustarAnchoFicha(GameObject ficha, float anchoDisponible)
        {
            TMP_Text texto = ficha.GetComponentInChildren<TMP_Text>();
            LayoutElement elemento = ficha.GetComponent<LayoutElement>();
            HorizontalLayoutGroup layout = ficha.GetComponent<HorizontalLayoutGroup>();
            if (texto == null || elemento == null || layout == null) return;

            texto.enableAutoSizing = false;
            texto.textWrappingMode = TextWrappingModes.NoWrap;
            float anchoTexto = texto.GetPreferredValues(texto.text).x;
            float anchoNecesario = Mathf.Max(elemento.minWidth,
                Mathf.Ceil(anchoTexto + layout.padding.horizontal + 12f));
            float ancho = anchoDisponible > 0f ? Mathf.Min(anchoNecesario, anchoDisponible) : anchoNecesario;
            elemento.layoutPriority = 1;
            elemento.preferredWidth = ancho;
            if (ancho < anchoNecesario)
            {
                texto.fontSizeMax = texto.fontSize;
                texto.fontSizeMin = Mathf.Min(24f, texto.fontSize);
                texto.enableAutoSizing = true;
            }
        }

        private void Distribuir(Transform contenedor, List<GameObject> fichas, List<RectTransform> filas)
        {
            filas.RemoveAll(fila => fila == null);
            foreach (RectTransform anterior in filas) anterior.gameObject.SetActive(false);
            VerticalLayoutGroup vertical = contenedor.GetComponent<VerticalLayoutGroup>();
            if (vertical == null) return;

            float ancho = AnchoDisponible(contenedor);
            RectTransform fila = null;
            int indiceFila = 0;
            float ocupado = 0;
            float altoFila = 0;
            int fichasEnFila = 0;
            foreach (GameObject ficha in fichas)
            {
                RectTransform rectFicha = ficha.transform as RectTransform;
                float preferido = Mathf.Max(0f, LayoutUtility.GetPreferredWidth(rectFicha));
                if (preferido <= 0f) preferido = rectFicha.rect.width;
                float altoPreferido = Mathf.Max(0f, LayoutUtility.GetPreferredHeight(rectFicha));
                if (altoPreferido <= 0f) altoPreferido = rectFicha.rect.height;

                if (fila == null || fichasEnFila >= maximoFichasPorFila ||
                    ocupado + separacionFichas + preferido > ancho)
                {
                    if (indiceFila == filas.Count)
                    {
                        GameObject row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                        row.layer = contenedor.gameObject.layer;
                        row.transform.SetParent(contenedor, false);
                        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
                        layout.spacing = separacionFichas;
                        layout.childControlWidth = layout.childControlHeight = true;
                        layout.childAlignment = alineacionFilas;
                        layout.childForceExpandWidth = layout.childForceExpandHeight = false;
                        filas.Add((RectTransform)row.transform);
                    }
                    fila = filas[indiceFila++];
                    fila.gameObject.SetActive(true);
                    fila.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ancho);
                    fila.SetAsLastSibling();
                    ocupado = 0;
                    altoFila = 0;
                    fichasEnFila = 0;
                }
                ficha.transform.SetParent(fila, false);
                ocupado += (fichasEnFila > 0 ? separacionFichas : 0f) + preferido;
                fichasEnFila++;
                altoFila = Mathf.Max(altoFila, altoPreferido);
                LayoutElement elementoFila = fila.GetComponent<LayoutElement>();
                elementoFila.minHeight = elementoFila.preferredHeight = altoFila;
            }

            float altoFilas = 0f;
            for (int i = 0; i < indiceFila; i++)
                altoFilas += filas[i].GetComponent<LayoutElement>().preferredHeight;
            LayoutElement elementoContenedor = contenedor.GetComponent<LayoutElement>();
            if (elementoContenedor != null)
                elementoContenedor.preferredHeight = Mathf.Max(elementoContenedor.minHeight,
                    vertical.padding.vertical + altoFilas + Mathf.Max(0, indiceFila - 1) * vertical.spacing);
        }

        private void AjustarAlturaCajaOracion()
        {
            Transform caja = contenedorDestino.parent;
            LayoutElement elementoCaja = caja.GetComponent<LayoutElement>();
            VerticalLayoutGroup layoutCaja = caja.GetComponent<VerticalLayoutGroup>();
            LayoutElement elementoRespuesta = contenedorDestino.GetComponent<LayoutElement>();
            if (elementoCaja == null || layoutCaja == null || elementoRespuesta == null) return;

            elementoCaja.preferredHeight = Mathf.Max(elementoCaja.minHeight,
                layoutCaja.padding.vertical + elementoRespuesta.preferredHeight);

            Transform contenido = caja.parent;
            LayoutElement elementoContenido = contenido.GetComponent<LayoutElement>();
            VerticalLayoutGroup layoutContenido = contenido.GetComponent<VerticalLayoutGroup>();
            if (elementoContenido == null || layoutContenido == null) return;

            float altoTitulo = ((RectTransform)contenido.GetChild(0)).rect.height;
            elementoContenido.preferredHeight = Mathf.Max(elementoContenido.minHeight,
                layoutContenido.padding.vertical + altoTitulo + layoutContenido.spacing +
                elementoCaja.preferredHeight);
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

        public void MostrarResultado(string semaforo)
        {
            var estado = semaforo switch
            {
                "verde" => ThemedButtonView.EstadoBoton.Correcto,
                "amarillo" => ThemedButtonView.EstadoBoton.Casi,
                _ => ThemedButtonView.EstadoBoton.Incorrecto
            };
            foreach (GameObject ficha in _respuesta)
                ficha.GetComponent<ThemedButtonView>().MostrarEstado(estado);
        }

        private static void Limpiar(List<GameObject> fichas)
        {
            foreach (GameObject ficha in fichas)
            {
                if (ficha != null) RuntimeActividadUIFactory.Retirar(ficha);
            }
            fichas.Clear();
        }

        private static void LimpiarFilasGeneradas(Transform contenedor, List<RectTransform> filas)
        {
            for (int indice = contenedor.childCount - 1; indice >= 0; indice--)
            {
                Transform hijo = contenedor.GetChild(indice);
                if (hijo.name == "Row" && hijo.GetComponent<HorizontalLayoutGroup>() != null)
                    RuntimeActividadUIFactory.Retirar(hijo.gameObject);
            }
            filas.Clear();
        }

        private void Audio() => OnAudioPromptClicked?.Invoke();
    }
}
