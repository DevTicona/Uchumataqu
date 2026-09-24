using Assets.Scripts.Core.UI.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Core.UI.Components
{
    [RequireComponent(typeof(Image))]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.Core.UI.Components", sourceAssembly: "Assembly-CSharp", sourceClassName: "ThemedGraphic")]
    public sealed class ThemedGraphicView : MonoBehaviour
    {
        public enum RolColor
        {
            Superficie = 1,
            SuperficieSuave = 2,
            Borde = 3,
            BordeSecundario = 17,
            Primario = 4,
            Secundario = 15,
            ControlSuave = 5,
            Exito = 6,
            Advertencia = 7,
            Error = 8,
            Informacion = 9,
            Scrim = 10,
            SobreColor = 11,
            ProgresoFondo = 14,
            TextoSecundario = 23,
            Deshabilitado = 27,
            TextoTerciario = 28
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
            UIThemeController.OnThemeChanged += AplicarTema;
            AplicarTema();
        }

        private void OnDisable() => UIThemeController.OnThemeChanged -= AplicarTema;

        public void AplicarTema()
        {
            UIThemeModel theme = UIThemeController.Theme;
            if (theme == null) return;
            if (_imagen == null) _imagen = GetComponent<Image>();
            Color color = ObtenerColor(theme);
            color.a *= multiplicadorAlpha;
            _imagen.color = color;
            _imagen.SetAllDirty();
        }

        private Color ObtenerColor(UIThemeModel theme)
        {
            switch (rol)
            {
                case RolColor.SuperficieSuave: return theme.colorSuperficieSuave;
                case RolColor.Borde: return theme.colorBorde;
                case RolColor.BordeSecundario: return theme.colorBordeSecundario;
                case RolColor.Primario: return theme.colorPrimario;
                case RolColor.Secundario: return theme.colorSecundario;
                case RolColor.ControlSuave: return theme.colorControlSuave;
                case RolColor.Exito: return theme.colorExito;
                case RolColor.Advertencia: return theme.colorAdvertencia;
                case RolColor.Error: return theme.colorError;
                case RolColor.Informacion: return theme.colorInformacion;
                case RolColor.Scrim: return theme.colorScrim;
                case RolColor.SobreColor: return theme.colorTextoSobreColor;
                case RolColor.ProgresoFondo: return theme.colorProgresoFondo;
                case RolColor.TextoSecundario: return theme.colorTextoSecundario;
                case RolColor.Deshabilitado: return theme.colorDeshabilitado;
                case RolColor.TextoTerciario: return theme.colorTextoTerciario;
                default: return theme.colorSuperficie;
            }
        }
    }
}
