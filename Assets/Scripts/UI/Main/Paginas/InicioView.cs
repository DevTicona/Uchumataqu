using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista de la pestaña de Inicio en la escena 02_Main.
    /// Especificado en Docs/UI/03-escena-main.md §4.
    /// </summary>
    public class InicioView : MonoBehaviour
    {
        [Header("Encabezado de Perfil")]
        [SerializeField] private TMP_Text txtSaludoUsuario;
        [SerializeField] private Image imgAvatarUsuario;

        [Header("Tarjeta Continuar")]
        [SerializeField] private TMP_Text txtTituloContinuar;
        [SerializeField] private TMP_Text txtSubtituloContinuar;
        [SerializeField] private Button btnContinuarLeccion;

        [Header("Resumen Rápido")]
        [SerializeField] private TMP_Text txtContadorLecciones;
        [SerializeField] private TMP_Text txtContadorRA;

        private string _leccionSiguienteId = "L01";

        private void Awake()
        {
            if (btnContinuarLeccion != null)
            {
                btnContinuarLeccion.onClick.AddListener(OnContinuarLeccionClicked);
            }
        }

        private void OnEnable()
        {
            RefrescarInicio();
        }

        public void RefrescarInicio()
        {
            var perfil = PerfilRepository.CargarPerfil();
            string nombre = (!string.IsNullOrEmpty(perfil?.nombre)) ? perfil.nombre : "Estudiante";

            if (txtSaludoUsuario != null)
            {
                txtSaludoUsuario.text = $"¡Hola, {nombre}!";
            }

            var progreso = ProgresoRepository.CargarProgreso();
            int completadas = progreso != null ? progreso.TotalLeccionesCompletadas : 0;
            int raDesbloqueados = progreso != null ? progreso.TotalEscenariosVisitados : 0;

            if (txtContadorLecciones != null)
                txtContadorLecciones.text = $"{completadas} / 27";

            if (txtContadorRA != null)
                txtContadorRA.text = $"{raDesbloqueados} / 6";

            // Determinar siguiente lección
            int siguienteNumero = Mathf.Clamp(completadas + 1, 1, 27);
            _leccionSiguienteId = $"L{siguienteNumero:D2}";

            var leccionData = CorpusRepository.ObtenerLeccionPorId(_leccionSiguienteId);
            string tituloLeccion = leccionData != null ? leccionData.titulo : $"Lección {siguienteNumero}";

            if (txtTituloContinuar != null)
                txtTituloContinuar.text = $"Continuar: {_leccionSiguienteId}";

            if (txtSubtituloContinuar != null)
                txtSubtituloContinuar.text = tituloLeccion;
        }

        private void OnContinuarLeccionClicked()
        {
            AudioService.Instance?.ReproducirEfecto("click_boton");
            NavigationService.IrALeccion(_leccionSiguienteId);
        }
    }
}
