using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Core.Services;

namespace Assets.Scripts.ASR
{
    /// <summary>
    /// Reconocedor offline para el modelo Zipformer2 CTC empaquetado en StreamingAssets.
    /// En Android delega la inferencia al runtime oficial de Sherpa ONNX.
    /// </summary>
    public sealed class SherpaOnnxReconocedorVoz : IReconocedorVoz, IDisposable
    {
        private const string RutaModelo = "sherpa/model.int8.bin";
        private const string RutaTokens = "sherpa/tokens.txt";
        private Task<bool> _preparacion;
        public string UltimoError { get; private set; } = string.Empty;

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject _reconocedor;
        private readonly object _accesoNativo = new object();
        private int _liberacionSolicitada;
        private int _numeroHilos = 2;
#endif

        public bool EstaDisponible()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            IntegridadRecursos.Cargar();
            _numeroHilos = Mathf.Max(1, Mathf.Min(4, SystemInfo.processorCount / 2));
            lock (_accesoNativo) return DisponibleNativo();
#else
            return false;
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private bool DisponibleNativo()
        {
            if (Volatile.Read(ref _liberacionSolicitada) != 0) return false;
            try
            {
                Inicializar();
                UltimoError = string.Empty;
                return _reconocedor != null;
            }
            catch (Exception ex)
            {
                UltimoError = ex.ToString();
                Debug.LogWarning($"[SherpaOnnx] No se pudo inicializar: {UltimoError}");
                LiberarNativo();
                return false;
            }
        }
#endif

        public Task<bool> PrepararAsync()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            IntegridadRecursos.Cargar();
            if (Volatile.Read(ref _liberacionSolicitada) != 0) return Task.FromResult(false);
#endif
            if (_preparacion != null && (!_preparacion.IsCompleted ||
                (_preparacion.Status == TaskStatus.RanToCompletion && _preparacion.Result)))
                return _preparacion;
#if UNITY_ANDROID && !UNITY_EDITOR
            _numeroHilos = Mathf.Max(1, Mathf.Min(4, SystemInfo.processorCount / 2));
            return _preparacion = EnHiloAndroid(DisponibleNativo);
#else
            return _preparacion = Task.FromResult(false);
#endif
        }

        public Task<string> TranscribirAudioAsync(AudioClip clip)
        {
            if (clip == null) throw new ArgumentNullException(nameof(clip));
#if UNITY_ANDROID && !UNITY_EDITOR
            IntegridadRecursos.Cargar();
            // AudioClip se lee en Unity; el trabajador recibe únicamente muestras y frecuencia.
            float[] muestras = ExtraerMono(clip);
            int frecuencia = clip.frequency;
            return EnHiloAndroid(() => TranscribirMuestras(muestras, frecuencia));
#else
            return Task.FromException<string>(new PlatformNotSupportedException("Sherpa ONNX requiere Android."));
#endif
        }

