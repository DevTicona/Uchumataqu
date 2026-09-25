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
        [SerializeField] private ThemedGraphicView bordeDetalle;
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

        private ThemedGraphicView.RolColor rolActivo;
        private Color colorRespaldoActivo;
        private bool tieneEstadoActivo;

        private void OnEnable()
        {
            UIThemeController.OnThemeChanged += ActualizarPaletaActiva;
            ActualizarPaletaActiva();
        }

        private void OnDisable()
        {
            UIThemeController.OnThemeChanged -= ActualizarPaletaActiva;
        }

        public bool ValidarConfiguracion(out string error)
        {
            bool completa = fondoFeedback != null && bordeDetalle != null && imgEstado != null && txtTitulo != null &&
                txtMensajeDetalle != null && iconoCorrecto != null && iconoAdvertencia != null && iconoIncorrecto != null;
            error = completa ? null : "El contenido de feedback tiene referencias incompletas.";
            return completa;
        }

        public void MostrarCorrecto(string detalle = null)
        {
            ConfigurarVisual(ThemedGraphicView.RolColor.Exito, colorCorrecto, iconoCorrecto,
                "¡Correcto!", detalle ?? "Resolviste la actividad. Continúa con el siguiente paso.");
            gameObject.SetActive(true);
        }

        public void MostrarCasi(string detalle = null, double similitud = 0)
        {
            string msg = string.IsNullOrEmpty(detalle)
                ? "Estás cerca de la respuesta esperada. Revisa los detalles y vuelve a intentarlo."
                : detalle;

            ConfigurarVisual(ThemedGraphicView.RolColor.Advertencia, colorCasi, iconoAdvertencia,
                "Casi", msg);
            gameObject.SetActive(true);
        }

        public void MostrarIncorrecto(string respuestaCorrecta = null)
        {
            string msg = string.IsNullOrEmpty(respuestaCorrecta)
                ? "Revisa la respuesta esperada y úsala en el siguiente intento."
                : respuestaCorrecta;

            ConfigurarVisual(ThemedGraphicView.RolColor.Error, colorIncorrecto, iconoIncorrecto,
                "Respuesta incorrecta", msg);
            gameObject.SetActive(true);
        }

        public void MostrarInformacion(string detalle = null)
        {
            ConfigurarVisual(ThemedGraphicView.RolColor.Informacion, Color.clear,
                iconoInformacion != null ? iconoInformacion : iconoAdvertencia,
                "Información", detalle ?? "Sigue las indicaciones para continuar con la actividad.");
            gameObject.SetActive(true);
        }

        public void Ocultar()
        {
            gameObject.SetActive(false);
        }

        private void ConfigurarVisual(ThemedGraphicView.RolColor rol, Color colorRespaldo,
            Sprite icono, string titulo, string detalle)
        {
            rolActivo = rol;
            colorRespaldoActivo = colorRespaldo;
            tieneEstadoActivo = true;

            if (imgEstado != null)
            {
                imgEstado.sprite = icono;
                imgEstado.gameObject.SetActive(icono != null);
            }
            if (txtTitulo != null)
            {
                txtTitulo.text = titulo;
            }
            if (txtMensajeDetalle != null)
            {
                txtMensajeDetalle.text = detalle;
            }

            ActualizarPaletaActiva();
        }

        private void ActualizarPaletaActiva()
        {
            if (!tieneEstadoActivo) return;

            Color colorEstado = ObtenerColorEstado(rolActivo, colorRespaldoActivo);
            if (fondoFeedback != null)
            {
                UIThemeModel tema = UIThemeController.Theme;
                Color superficie = tema != null ? tema.colorSuperficie : Color.white;
                fondoFeedback.color = Color.Lerp(superficie, colorEstado, 0.16f);
            }
            if (bordeDetalle != null)
                bordeDetalle.Rol = rolActivo;

            AplicarColorEstado(imgEstado, rolActivo, colorEstado);
            AplicarColorTitulo(txtTitulo, rolActivo, colorEstado);
            AplicarColorDetalle(txtMensajeDetalle);
        }

        private static void AplicarColorEstado(Image imagen, ThemedGraphicView.RolColor rol, Color colorRespaldo)
        {
            if (imagen == null) return;

            imagen.color = colorRespaldo;
            ThemedGraphicView themed = imagen.GetComponent<ThemedGraphicView>();
            if (themed != null) themed.Rol = rol;
        }

        private static void AplicarColorTitulo(TMP_Text texto, ThemedGraphicView.RolColor rol, Color colorRespaldo)
        {
            if (texto == null) return;

            texto.color = colorRespaldo;
            ThemedTextView themed = texto.GetComponent<ThemedTextView>();
            if (themed == null) return;

            themed.usarColorPersonalizado = false;
            themed.tipoColor = ObtenerTipoColorTitulo(rol);
            themed.AplicarTema();
        }

        private static void AplicarColorDetalle(TMP_Text texto)
        {
            if (texto == null) return;

            ThemedTextView themed = texto.GetComponent<ThemedTextView>();
            if (themed != null) themed.AplicarTema();
        }

        private static ThemedTextView.ColorTexto ObtenerTipoColorTitulo(ThemedGraphicView.RolColor rol)
        {
            switch (rol)
            {
                case ThemedGraphicView.RolColor.Exito: return ThemedTextView.ColorTexto.Exito;
                case ThemedGraphicView.RolColor.Advertencia: return ThemedTextView.ColorTexto.Advertencia;
                case ThemedGraphicView.RolColor.Error: return ThemedTextView.ColorTexto.Error;
                case ThemedGraphicView.RolColor.Informacion: return ThemedTextView.ColorTexto.Informacion;
                default: return ThemedTextView.ColorTexto.Primario;
            }
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
