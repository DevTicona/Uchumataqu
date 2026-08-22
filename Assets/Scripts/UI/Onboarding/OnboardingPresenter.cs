using UnityEngine;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.UI.Onboarding
{
    /// <summary>
    /// Presenter para la escena 01_Onboarding.
    /// Recopila nombre, rango de edad y avatar, guarda el perfil y navega a Main.
    /// Especificado en Docs/UI/02-escena-onboarding.md.
    /// </summary>
    public class OnboardingPresenter : MonoBehaviour
    {
        [SerializeField] private OnboardingView view;

        private int _pasoActual = 1;

        private void Start()
        {
            if (view == null) view = FindFirstObjectByType<OnboardingView>();

            if (view != null)
            {
                view.OnContinuarClick += AvanzarPaso;
                view.OnAtrasClick += RetrocederPaso;
                view.MostrarPaso(1);
            }
        }

        private void OnDestroy()
        {
            if (view != null)
            {
                view.OnContinuarClick -= AvanzarPaso;
                view.OnAtrasClick -= RetrocederPaso;
            }
        }

        private void AvanzarPaso()
        {
            if (_pasoActual == 1)
            {
                _pasoActual = 2;
                view.MostrarPaso(2);
            }
            else if (_pasoActual == 2)
            {
                string nombre = view.ObtenerNombre();
                if (string.IsNullOrWhiteSpace(nombre))
                {
                    Debug.LogWarning("[OnboardingPresenter] El nombre es obligatorio.");
                    return;
                }

                _pasoActual = 3;
                view.MostrarPaso(3);
            }
            else if (_pasoActual == 3)
            {
                FinalizarOnboarding();
            }
        }

        private void RetrocederPaso()
        {
            if (_pasoActual > 1)
            {
                _pasoActual--;
                view.MostrarPaso(_pasoActual);
            }
        }

        private void FinalizarOnboarding()
        {
            string nombre = view.ObtenerNombre();
            string rangoEdad = view.ObtenerRangoEdad();
            string avatarId = view.ObtenerAvatarId();

            PerfilRepository.CrearOActualizar(nombre, 0, rangoEdad, avatarId, onboardingCompletado: true);
            Debug.Log($"[OnboardingPresenter] Perfil guardado: {nombre} ({rangoEdad}). Navegando a Main...");
            NavigationService.IrAMain();
        }
    }
}
