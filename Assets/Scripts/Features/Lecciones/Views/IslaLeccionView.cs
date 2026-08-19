using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Assets.Scripts.Features.Lecciones.Models;

namespace Assets.Scripts.Features.Lecciones.Views
{
    public class IslaLeccionView : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private Button boton;
        [SerializeField] private Image fondoIsla;
        [SerializeField] private Image iconoEstado;
        [SerializeField] private TextMeshProUGUI tituloTexto;

        [Header("Colores por Estado")]
        [SerializeField] private Color colorBloqueada  = Color.gray;
        [SerializeField] private Color colorEnProgreso = Color.yellow;
        [SerializeField] private Color colorCompletada = Color.green;

        private IslaMapaData islaData;
        private EstadoLeccion estadoActual;
        private Action<IslaMapaData> callbackSeleccion;

        private Sprite spriteEstadoBloqueado;
        private Sprite spriteEstadoEnProgreso;
        private Sprite spriteEstadoCompletado;

        private void Awake()
        {
            if (boton == null)
                boton = GetComponent<Button>();
        }

        public void Configurar(
            IslaMapaData isla,
            EstadoLeccion estado,
            Sprite spriteBloqueado,
            Sprite spriteEnProgreso,
            Sprite spriteCompletado,
            Action<IslaMapaData> onSeleccion
        )
        {
            islaData          = isla;
            estadoActual      = estado;
            callbackSeleccion = onSeleccion;

            spriteEstadoBloqueado  = spriteBloqueado;
            spriteEstadoEnProgreso = spriteEnProgreso;
            spriteEstadoCompletado = spriteCompletado;

            if (tituloTexto != null)
                tituloTexto.text = isla.Nombre;

            AplicarEstadoVisual(estado);

            boton.onClick.RemoveAllListeners();
            boton.onClick.AddListener(OnBotonPresionado);
        }

        public void SetIslaSprite(Sprite spriteIsla)
        {
            if (fondoIsla == null || spriteIsla == null) return;
            fondoIsla.sprite = spriteIsla;
            fondoIsla.type = Image.Type.Simple;
            fondoIsla.preserveAspect = true;
        }

        private void AplicarEstadoVisual(EstadoLeccion estado)
        {
            Sprite spriteEstado;
            bool interactuable;

            switch (estado)
            {
                case EstadoLeccion.Bloqueada:
                    if (fondoIsla != null) fondoIsla.color = colorBloqueada;
                    spriteEstado  = spriteEstadoBloqueado;
                    interactuable = false;
                    break;
                case EstadoLeccion.EnProgreso:
                    if (fondoIsla != null) fondoIsla.color = colorEnProgreso;
                    spriteEstado  = spriteEstadoEnProgreso;
                    interactuable = true;
                    break;
                case EstadoLeccion.Completada:
                    if (fondoIsla != null) fondoIsla.color = colorCompletada;
                    spriteEstado  = spriteEstadoCompletado;
                    interactuable = true;
                    break;
                default:
                    spriteEstado  = null;
                    interactuable = false;
                    break;
            }

            if (iconoEstado != null)
            {
                iconoEstado.sprite  = spriteEstado;
                iconoEstado.enabled = spriteEstado != null;
            }

            if (boton != null)
                boton.interactable = interactuable;

            if (TryGetComponent(out CanvasGroup cg))
                cg.alpha = interactuable ? 1f : 0.6f;
        }

        private void OnBotonPresionado() => callbackSeleccion?.Invoke(islaData);

        public EstadoLeccion ObtenerEstado() => estadoActual;
        public IslaMapaData ObtenerIsla()    => islaData;
    }
}