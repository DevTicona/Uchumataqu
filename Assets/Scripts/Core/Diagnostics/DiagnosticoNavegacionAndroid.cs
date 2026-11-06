#if UNITY_ANDROID && (DEVELOPMENT_BUILD || UCHUMATAQU_VALIDACION) && !UNITY_EDITOR
using System;
using System.Threading.Tasks;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.Persistencia;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Core.Diagnostics
{
    /// <summary>Entrada explícita de QA con datos separados; no forma parte de una build release.</summary>
    internal static class DiagnosticoNavegacionAndroid
    {
        private const string ExtraDestino = "uchumataqu.validacion.destino";
        private const string ExtraPaso = "uchumataqu.validacion.paso";
        private static IDisposable scope;
        private static string destino;
        private static int pasoPreparado;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void PrepararDatos()
        {
            try
            {
                int paso;
                using (var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (var activity = player.GetStatic<AndroidJavaObject>("currentActivity"))
                using (var intent = activity.Call<AndroidJavaObject>("getIntent"))
                {
                    destino = intent.Call<string>("getStringExtra", ExtraDestino);
                    paso = intent.Call<int>("getIntExtra", ExtraPaso, 0);
                    pasoPreparado = Math.Max(0, paso);
                    intent.Call("removeExtra", ExtraDestino);
                    intent.Call("removeExtra", ExtraPaso);
                }
                if (string.IsNullOrEmpty(destino)) return;
                scope = JsonLocalService.UsarDirectorioValidacionDispositivo();
                bool guardado = PerfilRepository.GuardarPerfil(new PerfilUsuarioModel
                {
                    nombre = "Prueba",
                    avatar_id = "avatar_01",
                    onboarding_completado = destino != "onboarding"
                });
                ProgresoRepository.LimpiarCache();
                var progreso = ProgresoRepository.ObtenerProgreso();
                for (int numero = 1; numero <= 26; numero++)
                {
                    string id = $"L{numero:D2}";
                    bool reanudar = destino == "leccion:" + id;
                    progreso.lecciones.Add(new ProgresoLeccionModel
                    {
                        leccion_id = id,
                        estado = reanudar ? "en_progreso" : "completada",
                        paso_actual = reanudar ? Math.Max(0, paso) : 0,
                        veces_completada = reanudar ? 0 : 1
                    });
                }
                if (!guardado || !ProgresoRepository.Guardar())
                    throw new InvalidOperationException("No se pudieron preparar los datos aislados.");
                Debug.Log("[VALIDACION_ANDROID] Datos aislados activos; destino=" + destino);
            }
            catch (Exception ex)
            {
                destino = null;
                Debug.LogError("[VALIDACION_ANDROID] " + ex);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static async void AbrirDestino()
        {
            if (scope == null || string.IsNullOrEmpty(destino)) return;
            try
            {
                string inicial = destino == "onboarding" ? NavigationService.SCENE_ONBOARDING : NavigationService.SCENE_MAIN;
                await EsperarEscena(inicial);
                // LoadSceneAsync debe terminar antes de encadenar la segunda navegación.
                await Task.Delay(100);
                if (destino.StartsWith("leccion:", StringComparison.Ordinal))
                {
                    string id = destino.Substring("leccion:".Length);
                    var previo = ProgresoRepository.ObtenerProgreso().lecciones.Find(l => l.leccion_id == id);
                    previo.revision_contenido = CorpusRepository.ObtenerLeccion(id).RevisionContenido;
                    previo.paso_actual = pasoPreparado;
                    if (!ProgresoRepository.Guardar()) throw new InvalidOperationException("No se guardó el punto de prueba.");
                    NavigationService.IrALeccion(id);
                    await EsperarEscena(NavigationService.SCENE_LECCION_BASE);
                }
                else if (destino.StartsWith("ra:", StringComparison.Ordinal))
                {
                    NavigationService.IrARealidadAumentada(destino.Substring("ra:".Length));
                    await EsperarEscena(NavigationService.SCENE_REALIDAD_AUMENTADA);
                }
                Debug.Log("[VALIDACION_ANDROID] Escena preparada: " + SceneManager.GetActiveScene().name);
            }
            catch (Exception ex) { Debug.LogError("[VALIDACION_ANDROID] " + ex); }
        }

        private static async Task EsperarEscena(string nombre)
        {
            float limite = Time.realtimeSinceStartup + 30f;
            while (SceneManager.GetActiveScene().name != nombre)
            {
                if (Time.realtimeSinceStartup > limite)
                    throw new TimeoutException("No se abrió " + nombre);
                await Task.Delay(50);
            }
        }
    }
}
#endif
