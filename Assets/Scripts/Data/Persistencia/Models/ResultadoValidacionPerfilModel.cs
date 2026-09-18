using System;
using System.Collections.Generic;
using Assets.Scripts.Data.Persistencia;

namespace Assets.Scripts.Data.Persistencia
{
    public readonly struct ResultadoValidacionPerfilModel
    {
        public bool Valido { get; }
        public string Campo { get; }
        public string Mensaje { get; }

        private ResultadoValidacionPerfilModel(bool valido, string campo, string mensaje)
        {
            Valido = valido;
            Campo = campo;
            Mensaje = mensaje;
        }

        public static ResultadoValidacionPerfilModel Exito() => new ResultadoValidacionPerfilModel(true, null, null);
        public static ResultadoValidacionPerfilModel Error(string campo, string mensaje) =>
            new ResultadoValidacionPerfilModel(false, campo, mensaje);
    }
}
