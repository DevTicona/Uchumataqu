#if UNITY_ANDROID && UCHUMATAQU_VALIDACION && !UNITY_EDITOR
using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using Assets.Scripts.ASR;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Persistencia;
using Assets.Scripts.PLN;
using Assets.Scripts.RA;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace Assets.Scripts.Core.Diagnostics
{
    /// <summary>Instrumento optativo compilado únicamente con UCHUMATAQU_VALIDACION.</summary>
    internal sealed class MedicionAndroid : MonoBehaviour
    {
        private static MedicionAndroid instancia;
        private readonly StringBuilder cuadros = new StringBuilder();
        private string carpeta;
        private string modo;
        private long inicio;
        private float ultimoVaciado;
        private int numeroCuadro;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Preparar()
        {
            using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
            {
                string modo = intent.Call<string>("getStringExtra", "uchumataqu.medicion");
                if (string.IsNullOrEmpty(modo)) return;
                instancia = new GameObject("InstrumentoMedicionAndroid").AddComponent<MedicionAndroid>();
                DontDestroyOnLoad(instancia.gameObject);
                instancia.modo = modo;
                instancia.inicio = intent.Call<long>("getLongExtra", "uchumataqu.medicion.inicio_ms", 0L);
                instancia.carpeta = Path.Combine(Application.persistentDataPath, "Mediciones", DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff"));
                Directory.CreateDirectory(instancia.carpeta);
                File.WriteAllText(Path.Combine(instancia.carpeta, "cuadros.csv"), "utc,uptime_ms,escena,escenario,colocado,tracking,cuadro,delta_s\n");
                instancia.Registrar("instrumento", "version=" + Application.version + ";modo=" + modo);
            }
        }

        internal static void PantallaLista()
        {
            if (instancia != null) instancia.StartCoroutine(instancia.PrimerCuadroInteractivo());
        }

        private IEnumerator PrimerCuadroInteractivo()
        {
            yield return new WaitForEndOfFrame();
            long fin = Reloj();
            Registrar("inicio_interactivo", "inicio_ms=" + inicio + ";fin_ms=" + fin + ";duracion_ms=" + (fin - inicio));
            if (modo == "logica") EjecutarLogica();
            if (modo == "asr") EjecutarAsr();
        }

        private void Update()
        {
            // Se registra el intervalo de Unity, separado de cuadros presentados por SurfaceFlinger.
            if (SceneManager.GetActiveScene().name == "04_RealidadAumentada")
            {
                var colocador = FindAnyObjectByType<ColocadorEscenarioRA>();
                var gestor = FindAnyObjectByType<GestorEscenarioRA>();
                cuadros.Append(DateTime.UtcNow.ToString("O")).Append(',').Append(Reloj()).Append(",04_RealidadAumentada,")
                    .Append(gestor?.EscenarioActual?.id).Append(',').Append(colocador != null && colocador.EscenarioColocado)
                    .Append(',').Append(ARSession.state).Append(',').Append(++numeroCuadro).Append(',')
                    .Append(Time.unscaledDeltaTime.ToString("R", CultureInfo.InvariantCulture)).Append('\n');
            }
            if (Time.realtimeSinceStartup - ultimoVaciado >= 2f) Vaciar();
        }

        private void Vaciar()
        {
            if (cuadros.Length > 0)
            {
                File.AppendAllText(Path.Combine(carpeta, "cuadros.csv"), cuadros.ToString());
                cuadros.Clear();
            }
            ultimoVaciado = Time.realtimeSinceStartup;
        }

        private void OnApplicationPause(bool pausa) { if (pausa) Vaciar(); }
        private void OnDestroy() { Vaciar(); }

        private void EjecutarLogica()
        {
            string[] respuestas = { "Palitak", "Palit", "Ataluy" };
            for (int i = 0; i < 20; i++) MotorEvaluacion.EvaluarEscritura("Palitak", respuestas[i % 3]);
            var csv = new StringBuilder("repeticion,esperado,entrada,duracion_ms,aprobado,color\n");
            for (int i = 0; i < 100; i++)
            {
                var reloj = Stopwatch.StartNew();
                var resultado = MotorEvaluacion.EvaluarEscritura("Palitak", respuestas[i % 3]);
                reloj.Stop();
                csv.Append(i + 1).Append(",Palitak,").Append(respuestas[i % 3]).Append(',')
                    .Append(reloj.Elapsed.TotalMilliseconds.ToString("R", CultureInfo.InvariantCulture)).Append(',')
                    .Append(resultado.aprobado).Append(',').Append(resultado.colorSemaforo).Append('\n');
            }
            File.WriteAllText(Path.Combine(carpeta, "escritura.csv"), csv.ToString());
            Registrar("escritura_completa", "n=100;calentamiento=20;alcance=logica_sin_UI_ni_morfologia");
        }

        private async void EjecutarAsr()
        {
            try
            {
                using (var reconocedor = new SherpaOnnxReconocedorVoz())
                {
                    var reloj = Stopwatch.StartNew();
                    bool listo = await reconocedor.PrepararAsync();
                    Registrar("preparar_asr", "duracion_ms=" + reloj.Elapsed.TotalMilliseconds.ToString("R", CultureInfo.InvariantCulture) + ";disponible=" + listo);
                    if (!listo) return;
                    var csv = new StringBuilder("repeticion,referencia,fonemas,prediccion_textual,duracion_ms,duracion_audio_s\n");
                    string[] palabras = { "Wiyantam", "Palitak", "Sitsñi", "Ataluy", "Amthü" };
                    for (int i = 0; i < palabras.Length; i++)
                    {
                        string referencia = palabras[i];
                        var palabra = CorpusRepository.ObtenerTodasLasPalabras().Find(p =>
                            string.Equals(p.uch, referencia, StringComparison.OrdinalIgnoreCase));
                        if (palabra == null) { Registrar("asr_error", "palabra_no_disponible=" + referencia); continue; }
                        string clave = AudioAddress.Palabra(palabra.id);
                        var clip = await AddressablesService.CargarAssetAsync<AudioClip>(clave);
                        if (clip == null) { Registrar("asr_error", "audio_no_disponible=" + clave); continue; }
                        reloj.Restart();
                        string fonemas = await reconocedor.TranscribirAudioAsync(clip);
                        reloj.Stop();
                        // Sin objetivo: evita utilizar la referencia como hipótesis del decodificador textual.
                        string texto = DecodificadorFoneticoAsr.Decodificar(fonemas, null);
                        csv.Append(i + 1).Append(',').Append(referencia).Append(',').Append(fonemas.Replace(',', ' ')).Append(',')
                            .Append(texto.Replace(',', ' ')).Append(',').Append(reloj.Elapsed.TotalMilliseconds.ToString("R", CultureInfo.InvariantCulture))
                            .Append(',').Append(clip.length.ToString("R", CultureInfo.InvariantCulture)).Append('\n');
                        File.WriteAllText(Path.Combine(carpeta, "asr_control.csv"), csv.ToString());
                        AddressablesService.LiberarAsset<AudioClip>(clave);
                    }
                    Registrar("asr_completo", "muestra=audios_empaquetados;no_microfono;no_generalizacion");
                }
            }
            catch (Exception ex) { Registrar("asr_error", ex.ToString().Replace('\n', ' ')); }
        }

        private void Registrar(string evento, string detalle)
        {
            string linea = DateTime.UtcNow.ToString("O") + "|" + Reloj() + "|" + evento + "|" + detalle;
            File.AppendAllText(Path.Combine(carpeta, "eventos.txt"), linea + "\n");
            using (var log = new AndroidJavaClass("android.util.Log")) log.CallStatic<int>("i", "UchumataquQA", linea);
        }

        private static long Reloj()
        {
            using (var reloj = new AndroidJavaClass("android.os.SystemClock")) return reloj.CallStatic<long>("elapsedRealtime");
        }
    }
}
#endif
