using System;
using Assets.Scripts.Core;
using Assets.Scripts.UI.Avatares;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Presenta los datos dinámicos de Inicio sin modificar el diseño serializado en la escena.
    /// </summary>
    public class InicioView : MonoBehaviour
    {
        [Header("Encabezado de Perfil")]
        [SerializeField] private TMP_Text txtSaludoUsuario;
        [SerializeField] private Image imgAvatarUsuario;
        [SerializeField] private CatalogoAvataresModel catalogoAvatares;

        [Header("Tarjeta Continuar")]
        [SerializeField] private TMP_Text txtSubtituloContinuar;
        [SerializeField] private TMP_Text txtProgresoContinuar;
        [SerializeField] private TMP_Text txtPorcentajeContinuar;
        [SerializeField] private Slider sliderProgresoContinuar;
        [SerializeField] private Button btnContinuarLeccion;
        [SerializeField] private Image iconoAccion;
        [SerializeField] private Sprite iconoComenzar;
        [SerializeField] private Sprite iconoContinuar;

        [Header("Resumen Rápido")]
        [SerializeField] private TMP_Text txtContadorLecciones;
        [SerializeField] private TMP_Text txtContadorRA;

        private string _leccionSiguienteId = "L01";

        public event Action<string> OnContinuarLeccion;
        public event Action OnSolicitarActualizacion;

        private void Awake()
        {
            if (btnContinuarLeccion != null)
                btnContinuarLeccion.onClick.AddListener(OnContinuarLeccionClicked);
        }

        private void OnEnable()
        {
            OnSolicitarActualizacion?.Invoke();
        }

        private void OnDestroy()
        {
            if (btnContinuarLeccion != null)
                btnContinuarLeccion.onClick.RemoveListener(OnContinuarLeccionClicked);
        }

        public void Mostrar(InicioModel datos)
        {
            if (datos == null) return;

            string nombre = string.IsNullOrEmpty(datos.Nombre) ? "Estudiante" : datos.Nombre;
            if (imgAvatarUsuario != null && catalogoAvatares != null)
            {
                Sprite spriteAvatar = catalogoAvatares.ObtenerSprite(datos.AvatarId);
                if (spriteAvatar != null)
                    imgAvatarUsuario.sprite = spriteAvatar;
            }

            if (txtSaludoUsuario != null)
                txtSaludoUsuario.text = $"¡Hola, {nombre}!";

            if (txtContadorLecciones != null)
                txtContadorLecciones.text = $"{datos.LeccionesCompletadas} / {datos.TotalLecciones}";

            if (txtContadorRA != null)
                txtContadorRA.text = $"{datos.EscenariosVisitados} / 6";

            _leccionSiguienteId = datos.LeccionId;

            if (txtSubtituloContinuar != null)
                txtSubtituloContinuar.text = datos.TituloLeccion;

            int porcentaje = Mathf.Clamp(datos.PorcentajeLeccionActual, 0, 100);
            if (txtProgresoContinuar != null)
                txtProgresoContinuar.text = "Progreso";
            if (txtPorcentajeContinuar != null)
                txtPorcentajeContinuar.text = $"{porcentaje}%";

            if (sliderProgresoContinuar != null)
                sliderProgresoContinuar.value = porcentaje / 100f;

            if (btnContinuarLeccion == null) return;

            btnContinuarLeccion.interactable = !string.IsNullOrWhiteSpace(datos.LeccionId);
            TMP_Text etiquetaBoton = btnContinuarLeccion.GetComponentInChildren<TMP_Text>(true);
            if (etiquetaBoton != null)
            {
                etiquetaBoton.text = datos.CurriculoCompletado
                    ? $"REPASAR LECCIÓN {datos.TotalLecciones}"
                    : porcentaje > 0 ? "CONTINUAR" : "COMENZAR";
            }
            if (iconoAccion != null)
                iconoAccion.sprite = porcentaje > 0 || datos.CurriculoCompletado ? iconoContinuar : iconoComenzar;
        }

        private void OnContinuarLeccionClicked()
        {
            OnContinuarLeccion?.Invoke(_leccionSiguienteId);
        }
    }
}
