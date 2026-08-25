using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista de la pestaña de Ajustes en 02_Main.
    /// Especificado en Docs/UI/08-perfil-logros-ajustes-acerca.md §4.
    /// </summary>
    public class AjustesView : MonoBehaviour
    {
        [Header("Datos de Perfil")]
        [SerializeField] private TMP_InputField inputNombre;
        [SerializeField] private Button btnGuardarPerfil;

        [Header("Audio")]
        [SerializeField] private Slider sliderVolumenGeneral;
        [SerializeField] private Toggle toggleAudioAutomatico;
        [SerializeField] private Toggle toggleVibracion;

        [Header("Zona de Peligro")]
        [SerializeField] private Button btnReiniciarProgreso;
        [SerializeField] private GameObject dialogoConfirmacionReinicio;
        [SerializeField] private Button btnConfirmarReinicio;
        [SerializeField] private Button btnCancelarReinicio;

        public event Action<string> OnGuardarNombre;
        public event Action<float> OnCambiarVolumen;
        public event Action<bool> OnCambiarAudioAutomatico;
        public event Action<bool> OnCambiarVibracion;
        public event Action OnConfirmarReinicio;
        public event Action OnSolicitarActualizacion;

        private void Awake()
        {
            CrearControlesOpcionalesSiFaltan();

            if (btnGuardarPerfil != null)
                btnGuardarPerfil.onClick.AddListener(GuardarNombre);

            if (sliderVolumenGeneral != null)
                sliderVolumenGeneral.onValueChanged.AddListener(OnVolumenChanged);

            if (toggleAudioAutomatico != null)
                toggleAudioAutomatico.onValueChanged.AddListener(valor => OnCambiarAudioAutomatico?.Invoke(valor));

            if (toggleVibracion != null)
                toggleVibracion.onValueChanged.AddListener(valor => OnCambiarVibracion?.Invoke(valor));

            if (btnReiniciarProgreso != null)
                btnReiniciarProgreso.onClick.AddListener(() => MostrarDialogoReinicio(true));

            if (btnCancelarReinicio != null)
                btnCancelarReinicio.onClick.AddListener(() => MostrarDialogoReinicio(false));

            if (btnConfirmarReinicio != null)
                btnConfirmarReinicio.onClick.AddListener(EjecutarReinicio);
        }

        private void OnEnable()
        {
            MostrarDialogoReinicio(false);
            OnSolicitarActualizacion?.Invoke();
        }

        public void Mostrar(string nombre, float volumen, bool audioAutomatico, bool vibracion)
        {
            if (inputNombre != null) inputNombre.text = nombre ?? string.Empty;
            if (sliderVolumenGeneral != null) sliderVolumenGeneral.SetValueWithoutNotify(volumen);
            if (toggleAudioAutomatico != null) toggleAudioAutomatico.SetIsOnWithoutNotify(audioAutomatico);
            if (toggleVibracion != null) toggleVibracion.SetIsOnWithoutNotify(vibracion);
        }

        private void GuardarNombre()
        {
            if (inputNombre == null) return;
            string nuevoNombre = inputNombre.text.Trim();
            if (string.IsNullOrEmpty(nuevoNombre)) return;

            OnGuardarNombre?.Invoke(nuevoNombre);
        }

        private void OnVolumenChanged(float vol)
        {
            OnCambiarVolumen?.Invoke(vol);
        }

        private void MostrarDialogoReinicio(bool mostrar)
        {
            if (dialogoConfirmacionReinicio != null)
                dialogoConfirmacionReinicio.SetActive(mostrar);
        }

        private void EjecutarReinicio()
        {
            MostrarDialogoReinicio(false);
            OnConfirmarReinicio?.Invoke();
        }

        private void CrearControlesOpcionalesSiFaltan()
        {
            Transform contenedor = sliderVolumenGeneral != null ? sliderVolumenGeneral.transform.parent : transform;
            if (toggleAudioAutomatico == null)
                toggleAudioAutomatico = CrearToggle(contenedor, "ToggleAudioAutomatico", "Reproducir audio automáticamente");
            if (toggleVibracion == null)
                toggleVibracion = CrearToggle(contenedor, "ToggleVibracion", "Vibración");
        }

        private static Toggle CrearToggle(Transform padre, string nombre, string etiqueta)
        {
            var raiz = new GameObject(nombre, typeof(RectTransform), typeof(Toggle), typeof(LayoutElement));
            raiz.transform.SetParent(padre, false);
            raiz.GetComponent<LayoutElement>().preferredHeight = 56f;

            var fondo = new GameObject("Fondo", typeof(RectTransform), typeof(Image));
            fondo.transform.SetParent(raiz.transform, false);
            var fondoRect = (RectTransform)fondo.transform;
            fondoRect.anchorMin = new Vector2(0f, 0.5f);
            fondoRect.anchorMax = new Vector2(0f, 0.5f);
            fondoRect.anchoredPosition = new Vector2(24f, 0f);
            fondoRect.sizeDelta = new Vector2(36f, 36f);
            fondo.GetComponent<Image>().color = new Color(0.20f, 0.24f, 0.30f, 1f);

            var marca = new GameObject("Marca", typeof(RectTransform), typeof(Image));
            marca.transform.SetParent(fondo.transform, false);
            var marcaRect = (RectTransform)marca.transform;
            marcaRect.anchorMin = new Vector2(0.2f, 0.2f);
            marcaRect.anchorMax = new Vector2(0.8f, 0.8f);
            marcaRect.offsetMin = Vector2.zero;
            marcaRect.offsetMax = Vector2.zero;
            marca.GetComponent<Image>().color = new Color(0.03f, 0.60f, 0.65f, 1f);

            var textoObjeto = new GameObject("Etiqueta", typeof(RectTransform), typeof(TextMeshProUGUI));
            textoObjeto.transform.SetParent(raiz.transform, false);
            var textoRect = (RectTransform)textoObjeto.transform;
            textoRect.anchorMin = Vector2.zero;
            textoRect.anchorMax = Vector2.one;
            textoRect.offsetMin = new Vector2(56f, 0f);
            textoRect.offsetMax = Vector2.zero;
            var texto = textoObjeto.GetComponent<TextMeshProUGUI>();
            texto.text = etiqueta;
            texto.fontSize = 20f;
            texto.color = Color.white;
            texto.alignment = TextAlignmentOptions.MidlineLeft;

            Toggle toggle = raiz.GetComponent<Toggle>();
            toggle.targetGraphic = fondo.GetComponent<Image>();
            toggle.graphic = marca.GetComponent<Image>();
            toggle.isOn = true;
            return toggle;
        }
    }
}
