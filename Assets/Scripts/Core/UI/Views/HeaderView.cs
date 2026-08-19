using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.Core.UI.Views
{
    public class HeaderView : MonoBehaviour
    {
        [Header("Navegación")]
        [SerializeField] private Button botonAtras;

        [Header("Texto")]
        [SerializeField] private TextMeshProUGUI titulo;
        [SerializeField] private TextMeshProUGUI subtitulo;

        [Header("Icono (opcional)")]
        [SerializeField] private Image iconoLeccion;

        // -------- Navegación --------

        public void BindBotonAtras(System.Action onClick)
        {
            if (botonAtras == null) return;

            botonAtras.onClick.RemoveAllListeners();
            botonAtras.onClick.AddListener(() => onClick?.Invoke());
        }

        // -------- Texto --------

        public void SetTitulo(string texto)
        {
            if (titulo == null) return;
            titulo.text = texto;
        }

        public void SetSubtitulo(string texto)
        {
            if (subtitulo == null) return;

            bool mostrar = !string.IsNullOrEmpty(texto);
            subtitulo.gameObject.SetActive(mostrar);

            if (mostrar)
                subtitulo.text = texto;
        }

        // -------- Icono --------

        public void SetIcono(Sprite sprite)
        {
            if (iconoLeccion == null) return;

            bool mostrar = sprite != null;
            iconoLeccion.gameObject.SetActive(mostrar);

            if (mostrar)
                iconoLeccion.sprite = sprite;
        }

        public void OcultarIcono()
        {
            if (iconoLeccion == null) return;
            iconoLeccion.gameObject.SetActive(false);
        }
    }
}
