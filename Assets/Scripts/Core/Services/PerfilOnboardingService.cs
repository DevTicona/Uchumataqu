using Assets.Scripts.Data.Persistencia;

namespace Assets.Scripts.Core.Services
{
    public readonly struct ResultadoValidacionPerfil
    {
        public bool Valido { get; }
        public string Campo { get; }
        public string Mensaje { get; }

        private ResultadoValidacionPerfil(bool valido, string campo, string mensaje)
        {
            Valido = valido;
            Campo = campo;
            Mensaje = mensaje;
        }

        public static ResultadoValidacionPerfil Exito() => new ResultadoValidacionPerfil(true, null, null);
        public static ResultadoValidacionPerfil Error(string campo, string mensaje) =>
            new ResultadoValidacionPerfil(false, campo, mensaje);
    }

    public static class PerfilOnboardingService
    {
        public static ResultadoValidacionPerfil ValidarDatos(string nombre, string rangoEdad)
        {
            string nombreLimpio = nombre?.Trim() ?? string.Empty;
            if (nombreLimpio.Length < 2)
                return ResultadoValidacionPerfil.Error("nombre", "Escribe un nombre de al menos 2 caracteres.");
            if (nombreLimpio.Length > 30)
                return ResultadoValidacionPerfil.Error("nombre", "El nombre no puede superar 30 caracteres.");
            if (string.IsNullOrWhiteSpace(rangoEdad))
                return ResultadoValidacionPerfil.Error("edad", "Selecciona un rango de edad.");
            return ResultadoValidacionPerfil.Exito();
        }

        public static ResultadoValidacionPerfil ValidarAvatar(string avatarId) =>
            string.IsNullOrWhiteSpace(avatarId)
                ? ResultadoValidacionPerfil.Error("avatar", "Selecciona un avatar para continuar.")
                : ResultadoValidacionPerfil.Exito();

        public static ResultadoValidacionPerfil ValidarPerfilCompleto(
            string nombre,
            string rangoEdad,
            string avatarId)
        {
            ResultadoValidacionPerfil datos = ValidarDatos(nombre, rangoEdad);
            return datos.Valido ? ValidarAvatar(avatarId) : datos;
        }

        public static ResultadoValidacionPerfil AplicarEdicion(
            PerfilUsuario perfil,
            string nombre,
            string rangoEdad,
            string avatarId)
        {
            if (perfil == null)
                return ResultadoValidacionPerfil.Error("perfil", "No se pudo cargar el perfil.");

            ResultadoValidacionPerfil validacion = ValidarPerfilCompleto(nombre, rangoEdad, avatarId);
            if (!validacion.Valido) return validacion;

            perfil.nombre = nombre.Trim();
            perfil.edad = 0;
            perfil.rango_edad = rangoEdad;
            perfil.avatar_id = avatarId;
            return ResultadoValidacionPerfil.Exito();
        }
    }
}
