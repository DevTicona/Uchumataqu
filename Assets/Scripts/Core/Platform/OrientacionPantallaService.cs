using UnityEngine;

namespace Assets.Scripts.Core.Sistema
{
    /// <summary>
    /// Servicio técnico que configura la orientación de pantalla mediante Unity.
    /// Permite cambiar entre modo vertical y horizontal según la escena.
    /// </summary>
    public static class OrientacionPantallaService
    {
        /// <summary>
        /// Configuración inicial de la app (por defecto vertical).
        /// </summary>
        public static void ConfigurarInicial()
        {
            EstablecerVertical();
        }

        /// <summary>
        /// Activa la orientación vertical.
        /// Usado en menús y pantallas normales.
        /// </summary>
        public static void EstablecerVertical()
        {
            Screen.orientation = ScreenOrientation.Portrait;

            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }

        /// <summary>
        /// Activa la orientación horizontal.
        /// Usado en la escena de Realidad Aumentada.
        /// </summary>
        public static void EstablecerHorizontal()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
        }
    }
}