using Assets.Scripts.Data.Persistencia.Validation;
using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.Util;

namespace Assets.Scripts.Persistencia
{
    public static class GestorPerfil
    {
        public static bool ExistePerfil()
        {
            var perfil = PerfilRepository.ObtenerPerfil();
            return perfil != null &&
                   perfil.onboarding_completado &&
                   PerfilValidator.ValidarPerfilCompleto(perfil.nombre, perfil.avatar_id).Valido;
        }

        public static bool CrearOActualizar(
            string nombre,
            string avatarId,
            bool onboardingCompletado = true)
        {
            if (!PerfilValidator.ValidarPerfilCompleto(nombre, avatarId).Valido) return false;
            var perfil = new PerfilUsuarioModel
            {
                nombre = FormatoTexto.ConInicialMayuscula(nombre.Trim()),
                avatar_id = avatarId,
                onboarding_completado = onboardingCompletado
            };
            return PerfilRepository.GuardarPerfil(perfil);
        }

    }
}