        public string TranscribirAudio(AudioClip clip)
        {
            if (clip == null) throw new ArgumentNullException(nameof(clip));

#if UNITY_ANDROID && !UNITY_EDITOR
            lock (_accesoNativo) return TranscribirMuestras(ExtraerMono(clip), clip.frequency);
#else
            throw new PlatformNotSupportedException("Sherpa ONNX esta habilitado en builds Android.");
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private string TranscribirMuestras(float[] muestras, int frecuencia)
        {
            if (Volatile.Read(ref _liberacionSolicitada) != 0) throw new ObjectDisposedException(nameof(SherpaOnnxReconocedorVoz));
            Inicializar();
            using (AndroidJavaObject stream = _reconocedor.Call<AndroidJavaObject>("createStream"))
            {
                try
                {
                    // Inferencia ASR real en Android: entrega el audio, decodifica con Sherpa ONNX y lee
                    // la secuencia fonética. La respuesta esperada no se entrega al reconocedor.
                    stream.Call("acceptWaveform", muestras, frecuencia);
                    _reconocedor.Call("decode", stream);
                    using (AndroidJavaObject resultado = _reconocedor.Call<AndroidJavaObject>("getResult", stream))
                    {
                        string texto = resultado.Call<string>("getText");
                        return NormalizadorFoneticoAsr.NormalizarPrediccionSherpa(texto);
                    }
                }
                finally { stream.Call("release"); }
            }
        }

        private Task<T> EnHiloAndroid<T>(Func<T> operacion) => Task.Run(() =>
        {
            if (AndroidJNI.AttachCurrentThread() != 0) throw new InvalidOperationException("No se pudo conectar el trabajador ASR a Android.");
            try { lock (_accesoNativo) return operacion(); }
            finally { AndroidJNI.DetachCurrentThread(); }
        });
#endif

        public void Dispose()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (Interlocked.Exchange(ref _liberacionSolicitada, 1) != 0) return;
            if (Monitor.TryEnter(_accesoNativo))
            {
                try { LiberarNativo(); }
                finally { Monitor.Exit(_accesoNativo); }
            }
            else _ = EnHiloAndroid(() => { LiberarNativo(); return true; });
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private void LiberarNativo()
        {
            if (_reconocedor == null) return;
            AndroidJavaObject instancia = _reconocedor;
            _reconocedor = null;
            try { instancia.Call("release"); }
            catch (Exception ex) { Debug.LogWarning($"[SherpaOnnx] No se pudo liberar: {ex.Message}"); }
            finally { instancia.Dispose(); }
        }
        private void Inicializar()
        {
            if (_reconocedor != null) return;

            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (AndroidJavaObject assets = activity.Call<AndroidJavaObject>("getAssets"))
            using (var config = new AndroidJavaObject("com.k2fsa.sherpa.onnx.OfflineRecognizerConfig"))
            using (AndroidJavaObject modelo = config.Call<AndroidJavaObject>("getModelConfig"))
            using (AndroidJavaObject zipformer = modelo.Call<AndroidJavaObject>("getZipformerCtc"))
            using (AndroidJavaObject caracteristicas = config.Call<AndroidJavaObject>("getFeatConfig"))
            {
                // Sherpa termina el proceso nativo si falta un asset (por ejemplo, en un Patch de Unity).
                // Comprobamos el mismo AssetManager antes de entrar al constructor nativo.
                ComprobarAsset(assets, RutaTokens);
                ComprobarAsset(assets, RutaModelo);
                // Modelo local Zipformer CTC: CPU, características de 16 kHz/80 dimensiones y greedy search.
                // La extensión .bin corresponde al recurso ONNX empaquetado; aquí se carga, no se entrena.
                zipformer.Call("setModel", RutaModelo);
                modelo.Call("setTokens", RutaTokens);
                modelo.Call("setNumThreads", _numeroHilos);
                modelo.Call("setProvider", "cpu");
                caracteristicas.Call("setSampleRate", 16000);
                caracteristicas.Call("setFeatureDim", 80);
                config.Call("setDecodingMethod", "greedy_search");
                _reconocedor = new AndroidJavaObject(
                    "com.k2fsa.sherpa.onnx.OfflineRecognizer",
                    assets,
                    config);
            }
        }

        private static void ComprobarAsset(AndroidJavaObject assets, string ruta)
        {
            try
            {
                using (AndroidJavaObject archivo = assets.Call<AndroidJavaObject>("open", ruta))
                    archivo.Call("close");
                IntegridadRecursos.VerificarAssetAndroid(assets, ruta);
            }
            catch (AndroidJavaException ex)
            {
                throw new InvalidOperationException(
                    "Falta el recurso ASR '" + ruta + "' en la instalación. Instala la APK completa; evita Patch para esta compilación.", ex);
            }
        }
#endif

        private static float[] ExtraerMono(AudioClip clip)
        {
            var intercaladas = new float[clip.samples * clip.channels];
            if (!clip.GetData(intercaladas, 0))
                throw new InvalidOperationException("No se pudieron leer las muestras del microfono.");
            if (clip.channels == 1) return intercaladas;

            var mono = new float[clip.samples];
            for (int muestra = 0; muestra < clip.samples; muestra++)
            {
                float suma = 0f;
                int inicio = muestra * clip.channels;
                for (int canal = 0; canal < clip.channels; canal++) suma += intercaladas[inicio + canal];
                mono[muestra] = suma / clip.channels;
            }
            return mono;
        }

    }
}
