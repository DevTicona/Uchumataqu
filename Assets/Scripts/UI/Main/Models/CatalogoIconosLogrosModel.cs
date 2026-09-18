using System;
using UnityEngine;

namespace Assets.Scripts.UI.Main
{
    [CreateAssetMenu(menuName = "Uchumataqu/UI/Catálogo de iconos de logros")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.UI.Main.Paginas", sourceAssembly: "Assembly-CSharp", sourceClassName: "CatalogoIconosLogros")]
    public sealed class CatalogoIconosLogrosModel : ScriptableObject
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
