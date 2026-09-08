using System;
using System.Collections.Generic;
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
        private static readonly string[] Avatares =
        {
            "avatar_01",
            "avatar_02",
            "avatar_03",
            "avatar_04",
            "avatar_05",
            "avatar_06"
        };

        private static readonly HashSet<string> AvatarIds =
            new HashSet<string>(Avatares, StringComparer.Ordinal);

        public static IReadOnlyList<string> AvataresDisponibles { get; } = Array.AsReadOnly(Avatares);

        public static ResultadoValidacionPerfil ValidarNombre(string nombre)
        {
            string nombreLimpio = nombre?.Trim() ?? string.Empty;
            if (nombreLimpio.Length < 2)
                return ResultadoValidacionPerfil.Error("nombre", "Escribe un nombre de al menos 2 caracteres.");
            if (nombreLimpio.Length > 30)
                return ResultadoValidacionPerfil.Error("nombre", "El nombre no puede superar 30 caracteres.");
            return ResultadoValidacionPerfil.Exito();
        }

        public static ResultadoValidacionPerfil ValidarAvatar(string avatarId) =>
            !AvatarIds.Contains(avatarId ?? string.Empty)
                ? ResultadoValidacionPerfil.Error("avatar", "Selecciona un avatar para continuar.")
                : ResultadoValidacionPerfil.Exito();

        public static ResultadoValidacionPerfil ValidarPerfilCompleto(
            string nombre,
            string avatarId)
        {
            ResultadoValidacionPerfil nombreValido = ValidarNombre(nombre);
            return nombreValido.Valido ? ValidarAvatar(avatarId) : nombreValido;
        }

        public static ResultadoValidacionPerfil AplicarEdicion(
            PerfilUsuario perfil,
            string nombre,
            string avatarId)
        {
            if (perfil == null)
                return ResultadoValidacionPerfil.Error("perfil", "No se pudo cargar el perfil.");

            ResultadoValidacionPerfil validacion = ValidarPerfilCompleto(nombre, avatarId);
            if (!validacion.Valido) return validacion;

            perfil.nombre = nombre.Trim();
            perfil.avatar_id = avatarId;
            return ResultadoValidacionPerfil.Exito();
        }
    }
}
