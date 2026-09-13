using Assets.Scripts.Core.UI.Components;
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
        [SerializeField, Min(0.01f)] private float pixelsPerUnitMarco = 4f;

        public void Configurar(LogroData logro, EstadoLogro estado)
        {
            if (logro == null) return;
            txtTitulo.text = logro.Titulo;
            txtDescripcion.text = logro.Descripcion;
            txtEstado.text = estado switch
            {
                EstadoLogro.Obtenido => "Obtenido",
                _ => $"{Mathf.Clamp(logro.ProgresoActual, 0, Mathf.Max(0, logro.Objetivo))}/{logro.Objetivo}"
            };
            LogrosView.CargarIconoLogro(icono, logro);

            float progreso = logro.Objetivo > 0 ? (float)logro.ProgresoActual / logro.Objetivo : 0f;
            if (sliderProgreso != null)
                sliderProgreso.SetValueWithoutNotify(Mathf.Clamp01(progreso));

            ThemedGraphic.RolColor rolBadge = estado switch
            {
                EstadoLogro.Obtenido => ThemedGraphic.RolColor.BadgeDesbloqueado,
                EstadoLogro.EnProgreso => ThemedGraphic.RolColor.BadgeProgreso,
                _ => ThemedGraphic.RolColor.BadgeBloqueado
            };
            LogrosView.AsignarThemedGraphic(fondo, estado == EstadoLogro.Bloqueado
                ? ThemedGraphic.RolColor.TarjetaSecundaria : ThemedGraphic.RolColor.TarjetaPrincipal);
            LogrosView.AsignarThemedGraphic(badgeIcono, rolBadge);
            LogrosView.AsignarThemedGraphic(icono, estado switch
            {
                EstadoLogro.Obtenido => ThemedGraphic.RolColor.Exito,
                EstadoLogro.EnProgreso => ThemedGraphic.RolColor.Primario,
                _ => ThemedGraphic.RolColor.Secundario
            });
            LogrosView.AsignarThemedGraphic(rellenoProgreso, estado == EstadoLogro.Obtenido
                ? ThemedGraphic.RolColor.Exito : ThemedGraphic.RolColor.Primario);
            LogrosView.AsignarThemedGraphic(fondoEstado, rolBadge);
            if (marco != null)
            {
                marco.pixelsPerUnitMultiplier = pixelsPerUnitMarco;
                LogrosView.AsignarThemedGraphic(marco, estado switch
                {
                    EstadoLogro.Obtenido => ThemedGraphic.RolColor.BordeLogroObtenido,
                    EstadoLogro.EnProgreso => ThemedGraphic.RolColor.BordeLogroEnProgreso,
                    _ => ThemedGraphic.RolColor.BordeLogroPendiente
                });
            }
            LogrosView.AsignarThemedText(txtTitulo, ThemedText.TipoTexto.Normal, ThemedText.ColorTexto.Primario);
            LogrosView.AsignarThemedText(txtDescripcion, ThemedText.TipoTexto.Pequeño, ThemedText.ColorTexto.Secundario);
            LogrosView.AsignarThemedText(txtEstado, ThemedText.TipoTexto.Pequeño,
                estado == EstadoLogro.Obtenido ? ThemedText.ColorTexto.Exito : ThemedText.ColorTexto.Secundario);
        }
    }
}
