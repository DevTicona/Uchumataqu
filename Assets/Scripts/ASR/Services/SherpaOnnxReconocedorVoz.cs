using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

#if UNITY_ANDROID && !UNITY_EDITOR
        private AndroidJavaObject _reconocedor;
#endif

        public bool EstaDisponible()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                Inicializar();
                return _reconocedor != null;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SherpaOnnx] El reconocedor no esta disponible: {ex.Message}");
                Dispose();
                return false;
            }
#else
            return false;
#endif
        }

        public string TranscribirAudio(AudioClip clip)
        {
            if (clip == null) throw new ArgumentNullException(nameof(clip));

#if UNITY_ANDROID && !UNITY_EDITOR
            Inicializar();
            float[] muestras = ExtraerMono(clip);
            using (AndroidJavaObject stream = _reconocedor.Call<AndroidJavaObject>("createStream"))
            {
                stream.Call("acceptWaveform", muestras, clip.frequency);
                _reconocedor.Call("decode", stream);
                using (AndroidJavaObject resultado = _reconocedor.Call<AndroidJavaObject>("getResult", stream))
                {
                    string texto = resultado.Call<string>("getText");
                    string[] fonemas = resultado.Call<string[]>("getTokens") ?? Array.Empty<string>();
                    return DecodificadorLexico.Decodificar(fonemas, texto, Resources.Load<TextAsset>("Sherpa/lexicon")?.text);
                }
            }
#else
            throw new PlatformNotSupportedException("Sherpa ONNX esta habilitado en builds Android.");
#endif
        }

        public void Dispose()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_reconocedor == null) return;
            _reconocedor.Call("release");
            _reconocedor.Dispose();
            _reconocedor = null;
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
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
                zipformer.Call("setModel", RutaModelo);
                modelo.Call("setTokens", RutaTokens);
                modelo.Call("setNumThreads", Mathf.Max(1, Mathf.Min(4, SystemInfo.processorCount / 2)));
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
