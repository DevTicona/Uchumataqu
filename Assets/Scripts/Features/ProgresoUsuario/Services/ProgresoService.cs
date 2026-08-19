using UnityEngine;

namespace Assets.Scripts.Features.ProgresoUsuario.Services
{
    public static class ProgresoService
    {
        private const string PREF_COMPLETADA = "LECCION_COMPLETADA_";

        public static bool EstaCompletada(string idLeccion)
        {
            return PlayerPrefs.GetInt(PREF_COMPLETADA + idLeccion, 0) == 1;
        }

        public static void MarcarCompletada(string idLeccion)
        {
            PlayerPrefs.SetInt(PREF_COMPLETADA + idLeccion, 1);
            PlayerPrefs.Save();
        }
    }
}
