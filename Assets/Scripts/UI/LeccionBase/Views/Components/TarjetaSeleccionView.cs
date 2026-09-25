using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    [DisallowMultipleComponent]
    public sealed class TarjetaSeleccionView : MonoBehaviour
    {
        [SerializeField] private Image indicador;
        [SerializeField] private Image check;
        [SerializeField] private Sprite aro;
        [SerializeField] private Sprite iconoCorrecto;
        [SerializeField] private Sprite iconoIncorrecto;

        private bool _seleccionada;
        private bool? _resultado;
        private ThemedGraphicView _borde;

        private void OnEnable()
        {
            UIThemeController.OnThemeChanged += ActualizarEstadoVisual;
            ActualizarEstadoVisual();
        }

        private void OnDisable() => UIThemeController.OnThemeChanged -= ActualizarEstadoVisual;

        public void MostrarSeleccion(bool seleccionada)
        {
            _seleccionada = seleccionada;
            _resultado = null;
            ActualizarEstadoVisual();
        }

        public void MostrarResultado(bool correcto)
        {
            _resultado = correcto;
            ActualizarEstadoVisual();
        }

        private void ActualizarEstadoVisual()
        {
            bool marcada = _seleccionada || _resultado.HasValue;
            // El indicador conserva siempre el aro definido por el prefab. El estado se
            // representa con el icono interior, no reemplazando el aro por un círculo sólido.
            if (indicador != null && aro != null) indicador.sprite = aro;
            if (check != null)
            {
                check.gameObject.SetActive(marcada);
                Sprite icono = _resultado == false ? iconoIncorrecto : iconoCorrecto;
                if (icono != null) check.sprite = icono;
            }

            UIThemeModel tema = UIThemeController.Theme;
            if (tema == null) return;

            ThemedGraphicView.RolColor rolBorde = ObtenerRolBorde();
            if (_borde == null)
                _borde = transform.Find("Marco")?.GetComponent<ThemedGraphicView>();
            if (_borde != null) _borde.Rol = rolBorde;

            Color colorEstado = ObtenerColorEstado(tema);
            if (indicador != null) indicador.color = colorEstado;
            if (check != null) check.color = colorEstado;
        }

        private ThemedGraphicView.RolColor ObtenerRolBorde()
        {
            if (_resultado == true) return ThemedGraphicView.RolColor.Exito;
            if (_resultado == false) return ThemedGraphicView.RolColor.Error;
            return _seleccionada
                ? ThemedGraphicView.RolColor.Primario
                : ThemedGraphicView.RolColor.Borde;
        }

        private Color ObtenerColorEstado(UIThemeModel tema)
        {
            if (_resultado == true) return tema.colorExito;
            if (_resultado == false) return tema.colorError;
            return _seleccionada ? tema.colorPrimario : tema.colorBorde;
        }
    }
}
