using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Lecciones.Models;
using Assets.Scripts.Features.Lecciones.Actividades.Presenters;

namespace Assets.Scripts.Features.Lecciones.Views
{
    /// <summary>
    /// View contenedora de la lección. Muestra la fase de exposición (card + PLN)
    /// y delega las actividades de anclaje y producción a sub-views específicas
    /// (que se implementarán en Features/Lecciones/Actividades/).
    /// </summary>
    public class LeccionBaseView : MonoBehaviour
    {
        [Header("Paneles")]
        [SerializeField] private GameObject panelExposicion;
        [SerializeField] private GameObject panelActividad;

        [Header("Exposición — Card de Palabra")]
        [SerializeField] private PalabraCardView palabraCard;

        [Header("Actividades")]
        [SerializeField] private PlnPalabraActivityPresenter plnPalabraPresenter;

        // =========================================
        // EXPOSICIÓN
        // =========================================

        /// <summary>
        /// Muestra la card de la palabra con su audio y el micrófono PLN.
        /// onPLNCompletado se invoca cuando el usuario completa la evaluación.
        /// </summary>
        public void MostrarExposicion(
            PalabraItemData palabra,
            int numeroActual,
            int totalEnBloque,
            Action onPLNCompletado
        )
        {
            MostrarPanel(panelExposicion);

            if (palabraCard != null)
                palabraCard.Configurar(palabra, numeroActual, totalEnBloque, onPLNCompletado);
            else
                Debug.LogError("[LeccionBaseView] PalabraCardView no asignada.");
        }

        // =========================================
        // PLN PALABRA ACTIVIDAD (ACTIVIDAD 1)
        // =========================================

        /// <summary>
        /// Muestra la actividad "pln_palabra" con todas las palabras de la lección.
        /// Esta es la actividad inicial de cada lección para exposición.
        /// </summary>
        public void MostrarActividadPlnPalabra(
            List<PalabraItemData> palabras,
            Action onCompletada
        )
        {
            MostrarPanel(panelActividad);

            if (plnPalabraPresenter != null)
            {
                plnPalabraPresenter.Configurar(palabras, onCompletada);
            }
            else
            {
                Debug.LogError("[LeccionBaseView] PlnPalabraActivityPresenter no asignado.");
                onCompletada?.Invoke();
            }
        }

        // =========================================
        // ANCLAJE
        // =========================================

        /// <summary>
        /// Muestra la actividad de anclaje correspondiente al ID recibido.
        /// Las sub-views de actividades se implementan en Actividades/Views/.
        /// </summary>
        public void MostrarActividadAnclaje(
            string idActividad,
            List<PalabraItemData> palabrasBloque,
            Action onCompletada
        )
        {
            MostrarPanel(panelActividad);

            // TODO: usar ActivityFactory para instanciar la actividad correcta
            // por ahora se deja el stub para que el Presenter pueda compilar.
            Debug.Log($"[LeccionBaseView] Actividad anclaje: {idActividad} — pendiente de implementar.");

            // Temporal: completar automáticamente para poder probar el flujo
            // Eliminar cuando las actividades estén implementadas.
            onCompletada?.Invoke();
        }

        // =========================================
        // PRODUCCIÓN
        // =========================================

        /// <summary>
        /// Muestra la actividad de producción correspondiente al ID recibido.
        /// </summary>
        public void MostrarActividadProduccion(
            string idActividad,
            List<OracionItemData> oraciones,
            Action onCompletada
        )
        {
            MostrarPanel(panelActividad);

            // TODO: usar ActivityFactory para instanciar la actividad correcta.
            Debug.Log($"[LeccionBaseView] Actividad producción: {idActividad} — pendiente de implementar.");

            // Temporal: igual que arriba.
            onCompletada?.Invoke();
        }

        // =========================================
        // UTILIDADES
        // =========================================

        private void MostrarPanel(GameObject panel)
        {
            if (panelExposicion != null) panelExposicion.SetActive(false);
            if (panelActividad  != null) panelActividad.SetActive(false);
            if (panel           != null) panel.SetActive(true);
        }
    }
}