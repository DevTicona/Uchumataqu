using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Models;
using Assets.Scripts.Services;
using System;

namespace Assets.Scripts.Views.UI
{
    /// <summary>
    /// Vista de una lección representada como isla en el mapa.
    /// Maneja exclusivamente la representación visual y la interacción básica.
    /// </summary>
    public class IslaLeccionView : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private Button boton;
        [SerializeField] private Image fondoIsla;
        [SerializeField] private Image iconoLeccion;
        [SerializeField] private Image iconoEstado;
        [SerializeField] private TextMeshProUGUI tituloTexto;

        [Header("Colores por Estado")]
        [SerializeField] private Color colorBloqueada = Color.gray;
        [SerializeField] private Color colorEnProgreso = Color.yellow;
        [SerializeField] private Color colorCompletada = Color.green;

        private LeccionData leccionData;
        private EstadoLeccion estadoActual;
        private Action<LeccionData> callbackSeleccion;

        // Sprites de estado cargados desde Addressables
        private Sprite spriteEstadoBloqueado;
        private Sprite spriteEstadoEnProgreso;
        private Sprite spriteEstadoCompletado;

        private void Awake()
        {
            if (boton == null)
                boton = GetComponent<Button>();
        }

        /// <summary>
        /// Configura visualmente la isla según los datos y estado de la lección.
        /// </summary>
        public void Configurar(
            LeccionData leccion,
            Sprite icono,
            EstadoLeccion estado,
            Sprite spriteEstadoBloqueado,
            Sprite spriteEstadoEnProgreso,
            Sprite spriteEstadoCompletado,
            Action<LeccionData> onSeleccion
        )
        {
            leccionData = leccion;
            estadoActual = estado;
            callbackSeleccion = onSeleccion;

            // Asignar sprites de estado
            this.spriteEstadoBloqueado = spriteEstadoBloqueado;
            this.spriteEstadoEnProgreso = spriteEstadoEnProgreso;
            this.spriteEstadoCompletado = spriteEstadoCompletado;

            if (tituloTexto != null)
                tituloTexto.text = leccion.titulo;

            if (iconoLeccion != null)
            {
                iconoLeccion.sprite = icono;
                iconoLeccion.enabled = icono != null;
            }

            AplicarEstadoVisual(estado);

            boton.onClick.RemoveAllListeners();
            boton.onClick.AddListener(OnBotonPresionado);
        }

        /// <summary>
        /// Asigna el sprite visual de la isla (forma / bioma).
        /// Este método es llamado desde el menú o presenter.
        /// </summary>
        public void SetIslaSprite(Sprite spriteIsla)
        {
            if (fondoIsla == null || spriteIsla == null) return;

            fondoIsla.sprite = spriteIsla;
            fondoIsla.type = Image.Type.Simple;
            fondoIsla.preserveAspect = true;
        }

        /// <summary>
        /// Actualiza el icono de la lección cuando se carga de forma diferida.
        /// </summary>
        public void ActualizarIcono(Sprite icono)
        {
            if (iconoLeccion == null || icono == null) return;

            iconoLeccion.sprite = icono;
            iconoLeccion.enabled = true;
        }

        /// <summary>
        /// Aplica color, estado del botón y sprite de estado.
        /// </summary>
        private void AplicarEstadoVisual(EstadoLeccion estado)
        {
            bool interactuable;

            switch (estado)
            {
                case EstadoLeccion.Bloqueada:
                    fondoIsla.color = colorBloqueada;
                    iconoEstado.sprite = spriteEstadoBloqueado;
                    interactuable = false;
                    break;

                case EstadoLeccion.EnProgreso:
                    fondoIsla.color = colorEnProgreso;
                    iconoEstado.sprite = spriteEstadoEnProgreso;
                    interactuable = true;
                    break;

                case EstadoLeccion.Completada:
                    fondoIsla.color = colorCompletada;
                    iconoEstado.sprite = spriteEstadoCompletado;
                    interactuable = true;
                    break;

                default:
                    interactuable = false;
                    break;
            }

            if (iconoEstado != null)
                iconoEstado.enabled = true;

            if (boton != null)
                boton.interactable = interactuable;

            if (TryGetComponent(out CanvasGroup canvasGroup))
                canvasGroup.alpha = interactuable ? 1f : 0.6f;
        }

        private void OnBotonPresionado()
        {
            if (leccionData != null)
                callbackSeleccion?.Invoke(leccionData);
        }

        public EstadoLeccion ObtenerEstado() => estadoActual;
        public LeccionData ObtenerLeccion() => leccionData;
    }
}
