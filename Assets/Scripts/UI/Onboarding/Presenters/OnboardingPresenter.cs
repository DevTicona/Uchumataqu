using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.Data.Persistencia.Validation;
using UnityEngine;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.Sistema;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.UI.Onboarding
{
    public class OnboardingPresenter : MonoBehaviour
    {
        [SerializeField] private OnboardingView view;
        [SerializeField] private NavegacionAtrasView backHandler;

        private string _avatarSeleccionado;

        private void Start()
        {
            if (view == null || backHandler == null)
            {
                Debug.LogError("[OnboardingPresenter] Faltan referencias de composición.");
                enabled = false;
                return;
            }

            view.ConfigurarAvatares(PerfilValidator.AvataresDisponibles);
            view.OnComenzarClick += CompletarOnboarding;
            view.OnContinuarBienvenida += MostrarFormulario;
            view.OnAvatarSeleccionado += SeleccionarAvatar;
            backHandler.Configurar(Salir);
            view.MostrarBienvenida();
        }

        private void OnDestroy()
        {
            if (backHandler != null) backHandler.Configurar(null);
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
            ResultadoValidacionPerfilModel validacion =
                PerfilValidator.ValidarPerfilCompleto(nombre, _avatarSeleccionado);
            if (!validacion.Valido)
            {
                if (validacion.Campo == "avatar")
                    view.MostrarErrorAvatar(validacion.Mensaje);
                else
                    view.MostrarErrorPerfil(validacion.Mensaje);
                return;
            }

            bool guardado = GestorPerfil.CrearOActualizar(
                nombre,
                _avatarSeleccionado,
                onboardingCompletado: true);
            if (!guardado) return;
            NavigationService.IrAMain();
        }

        private static void Salir() => Application.Quit();
    }
}
