using Assets.Scripts.Core.UI.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Core.UI.Components
{
    [ExecuteAlways]
    [RequireComponent(typeof(Image))]
    public sealed class ThemedGraphic : MonoBehaviour
    {
        public enum RolColor
        {
            Fondo = 0,
            Superficie = 1,
            SuperficieSuave = 2,
            Borde = 3,
            Primario = 4,
            PrimarioSuave = 5,
            Exito = 6,
            Advertencia = 7,
            Error = 8,
            Informacion = 9,
            Scrim = 10,
            SobrePrimario = 11,
            Tarjeta = 12,
            TarjetaSuave = 13,
            ProgresoFondo = 14,
            ProgresoRelleno = 15,
            LogroDorado = 16,
            BadgeDesbloqueado = 17,
            BadgeProgreso = 18,
            BadgeBloqueado = 19,
            TarjetaPrincipal = 20,
            TarjetaSecundaria = 21,
            TarjetaDestacada = 22,
            Secundario = 23,
            BordeLogroObtenido = 24,
            BordeLogroEnProgreso = 25,
            BordeLogroPendiente = 26
        }

        [SerializeField] private RolColor rol = RolColor.Superficie;
        [SerializeField, Range(0f, 1f)] private float multiplicadorAlpha = 1f;

        private Image _imagen;

        public RolColor Rol
        {
            get => rol;
            set
            {
                rol = value;
                AplicarTema();
            }
        }

        private void OnEnable()
        {
            UIThemeManager.OnThemeChanged += AplicarTema;
            AplicarTema();
        }

        private void OnDisable() => UIThemeManager.OnThemeChanged -= AplicarTema;

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall -= AplicarTemaEnEditor;
            UnityEditor.EditorApplication.delayCall += AplicarTemaEnEditor;
        }

        private void AplicarTemaEnEditor()
        {
            if (this == null) return;
            if (UIThemeManager.Theme == null)
            {
                UITheme theme = UnityEditor.AssetDatabase.LoadAssetAtPath<UITheme>(
                    "Assets/Content/UI/UITheme.asset");
                if (theme != null) UIThemeManager.SetTheme(theme);
            }
            AplicarTema();
        }
#endif

        public void AplicarTema()
        {
            UITheme theme = UIThemeManager.Theme;
            if (theme == null) return;
            if (_imagen == null) _imagen = GetComponent<Image>();
            Color color = ObtenerColor(theme);
            color.a *= multiplicadorAlpha;
            _imagen.color = color;
            _imagen.SetAllDirty();
        }

        private Color ObtenerColor(UITheme theme)
        {
            switch (rol)
            {
                case RolColor.Fondo: return theme.colorFondo;
                case RolColor.SuperficieSuave: return theme.colorSuperficieSuave;
                case RolColor.TarjetaPrincipal: return theme.colorTarjetaPrincipal;
                case RolColor.TarjetaSecundaria: return theme.colorTarjetaSecundaria;
                case RolColor.TarjetaDestacada: return theme.colorTarjetaDestacada;
                case RolColor.Borde: return theme.colorBorde;
                case RolColor.Primario: return theme.colorPrimario;
                case RolColor.PrimarioSuave: return theme.colorBotonSuave;
                case RolColor.Exito: return theme.colorCorrecto;
                case RolColor.Advertencia: return theme.colorAdvertencia;
                case RolColor.Error: return theme.colorIncorrecto;
                case RolColor.Informacion: return theme.colorInformacion;
                case RolColor.Scrim: return theme.colorScrim;
                case RolColor.SobrePrimario: return theme.colorTextoSobreOscuro;
                case RolColor.Tarjeta: return theme.colorTarjetaPrincipal;
                case RolColor.TarjetaSuave: return theme.colorTarjetaSecundaria;
                case RolColor.ProgresoFondo: return theme.colorProgresoFondo;
                case RolColor.ProgresoRelleno: return theme.colorPrimario;
                case RolColor.LogroDorado: return theme.colorLogroDorado;
                case RolColor.BadgeDesbloqueado: return theme.colorBadgeDesbloqueado;
                case RolColor.BadgeProgreso: return theme.colorBadgeProgreso;
                case RolColor.BadgeBloqueado: return theme.colorBadgeBloqueado;
                case RolColor.Secundario: return theme.colorTextoSecundario;
                case RolColor.BordeLogroObtenido: return theme.colorBordeLogroObtenido;
                case RolColor.BordeLogroEnProgreso: return theme.colorBordeLogroEnProgreso;
                case RolColor.BordeLogroPendiente: return theme.colorBordeLogroPendiente;
                default: return theme.colorSuperficie;
            }
        }
    }
}
