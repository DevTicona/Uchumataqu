using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
using Assets.Scripts.UI.Main;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>Referencias y enlace de datos de una tarjeta de logro.</summary>
    public sealed class LogroItemView : MonoBehaviour
    {
        public enum EstadoLogro { Obtenido, EnProgreso, Bloqueado }

        [SerializeField] private Image fondo;
        [SerializeField] private Image badgeIcono;
        [SerializeField] private Image icono;
        [SerializeField] private TMP_Text txtTitulo;
        [SerializeField] private TMP_Text txtDescripcion;
        [SerializeField] private Slider sliderProgreso;
        [SerializeField] private Image rellenoProgreso;
        [SerializeField] private Image fondoEstado;
        [SerializeField] private TMP_Text txtEstado;
        [SerializeField] private Image marco;
        [SerializeField] private CatalogoIconosLogrosModel catalogoIconos;

        [Header("Estilos locales de Logros")]
        [SerializeField] private Color colorObtenido = new Color(0.12156863f, 0.68235294f, 0.35686275f, 1f);
        [SerializeField] private Color colorPendiente = new Color(0.65098039f, 0.67843137f, 0.70196078f, 1f);
        [SerializeField] private Color colorBadgeObtenido = new Color(0.86666667f, 0.96078431f, 0.90196078f, 1f);

        private EstadoLogro _estado;

        private void OnEnable()
        {
            UIThemeController.OnThemeChanged += AplicarEstilosLocales;
            AplicarEstilosLocales();
        }

        private void OnDisable() => UIThemeController.OnThemeChanged -= AplicarEstilosLocales;

        public void Configurar(LogroModel logro, EstadoLogro estado)
        {
            if (logro == null) return;
            _estado = estado;
            if (txtTitulo != null)
                txtTitulo.text = logro.Titulo;
            if (txtDescripcion != null)
                txtDescripcion.text = logro.Descripcion;
            if (txtEstado != null)
            {
                txtEstado.text = estado switch
                {
                    EstadoLogro.Obtenido => "Obtenido",
                    _ => $"{Mathf.Clamp(logro.ProgresoActual, 0, Mathf.Max(0, logro.Objetivo))}/{logro.Objetivo}"
                };
            }
            if (icono != null && catalogoIconos != null)
            {
                icono.sprite = catalogoIconos.ObtenerSprite(logro.IconoId);
                icono.enabled = icono.sprite != null;
            }

            float progreso = logro.Objetivo > 0 ? (float)logro.ProgresoActual / logro.Objetivo : 0f;
            if (sliderProgreso != null)
                sliderProgreso.SetValueWithoutNotify(Mathf.Clamp01(progreso));

            if (fondo != null)
            {
                LogrosView.AsignarThemedGraphic(fondo, estado == EstadoLogro.Bloqueado
                    ? ThemedGraphicView.RolColor.SuperficieSuave : ThemedGraphicView.RolColor.Superficie);
            }
            if (icono != null)
            {
                LogrosView.AsignarThemedGraphic(icono, estado switch
                {
                    EstadoLogro.Obtenido => ThemedGraphicView.RolColor.Exito,
                    EstadoLogro.EnProgreso => ThemedGraphicView.RolColor.Primario,
                    _ => ThemedGraphicView.RolColor.SobreColor
                });
            }
            if (rellenoProgreso != null)
            {
                LogrosView.AsignarThemedGraphic(rellenoProgreso, estado == EstadoLogro.Obtenido
                    ? ThemedGraphicView.RolColor.Exito : ThemedGraphicView.RolColor.Primario);
            }
            if (txtTitulo != null)
                LogrosView.AsignarThemedText(txtTitulo, ThemedTextView.TipoTexto.Normal, ThemedTextView.ColorTexto.Primario);
            if (txtDescripcion != null)
                LogrosView.AsignarThemedText(txtDescripcion, ThemedTextView.TipoTexto.Pequeño, ThemedTextView.ColorTexto.Secundario);
            if (txtEstado != null)
            {
                LogrosView.AsignarThemedText(txtEstado, ThemedTextView.TipoTexto.Pequeño,
                    estado == EstadoLogro.Obtenido ? ThemedTextView.ColorTexto.Exito : ThemedTextView.ColorTexto.Secundario);
            }
            AplicarEstilosLocales();
        }

        private void AplicarEstilosLocales()
        {
            UIThemeModel theme = UIThemeController.Theme;
            if (theme == null) return;

            Color badge = _estado switch
            {
                EstadoLogro.Obtenido => colorBadgeObtenido,
                EstadoLogro.EnProgreso => theme.colorControlSuave,
                _ => theme.colorDeshabilitado
            };
            AplicarColorLocal(badgeIcono, badge);
            AplicarColorLocal(fondoEstado, badge);
            AplicarColorLocal(marco, _estado switch
            {
                EstadoLogro.Obtenido => colorObtenido,
                EstadoLogro.EnProgreso => theme.colorPrimario,
                _ => colorPendiente
            });
        }

        private static void AplicarColorLocal(Image imagen, Color color)
        {
            if (imagen == null) return;
            ThemedGraphicView themed = imagen.GetComponent<ThemedGraphicView>();
            if (themed != null) themed.enabled = false;
            imagen.color = color;
        }
    }
}
