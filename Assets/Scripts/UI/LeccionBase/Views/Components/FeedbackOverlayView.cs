using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;

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
        [SerializeField] private Image fondoDetalle;
        [SerializeField] private Image imgEstado;
        [SerializeField] private Sprite iconoCorrecto;
        [SerializeField] private Sprite iconoAdvertencia;
        [SerializeField] private Sprite iconoIncorrecto;
        [SerializeField] private Sprite iconoInformacion;
        [SerializeField] private TMP_Text txtTitulo;
        [SerializeField] private TMP_Text txtMensajeDetalle;

        [Header("Paleta de Feedback")]
        [SerializeField] private Color colorCorrecto = new Color(0.24f, 0.65f, 0.35f, 0.95f);   // Verde #3DA559
        [SerializeField] private Color colorCasi = new Color(0.85f, 0.68f, 0.20f, 0.95f);       // Amarillo cálido #D9AD33
        [SerializeField] private Color colorIncorrecto = new Color(0.82f, 0.25f, 0.25f, 0.95f);  // Rojo suave #D14040

        public bool ValidarConfiguracion(out string error)
        {
            bool completa = fondoFeedback != null && imgEstado != null && txtTitulo != null &&
                txtMensajeDetalle != null && iconoCorrecto != null && iconoAdvertencia != null && iconoIncorrecto != null;
            error = completa ? null : "El contenido de feedback tiene referencias incompletas.";
            return completa;
        }

        public void MostrarCorrecto(string detalle = null)
        {
            ConfigurarVisual(ThemedGraphicView.RolColor.Exito, colorCorrecto, iconoCorrecto,
                "¡Correcto!", detalle ?? "¡Muy bien!");
            gameObject.SetActive(true);
        }

        public void MostrarCasi(string detalle = null, double similitud = 0)
        {
            string msg = string.IsNullOrEmpty(detalle)
                ? $"Casi exacto ({similitud:F0}% de similitud). ¡Sigue practicando!"
                : detalle;

            ConfigurarVisual(ThemedGraphicView.RolColor.Advertencia, colorCasi, iconoAdvertencia,
                "Casi", msg);
            gameObject.SetActive(true);
        }

        public void MostrarIncorrecto(string respuestaCorrecta = null)
        {
            string msg = string.IsNullOrEmpty(respuestaCorrecta)
                ? "Revisa la respuesta para la próxima vez."
                : respuestaCorrecta;

            ConfigurarVisual(ThemedGraphicView.RolColor.Error, colorIncorrecto, iconoIncorrecto,
                "Respuesta incorrecta", msg);
            gameObject.SetActive(true);
        }

        public void MostrarInformacion(string detalle = null)
        {
            ConfigurarVisual(ThemedGraphicView.RolColor.Informacion, Color.clear,
                iconoInformacion != null ? iconoInformacion : iconoAdvertencia,
                "Práctica de pronunciación", detalle ?? "Sigue las indicaciones para practicar.");
            gameObject.SetActive(true);
        }

        public void Ocultar()
        {
            gameObject.SetActive(false);
        }

        private void ConfigurarVisual(ThemedGraphicView.RolColor rol, Color colorRespaldo,
            Sprite icono, string titulo, string detalle)
        {
            if (fondoFeedback != null)
            {
                Color colorEstado = ObtenerColorEstado(rol, colorRespaldo);
                fondoFeedback.color = Color.Lerp(Color.white, colorEstado, 0.30f);
                if (fondoDetalle != null)
                    fondoDetalle.color = new Color(colorEstado.r, colorEstado.g, colorEstado.b, 80f / 255f);
            }
            if (imgEstado != null)
            {
                imgEstado.sprite = icono;
                imgEstado.color = ObtenerColorEstado(rol, colorRespaldo);
                imgEstado.gameObject.SetActive(icono != null);
            }
            Color colorTexto = ObtenerColorEstado(rol, colorRespaldo);
            if (txtTitulo != null)
            {
                txtTitulo.text = titulo;
                AplicarColorTexto(txtTitulo, colorTexto);
            }
            if (txtMensajeDetalle != null)
            {
                txtMensajeDetalle.text = detalle;
                AplicarColorTexto(txtMensajeDetalle, fondoDetalle != null && fondoDetalle.gameObject.activeInHierarchy
                    ? UIThemeController.Theme != null ? UIThemeController.Theme.colorPrimario : colorTexto
                    : colorTexto);
            }
        }

        private static void AplicarColorTexto(TMP_Text texto, Color color)
        {
            texto.color = color;
            ThemedTextView themed = texto.GetComponent<ThemedTextView>();
            if (themed == null) return;

            themed.usarColorPersonalizado = true;
            themed.colorPersonalizado = color;
            themed.AplicarTema();
        }

        private static Color ObtenerColorEstado(ThemedGraphicView.RolColor rol, Color respaldo)
        {
            UIThemeModel theme = UIThemeController.Theme;
            if (theme == null) return respaldo;

            switch (rol)
            {
                case ThemedGraphicView.RolColor.Exito: return theme.colorExito;
                case ThemedGraphicView.RolColor.Advertencia: return theme.colorAdvertencia;
                case ThemedGraphicView.RolColor.Error: return theme.colorError;
                case ThemedGraphicView.RolColor.Informacion: return theme.colorInformacion;
                default: return respaldo;
            }
        }
    }
}
