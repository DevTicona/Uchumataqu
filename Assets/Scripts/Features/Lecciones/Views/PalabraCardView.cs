using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Assets.Scripts.Features.Lecciones.Models;

namespace Assets.Scripts.Features.Lecciones.Views
{
    /// <summary>
    /// Card de exposición de una palabra: muestra texto, traducción, reproduce audio
    /// y evalúa pronunciación (PLN stub hasta que el sistema esté implementado).
    /// </summary>
    public class PalabraCardView : MonoBehaviour
    {
        [Header("Texto")]
        [SerializeField] private TextMeshProUGUI textoPalabra;
        [SerializeField] private TextMeshProUGUI textoTraduccion;
        [SerializeField] private TextMeshProUGUI textoProgreso;   // "1 / 4"

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private Button botonAudio;

        [Header("PLN")]
        [SerializeField] private Button botonMicrofono;
        [SerializeField] private Image  iconoMicrofono;
        [SerializeField] private TextMeshProUGUI textoFeedbackPLN;

        [Header("Colores feedback PLN")]
        [SerializeField] private Color colorCorrecto  = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color colorCasi      = new Color(1f,   0.8f, 0f);
        [SerializeField] private Color colorIncorrecto = new Color(0.9f, 0.2f, 0.2f);

        private PalabraItemData palabraActual;
        private Action callbackPLNCompletado;
        private bool grabando;

        // =========================================
        // CONFIGURAR
        // =========================================

        public void Configurar(
            PalabraItemData palabra,
            int numeroActual,
            int totalEnBloque,
            Action onPLNCompletado
        )
        {
            palabraActual        = palabra;
            callbackPLNCompletado = onPLNCompletado;
            grabando             = false;

            // Texto
            if (textoPalabra    != null) textoPalabra.text    = palabra.uch;
            if (textoTraduccion != null) textoTraduccion.text = palabra.es;
            if (textoProgreso   != null) textoProgreso.text   = $"{numeroActual} / {totalEnBloque}";

            // Feedback oculto al inicio
            if (textoFeedbackPLN != null)
            {
                textoFeedbackPLN.text    = "";
                textoFeedbackPLN.enabled = false;
            }

            // Botón audio
            botonAudio?.onClick.RemoveAllListeners();
            botonAudio?.onClick.AddListener(ReproducirAudio);

            // Botón micrófono
            botonMicrofono?.onClick.RemoveAllListeners();
            botonMicrofono?.onClick.AddListener(OnMicrofonoPresionado);
            SetMicrofonoInteractuable(true);

            // Reproducir audio automáticamente al mostrar la card
            ReproducirAudio();
        }

        // =========================================
        // AUDIO
        // =========================================

        private void ReproducirAudio()
        {
            if (audioSource == null) return;

            // TODO: cargar AudioClip desde Addressables usando palabra.id
            // Ejemplo: AddressablesService.CargarAssetAsync<AudioClip>($"audio_{palabraActual.id}")
            // Por ahora no hay clip, el botón queda funcional para cuando se integre.

            if (audioSource.clip != null)
                audioSource.Play();
            else
                Debug.Log($"[PalabraCardView] Sin audio para: {palabraActual?.id}");
        }

        // =========================================
        // PLN (STUB)
        // =========================================

        private void OnMicrofonoPresionado()
        {
            if (grabando) return;

            grabando = true;
            SetMicrofonoInteractuable(false);

            // TODO: reemplazar por llamada real al sistema PLN
            // PronunciacionService.EvaluarAsync(palabraActual.uch, OnResultadoPLN);

            // Stub: simula grabación y siempre aprueba después de 1.5s
            StartCoroutine(SimularEvaluacionPLN());
        }

        private System.Collections.IEnumerator SimularEvaluacionPLN()
        {
            yield return new WaitForSeconds(1.5f);

            // Stub: resultado siempre correcto
            OnResultadoPLN(ResultadoPLN.Correcto);
        }

        /// <summary>
        /// Recibe el resultado del PLN (real o stub) y reacciona.
        /// </summary>
        private void OnResultadoPLN(ResultadoPLN resultado)
        {
            grabando = false;

            switch (resultado)
            {
                case ResultadoPLN.Correcto:
                    MostrarFeedback("¡Correcto!", colorCorrecto);
                    Invoke(nameof(NotificarCompletado), 0.8f);
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

        private void NotificarCompletado()
        {
            callbackPLNCompletado?.Invoke();
        }

        // =========================================
        // UTILIDADES UI
        // =========================================

        private void MostrarFeedback(string mensaje, Color color)
        {
            if (textoFeedbackPLN == null) return;
            textoFeedbackPLN.text    = mensaje;
            textoFeedbackPLN.color   = color;
            textoFeedbackPLN.enabled = true;
        }

        private void SetMicrofonoInteractuable(bool interactuable)
        {
            if (botonMicrofono != null)
                botonMicrofono.interactable = interactuable;

            if (iconoMicrofono != null)
                iconoMicrofono.color = interactuable ? Color.white : Color.gray;
        }
    }

    /// <summary>
    /// Resultado de la evaluación PLN. Usado por el stub y por el sistema real.
    /// </summary>
    public enum ResultadoPLN { Correcto, Casi, Incorrecto }
}