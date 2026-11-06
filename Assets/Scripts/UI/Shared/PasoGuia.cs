using System;
using UnityEngine;

namespace Assets.Scripts.Core.UI.Ayuda
{
    [Serializable]
    public sealed class PasoGuia
    {
        public string identificador;
        public string titulo;
        [TextArea(2, 5)] public string texto;
        public RectTransform objetivo;
        public string nombreObjetivo;

        public PasoGuia(string titulo, string texto, RectTransform objetivo = null)
        {
            this.titulo = titulo;
            this.texto = texto;
            this.objetivo = objetivo;
        }
    }
}
