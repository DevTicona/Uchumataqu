#if UNITY_ANDROID && (DEVELOPMENT_BUILD || UCHUMATAQU_VALIDACION) && !UNITY_EDITOR
using System;
using System.IO;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.PLN;
using UnityEngine;

namespace Assets.Scripts.ASR
{
    /// <summary>Prueba del binding y modelo reales, activada explícitamente por un intent de desarrollo.</summary>
    internal static class DiagnosticoAsrAndroid
    {
        private static bool trazaHabilitada;

        [Serializable]
        private sealed class TrazaPronunciacion
        {
            public string esperado;
            public string fonemasEsperados;
            public string prediccion;
            public string textoReconocido;
            public float similitud;
            public bool aprobado;
            public float duracion;
            public float pico;
            public float rms;
        }

        internal static void RegistrarPronunciacion(AudioClip clip, ResultadoEvaluacion resultado)
        {
            if (!trazaHabilitada || clip == null || resultado == null) return;
            var muestras = new float[clip.samples * clip.channels];
            if (!clip.GetData(muestras, 0)) return;
            double suma = 0;
            float pico = 0;
            foreach (float muestra in muestras)
            {
                suma += (double)muestra * muestra;
                pico = Mathf.Max(pico, Mathf.Abs(muestra));
            }
            var traza = new TrazaPronunciacion
            {
                esperado = resultado.textoEsperado,
                fonemasEsperados = NormalizadorFoneticoAsr.ConvertirEsperado(resultado.textoEsperado),
                prediccion = resultado.textoIngresado,
                textoReconocido = resultado.textoReconocido,
                similitud = resultado.porcentajeSimilitud,
                aprobado = resultado.aprobado,
                duracion = clip.length,
                pico = pico,
                rms = muestras.Length > 0 ? (float)Math.Sqrt(suma / muestras.Length) : 0f
            };
            string json = JsonUtility.ToJson(traza);
            File.AppendAllText(Path.Combine(Application.persistentDataPath, "asr-traza.jsonl"), json + Environment.NewLine);
            Debug.Log("[ASR_TRAZA] " + json);
        }

        [Serializable]
        private sealed class Informe
        {
            public bool disponible;
            public string transcripcion;
            public bool evaluado;
            public bool aprobado;
            public float similitud;
            public string error;
            public float segundos;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void EjecutarSiSolicitado()
        {
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
            {
                trazaHabilitada = intent.Call<bool>("getBooleanExtra", "uchumataqu.asr.traza", false);
                intent.Call("removeExtra", "uchumataqu.asr.traza");
                if (!intent.Call<bool>("getBooleanExtra", "uchumataqu.asr.diagnostico", false)) return;
                intent.Call("removeExtra", "uchumataqu.asr.diagnostico");
            }

            var informe = new Informe();
            float inicio = Time.realtimeSinceStartup;
            using (var reconocedor = new SherpaOnnxReconocedorVoz())
            {
                try
                {
                    informe.disponible = await reconocedor.PrepararAsync();
                    if (informe.disponible)
                    {
                        var clip = await AddressablesService.CargarAssetAsync<AudioClip>(AudioAddress.Palabra("wiyantam"));
                        informe.transcripcion = await reconocedor.TranscribirAudioAsync(clip);
                        var resultado = EvaluadorVoz.EvaluarTranscripcion("Wiyantam", informe.transcripcion);
                        informe.evaluado = resultado.evaluado;
                        informe.aprobado = resultado.aprobado;
                        informe.similitud = resultado.porcentajeSimilitud;
                    }
                    else informe.error = reconocedor.UltimoError;
                }
                catch (Exception ex) { informe.error = ex.ToString(); }
            }
            informe.segundos = Time.realtimeSinceStartup - inicio;
            string json = JsonUtility.ToJson(informe, true);
            File.WriteAllText(Path.Combine(Application.persistentDataPath, "asr-diagnostico.json"), json);
            Debug.Log("[ASR_ANDROID_TEST] " + json);
        }
    }
}
#endif
