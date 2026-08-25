using System;
using System.Collections.Generic;
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

        public event Action<string, string, string> OnGuardarPerfil;
        public event Action<float> OnCambiarVolumen;
        public event Action<bool> OnCambiarAudioAutomatico;
        public event Action<bool> OnCambiarVibracion;
        public event Action OnConfirmarReinicio;
        public event Action OnSolicitarActualizacion;

        private static readonly string[] RangosEdad = { "6–9", "10–12", "13–17", "18+" };
        private readonly List<Button> _botonesRango = new List<Button>();
        private readonly List<Button> _botonesAvatar = new List<Button>();
        private string _rangoSeleccionado;
        private string _avatarSeleccionado;
        private TMP_Text _textoEstadoPerfil;

        private void Awake()
        {
            CrearControlesOpcionalesSiFaltan();

            if (btnGuardarPerfil != null)
                btnGuardarPerfil.onClick.AddListener(GuardarPerfil);

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

        public void Mostrar(
            string nombre,
            string rangoEdad,
            string avatarId,
            float volumen,
            bool audioAutomatico,
            bool vibracion)
        {
            if (inputNombre != null) inputNombre.text = nombre ?? string.Empty;
            SeleccionarRango(rangoEdad);
            SeleccionarAvatar(avatarId);
            MostrarEstadoPerfil(null, false);
            if (sliderVolumenGeneral != null) sliderVolumenGeneral.SetValueWithoutNotify(volumen);
            if (toggleAudioAutomatico != null) toggleAudioAutomatico.SetIsOnWithoutNotify(audioAutomatico);
            if (toggleVibracion != null) toggleVibracion.SetIsOnWithoutNotify(vibracion);
        }

        public void MostrarErrorPerfil(string mensaje) => MostrarEstadoPerfil(mensaje, true);

        public void MostrarPerfilGuardado() => MostrarEstadoPerfil("Perfil actualizado.", false);

        private void GuardarPerfil()
        {
            if (inputNombre == null) return;
            OnGuardarPerfil?.Invoke(inputNombre.text.Trim(), _rangoSeleccionado, _avatarSeleccionado);
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
            CrearControlesPerfilSiFaltan(contenedor);
            if (toggleAudioAutomatico == null)
                toggleAudioAutomatico = CrearToggle(contenedor, "ToggleAudioAutomatico", "Reproducir audio automáticamente");
            if (toggleVibracion == null)
                toggleVibracion = CrearToggle(contenedor, "ToggleVibracion", "Vibración");
        }

        private void CrearControlesPerfilSiFaltan(Transform contenedor)
        {
            if (contenedor == null) return;

            Transform selectorEdad = contenedor.Find("SelectorRangoEdad") ??
                                      CrearSelector(contenedor, "SelectorRangoEdad", "Rango de edad", 2, 112f);
            CrearBotonesSelector(selectorEdad, RangosEdad, _botonesRango, SeleccionarRango);

            Transform selectorAvatar = contenedor.Find("SelectorAvatar") ??
                                        CrearSelector(contenedor, "SelectorAvatar", "Avatar", 3, 112f);
            string[] avatares = { "avatar_01", "avatar_02", "avatar_03", "avatar_04", "avatar_05", "avatar_06" };
            CrearBotonesSelector(selectorAvatar, avatares, _botonesAvatar, SeleccionarAvatar);

            _textoEstadoPerfil = CrearTexto(contenedor, "EstadoPerfil", 18f);
            _textoEstadoPerfil.gameObject.SetActive(false);
        }

        private void SeleccionarRango(string rango)
        {
            _rangoSeleccionado = rango ?? string.Empty;
            ActualizarSeleccion(_botonesRango, _rangoSeleccionado);
            MostrarEstadoPerfil(null, false);
        }

        private void SeleccionarAvatar(string avatarId)
        {
            _avatarSeleccionado = avatarId ?? string.Empty;
            ActualizarSeleccion(_botonesAvatar, _avatarSeleccionado);
            MostrarEstadoPerfil(null, false);
        }

        private void MostrarEstadoPerfil(string mensaje, bool esError)
        {
            if (_textoEstadoPerfil == null) return;
            _textoEstadoPerfil.text = mensaje ?? string.Empty;
            _textoEstadoPerfil.color = esError
                ? new Color(0.92f, 0.3f, 0.25f)
                : new Color(0.20f, 0.75f, 0.48f);
            _textoEstadoPerfil.gameObject.SetActive(!string.IsNullOrWhiteSpace(mensaje));
        }

        private static Transform CrearSelector(
            Transform padre,
            string nombre,
            string titulo,
            int columnas,
            float altura)
        {
            var raiz = new GameObject(nombre, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            raiz.transform.SetParent(padre, false);
            raiz.GetComponent<LayoutElement>().preferredHeight = altura + 34f;
            var layout = raiz.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;

            CrearTexto(raiz.transform, "Titulo", 19f).text = titulo;
            var grilla = new GameObject("Opciones", typeof(RectTransform), typeof(GridLayoutGroup), typeof(LayoutElement));
            grilla.transform.SetParent(raiz.transform, false);
            grilla.GetComponent<LayoutElement>().preferredHeight = altura;
            var grid = grilla.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(132f, 48f);
            grid.spacing = new Vector2(8f, 8f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columnas;
            return raiz.transform;
        }

        private static void CrearBotonesSelector(
            Transform selector,
            IReadOnlyList<string> valores,
            ICollection<Button> destino,
            Action<string> alSeleccionar)
        {
            Transform grilla = selector.Find("Opciones");
            if (grilla == null) return;

            for (int i = 0; i < valores.Count; i++)
            {
                string valor = valores[i];
                Button boton = CrearBoton(grilla, valor);
                boton.onClick.AddListener(() => alSeleccionar(valor));
                destino.Add(boton);
            }
        }

        private static Button CrearBoton(Transform padre, string valor)
        {
            var objeto = new GameObject(valor, typeof(RectTransform), typeof(Image), typeof(Button));
            objeto.transform.SetParent(padre, false);
            Image fondo = objeto.GetComponent<Image>();
            fondo.color = new Color(0.20f, 0.24f, 0.30f, 1f);
            Button boton = objeto.GetComponent<Button>();
            boton.targetGraphic = fondo;
            TMP_Text texto = CrearTexto(objeto.transform, "Etiqueta", 17f);
            texto.text = valor.StartsWith("avatar_", StringComparison.Ordinal)
                ? $"Avatar {valor.Substring(valor.Length - 2)}"
                : valor;
            texto.alignment = TextAlignmentOptions.Center;
            RectTransform rect = (RectTransform)texto.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return boton;
        }

        private static TMP_Text CrearTexto(Transform padre, string nombre, float tamano)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            objeto.transform.SetParent(padre, false);
            var texto = objeto.GetComponent<TextMeshProUGUI>();
            texto.fontSize = tamano;
            texto.color = Color.white;
            texto.alignment = TextAlignmentOptions.MidlineLeft;
            return texto;
        }

        private static void ActualizarSeleccion(IEnumerable<Button> botones, string valorSeleccionado)
        {
            foreach (Button boton in botones)
            {
                Image fondo = boton != null ? boton.GetComponent<Image>() : null;
                if (fondo != null)
                    fondo.color = boton.name == valorSeleccionado
                        ? new Color(0.03f, 0.60f, 0.65f, 1f)
                        : new Color(0.20f, 0.24f, 0.30f, 1f);
            }
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
