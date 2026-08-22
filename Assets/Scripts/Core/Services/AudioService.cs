using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Core.Services
{
    /// <summary>
    /// Servicio de reproducción de audio pedagógico local offline.
    /// Resuelve clips mediante Addressables usando AudioAddress.
    /// Especificado en Docs/UI/04-escena-leccion-base.md (Sección 13).
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        private static AudioService _instance;
        private AudioSource _audioSource;

        public static AudioService Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[AudioService]");
                    _instance = go.AddComponent<AudioService>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f; // 2D UI Audio
        }

        public async Task ReproducirPorKeyAsync(string addressableKey)
        {
            if (string.IsNullOrEmpty(addressableKey)) return;

            try
            {
                Detener();
                var clip = await AddressablesService.CargarAssetAsync<AudioClip>(addressableKey);
                if (clip != null && _audioSource != null)
                {
                    _audioSource.clip = clip;
                    var config = AppConfig.Instance;
                    _audioSource.volume = config != null ? config.defaultMasterVolume * config.defaultWordVolume : 1.0f;
                    _audioSource.Play();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AudioService] No se pudo reproducir audio '{addressableKey}': {ex.Message}");
            }
        }

        public async Task ReproducirPalabraAsync(string palabraId)
        {
            await ReproducirPorKeyAsync(AudioAddress.Palabra(palabraId));
        }

        public void ReproducirPalabra(string palabraId)
        {
            _ = ReproducirPalabraAsync(palabraId);
        }

        public async Task ReproducirOracionAsync(string oracionId)
        {
            await ReproducirPorKeyAsync(AudioAddress.Oracion(oracionId));
        }

        public void ReproducirOracion(string oracionId)
        {
            _ = ReproducirOracionAsync(oracionId);
        }

        public void ReproducirEfecto(string nombreEfecto)
        {
            // Efectos de interfaz básicos sin bloquear
            _ = ReproducirPorKeyAsync($"sfx_{nombreEfecto}");
        }

        public void Detener()
        {
            if (_audioSource != null && _audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }

        public bool EstaReproduciendo => _audioSource != null && _audioSource.isPlaying;
    }
}
