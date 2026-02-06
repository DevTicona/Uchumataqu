using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Assets.Scripts.Views.UI
{
    /// <summary>
    /// Vista para el diálogo de selección entre RA y 2D.
    /// Manejando la UI y animaciones del modal.
    /// </summary>
    public class SeleccionModuloView : MonoBehaviour
    {
        [Header("Canvas")]
        [SerializeField] private CanvasGroup canvasGroupDialogo;
        [SerializeField] private RectTransform contenedorDialogo;

        [Header("Componentes")]
        [SerializeField] private TextMeshProUGUI txtMensaje;
        [SerializeField] private Button btnRA;
        [SerializeField] private Button btnDos2D;
        [SerializeField] private Button btnCancelar;

        [Header("Animación")]
        [SerializeField] private float duracionAnimacion = 0.3f;

        [Header("Colores")]
        [SerializeField] private Color colorBotonRA = new Color32(0xFF, 0x9E, 0x64, 0xFF);      // Naranja RA
        [SerializeField] private Color colorBotonDos2D = new Color32(0x3B, 0x6E, 0xF5, 0xFF);   // Azul 2D
        [SerializeField] private Color colorBotonCancelar = new Color32(0x6B, 0x6B, 0x6B, 0xFF); // Gris
        [SerializeField] private Color colorTextoBoton = Color.white;

        private bool dialogoMostrado = false;

        // Eventos
        public event Action OnSeleccionarRA;
        public event Action OnSeleccionarDos2D;
        public event Action OnCancelar;

        private void Awake()
        {
            ConfigurarBotones();

            if (canvasGroupDialogo != null)
                canvasGroupDialogo.alpha = 0f;
        }

        private void OnDestroy()
        {
            btnRA?.onClick.RemoveAllListeners();
            btnDos2D?.onClick.RemoveAllListeners();
            btnCancelar?.onClick.RemoveAllListeners();
        }

        // -------------------------------------------------
        // CONFIGURACIÓN
        // -------------------------------------------------

        private void ConfigurarBotones()
        {
            if (btnRA != null)
            {
                btnRA.onClick.AddListener(SeleccionarRA);
                btnRA.GetComponent<Image>().color = colorBotonRA;
                var txtRA = btnRA.GetComponentInChildren<TextMeshProUGUI>();
                if (txtRA != null) txtRA.color = colorTextoBoton;
            }

            if (btnDos2D != null)
            {
                btnDos2D.onClick.AddListener(SeleccionarDos2D);
                btnDos2D.GetComponent<Image>().color = colorBotonDos2D;
                var txtDos2D = btnDos2D.GetComponentInChildren<TextMeshProUGUI>();
                if (txtDos2D != null) txtDos2D.color = colorTextoBoton;
            }

            if (btnCancelar != null)
            {
                btnCancelar.onClick.AddListener(Cancelar);
                btnCancelar.GetComponent<Image>().color = colorBotonCancelar;
                var txtCancelar = btnCancelar.GetComponentInChildren<TextMeshProUGUI>();
                if (txtCancelar != null) txtCancelar.color = colorTextoBoton;
            }
        }

        // -------------------------------------------------
        // DIÁLOGO
        // -------------------------------------------------

        public void MostrarDialogo(string leccionTitulo)
        {
            if (dialogoMostrado) return;

            dialogoMostrado = true;

            // Actualizar texto (titulo + mensaje combinados)
            if (txtMensaje != null)
                txtMensaje.text = $"¿Cómo deseas aprender \"{leccionTitulo}\"?\n\nSelecciona el modo:";

            canvasGroupDialogo.gameObject.SetActive(true);
            StartCoroutine(AnimarAlfa(canvasGroupDialogo, 0f, 1f, duracionAnimacion));

            if (contenedorDialogo != null)
            {
                contenedorDialogo.localScale = Vector3.one * 0.8f;
                StartCoroutine(AnimarEscala(contenedorDialogo, contenedorDialogo.localScale, Vector3.one, duracionAnimacion));
            }
        }

        public void OcultarDialogo()
        {
            if (!dialogoMostrado) return;

            dialogoMostrado = false;
            StartCoroutine(AnimarAlfa(canvasGroupDialogo, 1f, 0f, duracionAnimacion, () =>
            {
                canvasGroupDialogo.gameObject.SetActive(false);
            }));
        }

        // -------------------------------------------------
        // MANEJADORES DE BOTONES
        // -------------------------------------------------

        private void SeleccionarRA()
        {
            OnSeleccionarRA?.Invoke();
            OcultarDialogo();
        }

        private void SeleccionarDos2D()
        {
            OnSeleccionarDos2D?.Invoke();
            OcultarDialogo();
        }

        private void Cancelar()
        {
            OnCancelar?.Invoke();
            OcultarDialogo();
        }

        // -------------------------------------------------
        // ANIMACIONES
        // -------------------------------------------------

        private System.Collections.IEnumerator AnimarAlfa(
            CanvasGroup canvasGroup,
            float inicial,
            float final,
            float duracion,
            Action onComplete = null
        )
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

        private System.Collections.IEnumerator AnimarEscala(
            RectTransform rect,
            Vector3 inicial,
            Vector3 final,
            float duracion
        )
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
