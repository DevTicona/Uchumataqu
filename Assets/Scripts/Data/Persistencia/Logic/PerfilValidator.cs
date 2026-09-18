using System;
using System.Collections.Generic;
using Assets.Scripts.Data.Persistencia;

namespace Assets.Scripts.Data.Persistencia.Validation
{

    public static class PerfilValidator
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

        public static ResultadoValidacionPerfilModel ValidarNombre(string nombre)
        {
            string nombreLimpio = nombre?.Trim() ?? string.Empty;
            if (nombreLimpio.Length < 2)
                return ResultadoValidacionPerfilModel.Error("nombre", "Escribe un nombre de al menos 2 caracteres.");
            if (nombreLimpio.Length > 30)
                return ResultadoValidacionPerfilModel.Error("nombre", "El nombre no puede superar 30 caracteres.");
            return ResultadoValidacionPerfilModel.Exito();
        }

        public static ResultadoValidacionPerfilModel ValidarAvatar(string avatarId) =>
            !AvatarIds.Contains(avatarId ?? string.Empty)
                ? ResultadoValidacionPerfilModel.Error("avatar", "Selecciona un avatar para continuar.")
                : ResultadoValidacionPerfilModel.Exito();

        public static ResultadoValidacionPerfilModel ValidarPerfilCompleto(
            string nombre,
            string avatarId)
        {
            ResultadoValidacionPerfilModel nombreValido = ValidarNombre(nombre);
            return nombreValido.Valido ? ValidarAvatar(avatarId) : nombreValido;
        }

        public static ResultadoValidacionPerfilModel AplicarEdicion(
            PerfilUsuarioModel perfil,
            string nombre,
            string avatarId)
        {
            if (perfil == null)
                return ResultadoValidacionPerfilModel.Error("perfil", "No se pudo cargar el perfil.");

            ResultadoValidacionPerfilModel validacion = ValidarPerfilCompleto(nombre, avatarId);
            if (!validacion.Valido) return validacion;

            perfil.nombre = nombre.Trim();
            perfil.avatar_id = avatarId;
            return ResultadoValidacionPerfilModel.Exito();
        }
    }
}
