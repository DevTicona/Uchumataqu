using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.UI.Avatares
{

    /// <summary>
    /// Catálogo centralizado de avatares fijos del proyecto.
    /// Especificado en Docs/UI/reglas_inspector_vs_addressables.md y Docs/instrucciones_refactor_recursos_ui.md.
    /// </summary>
    [CreateAssetMenu(fileName = "CatalogoAvatares", menuName = "Uchumataqu/UI/Catalogo Avatares")]
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.UI.Avatares", sourceAssembly: "Assembly-CSharp", sourceClassName: "CatalogoAvatares")]
    public class CatalogoAvataresModel : ScriptableObject
    {
        [SerializeField] private AvatarVisualModel[] avatares = Array.Empty<AvatarVisualModel>();

        public IReadOnlyList<AvatarVisualModel> Avatares => avatares;

        public Sprite ObtenerSprite(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId)) return null;
            for (int i = 0; i < avatares.Length; i++)
            {
                if (string.Equals(avatares[i].id, avatarId, StringComparison.OrdinalIgnoreCase))
                    return avatares[i].sprite;
            }
            return null;
        }

        public bool Existe(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId)) return false;
            for (int i = 0; i < avatares.Length; i++)
            {
                if (string.Equals(avatares[i].id, avatarId, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
