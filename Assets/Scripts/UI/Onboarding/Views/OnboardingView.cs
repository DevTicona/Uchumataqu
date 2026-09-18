using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Core.UI;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.UI.Avatares;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Onboarding
{
    public class OnboardingView : MonoBehaviour
    {
        [Header("Paneles")]
        [SerializeField] private GameObject pasoBienvenida;
        [SerializeField] private GameObject pasoPerfil;
        [SerializeField] private GameObject pasoAvatar;

        [Header("Perfil")]
        [SerializeField] private TMP_InputField inputNombre;

        [Header("Avatares")]
        [SerializeField] private Button[] botonesAvatar = Array.Empty<Button>();
        [SerializeField] private CatalogoAvataresModel catalogoAvatares;

        [Header("Validacion")]
        [SerializeField] private TMP_Text textoErrorPerfil;
        [SerializeField] private TMP_Text textoErrorAvatar;

        [Header("Acción")]
        [SerializeField] private Button botonContinuar;
        [SerializeField] private TextMeshProUGUI textoBotonContinuar;

        public event Action OnComenzarClick;
        public event Action OnContinuarBienvenida;
        public event Action<string> OnAvatarSeleccionado;

        private enum PasoActual { Ninguno, Bienvenida, Formulario }
        private PasoActual _paso = PasoActual.Ninguno;

        private readonly List<Button> _botonesAvatar = new List<Button>();
        private readonly List<string> _avatarIds = new List<string>();
        private Coroutine _animacionPaso;
        private readonly UIEventSubscriptions _avatarSubscriptions = new UIEventSubscriptions();

        private void OnDestroy()
        {
            _avatarSubscriptions.Dispose();
            if (botonContinuar != null) botonContinuar.onClick.RemoveListener(OnBotonContinuarClick);
        }

        public void ConfigurarAvatares(IReadOnlyList<string> avatarIds)
        {
            _avatarIds.Clear();
            if (avatarIds != null)
                _avatarIds.AddRange(avatarIds);
            PrepararAvatares();
        }

        private void Awake()
        {
            ConfigurarCampoNombre();

            // Un único listener fijo que despacha según el paso actual.
            // Nunca se llama RemoveAllListeners para no romper el ThemedButton.
            if (botonContinuar != null)
                botonContinuar.onClick.AddListener(OnBotonContinuarClick);

            PrepararAvatares();
        }

        /// <summary>Listener único del BtnContinuar — despacha según el paso activo.</summary>
        private void OnBotonContinuarClick()
        {
            switch (_paso)
            {
                case PasoActual.Bienvenida:
                    OnContinuarBienvenida?.Invoke();
                    break;
                case PasoActual.Formulario:
                    NotificarComienzo();
                    break;
            }
        }

        public void MostrarBienvenida()
        {
            _paso = PasoActual.Bienvenida;

            if (pasoBienvenida != null) pasoBienvenida.SetActive(true);
            if (pasoPerfil != null) pasoPerfil.SetActive(false);
            if (pasoAvatar != null) pasoAvatar.SetActive(false);
            if (botonContinuar != null) botonContinuar.gameObject.SetActive(true);
            if (textoBotonContinuar != null) textoBotonContinuar.text = "CONTINUAR";

            RestablecerTemaBoton();

            if (pasoBienvenida == null) return;
            if (_animacionPaso != null) StopCoroutine(_animacionPaso);
            _animacionPaso = StartCoroutine(AnimarEntrada(pasoBienvenida));
        }

        public void MostrarFormulario()
        {
            _paso = PasoActual.Formulario;

            LimpiarErrores();
            if (pasoBienvenida != null) pasoBienvenida.SetActive(false);
            if (pasoPerfil != null) pasoPerfil.SetActive(true);
            if (pasoAvatar != null) pasoAvatar.SetActive(true);
            if (botonContinuar != null) botonContinuar.gameObject.SetActive(true);
            if (textoBotonContinuar != null) textoBotonContinuar.text = "COMENZAR";

            RestablecerTemaBoton();

            if (pasoPerfil == null) return;
            if (_animacionPaso != null) StopCoroutine(_animacionPaso);
            _animacionPaso = StartCoroutine(AnimarEntrada(pasoPerfil));
        }

        private void RestablecerTemaBoton()
        {
            if (botonContinuar != null)
            {
                var themed = botonContinuar.GetComponent<ThemedButtonView>();
                if (themed != null) themed.AplicarTema();
            }
        }

        public string ObtenerNombre() => inputNombre != null ? inputNombre.text.Trim() : string.Empty;

        public void MostrarAvatarSeleccionado(string avatarId)
        {
            for (int i = 0; i < _botonesAvatar.Count; i++)
            {
                bool seleccionado = i < _avatarIds.Count &&
                                     string.Equals(avatarId, _avatarIds[i], StringComparison.Ordinal);
                Transform indicador = _botonesAvatar[i].transform.Find("Seleccion");
                if (indicador != null)
                    indicador.gameObject.SetActive(seleccionado);
            }
            MostrarErrorAvatar(null);
        }

        public void MostrarErrorPerfil(string mensaje) => MostrarError(textoErrorPerfil, mensaje);
        public void MostrarErrorAvatar(string mensaje) => MostrarError(textoErrorAvatar, mensaje);

        public void LimpiarErrores()
        {
            MostrarErrorPerfil(null);
            MostrarErrorAvatar(null);
        }

        private void PrepararAvatares()
        {
            _avatarSubscriptions.Dispose();
            if (pasoAvatar == null) return;

            _botonesAvatar.Clear();
            if (botonesAvatar != null)
            {
                foreach (Button boton in botonesAvatar)
                {
                    if (boton != null)
                        _botonesAvatar.Add(boton);
                }
            }

            if (_botonesAvatar.Count == 0)
                _botonesAvatar.AddRange(pasoAvatar.GetComponentsInChildren<Button>(true));

            _botonesAvatar.Sort((a, b) => string.CompareOrdinal(a.name, b.name));

            if (_avatarIds.Count == 0 && catalogoAvatares != null)
            {
                foreach (var av in catalogoAvatares.Avatares)
                    _avatarIds.Add(av.id);
            }

            int cantidad = Mathf.Min(_botonesAvatar.Count, _avatarIds.Count);
            for (int i = 0; i < cantidad; i++)
            {
                string avatarId = _avatarIds[i];
                _avatarSubscriptions.Add(_botonesAvatar[i], () => OnAvatarSeleccionado?.Invoke(avatarId));

                if (catalogoAvatares != null)
                {
                    Image img = _botonesAvatar[i].transform.Find("Icon")?.GetComponent<Image>();
                    if (img == null)
                        img = _botonesAvatar[i].GetComponentInChildren<Image>(true);

                    Sprite sprite = catalogoAvatares.ObtenerSprite(avatarId);
                    if (img != null && sprite != null)
                        img.sprite = sprite;
                }
            }
        }

        private void ConfigurarCampoNombre()
        {
            if (inputNombre == null) return;

            inputNombre.characterLimit = 30;
            inputNombre.lineType = TMP_InputField.LineType.SingleLine;
            inputNombre.onFocusSelectAll = false;
        }

        private void NotificarComienzo() => OnComenzarClick?.Invoke();

        private static void MostrarError(TMP_Text texto, string mensaje)
        {
            if (texto == null) return;
            texto.text = mensaje ?? string.Empty;
            texto.gameObject.SetActive(!string.IsNullOrWhiteSpace(mensaje));
        }

        private static IEnumerator AnimarEntrada(GameObject panel)
        {
            if (panel == null) yield break;

            CanvasGroup grupo = panel.GetComponent<CanvasGroup>();
            if (grupo == null)
                grupo = panel.AddComponent<CanvasGroup>();
            if (grupo == null) yield break;

            grupo.alpha = 0f;
            float tiempo = 0f;
            const float duracion = 0.2f;
            while (tiempo < duracion)
            {
                if (grupo == null) yield break;
                tiempo += Time.unscaledDeltaTime;
                grupo.alpha = Mathf.Clamp01(tiempo / duracion);
                yield return null;
            }
            if (grupo != null) grupo.alpha = 1f;
        }
    }
}
