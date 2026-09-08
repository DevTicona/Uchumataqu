using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
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

        [Header("Acción")]
        [SerializeField] private Button botonContinuar;
        [SerializeField] private TextMeshProUGUI textoBotonContinuar;

        public event Action OnComenzarClick;
        public event Action<string> OnAvatarSeleccionado;

        private readonly List<Button> _botonesAvatar = new List<Button>();
        private TMP_Text _textoErrorPerfil;
        private TMP_Text _textoErrorAvatar;
        private Coroutine _animacionPaso;

        private void Awake()
        {
            NormalizarColoresBotones(transform);
            if (inputNombre != null) inputNombre.characterLimit = 30;
            if (botonContinuar != null)
                botonContinuar.onClick.AddListener(NotificarComienzo);

            PrepararAvatares();
            _ = CargarSpritesAvataresAsync();
            _textoErrorPerfil = CrearTextoError(pasoPerfil, "ErrorPerfil");
            _textoErrorAvatar = CrearTextoError(pasoAvatar, "ErrorAvatar");
        }

        public void MostrarFormulario()
        {
            LimpiarErrores();
            if (pasoBienvenida != null) pasoBienvenida.SetActive(false);
            if (pasoPerfil != null) pasoPerfil.SetActive(true);
            if (pasoAvatar != null) pasoAvatar.SetActive(true);
            if (botonContinuar != null) botonContinuar.gameObject.SetActive(true);
            if (textoBotonContinuar != null) textoBotonContinuar.text = "COMENZAR";

            if (pasoPerfil == null) return;
            if (_animacionPaso != null) StopCoroutine(_animacionPaso);
            _animacionPaso = StartCoroutine(AnimarEntrada(pasoPerfil));
        }

        public string ObtenerNombre() => inputNombre != null ? inputNombre.text.Trim() : string.Empty;

        public void MostrarAvatarSeleccionado(string avatarId)
        {
            for (int i = 0; i < _botonesAvatar.Count; i++)
            {
                bool seleccionado = i < PerfilOnboardingService.AvataresDisponibles.Count &&
                                     avatarId == PerfilOnboardingService.AvataresDisponibles[i];
                Image imagen = _botonesAvatar[i].GetComponent<Image>();
                if (imagen != null)
                    imagen.color = seleccionado
                        ? new Color(0.96f, 0.66f, 0.20f, 1f)
                        : ColorAvatar(i);
                _botonesAvatar[i].transform.localScale = seleccionado ? Vector3.one * 1.06f : Vector3.one;
            }
            MostrarErrorAvatar(null);
        }

        public void MostrarErrorPerfil(string mensaje) => MostrarError(_textoErrorPerfil, mensaje);
        public void MostrarErrorAvatar(string mensaje) => MostrarError(_textoErrorAvatar, mensaje);

        public void LimpiarErrores()
        {
            MostrarErrorPerfil(null);
            MostrarErrorAvatar(null);
        }

        private void PrepararAvatares()
        {
            if (pasoAvatar == null) return;

            _botonesAvatar.AddRange(pasoAvatar.GetComponentsInChildren<Button>(true));
            _botonesAvatar.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            int cantidad = Mathf.Min(_botonesAvatar.Count, PerfilOnboardingService.AvataresDisponibles.Count);
            for (int i = 0; i < cantidad; i++)
            {
                string avatarId = PerfilOnboardingService.AvataresDisponibles[i];
                PrepararBotonAvatar(_botonesAvatar[i], i);
                _botonesAvatar[i].onClick.AddListener(() => OnAvatarSeleccionado?.Invoke(avatarId));
            }
        }

        private void NotificarComienzo() => OnComenzarClick?.Invoke();

        private async Task CargarSpritesAvataresAsync()
        {
            int cantidad = Mathf.Min(_botonesAvatar.Count, PerfilOnboardingService.AvataresDisponibles.Count);
            for (int i = 0; i < cantidad; i++)
            {
                Button boton = _botonesAvatar[i];
                string avatarId = PerfilOnboardingService.AvataresDisponibles[i];
                try
                {
                    Sprite sprite = await AddressablesService.CargarAssetAsync<Sprite>(ContentAddress.Avatar(avatarId));
                    if (boton == null) continue;

                    Image icono = null;
                    foreach (Image imagen in boton.GetComponentsInChildren<Image>(true))
                    {
                        if (imagen.gameObject == boton.gameObject) continue;
                        icono = imagen;
                        break;
                    }

                    if (icono == null) continue;
                    icono.sprite = sprite;
                    icono.color = Color.white;
                    icono.preserveAspect = true;
                    icono.gameObject.SetActive(true);

                    TMP_Text etiqueta = boton.GetComponentInChildren<TMP_Text>(true);
                    if (etiqueta != null) etiqueta.gameObject.SetActive(false);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[OnboardingView] No se pudo cargar {avatarId}: {ex.Message}");
                }
            }
        }

        private static void PrepararBotonAvatar(Button boton, int indice)
        {
            if (boton == null) return;
            Image fondo = boton.GetComponent<Image>();
            if (fondo != null) fondo.color = ColorAvatar(indice);

            TMP_Text etiqueta = boton.GetComponentInChildren<TMP_Text>(true);
            if (etiqueta == null) return;
            etiqueta.text = ((char)('A' + indice)).ToString();
            etiqueta.fontSize = 34f;
            etiqueta.fontStyle = FontStyles.Bold;
            etiqueta.alignment = TextAlignmentOptions.Center;
            etiqueta.color = Color.white;
        }

        private static Color ColorAvatar(int indice)
        {
            Color[] colores =
            {
                new Color(0.09f, 0.62f, 0.67f, 1f),
                new Color(0.47f, 0.36f, 0.76f, 1f),
                new Color(0.91f, 0.36f, 0.31f, 1f),
                new Color(0.21f, 0.67f, 0.42f, 1f)
            };
            return colores[Mathf.Abs(indice) % colores.Length];
        }

        private static void NormalizarColoresBotones(Transform raiz)
        {
            foreach (Button boton in raiz.GetComponentsInChildren<Button>(true))
            {
                ColorBlock colores = boton.colors;
                if (colores.highlightedColor.maxColorComponent <= 1f &&
                    colores.pressedColor.maxColorComponent <= 1f)
                    continue;

                Color normal = colores.normalColor;
                colores.highlightedColor = new Color(
                    Mathf.Min(1f, normal.r * 1.1f),
                    Mathf.Min(1f, normal.g * 1.1f),
                    Mathf.Min(1f, normal.b * 1.1f),
                    normal.a);
                colores.pressedColor = new Color(normal.r * 0.85f, normal.g * 0.85f, normal.b * 0.85f, normal.a);
                boton.colors = colores;
            }
        }

        private static TMP_Text CrearTextoError(GameObject padre, string nombre)
        {
            if (padre == null) return null;
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            objeto.transform.SetParent(padre.transform, false);
            objeto.GetComponent<LayoutElement>().preferredHeight = 32f;
            var texto = objeto.GetComponent<TextMeshProUGUI>();
            texto.fontSize = 20f;
            texto.color = new Color(0.92f, 0.3f, 0.25f);
            texto.alignment = TextAlignmentOptions.Center;
            objeto.SetActive(false);
            return texto;
        }

        private static void MostrarError(TMP_Text texto, string mensaje)
        {
            if (texto == null) return;
            texto.text = mensaje ?? string.Empty;
            texto.gameObject.SetActive(!string.IsNullOrWhiteSpace(mensaje));
        }

        private static IEnumerator AnimarEntrada(GameObject panel)
        {
            CanvasGroup grupo = panel.GetComponent<CanvasGroup>() ?? panel.AddComponent<CanvasGroup>();
            float tiempo = 0f;
            const float duracion = 0.2f;
            while (tiempo < duracion)
            {
                tiempo += Time.unscaledDeltaTime;
                grupo.alpha = Mathf.Clamp01(tiempo / duracion);
                yield return null;
            }
            grupo.alpha = 1f;
        }
    }
}
