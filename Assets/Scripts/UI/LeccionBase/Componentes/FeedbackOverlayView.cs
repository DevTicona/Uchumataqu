using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI.LeccionBase.Componentes
{
    /// <summary>
    /// Componente de feedback inferior para resultados inmediatos tras cada respuesta.
    /// Especificación: Docs/UI/04-escena-leccion-base.md §10 y §11
    /// </summary>
    public class FeedbackOverlayView : MonoBehaviour
    {
        [Header("Elementos de UI")]
        [SerializeField] private Image fondoFeedback;
        [SerializeField] private TMP_Text txtTitulo;
        [SerializeField] private TMP_Text txtMensajeDetalle;
        [SerializeField] private Button btnContinuar;

        [Header("Paleta de Feedback")]
        [SerializeField] private Color colorCorrecto = new Color(0.24f, 0.65f, 0.35f, 0.95f);   // Verde #3DA559
        [SerializeField] private Color colorCasi = new Color(0.85f, 0.68f, 0.20f, 0.95f);       // Amarillo cálido #D9AD33
        [SerializeField] private Color colorIncorrecto = new Color(0.82f, 0.25f, 0.25f, 0.95f);  // Rojo suave #D14040

        public event Action OnContinuarFeedbackClicked;

        private void CrearControlesRuntime()
        {
            var layout = gameObject.GetComponent<VerticalLayoutGroup>() ?? gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.padding = new RectOffset(16, 16, 16, 16);
            fondoFeedback = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            txtTitulo = CrearTexto("Titulo", 30f);
            txtMensajeDetalle = CrearTexto("Detalle", 23f);
            btnContinuar = CrearBoton("Continuar");
        }

        private void Awake()
        {
            if (btnContinuar == null) CrearControlesRuntime();
            if (btnContinuar != null)
                btnContinuar.onClick.AddListener(() => OnContinuarFeedbackClicked?.Invoke());
        }

        private TMP_Text CrearTexto(string nombre, float tamano)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(transform, false);
            var texto = go.GetComponent<TextMeshProUGUI>();
            texto.fontSize = tamano;
            texto.alignment = TextAlignmentOptions.Center;
            go.GetComponent<LayoutElement>().preferredHeight = tamano * 1.8f;
            return texto;
        }

        private Button CrearBoton(string etiqueta)
        {
            var go = new GameObject("BtnContinuar", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(transform, false);
            go.GetComponent<LayoutElement>().preferredHeight = 60f;
            TMP_Text texto = CrearTexto(etiqueta, 24f);
            texto.transform.SetParent(go.transform, false);
            return go.GetComponent<Button>();
        }

        public void MostrarCorrecto(string detalle = null)
        {
            ConfigurarVisual(colorCorrecto, "✓ ¡Correcto!", detalle ?? "¡Muy bien!");
            gameObject.SetActive(true);
        }

        public void MostrarCasi(string detalle = null, double similitud = 0)
        {
            string msg = string.IsNullOrEmpty(detalle)
                ? $"Casi exacto ({similitud:F0}% de similitud). ¡Sigue practicando!"
                : detalle;

            ConfigurarVisual(colorCasi, "⚡ Casi", msg);
            gameObject.SetActive(true);
        }

        public void MostrarIncorrecto(string respuestaCorrecta = null)
        {
            string msg = string.IsNullOrEmpty(respuestaCorrecta)
                ? "Revisa la respuesta para la próxima vez."
                : $"Respuesta correcta:\n<b>{respuestaCorrecta}</b>";

            ConfigurarVisual(colorIncorrecto, "✕ Respuesta incorrecta", msg);
            gameObject.SetActive(true);
        }

        public void Ocultar()
        {
            gameObject.SetActive(false);
        }

        private void ConfigurarVisual(Color colorFondo, string titulo, string detalle)
        {
            if (fondoFeedback != null) fondoFeedback.color = colorFondo;
            if (txtTitulo != null) txtTitulo.text = titulo;
            if (txtMensajeDetalle != null) txtMensajeDetalle.text = detalle;
        }
    }
}
