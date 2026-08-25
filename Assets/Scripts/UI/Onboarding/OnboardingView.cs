using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Onboarding
{
    public class OnboardingView : MonoBehaviour
    {
        [Header("Paneles de Pasos")]
        [SerializeField] private GameObject pasoBienvenida;
        [SerializeField] private GameObject pasoPerfil;
        [SerializeField] private GameObject pasoAvatar;

        [Header("Campos Perfil")]
        [SerializeField] private TMP_InputField inputNombre;
        [SerializeField] private TMP_Dropdown dropdownEdad;

        [Header("Navegación")]
        [SerializeField] private Button botonContinuar;
        [SerializeField] private Button botonAtras;
        [SerializeField] private TextMeshProUGUI textoBotonContinuar;

        public event Action OnContinuarClick;
        public event Action OnAtrasClick;
        public event Action<string> OnAvatarSeleccionado;

        private readonly List<Button> _botonesAvatar = new List<Button>();
        private TMP_Text _textoErrorPerfil;
        private TMP_Text _textoErrorAvatar;
        private Coroutine _animacionPaso;

        private void Awake()
        {
            if (botonContinuar != null)
                botonContinuar.onClick.AddListener(() => OnContinuarClick?.Invoke());
            if (botonAtras != null)
                botonAtras.onClick.AddListener(() => OnAtrasClick?.Invoke());

            if (pasoAvatar != null)
            {
                _botonesAvatar.AddRange(pasoAvatar.GetComponentsInChildren<Button>(true));
                _botonesAvatar.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
                for (int i = 0; i < _botonesAvatar.Count; i++)
                {
                    int indice = i;
                    _botonesAvatar[i].onClick.AddListener(() =>
                        OnAvatarSeleccionado?.Invoke($"avatar_{indice + 1:D2}"));
                }
            }

            _textoErrorPerfil = CrearTextoError(pasoPerfil, "ErrorPerfil");
            _textoErrorAvatar = CrearTextoError(pasoAvatar, "ErrorAvatar");
        }

        public void MostrarPaso(int paso)
        {
            LimpiarErrores();
            if (pasoBienvenida != null) pasoBienvenida.SetActive(paso == 1);
            if (pasoPerfil != null) pasoPerfil.SetActive(paso == 2);
            if (pasoAvatar != null) pasoAvatar.SetActive(paso == 3);
            if (botonAtras != null) botonAtras.gameObject.SetActive(paso > 1);
            if (textoBotonContinuar != null)
                textoBotonContinuar.text = paso == 3 ? "COMENZAR" : paso == 1 ? "EMPEZAR" : "CONTINUAR";

            GameObject panel = paso == 1 ? pasoBienvenida : paso == 2 ? pasoPerfil : pasoAvatar;
            if (panel == null) return;
            if (_animacionPaso != null) StopCoroutine(_animacionPaso);
            _animacionPaso = StartCoroutine(AnimarEntrada(panel));
        }

        public string ObtenerNombre() => inputNombre != null ? inputNombre.text.Trim() : string.Empty;

        public string ObtenerRangoEdad() =>
            dropdownEdad != null && dropdownEdad.options.Count > 0
                ? dropdownEdad.options[dropdownEdad.value].text
                : string.Empty;

        public void MostrarAvatarSeleccionado(string avatarId)
        {
            for (int i = 0; i < _botonesAvatar.Count; i++)
            {
                bool seleccionado = avatarId == $"avatar_{i + 1:D2}";
                Image imagen = _botonesAvatar[i].GetComponent<Image>();
                if (imagen != null)
                    imagen.color = seleccionado ? new Color(0.92f, 0.76f, 0.35f) : Color.white;
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

        private static TMP_Text CrearTextoError(GameObject padre, string nombre)
        {
            if (padre == null) return null;
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            objeto.transform.SetParent(padre.transform, false);
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
