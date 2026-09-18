using System;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;

namespace Assets.Scripts.UI.Main
{
    public static class IslaSpriteRepository
    {
        public static async Task<Sprite> ObtenerAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Falta el identificador de la isla.", nameof(id));
            Sprite sprite = await AddressablesService.CargarAssetAsync<Sprite>(ContentAddress.Isla(id));
            if (sprite == null) throw new InvalidOperationException($"No se encontró el sprite de la isla '{id}'.");
            return sprite;
        }
    }
}
