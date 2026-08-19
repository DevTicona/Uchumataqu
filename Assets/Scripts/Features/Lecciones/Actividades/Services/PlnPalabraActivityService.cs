using System.Collections.Generic;
using Assets.Scripts.Features.Lecciones.Models;

namespace Assets.Scripts.Features.Lecciones.Actividades.Services
{
    /// <summary>
    /// Servicio para la actividad "pln_palabra".
    /// Gestiona la lógica de palabras y evaluación.
    /// </summary>
    public static class PlnPalabraActivityService
    {
        /// <summary>
        /// Valida si una palabra ha sido completada correctamente.
        /// Actualmente es un stub que siempre devuelve true.
        /// </summary>
        public static bool ValidarPronunciacion(PalabraItemData palabra)
        {
            // TODO: Integrar con sistema PLN real
            // Por ahora siempre acepta como correcto
            return true;
        }

        /// <summary>
        /// Obtiene el feedback basado en la evaluación del PLN.
        /// </summary>
        public static string ObtenerFeedback(bool esCorrect)
        {
            return esCorrect 
                ? "¡Correcto!" 
                : "Intenta de nuevo.";
        }

        /// <summary>
        /// Obtiene el número de intentos restantes para una palabra.
        /// </summary>
        public static int ObtenerIntentosRestantes(PalabraItemData palabra)
        {
            // TODO: Implementar lógica de intentos si es necesaria
            return 999; // Intentos ilimitados por ahora
        }
    }
}
