using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Activities;

namespace Assets.Scripts.Presenters.Activities
{
    /// <summary>
    /// Clase base que define el contrato que TODAS las actividades deben cumplir.
    /// Ya no buscamos "MonoBehaviour", buscamos "ActividadPresenterBase".
    /// </summary>
    public abstract class ActividadPresenterBase : MonoBehaviour
    {
        // Evento común para todas las actividades
        public event Action<List<PalabraData>> AlFinalizarActividad;

        // Método abstracto: Obliga a los hijos a implementar su propia lógica de inicio
        public abstract void IniciarActividad(ActividadData data);

        // Método helper para que los hijos llamen al finalizar
        protected void NotificarFinalizacion(List<PalabraData> palabrasUsadas)
        {
            AlFinalizarActividad?.Invoke(palabrasUsadas);
        }
    }
}