using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Persistencia
{
    public static class AyudaRepository
    {
        private const string Archivo = "AyudasUsuario.json";
        private static Estado cache;

        public static void LimpiarCache() => cache = null;

        [Serializable]
        public sealed class Estado
        {
            public List<string> vistas = new List<string>();
            public bool reducir_movimiento;
            public List<PosicionBoton> posiciones = new List<PosicionBoton>();
        }

        [Serializable]
        public sealed class PosicionBoton
        {
            public string pantalla;
            public Vector2 posicion;
        }

        public static Estado Obtener()
        {
            if (cache != null) return cache;
            try
            {
                var estado = JsonLocalService.Leer<Estado>(Archivo) ?? new Estado();
                estado.vistas ??= new List<string>();
                estado.posiciones ??= new List<PosicionBoton>();
                cache = estado;
                return cache;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Ayuda] No se pudo leer la preferencia: " + ex.Message);
                cache = new Estado();
                return cache;
            }
        }

        public static bool FueVista(string id) => Obtener().vistas.Contains(id);

        public static bool MarcarVista(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) return false;
            var estado = Obtener();
            if (estado.vistas.Contains(id)) return true;
            estado.vistas.Add(id);
            return Guardar(estado);
        }

        public static bool EstablecerMovimientoReducido(bool valor)
        {
            var estado = Obtener();
            estado.reducir_movimiento = valor;
            return Guardar(estado);
        }

        public static bool ObtenerPosicion(string pantalla, out Vector2 posicion)
        {
            var guardada = Obtener().posiciones.Find(p => p != null && p.pantalla == pantalla);
            posicion = guardada?.posicion ?? Vector2.zero;
            return guardada != null && EsFinita(posicion.x) && EsFinita(posicion.y);
        }

        public static bool GuardarPosicion(string pantalla, Vector2 posicion)
        {
            if (string.IsNullOrWhiteSpace(pantalla) || !EsFinita(posicion.x) || !EsFinita(posicion.y)) return false;
            var estado = Obtener();
            var guardada = estado.posiciones.Find(p => p != null && p.pantalla == pantalla);
            if (guardada == null)
            {
                guardada = new PosicionBoton { pantalla = pantalla };
                estado.posiciones.Add(guardada);
            }
            guardada.posicion = new Vector2(Mathf.Clamp01(posicion.x), Mathf.Clamp01(posicion.y));
            return Guardar(estado);
        }

        private static bool EsFinita(float valor) => !float.IsNaN(valor) && !float.IsInfinity(valor);

        private static bool Guardar(Estado estado)
        {
            try
            {
                JsonLocalService.Escribir(Archivo, estado);
                cache = estado;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Ayuda] No se pudo guardar la preferencia: " + ex.Message);
                cache = null;
                return false;
            }
        }
    }
}
