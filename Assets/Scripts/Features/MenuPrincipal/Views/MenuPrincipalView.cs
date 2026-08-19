using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Assets.Scripts.Features.MenuPrincipal.Views
{
    public class MenuPrincipalView : MonoBehaviour
    {
        [Header("Botones")]
        [SerializeField] private Button btnLecciones;
        [SerializeField] private Button btnExplorarRA;
        [SerializeField] private Button btnSalir;

        [Header("Diálogo Confirmación")]
        [SerializeField] private CanvasGroup canvasGroupDialogo;
        [SerializeField] private RectTransform contenedorDialogo;
        [SerializeField] private TextMeshProUGUI mensajeDialogo;
        [SerializeField] private Button botonSi;
        [SerializeField] private Button botonNo;

        [Header("Colores")]
        [SerializeField] private Color colorBotonSi = new Color32(0xE8, 0x3E, 0x3E, 0xFF);
        [SerializeField] private Color colorBotonNo = new Color32(0x3B, 0x6E, 0xF5, 0xFF);
        [SerializeField] private Color colorTextoBoton = Color.white;

        [Header("Animación")]
        [SerializeField] private float duracionAnimacion = 0.3f;

        private bool dialogoMostrado = false;

        // Eventos expuestos para el Presenter
        public event Action OnLeccionesPressed;
        public event Action OnExplorarRAPressed;
        public event Action OnSalirPressed;

        private void Awake()
        {
            btnLecciones?.onClick.AddListener(() => OnLeccionesPressed?.Invoke());
            btnExplorarRA?.onClick.AddListener(() => OnExplorarRAPressed?.Invoke());
            btnSalir?.onClick.AddListener(() => OnSalirPressed?.Invoke());

            ConfigurarDialogoConfirmacion();

            if (canvasGroupDialogo != null)
                canvasGroupDialogo.alpha = 0f;
        }

        private void OnDestroy()
        {
            btnLecciones?.onClick.RemoveAllListeners();
            btnExplorarRA?.onClick.RemoveAllListeners();
            btnSalir?.onClick.RemoveAllListeners();
            botonSi?.onClick.RemoveAllListeners();
            botonNo?.onClick.RemoveAllListeners();
        }

        // -------------------------------------------------
        // DIÁLOGO CONFIRMACIÓN
        // -------------------------------------------------

        public void SolicitarConfirmacionSalida()
        {
            MostrarDialogo();
        }

        private void ConfigurarDialogoConfirmacion()
        {
            if (mensajeDialogo != null && string.IsNullOrEmpty(mensajeDialogo.text))
                mensajeDialogo.text = "Se perderá el progreso no guardado.";

            if (botonSi != null)
            {
                botonSi.onClick.AddListener(ConfirmarSalida);
                botonSi.GetComponent<Image>().color = colorBotonSi;
                botonSi.GetComponentInChildren<TextMeshProUGUI>().color = colorTextoBoton;
            }

            if (botonNo != null)
            {
                botonNo.onClick.AddListener(CancelarSalida);
                botonNo.GetComponent<Image>().color = colorBotonNo;
                botonNo.GetComponentInChildren<TextMeshProUGUI>().color = colorTextoBoton;
            }
        }

        private void MostrarDialogo()
        {
            if (dialogoMostrado) return;

            dialogoMostrado = true;
            canvasGroupDialogo.gameObject.SetActive(true);
            StartCoroutine(AnimarAlfa(canvasGroupDialogo, 0f, 1f, duracionAnimacion));

            if (contenedorDialogo != null)
            {
                contenedorDialogo.localScale = Vector3.one * 0.8f;
                StartCoroutine(AnimarEscala(contenedorDialogo, contenedorDialogo.localScale, Vector3.one, duracionAnimacion));
            }
        }

        private void OcultarDialogo()
        {
            dialogoMostrado = false;
            StartCoroutine(AnimarAlfa(canvasGroupDialogo, 1f, 0f, duracionAnimacion, () =>
            {
                canvasGroupDialogo.gameObject.SetActive(false);
            }));
        }

        private void ConfirmarSalida()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void CancelarSalida()
        {
            OcultarDialogo();
        }

        // -------------------------------------------------
        // ANIMACIONES
        // -------------------------------------------------

        private System.Collections.IEnumerator AnimarAlfa(CanvasGroup canvasGroup, float inicial, float final, float duracion, Action onComplete = null)
        {
            float tiempo = 0f;
            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(inicial, final, tiempo / duracion);
                yield return null;
            }
            canvasGroup.alpha = final;
            onComplete?.Invoke();
        }

        private System.Collections.IEnumerator AnimarEscala(RectTransform rect, Vector3 inicial, Vector3 final, float duracion)
        {
            float tiempo = 0f;
            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                rect.localScale = Vector3.Lerp(inicial, final, tiempo / duracion);
                yield return null;
            }
            rect.localScale = final;
        }
    }
}
