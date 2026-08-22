using System;
using UnityEngine;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.RA
{
    /// <summary>
    /// Componente interactivo asignado a cada modelo 3D en la escena de Realidad Aumentada.
    /// Al tocar el objeto reproduce audio pedagógico y notifica datos al panel UI.
    /// Especificado en Docs/Specifications/03-modulo-ra.md (Sección 6).
    /// </summary>
    public class InteractorObjetoRA : MonoBehaviour
    {
        [SerializeField] private string palabraId;

        public event Action<PalabraData> OnObjetoTocado;

        public void Configurar(string palabraId)
        {
            this.palabraId = palabraId;
        }

        public async void TocarObjeto()
        {
            if (string.IsNullOrEmpty(palabraId)) return;

            var palabra = CorpusRepository.ObtenerPalabra(palabraId);
            OnObjetoTocado?.Invoke(palabra);

            // Reproducir audio offline por convención Addressables
            await AudioService.Instance.ReproducirPalabraAsync(palabraId);
        }

        private void OnMouseDown()
        {
            TocarObjeto();
        }
    }
}
