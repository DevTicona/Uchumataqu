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

        private int _pasoActual = 1;
        private string _avatarSeleccionado;

        private void Start()
        {
            if (view == null || backHandler == null)
            {
                Debug.LogError("[OnboardingPresenter] Faltan referencias de composición.");
                enabled = false;
                return;
            }

            view.OnContinuarClick += AvanzarPaso;
            view.OnAtrasClick += RetrocederPaso;
            view.OnAvatarSeleccionado += SeleccionarAvatar;
            backHandler.Configurar(RetrocederPaso);
            view.MostrarPaso(1);
        }

        private void OnDestroy()
        {
            if (view == null) return;
            view.OnContinuarClick -= AvanzarPaso;
            view.OnAtrasClick -= RetrocederPaso;
            view.OnAvatarSeleccionado -= SeleccionarAvatar;
        }

        private void AvanzarPaso()
        {
            if (_pasoActual == 1)
            {
                _pasoActual = 2;
                view.MostrarPaso(2);
                return;
            }

            if (_pasoActual == 2)
            {
                ResultadoValidacionPerfil resultado =
                    PerfilOnboardingService.ValidarDatos(view.ObtenerNombre(), view.ObtenerRangoEdad());
                if (!resultado.Valido)
                {
                    view.MostrarErrorPerfil(resultado.Mensaje);
                    return;
                }

                _pasoActual = 3;
                view.MostrarPaso(3);
                return;
            }

            ResultadoValidacionPerfil avatar = PerfilOnboardingService.ValidarAvatar(_avatarSeleccionado);
            if (!avatar.Valido)
            {
                view.MostrarErrorAvatar(avatar.Mensaje);
                return;
            }
            FinalizarOnboarding();
        }

        private void RetrocederPaso()
        {
            if (_pasoActual <= 1)
            {
                Application.Quit();
                return;
            }

            _pasoActual--;
            view.MostrarPaso(_pasoActual);
        }

        private void SeleccionarAvatar(string avatarId)
        {
            _avatarSeleccionado = avatarId;
            view.MostrarAvatarSeleccionado(avatarId);
        }

        private void FinalizarOnboarding()
        {
            string nombre = view.ObtenerNombre();
            string rangoEdad = view.ObtenerRangoEdad();
            ResultadoValidacionPerfil datos = PerfilOnboardingService.ValidarDatos(nombre, rangoEdad);
            if (!datos.Valido)
            {
                _pasoActual = 2;
                view.MostrarPaso(2);
                view.MostrarErrorPerfil(datos.Mensaje);
                return;
            }

            PerfilRepository.CrearOActualizar(
                nombre,
                edad: 0,
                rangoEdad,
                _avatarSeleccionado,
                onboardingCompletado: true);
            NavigationService.IrAMain();
        }
    }
}
