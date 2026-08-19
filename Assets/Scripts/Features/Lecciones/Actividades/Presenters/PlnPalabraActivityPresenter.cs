using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Lecciones.Models;
using Assets.Scripts.Features.Lecciones.Actividades.Views;

namespace Assets.Scripts.Features.Lecciones.Actividades.Presenters
{
    /// <summary>
    /// Presentador para la actividad "pln_palabra".
    /// Controla la lógica de mostrar palabras y notificar cuando se completa la actividad.
    /// </summary>
    public class PlnPalabraActivityPresenter : MonoBehaviour
    {
        [SerializeField] private PlnPalabraActivityView activityView;

        private List<PalabraItemData> palabras;
        private Action callbackCompletada;

        // =========================================
        // CONFIGURAR
        // =========================================

        public void Configurar(
            List<PalabraItemData> palabrasList,
            Action onCompletada
        )
        {
            palabras = palabrasList;
            callbackCompletada = onCompletada;

            if (activityView != null)
            {
                activityView.Configurar(palabras, OnPalabrasCompletadas);
            }
            else
            {
                Debug.LogError("[PlnPalabraActivityPresenter] PlnPalabraActivityView no asignada.");
            }
        }

        // =========================================
        // CALLBACKS
        // =========================================

        private void OnPalabrasCompletadas(int indiceUltima)
        {
            Debug.Log($"[PlnPalabraActivityPresenter] Actividad pln_palabra completada. Última palabra: {indiceUltima}");
            callbackCompletada?.Invoke();
        }
    }
}
