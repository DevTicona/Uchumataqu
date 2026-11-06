using System;
using Assets.Scripts.Core.UI.Theme;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public enum IconoRA { Camara, Colocar, Enfocar, Escanear, Mision, Completada, Mover, Objetivo, PermisoCamara, RealidadAumentada, Recolocar, Rotar, SuperficieDetectada, Hablar }

    [Serializable]
    public struct RecursoIconoRA
    {
        public IconoRA tipo;
        public Sprite sprite;
    }

    public sealed class IconografiaRAView : MonoBehaviour
    {
        [SerializeField] private RecursoIconoRA[] recursos;
        [SerializeField] private Image estadoEscaneo;
        [SerializeField] private Image guiaCentro;

        public Sprite Obtener(IconoRA tipo)
        {
            foreach (var recurso in recursos)
                if (recurso.tipo == tipo) return recurso.sprite;
            return null;
        }

        public void Aplicar(Image imagen, IconoRA tipo)
        {
            if (imagen == null) return;
            imagen.sprite = Obtener(tipo);
            imagen.preserveAspect = true;
            imagen.raycastTarget = false;
        }

        public void MostrarEscaneo(bool superficieValida, bool camaraPreparada = true)
        {
            bool permisoPendiente = false;
#if UNITY_ANDROID && !UNITY_EDITOR
            permisoPendiente = !UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera);
#endif
            Aplicar(estadoEscaneo, permisoPendiente ? IconoRA.PermisoCamara :
                superficieValida ? IconoRA.SuperficieDetectada : camaraPreparada ? IconoRA.Escanear : IconoRA.Camara);
            Aplicar(guiaCentro, superficieValida ? IconoRA.Colocar : IconoRA.Enfocar);
            Color color = superficieValida ? UIThemeController.Theme?.colorExito ?? Color.green : Color.white;
            if (estadoEscaneo != null) estadoEscaneo.color = color;
            if (guiaCentro != null) guiaCentro.color = color;
        }

        public void MostrarEstado(IconoRA tipo)
        {
            Aplicar(estadoEscaneo, tipo);
            if (estadoEscaneo != null) estadoEscaneo.color = Color.white;
        }

    }
}
