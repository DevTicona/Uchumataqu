using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI.Main.Paginas
{
    public enum EstadoIsla
    {
        Bloqueada,
        Disponible,
        Completada
    }

    /// <summary>
    /// Componente de botón de nodo/isla dentro del mapa curricular de 27 lecciones e hitos RA.
    /// Especificado en Docs/UI/03-escena-main.md §5.
    /// </summary>
    public class IslaItemView : MonoBehaviour
    {
        [Header("Elementos de UI")]
        [SerializeField] private Button btnIsla;
        [SerializeField] private TMP_Text txtNumero;
        [SerializeField] private TMP_Text txtTitulo;
        [SerializeField] private Image imgFondo;
        [SerializeField] private GameObject iconoCandado;
        [SerializeField] private GameObject iconoEstrella;
        [SerializeField] private GameObject indicadorRA; // Si es un hito de RA

        [Header("Colores por Estado")]
        [SerializeField] private Color colorBloqueada = new Color(0.70f, 0.70f, 0.70f, 1f);
        [SerializeField] private Color colorDisponible = new Color(0.92f, 0.76f, 0.35f, 1f); // Tierra
        [SerializeField] private Color colorCompletada = new Color(0.35f, 0.72f, 0.45f, 1f); // Verde acierto

        public string LeccionId { get; private set; }
        public string EscenarioRaId { get; private set; }
        public bool EsHitoRA => !string.IsNullOrEmpty(EscenarioRaId);

        public event Action<string> OnLeccionSeleccionada;
        public event Action<string> OnHitoRaSeleccionado;

        private void Awake()
        {
            if (btnIsla != null)
            {
                btnIsla.onClick.AddListener(OnIslaClicked);
            }
        }

        public void ConfigurarLeccion(string id, int numero, string titulo, EstadoIsla estado)
        {
            LeccionId = id;
            EscenarioRaId = null;

            if (txtNumero != null) txtNumero.text = $"L{numero:D2}";
            if (txtTitulo != null) txtTitulo.text = titulo;
            if (indicadorRA != null) indicadorRA.SetActive(false);

            ActualizarEstadoVisual(estado);
        }

        public void ConfigurarHitoRA(string escenarioId, string titulo, EstadoIsla estado)
        {
            LeccionId = null;
            EscenarioRaId = escenarioId;

            if (txtNumero != null) txtNumero.text = "RA";
            if (txtTitulo != null) txtTitulo.text = titulo;
            if (indicadorRA != null) indicadorRA.SetActive(true);

            ActualizarEstadoVisual(estado);
        }

        private void ActualizarEstadoVisual(EstadoIsla estado)
        {
            if (btnIsla != null)
                btnIsla.interactable = (estado != EstadoIsla.Bloqueada);

            if (iconoCandado != null)
                iconoCandado.SetActive(estado == EstadoIsla.Bloqueada);

            if (iconoEstrella != null)
                iconoEstrella.SetActive(estado == EstadoIsla.Completada);

            if (imgFondo != null)
            {
                switch (estado)
                {
                    case EstadoIsla.Bloqueada:
                        imgFondo.color = colorBloqueada;
                        break;
                    case EstadoIsla.Disponible:
                        imgFondo.color = colorDisponible;
                        break;
                    case EstadoIsla.Completada:
                        imgFondo.color = colorCompletada;
                        break;
                }
            }
        }

        private void OnIslaClicked()
        {
            if (EsHitoRA)
            {
                OnHitoRaSeleccionado?.Invoke(EscenarioRaId);
            }
            else
            {
                OnLeccionSeleccionada?.Invoke(LeccionId);
            }
        }
    }
}
