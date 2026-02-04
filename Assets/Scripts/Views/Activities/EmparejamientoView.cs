using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Services;

namespace Assets.Scripts.Views.Activities
{
    /// <summary>
    /// Vista de la actividad de Emparejamiento.
    /// Columna izquierda: Español
    /// Columna derecha: Uchumataqu
    /// Solo maneja UI e interacción visual.
    /// </summary>
    public class EmparejamientoView : MonoBehaviour
    {
        // =========================
        // UI (Inspector)
        // =========================
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI txtIndicacion;

        [Header("Columnas")]
        [SerializeField] private Transform contenedorColumnaIzquierda;
        [SerializeField] private Transform contenedorColumnaDerecha;

        [Header("Feedback")]
        [SerializeField] private TextMeshProUGUI txtFeedback;

        [Header("Tiempos")]
        [SerializeField] private float tiempoFeedbackIncorrecto = 1.5f;

        // =========================
        // Colores
        // =========================
        private readonly Color colorBase = new Color(0.35f, 0.78f, 0.76f);
        private readonly Color colorSeleccionado = new Color(0.90f, 0.85f, 0.40f);
        private readonly Color colorCorrecto = new Color(0.55f, 0.85f, 0.55f);
        private readonly Color colorIncorrecto = new Color(0.90f, 0.45f, 0.45f);

        // =========================
        // Eventos
        // =========================
        public event Action<int> AlSeleccionarOpcion;

        // =========================
        // Estado interno
        // =========================
        private readonly List<Button> botonesInstanciados = new();
        private bool esperandoReset = false;

        // =========================
        // Render
        // =========================
        public void Renderizar(
            string indicacion,
            List<string> columnaIzquierda,
            List<string> columnaDerecha
        )
        {
            txtIndicacion.text = indicacion;

            txtFeedback.text = string.Empty;
            txtFeedback.gameObject.SetActive(false);

            LimpiarOpciones();
            esperandoReset = false;

            int indiceGlobal = 0;

            // Columna izquierda (Español)
            for (int i = 0; i < columnaIzquierda.Count; i++)
            {
                CrearBoton(
                    columnaIzquierda[i],
                    contenedorColumnaIzquierda,
                    indiceGlobal
                );
                indiceGlobal++;
            }

            // Columna derecha (Uchumataqu)
            for (int i = 0; i < columnaDerecha.Count; i++)
            {
                CrearBoton(
                    columnaDerecha[i],
                    contenedorColumnaDerecha,
                    indiceGlobal
                );
                indiceGlobal++;
            }
        }

        // =========================
        // Creación de botones
        // =========================
        private void CrearBoton(
            string texto,
            Transform contenedor,
            int indice
        )
        {
            StartCoroutine(CrearBotonAsync(texto, contenedor, indice));
        }

        private IEnumerator CrearBotonAsync(
            string texto,
            Transform contenedor,
            int indice
        )
        {
            bool instanciaCompletada = false;
            GameObject botonGO = null;

            LeccionBaseService.InstanciarBoton(
                contenedor,
                gameObject =>
                {
                    botonGO = gameObject;
                    instanciaCompletada = true;
                },
                () =>
                {
                    Debug.LogError("[EmparejamientoView] No se pudo instanciar botón.");
                    instanciaCompletada = true;
                }
            );

            yield return new WaitUntil(() => instanciaCompletada);

            if (botonGO == null)
                yield break;

            Button boton = botonGO.GetComponent<Button>();
            TextMeshProUGUI txt = botonGO.GetComponentInChildren<TextMeshProUGUI>();

            if (boton == null || txt == null)
            {
                Debug.LogError("[EmparejamientoView] Botón instanciado sin componentes necesarios.");
                Destroy(botonGO);
                yield break;
            }

            txt.text = texto;
            boton.image.color = colorBase;
            boton.interactable = true;

            boton.onClick.AddListener(() =>
            {
                if (!esperandoReset)
                    AlSeleccionarOpcion?.Invoke(indice);
            });

            botonesInstanciados.Add(boton);
        }

        // =========================
        // Feedback visual
        // =========================
        public void MarcarSeleccion(int indice)
        {
            botonesInstanciados[indice].image.color = colorSeleccionado;
        }

        public void MarcarParCorrecto(int indice1, int indice2)
        {
            botonesInstanciados[indice1].image.color = colorCorrecto;
            botonesInstanciados[indice2].image.color = colorCorrecto;

            botonesInstanciados[indice1].interactable = false;
            botonesInstanciados[indice2].interactable = false;

            MostrarFeedback("¡Correcto!", colorCorrecto);
        }

        public void MostrarParIncorrecto(int indice1, int indice2)
        {
            esperandoReset = true;

            botonesInstanciados[indice1].image.color = colorIncorrecto;
            botonesInstanciados[indice2].image.color = colorIncorrecto;

            MostrarFeedback("Incorrecto, intenta de nuevo", colorIncorrecto);

            Invoke(nameof(RestablecerSelecciones), tiempoFeedbackIncorrecto);
        }

        // =========================
        // Helpers
        // =========================
        private void MostrarFeedback(string mensaje, Color color)
        {
            txtFeedback.gameObject.SetActive(true);
            txtFeedback.text = mensaje;
            txtFeedback.color = color;
        }

        private void RestablecerSelecciones()
        {
            foreach (var boton in botonesInstanciados)
            {
                if (boton.interactable)
                    boton.image.color = colorBase;
            }

            txtFeedback.gameObject.SetActive(false);
            esperandoReset = false;
        }

        private void LimpiarOpciones()
        {
            foreach (var boton in botonesInstanciados)
                Destroy(boton.gameObject);

            botonesInstanciados.Clear();
        }
    }
}
