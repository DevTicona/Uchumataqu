using TMPro;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Core.UI.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class TarjetaSeleccionView : MonoBehaviour
    {
        [SerializeField] private Image fondo;
        [SerializeField] private Image marco;
        [SerializeField] private Image indicador;
        [SerializeField] private Image check;
        [SerializeField] private TMP_Text texto;
        [SerializeField] private Sprite circulo;
        [SerializeField] private Sprite aro;

        private bool _seleccionada;
        private bool? _resultado;

        private void OnEnable()
        {
            UIThemeController.OnThemeChanged += AplicarTema;
            AplicarTema();
        }

        private void OnDisable() => UIThemeController.OnThemeChanged -= AplicarTema;

        public void MostrarSeleccion(bool seleccionada)
        {
            _seleccionada = seleccionada;
            _resultado = null;
            AplicarTema();
        }

        public void MostrarResultado(bool correcto)
        {
            _resultado = correcto;
            AplicarTema();
        }

        public void AplicarTema()
        {
            UIThemeModel theme = UIThemeController.Theme;
            if (theme == null || fondo == null || marco == null || indicador == null || check == null || texto == null) return;
            bool marcada = _seleccionada || _resultado.HasValue;
            Color color = _resultado == false ? theme.colorError : theme.colorExito;
            fondo.color = marcada ? Color.Lerp(theme.colorSuperficie, color, 0.18f) : theme.colorSuperficie;
            marco.color = indicador.color = marcada ? color : theme.colorBorde;
            indicador.sprite = marcada ? circulo : aro;
            check.gameObject.SetActive(marcada);
            check.color = theme.colorTextoSobreColor;
            var themed = texto.GetComponent<ThemedTextView>();
            if (themed != null)
            {
                themed.tipoColor = !marcada ? ThemedTextView.ColorTexto.Primario :
                    _resultado == false ? ThemedTextView.ColorTexto.Error : ThemedTextView.ColorTexto.Exito;
                themed.AplicarTema();
            }
            texto.fontStyle = marcada ? FontStyles.Bold : FontStyles.Normal;
        }
    }
}
