using System;
using UnityEngine;

namespace Assets.Scripts.UI.Main.Paginas
{
    [CreateAssetMenu(menuName = "Uchumataqu/UI/Catálogo de iconos de logros")]
    public sealed class CatalogoIconosLogros : ScriptableObject
    {
        [Serializable]
        private struct Entrada
        {
            public string id;
            public Sprite sprite;
        }

        [SerializeField] private Entrada[] iconos = Array.Empty<Entrada>();

        public Sprite ObtenerSprite(string id)
        {
            foreach (Entrada entrada in iconos)
                if (string.Equals(entrada.id, id, StringComparison.Ordinal)) return entrada.sprite;
            return null;
        }
    }
}
