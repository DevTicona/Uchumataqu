using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Assets;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.Sistema;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;
using Assets.Scripts.RA;
using UnityEngine;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public class RealidadAumentadaPresenter : MonoBehaviour
    {
        [SerializeField] private RealidadAumentadaView view;
        [SerializeField] private GestorEscenarioRA gestorRA;
        [SerializeField] private NavegacionAtrasView backHandler;
        [SerializeField] private ColocadorEscenarioRA colocador;
        [SerializeField] private GestorInteraccionRA interaccion;
        [SerializeField] private Transform runtimeContent;
        private struct EstadoObjeto
        {
            public Transform Parent;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
        }

        private readonly Dictionary<string, ObjetoRAView> objetos = new Dictionary<string, ObjetoRAView>();
        private readonly Dictionary<string, DestinoRA> destinos = new Dictionary<string, DestinoRA>();
        private readonly Dictionary<ObjetoRAView, EstadoObjeto> estadosIniciales = new Dictionary<ObjetoRAView, EstadoObjeto>();
        private GestorMisionRA mision;
        private GameObject root;
        private PalabraModel palabraActiva;
        private PasoRAModel ultimoEstimulo;
        private bool visitadoEnSesion;
        private bool cargando;
        private bool ultimaPoseVisible;

        private async void Start()
        {
            if (view == null || gestorRA == null || backHandler == null || colocador == null ||
                interaccion == null || runtimeContent == null)
            {
                Debug.LogError("[RA] Faltan referencias de composición.");
                enabled = false;
                return;
            }

            backHandler.Configurar(VolverAlMapa);
            view.OnVolverClick += VolverAlMapa;
            view.OnAudioObjetoClick += ReproducirAudioObjeto;
            view.OnRepetirAudioClick += RepetirEstimulo;
            view.OnRecolocarClick += Recolocar;
            view.OnReiniciarClick += ReiniciarMision;
            view.OnExplorarClick += ExplorarDespuesDeMision;
            interaccion.PulsacionLibre += IntentarColocar;
            interaccion.ObjetoTocado += TocarObjeto;
            interaccion.DestinoTocado += TocarDestino;

#if UNITY_EDITOR
            bool corpusListo = await System.Threading.Tasks.Task.FromResult(CorpusRepository.Inicializar());
#else
            bool corpusListo = await CorpusRepository.InicializarAsync();
#endif
            if (!corpusListo)
            {
                view.MostrarError("No se pudo cargar el contenido. Vuelve a intentarlo.");
                return;
            }

            if (!gestorRA.CargarEscenario(ContextoNavegacionModel.EscenarioRaId))
            {
                view.MostrarError("No se pudo cargar este escenario.");
                return;
            }

            view.ConfigurarHeader(gestorRA.EscenarioActual.titulo);
            mision = new GestorMisionRA(gestorRA.EscenarioActual.mision);
            mision.EstadoCambio += ActualizarMision;
            mision.PasoAcertado += Reaccionar;
            mision.MisionCompletada += CompletarMision;
            view.MostrarEscaneo(false);
        }

        private void Update()
        {
            if (root == null && !cargando && colocador != null && view != null &&
                colocador.TienePoseValida != ultimaPoseVisible)
            {
                ultimaPoseVisible = colocador.TienePoseValida;
                view.MostrarEscaneo(ultimaPoseVisible);
            }
        }

        private void OnDestroy()
        {
            if (backHandler != null) backHandler.Configurar(null);
            if (view != null)
            {
                view.OnVolverClick -= VolverAlMapa;
                view.OnAudioObjetoClick -= ReproducirAudioObjeto;
                view.OnRepetirAudioClick -= RepetirEstimulo;
                view.OnRecolocarClick -= Recolocar;
                view.OnReiniciarClick -= ReiniciarMision;
                view.OnExplorarClick -= ExplorarDespuesDeMision;
            }
            if (interaccion != null)
            {
                interaccion.PulsacionLibre -= IntentarColocar;
                interaccion.ObjetoTocado -= TocarObjeto;
                interaccion.DestinoTocado -= TocarDestino;
            }
            if (mision != null)
            {
                mision.EstadoCambio -= ActualizarMision;
                mision.PasoAcertado -= Reaccionar;
                mision.MisionCompletada -= CompletarMision;
            }
            LiberarRoot();
        }

        private async void IntentarColocar(Vector2 posicion)
        {
            if (cargando || root != null || mision == null || !colocador.IntentarColocar()) return;
            cargando = true;
            view.MostrarError("Preparando escenario...");
            try
            {
                var datos = gestorRA.EscenarioActual;
                Vector3 haciaCamara = interaccion.Camara.transform.position - colocador.PoseActual.position;
                haciaCamara.y = 0f;
                Quaternion orientacion = haciaCamara.sqrMagnitude > 0.001f
                    ? Quaternion.LookRotation(haciaCamara.normalized) * Quaternion.Euler(0f, 90f, 0f)
                    : colocador.PoseActual.rotation;
                root = await AddressablesService.InstanciarMundoAsync(
                    datos.prefab_key, colocador.PoseActual.position, orientacion, runtimeContent);
                root.transform.localScale *= datos.escala_base > 0f ? datos.escala_base : 1f;
                if (!VincularObjetos(datos)) throw new InvalidOperationException("El prefab no coincide con el JSON.");
                mision.Reiniciar();
                ActualizarMision();
            }
            catch (Exception ex)
            {
                Debug.LogError("[RA] No se pudo colocar " + gestorRA.EscenarioActual.prefab_key + ": " + ex);
                LiberarRoot();
                colocador.Recolocar();
                view.MostrarError("No se pudo cargar el escenario. Reintenta la colocación.");
            }
            finally { cargando = false; }
        }

        private bool VincularObjetos(EscenarioRAModel datos)
        {
            objetos.Clear();
            destinos.Clear();
            estadosIniciales.Clear();
            foreach (var vista in root.GetComponentsInChildren<ObjetoRAView>(true))
            {
                if (string.IsNullOrEmpty(vista.ObjetoId) || objetos.ContainsKey(vista.ObjetoId))
                {
                    Debug.LogError("[RA_CONFIG_BINDING_ERROR] ID de objeto vacío o repetido.");
                    return false;
                }
                objetos.Add(vista.ObjetoId, vista);
                estadosIniciales.Add(vista, new EstadoObjeto
                {
                    Parent = vista.transform.parent,
                    Position = vista.transform.localPosition,
                    Rotation = vista.transform.localRotation,
                    Scale = vista.transform.localScale
                });
            }

            foreach (var config in datos.objetos)
            {
                if (!objetos.TryGetValue(config.id, out var vista))
                {
                    Debug.LogError("[RA_CONFIG_BINDING_ERROR] " + datos.id + "/" + config.id);
                    return false;
                }
                if (CorpusRepository.ObtenerPalabra(config.palabra_id) == null)
                {
                    Debug.LogError("[RA_CONFIG_BINDING_ERROR] Palabra ausente: " + config.palabra_id);
                    return false;
                }
                vista.Configurar(config.id, config.palabra_id);
            }

            foreach (var destino in root.GetComponentsInChildren<DestinoRA>(true))
            {
                if (string.IsNullOrEmpty(destino.DestinoId) || destinos.ContainsKey(destino.DestinoId))
                    return false;
                destinos.Add(destino.DestinoId, destino);
                destino.gameObject.SetActive(false);
            }

            foreach (var fase in datos.mision.fases)
                foreach (var paso in fase.pasos)
                    if (paso.tipo == "seleccionar_colocar" && !destinos.ContainsKey(paso.destino_id))
                    {
                        Debug.LogError("[RA_CONFIG_BINDING_ERROR] Destino ausente: " + paso.destino_id);
                        return false;
                    }
            return true;
        }

        private void TocarObjeto(ObjetoRAView vista)
        {
            if (root == null || vista == null || !objetos.ContainsKey(vista.ObjetoId)) return;
            var palabra = CorpusRepository.ObtenerPalabra(vista.PalabraId);
            if (palabra == null) return;
            palabraActiva = palabra;
            view.MostrarPanelObjeto(palabra.uch, palabra.es);
            ReproducirAudioObjeto();

            if (!visitadoEnSesion)
            {
                visitadoEnSesion = gestorRA.RegistrarVisita();
                if (!visitadoEnSesion) Debug.LogWarning("[RA] No se pudo guardar visitado.");
            }

            int antes = mision.Descubiertos;
            mision.RegistrarObjeto(vista.ObjetoId);
            if (mision.Descubiertos > antes) vista.MarcarDescubierto();
        }

        private void TocarDestino(DestinoRA destino)
        {
            if (root == null || destino == null || mision == null || !mision.EsperandoDestino) return;
            var paso = mision.PasoActual;
            if (paso != null && mision.RegistrarDestino(destino.DestinoId) &&
                objetos.TryGetValue(paso.objetivo_objeto_id, out var objeto))
                StartCoroutine(MoverADestino(objeto.transform, destino.transform));
        }

        private IEnumerator MoverADestino(Transform objeto, Transform destino)
        {
            Vector3 inicio = objeto.position;
            Quaternion giro = objeto.rotation;
            float tiempo = 0f;
            while (tiempo < 0.55f)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / 0.55f));
                objeto.SetPositionAndRotation(Vector3.Lerp(inicio, destino.position, t),
                    Quaternion.Slerp(giro, destino.rotation, t));
                yield return null;
            }
            objeto.SetParent(destino, true);
            objeto.gameObject.SetActive(false);
        }

        private void Reaccionar(PasoRAModel paso)
        {
            if (objetos.TryGetValue(paso.objetivo_objeto_id, out var objeto))
                objeto.Reaccionar(paso.reaccion_id);
        }

        private void ActualizarMision()
        {
            if (root == null || mision == null) return;
            if (mision.Completada) { view.MostrarResultado(); return; }

            var fase = mision.FaseActual;
            if (fase.tipo == "exploracion_libre")
            {
                view.MostrarMision("Explora la casa: " + mision.Descubiertos + "/" +
                    mision.TotalExploracion + " descubiertos");
            }
            else
            {
                var paso = mision.PasoActual;
                if (paso == null) return;
                string instruccion = fase.id == "preparar_mochila"
                    ? "Escucha y prepara la mochila"
                    : "Encuentra lo que escuchas";
                string estado = mision.EsperandoDestino ? " · ahora toca la mochila" : "";
                string error = mision.Errores > 0 ? " · Intenta otra vez" : "";
                string ayuda = mision.MostrarAyuda ? " · observa el objeto que brilla" : "";
                view.MostrarMision(instruccion + " · Paso " + mision.NumeroPaso + "/" +
                    mision.TotalPasos + estado + error + ayuda);
                foreach (var destino in destinos.Values)
                    destino.gameObject.SetActive(mision.EsperandoDestino && destino.DestinoId == paso.destino_id);
                if (mision.MostrarAyuda && objetos.TryGetValue(paso.objetivo_objeto_id, out var pista))
                    pista.Resaltar();
                if (ultimoEstimulo != paso)
                {
                    ultimoEstimulo = paso;
                    RepetirEstimulo();
                }
            }
        }

        private async void RepetirEstimulo()
        {
            var paso = mision?.PasoActual;
            if (paso == null) return;
            try
            {
                if (paso.estimulo_tipo == "oracion")
                    await AudioService.Instance.ReproducirOracionAsync(paso.estimulo_id);
                else
                    await AudioService.Instance.ReproducirPalabraAsync(paso.estimulo_id);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[RA] Audio de misión no disponible: " + ex.Message);
                string pista = paso.estimulo_tipo == "oracion"
                    ? CorpusRepository.ObtenerOracion(paso.estimulo_id)?.es
                    : CorpusRepository.ObtenerPalabra(paso.estimulo_id)?.es;
                view.MostrarMision("Audio no disponible. " + pista + " · toca Escuchar para reintentar.");
            }
        }

        private async void ReproducirAudioObjeto()
        {
            if (palabraActiva == null) return;
            try { await AudioService.Instance.ReproducirPalabraAsync(palabraActiva.id); }
            catch (Exception ex) { Debug.LogWarning("[RA] Audio de palabra no disponible: " + ex.Message); }
        }

        private async void CompletarMision()
        {
            view.MostrarResultado();
            if (objetos.TryGetValue("obj_mochila", out var mochila))
                mochila.Reaccionar("mochila_lista");
            var fases = gestorRA.EscenarioActual.mision.fases;
            string despedida = fases[fases.Count - 1].resultado_audio_palabra_id;
            if (string.IsNullOrEmpty(despedida)) return;
            try { await AudioService.Instance.ReproducirPalabraAsync(despedida); }
            catch (Exception ex) { Debug.LogWarning("[RA] Despedida no disponible: " + ex.Message); }
        }

        private void ReiniciarMision()
        {
            StopAllCoroutines();
            foreach (var estado in estadosIniciales)
            {
                if (estado.Key == null) continue;
                estado.Key.transform.SetParent(estado.Value.Parent, false);
                estado.Key.transform.localPosition = estado.Value.Position;
                estado.Key.transform.localRotation = estado.Value.Rotation;
                estado.Key.transform.localScale = estado.Value.Scale;
                estado.Key.gameObject.SetActive(true);
                estado.Key.LimpiarMarca();
            }
            foreach (var destino in destinos.Values)
                if (destino != null) destino.gameObject.SetActive(false);
            ultimoEstimulo = null;
            mision?.Reiniciar();
        }

        private void ExplorarDespuesDeMision()
        {
            if (mision != null && mision.Completada)
                view.MostrarExploracionFinal();
        }

        private void Recolocar()
        {
            LiberarRoot();
            colocador.Recolocar();
            ultimaPoseVisible = false;
            ReiniciarMision();
            view.OcultarPanelObjeto();
            view.MostrarEscaneo(false);
        }

        private void LiberarRoot()
        {
            if (root != null) AddressablesService.LiberarInstancia(root);
            root = null;
            objetos.Clear();
            destinos.Clear();
            estadosIniciales.Clear();
        }

        private void VolverAlMapa() => NavigationService.IrAMain(PaginaMain.Lecciones);
    }
}
