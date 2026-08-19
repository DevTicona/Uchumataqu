using UnityEngine;
using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Lecciones.Models;
using Assets.Scripts.Features.Lecciones.Actividades.Views;
using Assets.Scripts.Features.Lecciones.Actividades.Presenters;

namespace Assets.Scripts.Features.Lecciones.Actividades.Base
{
    /// <summary>
    /// Contenedor del prefab pln_palabra que facilita la configuración e inicialización.
    /// </summary>
    public class PlnPalabraActivityContainer : MonoBehaviour
    {
        private PlnPalabraActivityPresenter presenter;
        private PlnPalabraActivityView view;

        private void OnEnable()
        {
            // Obtener componentes al iniciar
            presenter = GetComponent<PlnPalabraActivityPresenter>();
            view = GetComponent<PlnPalabraActivityView>();

            if (presenter == null)
                Debug.LogError("[PlnPalabraActivityContainer] PlnPalabraActivityPresenter no encontrado en el prefab.");
            if (view == null)
                Debug.LogError("[PlnPalabraActivityContainer] PlnPalabraActivityView no encontrada en el prefab.");
        }

        /// <summary>
        /// Configura y inicia la actividad con las palabras proporcionadas.
        /// </summary>
        public void IniciarActividad(List<PalabraItemData> palabras, Action onCompletada)
        {
            if (presenter != null)
            {
                presenter.Configurar(palabras, onCompletada);
            }
            else
            {
                Debug.LogError("[PlnPalabraActivityContainer] No se puede iniciar sin Presenter.");
            }
        }
    }
}
