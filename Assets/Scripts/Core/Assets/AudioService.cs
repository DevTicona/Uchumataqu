using System;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Core.Services;

namespace Assets.Scripts.Core.Assets
{
    /// <summary>
    /// Servicio técnico de reproducción de clips y efectos locales mediante AudioSource.
    /// Delega la carga a AddressablesService y recibe el volumen desde la composición de la app.
    /// Especificado en Docs/Specifications/05-guia-corpus-datos.md.
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.Core.Services", sourceAssembly: "Assembly-CSharp", sourceClassName: "AudioService")]
    public class AudioService : MonoBehaviour
    {
        private static AudioService _instance;
        private AudioSource _audioSource;
        private AudioSource _efectosSource;
        private int versionReproduccion;
        private int versionEfecto;
        private float gananciaVoz = 1f;
        private RecursoAddressable<AudioClip> vozCargada;
        private RecursoAddressable<AudioClip> efectoCargado;

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
            _efectosSource = gameObject.AddComponent<AudioSource>();
            _efectosSource.playOnAwake = false;
            _efectosSource.spatialBlend = 0f;
            _efectosSource.volume = 1f;
        }

        public void ConfigurarVolumen(float volumen)
        {
            _audioSource.volume = Mathf.Clamp01(volumen);
        }

        public void ConfigurarVolumenEfectos(float volumen)
        {
            _efectosSource.volume = Mathf.Clamp01(volumen);
        }

        public void ConfigurarGananciaVoz(float ganancia)
        {
            gananciaVoz = Mathf.Clamp(ganancia, 1f, 1.4f);
        }

        public async Task ReproducirPorKeyAsync(string addressableKey)
        {
            if (string.IsNullOrEmpty(addressableKey)) return;

            RecursoAddressable<AudioClip> recurso = null;
            try
            {
                DetenerVoz();
                int version = versionReproduccion;

                recurso = await AddressablesService.CargarTemporalAsync<AudioClip>(addressableKey);
                var clip = recurso.Asset;
                if (this != null && version == versionReproduccion && clip != null && _audioSource != null)
                {
                    vozCargada = recurso;
                    recurso = null;
                    // El clip activo queda identificable para repetir y diagnosticar el estímulo.
                    _audioSource.clip = clip;
                    _audioSource.PlayOneShot(clip, gananciaVoz);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AudioService] No se pudo reproducir audio '{addressableKey}': {ex.Message}");
            }
            finally { recurso?.Dispose(); }
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
            if (!string.IsNullOrWhiteSpace(nombreEfecto))
                _ = CargarEfectoAsync($"sfx_{nombreEfecto}", ++versionEfecto);
        }

        private async Task CargarEfectoAsync(string key, int version)
        {
            RecursoAddressable<AudioClip> recurso = null;
            try
            {
                recurso = await AddressablesService.CargarTemporalAsync<AudioClip>(key);
                if (version == versionEfecto && this != null)
                {
                    ReproducirEfecto(recurso.Asset);
                    efectoCargado = recurso;
                    recurso = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[AudioService] No se pudo reproducir efecto '{key}': {ex.Message}");
            }
            finally { recurso?.Dispose(); }
        }

        public void ReproducirEfecto(AudioClip clip)
        {
            if (clip == null || _efectosSource == null) return;
            versionEfecto++;
            _efectosSource.Stop();
            efectoCargado?.Dispose();
            efectoCargado = null;
            _efectosSource.PlayOneShot(clip);
        }

        public void Detener()
        {
            DetenerVoz();
            versionEfecto++;
            if (_efectosSource != null) _efectosSource.Stop();
            efectoCargado?.Dispose();
            efectoCargado = null;
        }

        private void DetenerVoz()
        {
            versionReproduccion++;

            if (_audioSource != null)
            {
                _audioSource.Stop();
                _audioSource.clip = null;
            }
            vozCargada?.Dispose();
            vozCargada = null;
        }

        private void OnDestroy()
        {
            if (_instance != this) return;
            Detener();
            _instance = null;
        }

        public bool EstaReproduciendo => _audioSource != null && _audioSource.isPlaying;

        public static void DetenerSiExiste()
        {
            if (_instance != null) _instance.Detener();
        }
    }
}

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Convención oficial para resolver claves Addressables de audio pedagógico según el id del corpus.
    /// Especificado en 06-guia-arquitectura-proyecto.md (Sección 2).
    /// </summary>
    public static class AudioAddress
    {
        public static string Palabra(string id) => $"audio_palabra/{id}";
        public static string Oracion(string id) => $"audio_oracion/{id}";
    }
}
