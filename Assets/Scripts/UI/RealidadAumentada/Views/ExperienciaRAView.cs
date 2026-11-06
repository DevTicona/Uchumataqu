using System;
using Assets.Scripts.Util;
using Assets.Scripts.UI.LeccionBase.Componentes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.RealidadAumentada
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.UI.RealidadAumentada", sourceAssembly: "Assembly-CSharp", sourceClassName: "ExperienciaRA1View")]
    public sealed class ExperienciaRAView : MonoBehaviour
    {
        public enum Estado { Instruccion, Correcto, Reintento, Incorrecto }

        [SerializeField] private RectTransform footer;
        [SerializeField] private FeedbackOverlayView feedback;
        [SerializeField] private TMP_Text titulo;
        [SerializeField] private TMP_Text detalle;
        [SerializeField] private TMP_Text ultimaInstruccion;
        [SerializeField] private Button principal;
        [SerializeField] private TMP_Text etiqueta;
        [SerializeField] private Image icono;
        [SerializeField] private Sprite comenzar;
        [SerializeField] private Sprite continuar;
        [SerializeField] private Sprite comprobar;
        [SerializeField] private Sprite finalizar;
        [SerializeField] private Sprite escuchar;
        [SerializeField] private Sprite microfono;
        [SerializeField] private Sprite detener;
        [SerializeField] private DialogoPersonajeRAView dialogo;
        [SerializeField] private RectTransform bienvenida;
        [SerializeField] private PulsoGuiaRAView guia;
        [Header("Entrada de escritura")]
        [Tooltip("Edita Height y el sprite de Image en este objeto de la escena.")]
        [SerializeField] private TMP_InputField entrada;
        [SerializeField, Min(0f)] private float separacionEscritura = 12f;
        [SerializeField, Min(0f)] private float espacioVerticalEscritura = 32f;
        [SerializeField] private TarjetaArrastrableRAView tarjeta;
        [SerializeField] private IconografiaRAView iconografia;
        [SerializeField] private Image iconoEstado;
        private float margenSuperiorDetalle;
        private float margenInferiorDetalle;
        private bool medidasInicializadas;
        private float anchoCalculado;
        private float margenEscrituraCalculado;
        private LayoutElement disposicionDetalle;
        private string instruccionEscritura, feedbackEscritura, cierreEscritura;
        private Color colorDetalle;
        private bool accionDisponible;

        public event Action AccionPrincipal;
        public event Action<string> RespuestaEditada;
        public event Action RepetirDialogo;
        public DialogoPersonajeRAView Dialogo => dialogo;
        public TarjetaArrastrableRAView Tarjeta => tarjeta;
        public bool Visible => footer != null && footer.gameObject.activeSelf;
        public string Accion => etiqueta.text;
        public bool BienvenidaVisible => bienvenida != null && bienvenida.gameObject.activeSelf;
        public string Respuesta => entrada != null ? FormatoTexto.ConInicialMayuscula(entrada.text) : string.Empty;

        private void Awake()
        {
            detalle.richText = true;
            colorDetalle = detalle.color;
            InicializarMedidas();
            principal.onClick.AddListener(NotificarAccion);
            dialogo.Repetir += NotificarRepeticion;
            if (entrada != null) entrada.onSubmit.AddListener(EnviarEscritura);
            if (entrada != null) entrada.onValueChanged.AddListener(EditarTextoEscritura);
            Ocultar();
        }

        private void OnDestroy()
        {
            if (principal != null) principal.onClick.RemoveListener(NotificarAccion);
            if (dialogo != null) dialogo.Repetir -= NotificarRepeticion;
            if (entrada != null) entrada.onSubmit.RemoveListener(EnviarEscritura);
            if (entrada != null) entrada.onValueChanged.RemoveListener(EditarTextoEscritura);
        }

        private void NotificarAccion() => AccionPrincipal?.Invoke();
        private void NotificarRepeticion() => RepetirDialogo?.Invoke();
        private void EnviarEscritura(string respuesta)
        {
            if (principal.interactable && entrada.gameObject.activeInHierarchy) NotificarAccion();
        }

        public void Mostrar(string encabezado, string instruccion, string accion,
            bool disponible, Estado estado = Estado.Instruccion, bool bienvenida = false, IconoRA guiaVisual = IconoRA.Mision,
            string diagnosticoMorfologico = null)
        {
            InicializarMedidas();
            footer.gameObject.SetActive(true);
            this.bienvenida.gameObject.SetActive(bienvenida);
            guia.Activar(bienvenida);
            switch (estado)
            {
                case Estado.Correcto: feedback.MostrarCorrecto(instruccion); break;
                case Estado.Reintento: feedback.MostrarCasi(instruccion); break;
                case Estado.Incorrecto: feedback.MostrarIncorrecto(instruccion); break;
                default: feedback.MostrarInformacion(instruccion); break;
            }
            feedback.MostrarDiagnosticoMorfologico(diagnosticoMorfologico);
            if (estado == Estado.Instruccion || guiaVisual == IconoRA.Completada)
                iconografia?.Aplicar(iconoEstado, bienvenida ? IconoRA.RealidadAumentada : guiaVisual);
            titulo.text = encabezado;
            detalle.text = instruccion;
            detalle.color = colorDetalle;
            etiqueta.text = accion.ToUpperInvariant();
            accionDisponible = disponible;
            ActualizarDisponibilidadAccion();
            icono.sprite = accion switch
            {
                "Comenzar" => comenzar,
                "Comprobar" => comprobar,
                "Finalizar" => finalizar,
                "Escuchar" or "Escuchando…" => escuchar,
                "Hablar" or "Pronunciar" or "Reintentar" => microfono,
                "Terminar" or "Detener" or "Comprobando…" => detener,
                _ => continuar
            };
            RecalcularAltura();
        }

        public void Ocultar(bool conservarDialogo = false)
        {
            if (feedback != null) feedback.MostrarDiagnosticoMorfologico(null);
            if (footer != null) footer.gameObject.SetActive(false);
            if (bienvenida != null) bienvenida.gameObject.SetActive(false);
            if (guia != null) guia.Activar(false);
            if (dialogo != null)
            {
                if (conservarDialogo) dialogo.Suspender(true);
                else dialogo.Ocultar();
            }
            if (tarjeta != null) tarjeta.Ocultar();
        }

        public void ReproducirSonidoRespuesta(Estado estado)
        {
            feedback.ReproducirSonidoRespuesta(estado switch
            {
                Estado.Correcto => "verde",
                Estado.Reintento => "amarillo",
                Estado.Incorrecto => "rojo",
                _ => "informacion"
            });
        }

        public void ConfigurarBienvenida(string encabezado, string descripcion)
        {
            bienvenida.Find("TituloBienvenida").GetComponent<TMP_Text>().text = encabezado;
            bienvenida.Find("DescripcionBienvenida").GetComponent<TMP_Text>().text = descripcion;
        }

        public void MostrarEscritura(bool visible, bool limpiar = false, bool disponible = true,
            string instruccion = "", string textoFeedback = "", string ultima = "")
        {
            if (entrada == null) return;
            InicializarMedidas();
            if (limpiar) entrada.SetTextWithoutNotify(string.Empty);
            entrada.gameObject.SetActive(visible);
            entrada.interactable = disponible;
            ActualizarDisponibilidadAccion();
            instruccionEscritura = instruccion;
            feedbackEscritura = textoFeedback;
            cierreEscritura = ultima;
            if (ultimaInstruccion != null) ultimaInstruccion.gameObject.SetActive(false);
            if (visible) ActualizarTextoEscritura(entrada.text);
            RecalcularAltura();
        }

        private void EditarTextoEscritura(string respuesta)
        {
            feedback.MostrarDiagnosticoMorfologico(null);
            ActualizarDisponibilidadAccion();
            ActualizarTextoEscritura(respuesta);
            RespuestaEditada?.Invoke(respuesta);
        }

        private void ActualizarDisponibilidadAccion()
        {
            principal.interactable = accionDisponible && (entrada == null || !entrada.gameObject.activeSelf ||
                !string.IsNullOrWhiteSpace(entrada.text));
        }

        private void ActualizarTextoEscritura(string respuesta)
        {
            if (entrada == null || !entrada.gameObject.activeSelf || string.IsNullOrEmpty(instruccionEscritura)) return;
            bool separar = !string.IsNullOrEmpty(respuesta) || !string.IsNullOrEmpty(feedbackEscritura);
            detalle.text = separar ? feedbackEscritura : instruccionEscritura;
            detalle.color = separar ? colorDetalle : new Color(0.12f, 0.36f, 0.56f);
            if (ultimaInstruccion != null)
            {
                ultimaInstruccion.gameObject.SetActive(separar && !string.IsNullOrEmpty(cierreEscritura));
                ultimaInstruccion.text = cierreEscritura;
            }
            RecalcularAltura();
        }

        private void InicializarMedidas()
        {
            if (medidasInicializadas) return;
            colorDetalle = detalle.color;
            margenSuperiorDetalle = detalle.rectTransform.offsetMax.y;
            margenInferiorDetalle = detalle.rectTransform.offsetMin.y;
            disposicionDetalle = detalle.transform.parent.GetComponent<LayoutElement>();
            medidasInicializadas = true;
        }

        private void LateUpdate()
        {
            if (Visible && (!Mathf.Approximately(footer.rect.width, anchoCalculado) ||
                !Mathf.Approximately(MargenEscritura(), margenEscrituraCalculado))) RecalcularAltura();
        }

        private float MargenEscritura() => entrada != null && entrada.gameObject.activeSelf
            ? Mathf.Max(0f, entrada.GetComponent<RectTransform>().offsetMax.y) + separacionEscritura
            : margenInferiorDetalle;

        private void RecalcularAltura()
        {
            InicializarMedidas();
            if (disposicionDetalle == null) return;
            // Primero resolver el ancho; la altura depende del ajuste de línea real de TMP.
            LayoutRebuilder.ForceRebuildLayoutImmediate(footer);
            var rect = detalle.rectTransform;
            bool escritura = entrada != null && entrada.gameObject.activeSelf;
            float margenEscritura = MargenEscritura();
            float altoInstruccion = ultimaInstruccion != null && ultimaInstruccion.gameObject.activeSelf
                ? ultimaInstruccion.GetPreferredValues(ultimaInstruccion.text, Mathf.Max(1f, rect.rect.width), Mathf.Infinity).y : 0f;
            if (ultimaInstruccion != null)
            {
                ultimaInstruccion.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, altoInstruccion);
                if (escritura)
                    ultimaInstruccion.rectTransform.anchoredPosition = new Vector2(
                        ultimaInstruccion.rectTransform.anchoredPosition.x, margenEscritura);
            }
            rect.offsetMin = new Vector2(rect.offsetMin.x, escritura
                ? margenEscritura + (altoInstruccion > 0f ? altoInstruccion + separacionEscritura : 0f) : margenInferiorDetalle);
            rect.offsetMax = new Vector2(rect.offsetMax.x,
                margenSuperiorDetalle - (escritura ? espacioVerticalEscritura : 0f));
            float altoTexto = string.IsNullOrEmpty(detalle.text) ? 0f :
                detalle.GetPreferredValues(detalle.text, Mathf.Max(1f, rect.rect.width), Mathf.Infinity).y;
            disposicionDetalle.preferredHeight = Mathf.Ceil(altoTexto + rect.offsetMin.y - rect.offsetMax.y);
            anchoCalculado = footer.rect.width;
            margenEscrituraCalculado = margenEscritura;
            LayoutRebuilder.ForceRebuildLayoutImmediate(footer);
        }
    }
}
