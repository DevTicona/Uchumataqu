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
    /// Vista de la actividad Selección Simple.
    /// Maneja únicamente UI e interacción del usuario.
    /// </summary>
    public class SeleccionSimpleView : MonoBehaviour
    {
        // =========================
        // UI
        // =========================
        [Header("Textos")]
        [SerializeField] private TextMeshProUGUI txtPregunta;
        [SerializeField] private TextMeshProUGUI txtPalabra;
        [SerializeField] private TextMeshProUGUI txtIndicacion;
        [SerializeField] private TextMeshProUGUI txtFeedback;

        [Header("Opciones")]
        [SerializeField] private Transform contenedorOpciones;

        // =========================
        // Colores
        // =========================
        private readonly Color colorBase = new Color(0.35f, 0.78f, 0.76f);
        private readonly Color colorCorrecto = new Color(0.55f, 0.85f, 0.55f);
        private readonly Color colorIncorrecto = new Color(0.90f, 0.45f, 0.45f);

        public event Action<int> AlSeleccionarOpcion;

        private readonly List<Button> botonesInstanciados = new();

        // =========================
        // Renderizado
        // =========================
        public void Renderizar(
            string textoPregunta,
            string palabra,
            string indicacion,
            List<string> opciones
        )
        {
            txtPregunta.text = textoPregunta;
            txtPalabra.text = palabra;
            txtIndicacion.text = indicacion;

            txtFeedback.text = string.Empty;
            txtFeedback.gameObject.SetActive(false);

            LimpiarOpciones();
            CrearBotones(opciones);
        }

        private void CrearBotones(List<string> opciones)
        {
            for (int i = 0; i < opciones.Count; i++)
            {
                int indice = i;
                StartCoroutine(CrearBotonAsync(opciones[i], indice));
            }
        }

        private IEnumerator CrearBotonAsync(string textoOpcion, int indice)
        {
            bool instanciaCompletada = false;
            GameObject botonGO = null;

            LeccionBaseService.InstanciarBoton(
                contenedorOpciones,
                gameObject =>
                {
                    botonGO = gameObject;
                    instanciaCompletada = true;
                },
                () =>
                {
                    Debug.LogError("[SeleccionSimpleView] No se pudo instanciar botón.");
                    instanciaCompletada = true;
                }
            );

            yield return new WaitUntil(() => instanciaCompletada);

            if (botonGO == null)
                yield break;

            Button boton = botonGO.GetComponent<Button>();
            TextMeshProUGUI textoBoton = botonGO.GetComponentInChildren<TextMeshProUGUI>();

            if (boton == null || textoBoton == null)
            {
                Debug.LogError("[SeleccionSimpleView] Botón instanciado sin componentes necesarios.");
                Destroy(botonGO);
                yield break;
            }

            textoBoton.text = textoOpcion;
            boton.image.color = colorBase;
            boton.interactable = true;

            boton.onClick.AddListener(() =>
                AlSeleccionarOpcion?.Invoke(indice)
            );

            botonesInstanciados.Add(boton);
        }

        // =========================
        // Feedback visual
        // =========================
        public void MostrarFeedbackCorrecto(int indiceCorrecto)
        {
            txtFeedback.gameObject.SetActive(true);
            txtFeedback.text = "¡Correcto!";
            txtFeedback.color = colorCorrecto;

            BloquearBotones();

            if (indiceCorrecto >= 0 && indiceCorrecto < botonesInstanciados.Count)
            {
                botonesInstanciados[indiceCorrecto].image.color = colorCorrecto;
            }
        }

        public void MostrarFeedbackIncorrecto(int indiceSeleccionado)
        {
            txtFeedback.gameObject.SetActive(true);
            txtFeedback.text = "Intenta de nuevo";
            txtFeedback.color = colorIncorrecto;

            if (indiceSeleccionado >= 0 && indiceSeleccionado < botonesInstanciados.Count)
            {
                botonesInstanciados[indiceSeleccionado].image.color = colorIncorrecto;
            }
        }

        // =========================
        // API para el Presenter
        // =========================
        public string ObtenerTextoOpcion(int indice)
        {
            if (indice < 0 || indice >= botonesInstanciados.Count)
                return string.Empty;

            return botonesInstanciados[indice]
                .GetComponentInChildren<TextMeshProUGUI>()
                .text;
        }

        // =========================
        // Utilidades
        // =========================
        private void BloquearBotones()
        {
            foreach (var boton in botonesInstanciados)
            {
                boton.interactable = false;
            }
        }

        private void LimpiarOpciones()
        {
            foreach (var boton in botonesInstanciados)
            {
                Destroy(boton.gameObject);
            }

            botonesInstanciados.Clear();
        }
    }
}
