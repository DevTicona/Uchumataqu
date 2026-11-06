using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.Core.Services
{
    /// <summary>
    /// Servicio centralizado de navegación entre las 5 escenas oficiales.
    /// Especificado en Docs/UI/00-arquitectura-ui-ux-general.md y 07-navegacion-y-estado-ui.md.
    /// </summary>
    public static class NavigationService
    {
        public const string SCENE_BOOTSTRAP = "00_Bootstrap";
        public const string SCENE_ONBOARDING = "01_Onboarding";
        public const string SCENE_MAIN = "02_Main";
        public const string SCENE_LECCION_BASE = "03_LeccionBase";
        public const string SCENE_REALIDAD_AUMENTADA = "04_RealidadAumentada";

        // Fallbacks para compatibilidad con escenas creadas anteriormente
        private static string ResolverNombreEscena(string escenaObjetivo)
        {
            if (Application.CanStreamedLevelBeLoaded(escenaObjetivo))
                return escenaObjetivo;

            // Mapeo a nombres anteriores si la numeración 00_ no está añadida a Build Settings todavía
            switch (escenaObjetivo)
            {
                case SCENE_BOOTSTRAP:
                    if (Application.CanStreamedLevelBeLoaded("Bootstrap")) return "Bootstrap";
                    break;
                case SCENE_ONBOARDING:
                    if (Application.CanStreamedLevelBeLoaded("Onboarding")) return "Onboarding";
                    break;
                case SCENE_MAIN:
                    if (Application.CanStreamedLevelBeLoaded("Main")) return "Main";
                    if (Application.CanStreamedLevelBeLoaded("MenuPrincipal")) return "MenuPrincipal";
                    break;
                case SCENE_LECCION_BASE:
                    if (Application.CanStreamedLevelBeLoaded("LeccionBase")) return "LeccionBase";
                    break;
                case SCENE_REALIDAD_AUMENTADA:
                    if (Application.CanStreamedLevelBeLoaded("RealidadAumentada")) return "RealidadAumentada";
                    break;
            }

            return escenaObjetivo;
        }

        private static bool _cargando = false;

        private static void CargarEscena(string nombreEscena, Action prepararContexto = null)
        {
            if (_cargando) return;
            string escenaFinal = ResolverNombreEscena(nombreEscena);
            if (!Application.CanStreamedLevelBeLoaded(escenaFinal))
            {
                Debug.LogError($"[NavigationService] La escena '{escenaFinal}' no está disponible en la compilación.");
                return;
            }

            _cargando = true;
            try
            {
                prepararContexto?.Invoke();
                Debug.Log($"[NavigationService] Cargando escena: {escenaFinal} (Objetivo: {nombreEscena})");
                var operacion = SceneManager.LoadSceneAsync(escenaFinal);
                if (operacion != null) operacion.completed += _ => _cargando = false;
                else _cargando = false;
            }
            catch (Exception exception)
            {
                _cargando = false;
                Debug.LogError($"[NavigationService] No se pudo cargar '{escenaFinal}': {exception.Message}");
            }
        }

        public static void IrABootstrap()
        {
            CargarEscena(SCENE_BOOTSTRAP);
        }

        public static void IrAOnboarding()
        {
            CargarEscena(SCENE_ONBOARDING);
        }

        public static void IrAMain(PaginaMain pagina = PaginaMain.Inicio)
        {
            CargarEscena(SCENE_MAIN, () => ContextoNavegacionModel.PaginaDestino = pagina);
        }

        public static void IrALeccion(string leccionId)
        {
            if (string.IsNullOrEmpty(leccionId)) return;
            if (!AppConfigRepository.AccesoLibrePruebas &&
                !GestorProgreso.EstaLeccionDesbloqueada(leccionId)) return;
            CargarEscena(SCENE_LECCION_BASE, () => ContextoNavegacionModel.LeccionId = leccionId);
        }

        public static void IrARealidadAumentada(string escenarioRaId)
        {
            if (string.IsNullOrEmpty(escenarioRaId)) return;
            if (!AppConfigRepository.AccesoLibrePruebas &&
                !GestorProgreso.EstaEscenarioDesbloqueado(escenarioRaId)) return;
            CargarEscena(SCENE_REALIDAD_AUMENTADA, () => ContextoNavegacionModel.EscenarioRaId = escenarioRaId);
        }
    }
}
