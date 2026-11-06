using Assets.Scripts.UI.Main;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Main.Paginas
{

    /// <summary>
    /// Controla la interacción y el estado visual del prefab ContenedorIsla.
    /// IconoEstado muestra candado, ubicación o estrella; IconoRa identifica los hitos RA.
    /// </summary>
    public class IslaItemView : MonoBehaviour
    {
        [Header("Elementos del prefab ContenedorIsla")]
        [SerializeField] private Image iconoEstado;
        [SerializeField] private GameObject iconoRa;
        [SerializeField] private Button btnIsla;
        [SerializeField] private Image imgFondo;

        [Header("Iconos de Estado")]
        [SerializeField] private Sprite iconoBloqueado;
        [SerializeField] private Sprite iconoActual;
        [SerializeField] private Sprite iconoCompletado;

        public Sprite IconoBloqueado => iconoBloqueado;
        public Sprite IconoActual => iconoActual;
        public Sprite IconoCompletado => iconoCompletado;

        private static readonly Color ColorIslaNormal = Color.white;
        private static readonly Color ColorIslaBloqueada = new Color(0.40f, 0.40f, 0.44f, 1f);
        private static readonly Color ColorEstadoBloqueado = new Color(0.40f, 0.47f, 0.54f, 1f);
        private static readonly Color ColorEstadoActual = new Color(0.96f, 0.55f, 0.08f, 1f);
        private static readonly Color ColorEstadoCompletado = new Color(1f, 0.78f, 0.08f, 1f);
        private static readonly Color ColorIndicadorRa = new Color(0.06f, 0.20f, 0.38f, 1f);

        public string LeccionId { get; private set; }
        public string EscenarioRaId { get; private set; }
        public bool EsHitoRA => !string.IsNullOrEmpty(EscenarioRaId);

        public event Action<string> OnLeccionSeleccionada;
        public event Action<string> OnHitoRaSeleccionado;
        public event Action<IslaItemView, string> OnSpriteSolicitado;

        private EstadoIsla _estadoActual = EstadoIsla.Disponible;
        private bool _esActual;

        private void Awake()
        {
            AsegurarReferencias();
            if (btnIsla != null)
            {
                btnIsla.onClick.AddListener(OnIslaClicked);
            }
        }

        public void InicializarRuntime()
        {
            AsegurarReferencias();
            var rect = GetComponent<RectTransform>();
            if (rect != null && rect.sizeDelta == Vector2.zero)
            {
                rect.sizeDelta = new Vector2(300f, 300f);
            }
        }

        private void AsegurarReferencias()
        {
            if (imgFondo == null)
                imgFondo = transform.Find("ImgIsla")?.GetComponent<Image>();

            if (iconoEstado == null)
                iconoEstado = transform.Find("IconoEstado")?.GetComponent<Image>();

            if (iconoRa == null)
                iconoRa = transform.Find("IconoRa")?.gameObject;

            if (btnIsla == null)
                btnIsla = GetComponent<Button>();

            if (btnIsla != null && imgFondo != null)
            {
                btnIsla.targetGraphic = imgFondo;
                btnIsla.transition = Selectable.Transition.None;
            }
        }

        public void ConfigurarLeccion(string id, EstadoIsla estado, bool esActual)
        {
            LeccionId = id;
            EscenarioRaId = null;
            _estadoActual = estado;
            _esActual = esActual;

            AsegurarReferencias();
            ActualizarEstadoVisual();
            OnSpriteSolicitado?.Invoke(this, id);
        }

        public void ConfigurarHitoRA(string escenarioId, EstadoIsla estado, bool esActual)
        {
            LeccionId = null;
            EscenarioRaId = escenarioId;
            _estadoActual = estado;
            _esActual = esActual;

            AsegurarReferencias();
            ActualizarEstadoVisual();
            OnSpriteSolicitado?.Invoke(this, escenarioId);
        }

        private void ActualizarEstadoVisual()
        {
            bool bloqueada = _estadoActual == EstadoIsla.Bloqueada;
            GetComponent<Assets.Scripts.Core.UI.Components.FlotacionUI>()?.EstablecerActivo(false);

            if (btnIsla != null)
                btnIsla.interactable = !bloqueada;

            if (imgFondo != null)
                imgFondo.color = bloqueada ? ColorIslaBloqueada : ColorIslaNormal;

            if (iconoRa != null)
            {
                iconoRa.SetActive(EsHitoRA);
                if (EsHitoRA)
                    SetColorIcono(iconoRa, ColorIndicadorRa);
            }

            ActualizarIconoEstado();
        }

        private void ActualizarIconoEstado()
        {
            if (iconoEstado == null) return;
            bool bloqueada = _estadoActual == EstadoIsla.Bloqueada;
            bool completada = _estadoActual == EstadoIsla.Completada;
            bool mostrarIcono = bloqueada || completada || _esActual;
            iconoEstado.gameObject.SetActive(mostrarIcono);
            if (!mostrarIcono) return;

            Sprite sprite = ObtenerSpriteEstado(bloqueada, completada);
            if (sprite != null)
                iconoEstado.sprite = sprite;

            iconoEstado.color = ObtenerColorEstado(bloqueada, completada);
        }

        private Sprite ObtenerSpriteEstado(bool bloqueada, bool completada)
        {
            if (bloqueada) return iconoBloqueado;
            if (completada) return iconoCompletado;
            return iconoActual;
        }

        private static Color ObtenerColorEstado(bool bloqueada, bool completada)
        {
            if (bloqueada) return ColorEstadoBloqueado;
            if (completada) return ColorEstadoCompletado;
            return ColorEstadoActual;
        }

        private static void SetColorIcono(GameObject obj, Color color)
        {
            var img = obj.GetComponent<Image>() ?? obj.GetComponentInChildren<Image>(true);
            if (img != null)
            {
                img.color = color;
            }
        }

        private void OnIslaClicked()
        {
            if (_estadoActual == EstadoIsla.Bloqueada) return;
            if (EsHitoRA)
                OnHitoRaSeleccionado?.Invoke(EscenarioRaId);
            else
                OnLeccionSeleccionada?.Invoke(LeccionId);
        }

        public void MostrarSprite(Sprite sprite, string id)
        {
            if (sprite == null || imgFondo == null || (LeccionId ?? EscenarioRaId) != id) return;
            imgFondo.sprite = sprite;
            imgFondo.preserveAspect = true;
            ActualizarEstadoVisual();
        }

        private void OnDestroy()
        {
            if (btnIsla != null) btnIsla.onClick.RemoveListener(OnIslaClicked);
        }
    }
}
