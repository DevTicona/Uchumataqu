using UnityEngine;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.Sistema;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.UI.Onboarding
{
    public class OnboardingPresenter : MonoBehaviour
    {
        [SerializeField] private OnboardingView view;
        [SerializeField] private NavigationBackHandler backHandler;

        private string _avatarSeleccionado;

        private void Start()
        {
            if (view == null || backHandler == null)
            {
                Debug.LogError("[OnboardingPresenter] Faltan referencias de composición.");
                enabled = false;
                return;
            }

            view.ConfigurarAvatares(PerfilOnboardingService.AvataresDisponibles);
            view.OnComenzarClick += CompletarOnboarding;
            view.OnContinuarBienvenida += MostrarFormulario;
            view.OnAvatarSeleccionado += SeleccionarAvatar;
            backHandler.Configurar(Salir);
            view.MostrarBienvenida();
        }

        private void OnDestroy()
        {
            if (view == null) return;
            view.OnComenzarClick -= CompletarOnboarding;
            view.OnContinuarBienvenida -= MostrarFormulario;
            view.OnAvatarSeleccionado -= SeleccionarAvatar;
        }

        private void MostrarFormulario() => view.MostrarFormulario();

        private void SeleccionarAvatar(string avatarId)
        {
            _avatarSeleccionado = avatarId;
            view.MostrarAvatarSeleccionado(avatarId);
        }

        private void CompletarOnboarding()
        {
            string nombre = view.ObtenerNombre();
            ResultadoValidacionPerfil validacion =
                PerfilOnboardingService.ValidarPerfilCompleto(nombre, _avatarSeleccionado);
            if (!validacion.Valido)
            {
                if (validacion.Campo == "avatar")
                    view.MostrarErrorAvatar(validacion.Mensaje);
                else
                    view.MostrarErrorPerfil(validacion.Mensaje);
                return;
            }

            PerfilRepository.CrearOActualizar(
                nombre,
                _avatarSeleccionado,
                onboardingCompletado: true);
            NavigationService.IrAMain();
        }

        private static void Salir() => Application.Quit();
    }
}
