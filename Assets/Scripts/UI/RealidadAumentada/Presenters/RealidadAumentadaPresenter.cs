using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Assets;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.Sistema;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;
using Assets.Scripts.RA;
using Assets.Scripts.ASR;
using Assets.Scripts.PLN;

using UnityEngine;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public partial class RealidadAumentadaPresenter : MonoBehaviour
    {
        [SerializeField] private RealidadAumentadaView view;
        [SerializeField] private GestorEscenarioRA gestorRA;
        [SerializeField] private NavegacionAtrasView backHandler;
        [SerializeField] private ColocadorEscenarioRA colocador;
        [SerializeField] private GestorInteraccionRA interaccion;
        [SerializeField, Min(0.01f)] private float multiplicadorEscala = EscalaEscenarioRA.MultiplicadorComun;
        [SerializeField, Min(0.01f)] private float anchoEscenarioReferencia = 1.792f;
        [UnityEngine.Serialization.FormerlySerializedAs("experienciaRA1")]
        [SerializeField] private ExperienciaRAView experiencia;
        private readonly GrabacionVozService grabacionVoz = new GrabacionVozService();
        private SherpaOnnxReconocedorVoz reconocedorVoz;
        private Coroutine rutinaGrabacion;
        private bool procesandoVoz;
        private bool iniciandoMicrofono;
        private int versionVoz;
        private string feedbackActividad = string.Empty;

        private struct EstadoObjeto
        {
            public Transform Parent;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
            public bool Active;
        }

        private readonly Dictionary<string, ObjetoRAView> objetos = new Dictionary<string, ObjetoRAView>();
        private readonly Dictionary<string, DestinoRA> destinos = new Dictionary<string, DestinoRA>();
        private readonly Dictionary<ObjetoRAView, EstadoObjeto> estadosIniciales = new Dictionary<ObjetoRAView, EstadoObjeto>();
        private GestorMisionRA mision;
        private GameObject root;
        private Vector3 puntoAnclajeLocal;
        private PresentacionCantidadesRA cantidades;
        private PalabraModel palabraActiva;
        private PasoRAModel ultimoEstimulo;
        private bool visitadoEnSesion;
        private bool cargando;
        private bool ultimaPoseVisible;
        private bool animandoColocacion;
        private string faseVisible;
        private bool destruido;
        private int versionColocacion;
        private string mensajeEscaneo;
        private bool seguimientoPerdido;

        private async void Start()
        {
            if (ayuda == null) ayuda = Assets.Scripts.Core.UI.Ayuda.AyudaBrevePantalla.BuscarEnEscena(gameObject.scene);
            if (view == null || gestorRA == null || backHandler == null || colocador == null ||
                interaccion == null || interaccion.Camara == null || experiencia == null)
            {
                Debug.LogError("[RA] Faltan referencias de composición.");
                enabled = false;
                return;
            }

            IniciarPrecargaReconocedorVoz();

            backHandler.Configurar(SolicitarSalida);
            view.OnVolverClick += SolicitarSalida;
            view.OnConfirmarSalidaClick += VolverAlMapa;
            view.OnCancelarSalidaClick += CancelarSalida;
            view.OnRecolocarClick += Recolocar;
            ConectarExperienciaRA1();
            ConectarExperienciaGuiada();

            interaccion.PulsacionLibre += IntentarColocar;
            interaccion.ObjetoTocado += TocarObjeto;
            interaccion.DestinoTocado += TocarDestino;
            interaccion.ActorTocado += TocarActor;
            interaccion.ArrastreCompletado += ColocarArrastradoGuiado;
            colocador.SuperficieDetectada += RetroalimentacionHapticaService.ConfirmarSuperficieDetectada;

#if UNITY_EDITOR
            bool corpusListo = await System.Threading.Tasks.Task.FromResult(CorpusRepository.Inicializar());
#else
            bool corpusListo = await CorpusRepository.InicializarAsync();
#endif
            if (destruido) return;
            if (!corpusListo)
            {
                view.MostrarError("No se pudo cargar el contenido. Vuelve a intentarlo.");
                LiberarReconocedorVoz();
                return;
            }

            if (!gestorRA.CargarEscenario(ContextoNavegacionModel.EscenarioRaId))
            {
                view.MostrarError("No se pudo cargar este escenario.");
                LiberarReconocedorVoz();
                return;
            }

            mision = new GestorMisionRA(gestorRA.EscenarioActual.mision);
            view.ActualizarProgreso(mision.Progreso);
            mision.EstadoCambio += ActualizarMision;
            mision.PasoAcertado += Reaccionar;
            mision.MisionCompletada += CompletarMision;
            experiencia.ConfigurarBienvenida(gestorRA.EscenarioActual.titulo,
                ContenidoExperienciaRA.Bienvenida(gestorRA.EscenarioActual.id));
            interaccion.PermitirArrastre = UsaFooterGuiado;
            if (UsaFooterGuiado)
            {
                interaccion.PuedeArrastrarObjeto = PuedeArrastrarObjetoGuiado;
                interaccion.ResolverDestinoArrastre = ResolverDestinoArrastreGuiado;
            }
            view.MostrarEscaneo(false);
        }

        private void LiberarReconocedorVoz()
        {
            reconocedorVoz?.Dispose();
            reconocedorVoz = null;
        }

        private void IniciarPrecargaReconocedorVoz()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            reconocedorVoz ??= new SherpaOnnxReconocedorVoz();
            _ = ObservarPreparacionReconocedorVozAsync(reconocedorVoz);
#endif
        }

        private async Task ObservarPreparacionReconocedorVozAsync(SherpaOnnxReconocedorVoz reconocedor)
        {
            try
            {
                await reconocedor.PrepararAsync();
            }
            catch (Exception exception)
            {
                if (!destruido)
                    Debug.LogWarning("[RA_VOZ] No se pudo precargar el reconocedor: " + exception.Message);
            }
        }

        private void Update()
        {
            if (colocador == null || view == null || mision == null || cargando) return;
            if (root != null)
            {
                bool perdido = !colocador.TieneSeguimiento;
                if (perdido == seguimientoPerdido) return;
                seguimientoPerdido = perdido;
                if (perdido)
                {
                    CancelarGrabacion();
                    if (EsRA1) SuspenderExperienciaRA1();
                    if (UsaFooterGuiado) SuspenderExperienciaGuiada();
                }

                root.SetActive(!perdido);
                if (perdido) view.MostrarError("Seguimiento perdido. Apunta lentamente hacia la superficie donde colocaste el escenario.", IconoRA.Enfocar);
                else ActualizarMision();
                return;
            }
            if (colocador.TienePoseValida != ultimaPoseVisible || mensajeEscaneo != colocador.MensajeEscaneo)
            {
                ultimaPoseVisible = colocador.TienePoseValida;
                mensajeEscaneo = colocador.MensajeEscaneo;
                view.MostrarEscaneo(ultimaPoseVisible, mensajeEscaneo);
            }
        }

        private void OnDestroy()
        {
            destruido = true;
            DesconectarExperienciaRA1();
            DesconectarExperienciaGuiada();
            CancelarGrabacion();
            LiberarReconocedorVoz();

            versionColocacion++;
            if (colocador != null)
                colocador.SuperficieDetectada -= RetroalimentacionHapticaService.ConfirmarSuperficieDetectada;
            if (backHandler != null) backHandler.Configurar(null);
            if (view != null)
            {
                view.OnVolverClick -= SolicitarSalida;
                view.OnConfirmarSalidaClick -= VolverAlMapa;
                view.OnCancelarSalidaClick -= CancelarSalida;
                view.OnRecolocarClick -= Recolocar;
            }
            if (interaccion != null)
            {
                interaccion.PuedeArrastrarObjeto = null;
                interaccion.ResolverDestinoArrastre = null;
                interaccion.PulsacionLibre -= IntentarColocar;
                interaccion.ObjetoTocado -= TocarObjeto;
                interaccion.DestinoTocado -= TocarDestino;
                interaccion.ActorTocado -= TocarActor;
                interaccion.ArrastreCompletado -= ColocarArrastradoGuiado;
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
            if (cargando || animandoColocacion || !colocador.TieneSeguimiento) return;
            if (root != null)
            {
                if (!EsRA1 && !UsaFooterGuiado) ContinuarActividad();
                return;
            }
            if (mision == null) return;
            cargando = true;
            int version = ++versionColocacion;
            view.MostrarError("Preparando escenario...", IconoRA.Colocar);
            try
            {
                bool colocado = await colocador.IntentarColocarAsync();
                if (destruido || version != versionColocacion) return;
                if (!colocado)
                {
                    view.MostrarEscaneo(colocador.TienePoseValida);
                    return;
                }
                var datos = gestorRA.EscenarioActual;
                Transform superficie = colocador.SuperficieColocada;
                Transform camara = interaccion.Camara.transform;
                Quaternion orientacion = GeometriaColocacionRA.OrientarHaciaCamara(
                    colocador.PoseActual.position, camara.position, camara.forward);
                Vector3 puntoLocal = superficie.InverseTransformPoint(colocador.PoseActual.position);
                Quaternion giroLocal = Quaternion.Inverse(superficie.rotation) * orientacion;
                var instancia = await AddressablesService.InstanciarMundoAsync(
                    datos.prefab_key, colocador.PoseActual.position, orientacion, superficie);
                if (destruido || version != versionColocacion || superficie == null)
                {
                    AddressablesService.LiberarInstancia(instancia);
                    if (!destruido && version == versionColocacion)
                        throw new InvalidOperationException("La superficie dejó de estar disponible.");
                    return;
                }
                root = instancia;
                puntoAnclajeLocal = puntoLocal;
                root.transform.SetLocalPositionAndRotation(puntoLocal, giroLocal);
                AplicarEscalaComun();
                if (!VincularObjetos(datos)) throw new InvalidOperationException("El prefab no coincide con el JSON.");
                mision.Reiniciar();
                ActualizarMision();
            }
            catch (Exception ex)
            {
                if (destruido || version != versionColocacion) return;
                Debug.LogError("[RA] No se pudo colocar " + gestorRA.EscenarioActual.prefab_key + ": " + ex);
                LiberarRoot();
                colocador.Recolocar();
                view.MostrarError("No se pudo cargar el escenario. Reintenta la colocación.");
            }
            finally { if (version == versionColocacion) cargando = false; }
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
                    Scale = vista.transform.localScale,
                    Active = vista.gameObject.activeSelf
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
            foreach (var vista in objetos.Values)
            {
                bool obligatorio = datos.objetos.Exists(o => o.id == vista.ObjetoId);
                bool opcional = EsRA2 && datos.objetos_opcionales_exploracion_ids?.Contains(vista.ObjetoId) == true &&
                    CorpusRepository.ObtenerPalabra(vista.PalabraId) != null;
                if (!obligatorio && !opcional) return false;
            }

            foreach (var destino in root.GetComponentsInChildren<DestinoRA>(true))
            {
                if (string.IsNullOrEmpty(destino.DestinoId) || destinos.ContainsKey(destino.DestinoId))
                    return false;
                destinos.Add(destino.DestinoId, destino);
                destino.MostrarIndicador(false);
            }

            foreach (var fase in datos.mision.fases)
                foreach (var paso in fase.pasos)
                {
                    if (paso.tipo == "escuchar_seleccionar" && paso.opciones_ayuda_objeto_ids != null)
                        foreach (string opcion in paso.opciones_ayuda_objeto_ids)
                            if (!objetos.ContainsKey(opcion))
                            {
                                Debug.LogError("[RA_CONFIG_BINDING_ERROR] Opción ausente: " + opcion);
                                return false;
                            }
                    if (paso.tipo == "arrastrar" && !destinos.ContainsKey(paso.destino_id))
                    {
                        Debug.LogError("[RA_CONFIG_BINDING_ERROR] Destino ausente: " + paso.destino_id);
                        return false;
                    }
                    if (!string.IsNullOrEmpty(paso.destino_acierto_id) && !destinos.ContainsKey(paso.destino_acierto_id))
                    {
                        Debug.LogError("[RA_CONFIG_BINDING_ERROR] Destino de acierto ausente: " + paso.destino_acierto_id);
                        return false;
                    }
                }
            cantidades?.Dispose();
            cantidades = datos.id == "ra_familia_vestimenta" ? null : new PresentacionCantidadesRA(root.transform, datos, objetos);
            return true;
        }

        private void TocarObjeto(ObjetoRAView vista)
        {
            if (AyudaBloqueaInteraccion) return;
            if (UsaFooterGuiado) { TocarObjetoGuiado(vista); return; }
            if (EsRA1 && (vista == null || InteraccionRA1Bloqueada)) return;
            if (EsRA1 && mision.Completada) { ExplorarObjetoRA1(vista); return; }
            if (EsRA1 && (mision.PasoActual?.tipo == "historia" || mision.PasoActual?.tipo == "repetir_voz"))
            {
                RepetirAudioRA1();
                return;
            }
            if (EsRA1 && mision.PasoActual?.EsHistoriaInteractiva == true)
            {
                if (root == null || animandoColocacion || !colocador.TieneSeguimiento) return;
                if (mision.PasoActual.objetivo_objeto_id == vista.ObjetoId)
                {
                    feedbackActividad = string.Empty;
                    vista.LimpiarMarca();
                    StartCoroutine(EjecutarInteraccionNarrativa(vista, mision.PasoActual));
                }
                else
                {
                    feedbackActividad = "Toca al profesor resaltado para que se acerque.";
                    ActualizarMision();
                }
                return;
            }
            if (mision?.PasoActual?.tipo == "historia") { ContinuarActividad(); return; }
            if (mision?.PasoActual?.tipo == "repetir_voz" || mision?.PasoActual?.tipo == "escribir") return;

            if (root == null || vista == null || animandoColocacion || !colocador.TieneSeguimiento ||
                !objetos.ContainsKey(vista.ObjetoId)) return;
            var palabra = CorpusRepository.ObtenerPalabra(vista.PalabraId);
            if (palabra == null) return;
            palabraActiva = palabra;
            if (mision.Completada || mision.FaseActual?.tipo == "exploracion_libre")
                ReproducirAudioObjeto();

            RegistrarVisitaSesion();

            if ((mision.PasoActual?.EsHistoriaInteractiva == true || mision.PasoActual?.tipo == "escuchar_seleccionar") &&
                mision.PasoActual.objetivo_objeto_id == vista.ObjetoId &&
                root.GetComponent<PresentacionReaccionesRA>() != null)
            {
                StartCoroutine(EjecutarInteraccionNarrativa(vista, mision.PasoActual));
                return;
            }
            int antes = mision.Descubiertos;
            mision.RegistrarObjeto(vista.ObjetoId);
            if (mision.Descubiertos > antes && mision.FaseActual?.tipo == "exploracion_libre")
                vista.MarcarDescubierto();
        }

        private void TocarDestino(DestinoRA destino)
        {
            if (AyudaBloqueaInteraccion) return;
            if (mision?.PasoActual?.UsaTarjeta == true) return;
            if (UsaFooterGuiado && (!inicioGuiado || aciertoPendienteGuiado != null || !PuedeInteractuarGuiado)) return;
            if (root == null || destino == null || mision == null || animandoColocacion ||
                !colocador.TieneSeguimiento || !mision.EsperandoDestino) return;
            var paso = mision.PasoActual;
            if (paso == null) return;
            if (UsaFooterGuiado) DetenerAudioGuiado();
            if (paso.destino_id != destino.DestinoId)
            {
                if (UsaFooterGuiado)
                {
                    feedbackActividad = ContenidoExperienciaRA.Reintento(paso);
                    estadoErrorGuiado = ExperienciaRAView.Estado.Incorrecto;
                    experiencia.ReproducirSonidoRespuesta(estadoErrorGuiado);
                    destino.GetComponentInParent<ObjetoRAView>()?.MostrarEstadoSeleccion(ObjetoRAView.EstadoSeleccion.Incorrecto);
                }
                mision.RegistrarDestino(destino.DestinoId);
                return;
            }
            if (objetos.TryGetValue(paso.objetivo_objeto_id, out var objeto))
            {
                feedbackActividad = string.Empty;
                estadoErrorGuiado = ExperienciaRAView.Estado.Instruccion;
                destino.GetComponentInParent<ObjetoRAView>()?.MostrarEstadoSeleccion(ObjetoRAView.EstadoSeleccion.Correcto);
                animandoColocacion = true;
                if (UsaFooterGuiado) ActualizarMision();
                StartCoroutine(MoverADestino(objeto.transform, destino));
            }
        }

        private void TocarActor(ActorNarrativoRA actor)
        {
            if (AyudaBloqueaInteraccion) return;
            var paso = mision?.PasoActual;
            if (root == null || actor == null || animandoColocacion || !colocador.TieneSeguimiento ||
                paso?.EsHistoriaInteractiva != true || paso.objetivo_objeto_id != actor.ObjetoId) return;
            if (UsaFooterGuiado)
            {
                if (!inicioGuiado || OcupadoGuiado || aciertoPendienteGuiado != null || !PuedeInteractuarGuiado) return;
                StartCoroutine(ConfirmarAciertoGuiado(paso, null));
                return;
            }
            StartCoroutine(InteraccionActor(paso));
        }

        private IEnumerator InteraccionActor(PasoRAModel paso)
        {
            animandoColocacion = true;
            yield return PresentarAcierto(paso, null);
            animandoColocacion = false;
            if (mision.PasoActual == paso) mision.RegistrarObjeto(paso.objetivo_objeto_id);
        }

        private IEnumerator MoverADestino(Transform objeto, DestinoRA destino)
        {
            var paso = mision.PasoActual;
            if (root.GetComponent<IAccionEscenarioRA>()?.SustituyeColocacion(paso.objetivo_objeto_id) == true)
            {
                if (UsaFooterGuiado)
                {
                    yield return ConfirmarAciertoGuiado(paso, objeto.GetComponent<ObjetoRAView>());
                    yield break;
                }
                yield return PresentarAcierto(paso, objeto.GetComponent<ObjetoRAView>());
                animandoColocacion = false;
                mision.RegistrarDestino(destino.DestinoId);
                yield break;
            }
            yield return AnimarColocacionObjeto(objeto.GetComponent<ObjetoRAView>(), destino, paso.colocacion_vertical);
            if (UsaFooterGuiado)
            {
                yield return ConfirmarAciertoGuiado(paso, objeto.GetComponent<ObjetoRAView>());
                yield break;
            }
            if (root.GetComponent<PresentacionReaccionesRA>() != null) yield return PresentarAcierto(paso, objeto.GetComponent<ObjetoRAView>());
            animandoColocacion = false;
            mision.RegistrarDestino(destino.DestinoId);
        }

        private void Reaccionar(PasoRAModel paso)
        {
            if (paso != null && !string.IsNullOrEmpty(paso.objetivo_objeto_id) &&
                objetos.TryGetValue(paso.objetivo_objeto_id, out var resuelto))
                ConservarDialogoResuelto(resuelto);
            if (paso != null) RegistrarVisitaSesion();

            if (paso == null || paso.tipo == "arrastrar" || paso.tipo == "historia" ||
                paso.EsHistoriaInteractiva ||
                string.IsNullOrEmpty(paso.objetivo_objeto_id)) return;
            if (objetos.TryGetValue(paso.objetivo_objeto_id, out var objeto) &&
                root.GetComponent<PresentacionReaccionesRA>()?.Contiene(paso.reaccion_id) != true)
                objeto.Reaccionar(paso.reaccion_id);
        }

        private void ActualizarMision()
        {
            if (mision != null) view.ActualizarProgreso(mision.Progreso);
            if (root == null || mision == null) return;
            if (ultimoEstimulo != mision.PasoActual)
            {
                LimpiarDialogoPersistente();
                experiencia?.Tarjeta?.Ocultar();
                LimpiarMarcasObjetos();
                if (mision.Completada) ultimoEstimulo = mision.PasoActual;
            }
            if (UsaSeleccionGuiadaRA1) { inicioGuiado = inicioRA1; ActualizarExperienciaGuiada(); }
            else if (EsRA1) ActualizarExperienciaRA1();
            else if (UsaFooterGuiado) ActualizarExperienciaGuiada();
            PrepararAyudaActividad();
        }

        private void RepetirEstimulo()
        {
            if (UsaSeleccionGuiadaRA1) RepetirAudioGuiado();
            else if (EsRA1) RepetirAudioRA1();
            else if (UsaFooterGuiado) RepetirAudioGuiado();
        }

        private async void ReproducirAudioObjeto()
        {
            if (palabraActiva == null) return;
            try { await AudioService.Instance.ReproducirPalabraAsync(palabraActiva.id); }
            catch (Exception ex) { Debug.LogWarning("[RA] Audio de palabra no disponible: " + ex.Message); }
        }

        private void CompletarMision() => ActualizarMision();

        private void ReiniciarMision()
        {
            interaccion?.CancelarArrastre();
            ReiniciarEstadoRA1();
            ReiniciarEstadoGuiado();
            CancelarGrabacion();
            if (root != null) root.GetComponent<PresentacionReaccionesRA>()?.RestaurarIdle();

            StopAllCoroutines();
            animandoColocacion = false;
            faseVisible = null;
            root?.GetComponent<EstadoInicialEscenarioRA>()?.Reiniciar();
            foreach (var estado in estadosIniciales)
            {
                if (estado.Key == null) continue;
                estado.Key.transform.SetParent(estado.Value.Parent, false);
                estado.Key.transform.localPosition = estado.Value.Position;
                estado.Key.transform.localRotation = estado.Value.Rotation;
                estado.Key.transform.localScale = estado.Value.Scale;
                estado.Key.gameObject.SetActive(estado.Value.Active);
                estado.Key.ReiniciarVisual();
            }
            foreach (var destino in destinos.Values)
                if (destino != null) destino.MostrarIndicador(false);
            ultimoEstimulo = null;
            cantidades?.Reiniciar();
            mision?.Reiniciar();
        }

        private void Recolocar()
        {
            versionColocacion++;
            cargando = false;
            LiberarRoot();
            colocador.Recolocar();
            ultimaPoseVisible = false;
            mensajeEscaneo = null;
            seguimientoPerdido = false;
            ReiniciarMision();
            view.MostrarEscaneo(false);
        }

        private void LiberarRoot()
        {
            ReiniciarEstadoRA1();
            ReiniciarEstadoGuiado();
            AudioService.DetenerSiExiste();

            CancelarGrabacion();

            StopAllCoroutines();
            animandoColocacion = false;
            cantidades?.Dispose();
            cantidades = null;
            if (!ReferenceEquals(root, null)) AddressablesService.LiberarInstancia(root);
            root = null;
            objetos.Clear();
            destinos.Clear();
            estadosIniciales.Clear();
        }

        private void SolicitarSalida()
        {
            if (ayuda != null && ayuda.CerrarSiAbierta()) return;
            if (view.ConfirmacionSalidaVisible)
            {
                CancelarSalida();
                return;
            }
            CancelarGrabacion();
            view.MostrarConfirmacionSalida(true);
        }

        private void CancelarSalida() => view.MostrarConfirmacionSalida(false);

        private void VolverAlMapa() => NavigationService.IrAMain(PaginaMain.Lecciones);

        private void ContinuarActividad()
        {
            if (EsRA1) { EjecutarAccionRA1(); return; }
            if (UsaFooterGuiado) { EjecutarAccionGuiada(); return; }
            if (root == null || procesandoVoz || animandoColocacion || !colocador.TieneSeguimiento) return;
            CancelarGrabacion();
            if (mision?.PasoActual?.tipo == "historia") mision.AvanzarNarrativa();
            else mision?.ContinuarVoz();
        }

        private IEnumerator PresentarNarrativa(PasoRAModel paso)
        {
            animandoColocacion = true;
            objetos.TryGetValue(paso.objetivo_objeto_id ?? string.Empty, out var actor);
            yield return root.GetComponent<PresentacionReaccionesRA>().Ejecutar(
                string.IsNullOrEmpty(paso.reaccion_id) ? paso.id : paso.reaccion_id, actor?.transform, paso.trayecto);
            animandoColocacion = false;
            if (mision?.PasoActual == paso) ActualizarMision();
        }

        private IEnumerator EjecutarInteraccionNarrativa(ObjetoRAView actor, PasoRAModel paso)
        {
            animandoColocacion = true;
            if (EsRA1) ActualizarMision();
            yield return PresentarAcierto(paso, actor);
            animandoColocacion = false;
            if (mision?.PasoActual != paso) yield break;
            mision.RegistrarObjeto(actor.ObjetoId);
        }

        // Entrada de la actividad oral en RA: valida el paso, el seguimiento y el permiso del micrófono
        // antes de preparar Sherpa y comenzar la captura. Una segunda pulsación finaliza la grabación.
        private void PracticarVoz()
        {
            if (mision?.PasoActual?.tipo != "repetir_voz" || root == null || procesandoVoz || iniciandoMicrofono ||
                animandoColocacion || !colocador.TieneSeguimiento) return;
            if (grabacionVoz.EstaGrabando) { FinalizarGrabacion(); return; }
            LimpiarResultadoVoz();
            if (!PermisoMicrofonoService.TienePermiso())
            {
                if (EsRA1 && solicitudesMicrofonoRA1++ > 0)
                    InformarFalloVoz("Sin permiso de micrófono. Pulsa Continuar para omitir esta pronunciación.");
                else if (EsRA1)
                {
                    PermisoMicrofonoService.SolicitarPermiso();
                    feedbackActividad = "Permite el micrófono y pulsa Hablar.";
                    ActualizarMision();
                }
                else
                {
                    PermisoMicrofonoService.SolicitarPermiso();
                    InformarFalloGrabacion("Permite el micrófono y pulsa Hablar.");
                }
                return;
            }
            if (!grabacionVoz.TieneDispositivo)
            {
                InformarFalloGrabacion("No se encontró un micrófono. Comprueba su conexión y pulsa Hablar.");
                return;
            }
            iniciandoMicrofono = true;
            feedbackActividad = "Preparando el reconocimiento de voz…";
            ActualizarMision();
            rutinaGrabacion = StartCoroutine(EsperarMicrofono(++versionVoz));
        }

        private IEnumerator EsperarMicrofono(int version)
        {
            yield return null;
            reconocedorVoz ??= new SherpaOnnxReconocedorVoz();
            var tarea = reconocedorVoz.PrepararAsync();
            while (!tarea.IsCompleted) yield return null;
            if (version != versionVoz || destruido) yield break;
            bool disponible = false;
            try { disponible = tarea.GetAwaiter().GetResult(); }
            catch (Exception ex) { Debug.LogWarning("[RA_VOZ] " + ex); }
            if (!disponible)
            {
                InformarFalloVoz("No se pudo preparar la voz. Reintenta o continúa.");
                yield break;
            }
            bool abierto = false;
            try
            {
                if (EsRA1) DetenerAudioRA1(false);
                AudioService.Instance.Detener();
                abierto = grabacionVoz.Iniciar(30);
            }
            catch (Exception ex) { Debug.LogWarning("[RA_VOZ] " + ex); }
            if (!abierto)
            {
                InformarFalloGrabacion("No se pudo abrir el micrófono. Pulsa Hablar para reintentar.");
                yield break;
            }
            float limite = Time.realtimeSinceStartup + 3f;
            while (grabacionVoz.EstaGrabando && !grabacionVoz.TieneMuestras && Time.realtimeSinceStartup < limite)
                yield return null;
            rutinaGrabacion = null;
            iniciandoMicrofono = false;
            if (!grabacionVoz.TieneMuestras)
            {
                InformarFalloGrabacion("El micrófono no está listo. Pulsa Hablar para reintentar.");
                yield break;
            }
            feedbackActividad = "Habla (máx. 30 s) y pulsa Detener al acabar.";
            ActualizarMision();
        }

        private void InformarFalloGrabacion(string mensaje)
        {
            LimpiarResultadoVoz();
            Debug.LogWarning("[RA_VOZ] " + mensaje);
            CancelarGrabacion();
            feedbackActividad = mensaje;
            ActualizarMision();
        }

        private async void FinalizarGrabacion()
        {
            var paso = mision?.PasoActual;
            if (iniciandoMicrofono || !grabacionVoz.EstaGrabando || procesandoVoz) return;
            if (paso?.tipo != "repetir_voz") { CancelarGrabacion(); return; }
            if (rutinaGrabacion != null) StopCoroutine(rutinaGrabacion);
            rutinaGrabacion = null;
            AudioClip clip = null;
            var reconocedor = reconocedorVoz;
            procesandoVoz = true;
            int version = versionVoz;
            ActualizarMision();
            try
            {
                clip = grabacionVoz.Finalizar();
                if (clip == null)
                {
                    InformarFalloGrabacion("No se grabó audio. Pulsa Hablar y espera la indicación antes de responder.");
                    return;
                }
                string esperado = ContenidoActividadRA.TextoEsperado(paso);
                if (string.IsNullOrWhiteSpace(esperado))
                {
                    InformarFalloVoz("No se pudo resolver el contenido de la actividad.");
                    return;
                }
                // Conecta la actividad RA con la inferencia local. La versión y el paso descartan respuestas
                // tardías si el usuario salió o cambió de actividad mientras se procesaba el audio.
                string transcripcion = await reconocedor.TranscribirAudioAsync(clip);
                if (version != versionVoz || destruido || mision?.PasoActual != paso) return;
                if (string.IsNullOrWhiteSpace(transcripcion))
                {
                    procesandoVoz = false;
                    RegistrarResultadoVoz(paso, EvaluadorVoz.EvaluarTranscripcion(esperado, string.Empty));
                    return;
                }
                // Evalúa los fonemas y aplica las reglas del turno (por ejemplo, un prefijo de presentación).
                // El umbral del paso decide después si se confirma el acierto o se muestra el reintento.
                var resultado = EvaluadorVoz.EvaluarTranscripcion(esperado, transcripcion,
                    (paso.sufijo_nombre || paso.nombre_perfil) && !string.IsNullOrWhiteSpace(paso.comparar_texto),
                    ContenidoActividadRA.TextoVisible(paso, PerfilRepository.ObtenerPerfil().nombre),
                    paso.prefijo_voz_valido);
                procesandoVoz = false;
                if (!resultado.evaluado) { InformarFalloVoz("No se pudo evaluar la grabación. Reintenta o continúa."); return; }

                if (resultado.porcentajeSimilitud >= (paso.umbral_aprobacion > 0 ? paso.umbral_aprobacion : 85))
                {
                    StartCoroutine(ConfirmarPronunciacion(paso, resultado));
                    return;
                }
                RegistrarResultadoVoz(paso, resultado);
            }
            catch (Exception ex)
            {
                if (version == versionVoz && !destruido) InformarFalloVoz("No se pudo evaluar la voz: " + ex.Message);
            }
            finally
            {
                if (version == versionVoz) procesandoVoz = false;
                if (clip != null) Destroy(clip);
            }
        }

        private void InformarFalloVoz(string mensaje)
        {
            LimpiarResultadoVoz();
            CancelarGrabacion();
            feedbackActividad = mensaje;
            mision?.RegistrarEvaluacion(new ResultadoEvaluacion { evaluado = false });
        }

        // Reutiliza el mismo motor de escritura y el índice morfológico de las lecciones dentro de RA.
        // Las experiencias con interfaz guiada delegan su confirmación a EvaluarEscrituraGuiada.
        private void EvaluarEscritura(string respuesta)
        {
            if (UsaFooterGuiado) { EvaluarEscrituraGuiada(respuesta); return; }
            if (mision?.PasoActual?.tipo != "escribir" || root == null ||
                !colocador.TieneSeguimiento || animandoColocacion) return;
            string esperado = ContenidoActividadRA.TextoEsperado(mision.PasoActual);
            if (string.IsNullOrWhiteSpace(esperado)) { view.MostrarError("Falta el contenido de escritura."); return; }
            var resultado = MotorEvaluacion.EvaluarEscritura(esperado, respuesta, CorpusRepository.Morfologia);
            if (resultado.aprobado) { StartCoroutine(ConfirmarPronunciacion(mision.PasoActual, resultado)); return; }
            if (!mision.RegistrarEvaluacion(resultado))
            {
                feedbackActividad = "Revisa la escritura e intenta otra vez.";
                ActualizarMision();
            }
        }

        private void CancelarGrabacion()
        {
            versionVoz++;
            if (grabacionVoz.EstaGrabando) feedbackActividad = "Grabación interrumpida. Pulsa Hablar para reintentar.";

            if (rutinaGrabacion != null) StopCoroutine(rutinaGrabacion);
            rutinaGrabacion = null;
            iniciandoMicrofono = false;
            grabacionVoz.Dispose();
            procesandoVoz = false;
        }

        private void OnApplicationPause(bool pausado)
        {
            pausadoRA1 = pausado;
            if (pausado)
            {
                CancelarGrabacion();
                if (UsaFooterGuiado && root != null) root.SetActive(false);
                if (EsRA1) { SuspenderExperienciaRA1(); experiencia.Dialogo.Suspender(true); }
                if (UsaFooterGuiado) { SuspenderExperienciaGuiada(); experiencia.Dialogo.Suspender(true); }
            }
            else if (!destruido)
            {
                if (UsaFooterGuiado && root != null) root.SetActive(colocador.TieneSeguimiento);
                experiencia?.Dialogo.Suspender(false);
                ActualizarMision();
            }
        }

        private void OnDisable()
        {
            CancelarAyudaActividad();
            CancelarGrabacion();
            if (!destruido && EsRA1) SuspenderExperienciaRA1();
            if (!destruido && UsaFooterGuiado) SuspenderExperienciaGuiada();
        }

        private void OnEnable()
        {
            if ((EsRA1 || UsaFooterGuiado) && root != null && mision != null) ActualizarMision();
        }

        private IEnumerator PresentarAcierto(PasoRAModel paso, ObjetoRAView actor)
        {
            var presentacion = root.GetComponent<PresentacionReaccionesRA>();
            string reaccion = paso.reaccion_id;
            if (string.IsNullOrEmpty(reaccion) && paso.tipo == "repetir_voz") reaccion = "asr_acierto";
            else if (string.IsNullOrEmpty(reaccion) && actor != null) reaccion = "objeto_acierto";
            if (presentacion != null) yield return presentacion.Ejecutar(reaccion, actor?.transform, paso.trayecto);
            if (paso.interaccion_posterior != null)
            {
                yield return ReproducirRefuerzo(paso.interaccion_posterior.estimulo_id);
                if (presentacion != null) yield return presentacion.Ejecutar(paso.interaccion_posterior.reaccion_id);
            }
            if (!EsApoyoDePalabraGuiado(paso) && !string.IsNullOrEmpty(paso.refuerzo_oracion_id))
                yield return ReproducirRefuerzo(paso.refuerzo_oracion_id);
        }

        private IEnumerator ReproducirRefuerzo(string oracionId)
        {
            if (string.IsNullOrEmpty(oracionId)) yield break;
            if (UsaFooterGuiado && aciertoPendienteGuiado != null &&
                objetos.TryGetValue(aciertoPendienteGuiado.objetivo_objeto_id, out var actor))
            {
                var oracion = CorpusRepository.ObtenerOracion(oracionId);
                if (oracion != null) MostrarDialogoGuiado(actor, oracion.uch, oracion.es, false);
            }
            var tarea = AudioService.Instance.ReproducirOracionAsync(oracionId);
            while (!tarea.IsCompleted) yield return null;
            while (AudioService.Instance.EstaReproduciendo) yield return null;
        }
        private IEnumerator ConfirmarPronunciacion(PasoRAModel paso, ResultadoEvaluacion resultado)
        {
            if (UsaFooterGuiado)
            {
                objetos.TryGetValue(paso.objetivo_objeto_id, out var actorRA2);
                yield return ConfirmarAciertoGuiado(paso, actorRA2, resultado);
                yield break;
            }
            animandoColocacion = true;
            if (EsRA1)
            {
                pasoCorrectoRA1 = paso;
                respuestaCorrectaRA1 = ContenidoActividadRA.TextoVisible(paso, PerfilRepository.ObtenerPerfil().nombre);
                pasoResultadoVoz = paso;
                resultadoVoz = resultado;
                experiencia.ReproducirSonidoRespuesta(ExperienciaRAView.Estado.Correcto);
                ActualizarMision();
            }
            var presentacion = root.GetComponent<PresentacionReaccionesRA>();
            objetos.TryGetValue(paso.objetivo_objeto_id ?? string.Empty, out var actor);
            if (presentacion != null) yield return PresentarAcierto(paso, actor);
            if (EsRA1)
            {
                while (!PuedeEjecutarTurnoRA1 && mision.PasoActual == paso) yield return null;
                animandoColocacion = false;
                ActualizarMision();
                yield break;
            }
            if (mision.PasoActual != paso) yield break;
            while (EsRA1 && !PuedeEjecutarTurnoRA1) yield return null;
            animandoColocacion = false;
            reanudarDialogoRA1 = false;
            if (mision.PasoActual == paso) mision.RegistrarEvaluacion(resultado);
        }

        private void RegistrarVisitaSesion()
        {
            if (visitadoEnSesion || root == null) return;
            visitadoEnSesion = gestorRA.RegistrarVisita();
            if (!visitadoEnSesion) Debug.LogWarning("[RA] No se pudo guardar visitado.");
        }
    }
}
