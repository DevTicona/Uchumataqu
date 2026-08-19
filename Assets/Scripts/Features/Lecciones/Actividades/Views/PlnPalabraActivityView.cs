using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using Assets.Scripts.Features.Lecciones.Models;

namespace Assets.Scripts.Features.Lecciones.Actividades.Views
{
    /// <summary>
    /// Vista para la actividad "pln_palabra": exposición de palabras con evaluación de pronunciación.
    /// Muestra una palabra Uchumataqu, su traducción, audio y permite grabar/evaluar.
    /// </summary>
    public class PlnPalabraActivityView : MonoBehaviour
    {
        [Header("Texto")]
        [SerializeField] private TextMeshProUGUI textoPalabra;
        [SerializeField] private TextMeshProUGUI textoTraduccion;
        [SerializeField] private TextMeshProUGUI textoProgreso;   // "1 / N"

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Button botonAudio;

        [Header("Micrófono PLN")]
        [SerializeField] private Button botonMicrofono;
        [SerializeField] private Image iconoMicrofono;
        [SerializeField] private TextMeshProUGUI textoFeedbackPLN;

        [Header("Colores Feedback")]
        [SerializeField] private Color colorCorrecto = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color colorCasi = new Color(1f, 0.8f, 0f);
        [SerializeField] private Color colorIncorrecto = new Color(0.9f, 0.2f, 0.2f);

        [Header("Botones Navegación")]
        [SerializeField] private Button botonSiguiente;
        [SerializeField] private Button botonAnterior;

        private List<PalabraItemData> palabrasActuales;
        private int indicePalabraActual;
        private Action<int> callbackOnPalabrasCambiadas;
        private bool grabando;

        // =========================================
        // INICIALIZACIÓN
        // =========================================

        public void Configurar(
            List<PalabraItemData> palabras,
            Action<int> onCompleted
        )
        {
            palabrasActuales = palabras;
            callbackOnPalabrasCambiadas = onCompleted;
            indicePalabraActual = 0;
            grabando = false;

            // Configurar botones
            botonAudio?.onClick.RemoveAllListeners();
            botonAudio?.onClick.AddListener(ReproducirAudio);

            botonMicrofono?.onClick.RemoveAllListeners();
            botonMicrofono?.onClick.AddListener(OnMicrofonoPresionado);

            botonSiguiente?.onClick.RemoveAllListeners();
            botonSiguiente?.onClick.AddListener(OnSiguiente);

            botonAnterior?.onClick.RemoveAllListeners();
            botonAnterior?.onClick.AddListener(OnAnterior);

            // Mostrar primera palabra
            MostrarPalabraActual();

            // Reproducir audio automáticamente
            ReproducirAudio();
        }

        // =========================================
        // PALABRAS
        // =========================================

        private void MostrarPalabraActual()
        {
            if (palabrasActuales == null || palabrasActuales.Count == 0)
            {
                Debug.LogError("[PlnPalabraActivityView] Sin palabras para mostrar.");
                return;
            }

            var palabra = palabrasActuales[indicePalabraActual];

            // Mostrar contenido
            if (textoPalabra != null)
                textoPalabra.text = palabra.uch;

            if (textoTraduccion != null)
                textoTraduccion.text = palabra.es;

            if (textoProgreso != null)
                textoProgreso.text = $"{indicePalabraActual + 1} / {palabrasActuales.Count}";

            // Limpiar feedback
            if (textoFeedbackPLN != null)
            {
                textoFeedbackPLN.text = "";
                textoFeedbackPLN.enabled = false;
            }

            // Habilitar micrófono
            SetMicrofonoInteractuable(true);
            grabando = false;

            // Actualizar estado de botones de navegación
            ActualizarBotonesNavegacion();
        }

        private void ActualizarBotonesNavegacion()
        {
            if (botonAnterior != null)
                botonAnterior.interactable = (indicePalabraActual > 0);

            if (botonSiguiente != null)
                botonSiguiente.interactable = (indicePalabraActual < palabrasActuales.Count - 1);
        }

        private void OnAnterior()
        {
            if (indicePalabraActual > 0)
            {
                indicePalabraActual--;
                MostrarPalabraActual();
                ReproducirAudio();
            }
        }

        private void OnSiguiente()
        {
            if (indicePalabraActual < palabrasActuales.Count - 1)
            {
                indicePalabraActual++;
                MostrarPalabraActual();
                ReproducirAudio();
            }
            else
            {
                // Todas las palabras completadas
                NotificarCompletado();
            }
        }

        // =========================================
        // AUDIO
        // =========================================

        private void ReproducirAudio()
        {
            if (audioSource == null) return;

            // TODO: cargar AudioClip desde Addressables usando palabra.id
            // Ejemplo: AddressablesService.CargarAssetAsync<AudioClip>($"audio_{palabrasActuales[indicePalabraActual].id}")

            if (audioSource.clip != null)
                audioSource.Play();
        }

        // =========================================
        // PLN (STUB)
        // =========================================

        private void OnMicrofonoPresionado()
        {
            if (grabando) return;

            grabando = true;
            SetMicrofonoInteractuable(false);

            // Simular grabación y evaluación
            StartCoroutine(SimularEvaluacionPLN());
        }

        private System.Collections.IEnumerator SimularEvaluacionPLN()
        {
            yield return new WaitForSeconds(1.5f);

            // Stub: siempre correcto
            OnResultadoPLN(ResultadoPLN.Correcto);
        }

        private void OnResultadoPLN(ResultadoPLN resultado)
        {
            grabando = false;

            switch (resultado)
            {
                case ResultadoPLN.Correcto:
                    MostrarFeedback("¡Correcto!", colorCorrecto);
                    Invoke(nameof(OnSiguiente), 0.8f);
                    break;

                case ResultadoPLN.Casi:
                    MostrarFeedback("¡Casi! Intenta de nuevo.", colorCasi);
                    SetMicrofonoInteractuable(true);
                    break;

                case ResultadoPLN.Incorrecto:
                    MostrarFeedback("Inténtalo otra vez.", colorIncorrecto);
                    SetMicrofonoInteractuable(true);
                    break;
            }
        }

        private void MostrarFeedback(string mensaje, Color color)
        {
            if (textoFeedbackPLN != null)
            {
                textoFeedbackPLN.text = mensaje;
                textoFeedbackPLN.color = color;
                textoFeedbackPLN.enabled = true;
            }
        }

        private void SetMicrofonoInteractuable(bool interactuable)
        {
            if (botonMicrofono != null)
                botonMicrofono.interactable = interactuable;

            if (iconoMicrofono != null)
            {
                iconoMicrofono.color = interactuable ? Color.white : new Color(0.5f, 0.5f, 0.5f);
            }
        }

        private void NotificarCompletado()
        {
            callbackOnPalabrasCambiadas?.Invoke(indicePalabraActual);
        }
    }

    /// <summary>
    /// Enum para representar el resultado de la evaluación PLN.
    /// </summary>
    public enum ResultadoPLN
    {
        Correcto,
        Casi,
        Incorrecto
    }
}
