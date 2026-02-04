using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Activities;
using Assets.Scripts.Views.Activities;

namespace Assets.Scripts.Presenters.Activities
{
    /// <summary>
    /// Presenter de la actividad Selección Simple.
    /// Controla la lógica de la pregunta, evaluación y finalización.
    /// </summary>
    public class SeleccionSimplePresenter : ActividadPresenterBase
    {
        [Header("Vista")]
        [SerializeField] private SeleccionSimpleView vista;

        [Header("Timing")]
        [SerializeField] private float tiempoFeedbackExito = 2f;

        private ActividadData actividadData;
        private string textoRespuestaCorrecta;
        private bool respuestaCorrectaConfirmada;

        // =========================
        // Inicialización
        // =========================
        public override void IniciarActividad(ActividadData data)
        {
            if (vista == null)
            {
                Debug.LogError("[SeleccionSimplePresenter] Vista no asignada.");
                return;
            }

            if (data == null || data.palabrasCorrectas == null || data.palabrasCorrectas.Count == 0)
            {
                Debug.LogError("[SeleccionSimplePresenter] ActividadData inválida.");
                return;
            }

            actividadData = data;
            respuestaCorrectaConfirmada = false;

            // Suscripción segura
            vista.AlSeleccionarOpcion -= EvaluarRespuesta;
            vista.AlSeleccionarOpcion += EvaluarRespuesta;

            List<string> opciones = GenerarOpciones();

            vista.Renderizar(
                ObtenerTextoPregunta(),
                ObtenerTextoPalabra(),
                "Selecciona la opción correcta",
                opciones
            );
        }

        private void OnDestroy()
        {
            if (vista != null)
            {
                vista.AlSeleccionarOpcion -= EvaluarRespuesta;
            }
        }

        // =========================
        // Generación de opciones
        // =========================
        private List<string> GenerarOpciones()
        {
            var opciones = new List<string>();
            PalabraData palabraCorrecta = actividadData.palabrasCorrectas[0];

            if (actividadData.direccion == DireccionPregunta.UchumataquAEspanol)
            {
                textoRespuestaCorrecta = palabraCorrecta.espanol;
                opciones.Add(textoRespuestaCorrecta);
                opciones.AddRange(actividadData.palabrasIncorrectas.Select(p => p.espanol));
            }
            else
            {
                textoRespuestaCorrecta = palabraCorrecta.uchumataqu;
                opciones.Add(textoRespuestaCorrecta);
                opciones.AddRange(actividadData.palabrasIncorrectas.Select(p => p.uchumataqu));
            }

            // Mezcla aleatoria
            return opciones.OrderBy(_ => UnityEngine.Random.value).ToList();
        }

        // =========================
        // Evaluación de respuesta
        // =========================
        private void EvaluarRespuesta(int indiceSeleccionado)
        {
            if (respuestaCorrectaConfirmada)
                return;

            string textoSeleccionado = vista.ObtenerTextoOpcion(indiceSeleccionado);

            if (textoSeleccionado == textoRespuestaCorrecta)
            {
                respuestaCorrectaConfirmada = true;
                vista.MostrarFeedbackCorrecto(indiceSeleccionado);
                Invoke(nameof(FinalizarActividad), tiempoFeedbackExito);
            }
            else
            {
                vista.MostrarFeedbackIncorrecto(indiceSeleccionado);
            }
        }

        private void FinalizarActividad()
        {
            NotificarFinalizacion(new List<PalabraData>(actividadData.palabrasCorrectas));
        }

        // =========================
        // Textos dinámicos
        // =========================
        private string ObtenerTextoPregunta()
        {
            return actividadData.direccion == DireccionPregunta.UchumataquAEspanol
                ? "¿Cómo se dice en español?"
                : "¿Cómo se dice en uchumataqu?";
        }

        private string ObtenerTextoPalabra()
        {
            PalabraData palabra = actividadData.palabrasCorrectas[0];

            return actividadData.direccion == DireccionPregunta.UchumataquAEspanol
                ? palabra.uchumataqu
                : palabra.espanol;
        }
    }
}
