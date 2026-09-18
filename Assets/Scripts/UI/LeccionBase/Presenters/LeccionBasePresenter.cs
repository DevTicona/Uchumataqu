using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.Assets;
using Assets.Scripts.Core.Sistema;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Lecciones;
using Assets.Scripts.PLN;
using Assets.Scripts.Persistencia;
using Assets.Scripts.ASR;
using Assets.Scripts.UI.LeccionBase.Actividades;
using System.Collections;

namespace Assets.Scripts.UI.LeccionBase
{
    public class LeccionBasePresenter : MonoBehaviour
    {
        [SerializeField] private LeccionBaseView view;
        [SerializeField] private NavegacionAtrasView backHandler;

        private readonly System.Random _random = new System.Random();
        private GestorLeccion _gestorLeccion;
        private bool _esperandoFeedback;
        private readonly List<Action> _activityUnsubscribe = new List<Action>();
        private Action _ejecutarAccionPrincipal;
        private Func<bool> _puedeEjecutarAccionPrincipal;
        private LeccionBaseView.AccionPrincipal _accionPrincipal;
        private readonly IReconocedorVoz _reconocedorVoz = new SherpaOnnxReconocedorVoz();
        private readonly GrabacionVozService _grabacionVoz = new GrabacionVozService();
        private string _objetivoGrabacionPresentacion;

        private async void Start()
        {
            if (view == null || backHandler == null)
            {
                Debug.LogError("[LeccionBasePresenter] Faltan referencias de composición de la escena.");
                enabled = false;
                return;
            }

            if (!view.ValidarConfiguracion(out string errorConfiguracion))
            {
                Debug.LogError($"[LeccionBasePresenter] {errorConfiguracion}");
                enabled = false;
                return;
            }

            bool corpusInicializado = await CorpusRepository.InicializarAsync();
            if (this == null) return;

            if (!corpusInicializado)
            {
                Debug.LogError("[LeccionBasePresenter] No se pudo inicializar el corpus para iniciar la lección.");
                enabled = false;
                return;
            }

            backHandler.Configurar(SolicitarSalida);
            view.OnAtrasClick += SolicitarSalida;
            view.OnAccionPrincipalClick += EjecutarAccionPrincipal;
            view.OnVolverAlMapaClick += VolverAlMapa;
            view.OnContinuarLeccionClick += VolverAlMapa;
            view.OnConfirmarSalidaClick += ConfirmarSalida;
            view.OnCancelarSalidaClick += CancelarSalida;

            IniciarEjecucionLeccion();
        }

        private void OnDestroy()
        {
            DesuscribirActividadActual();
            if (backHandler != null) backHandler.Configurar(null);
            _grabacionVoz.Dispose();
            (_reconocedorVoz as IDisposable)?.Dispose();
            if (view == null) return;
            view.OnAtrasClick -= SolicitarSalida;
            view.OnAccionPrincipalClick -= EjecutarAccionPrincipal;
            view.OnVolverAlMapaClick -= VolverAlMapa;
            view.OnContinuarLeccionClick -= VolverAlMapa;
            view.OnConfirmarSalidaClick -= ConfirmarSalida;
            view.OnCancelarSalidaClick -= CancelarSalida;
        }

        private void DesuscribirActividadActual()
        {
            foreach (Action unsubscribe in _activityUnsubscribe) unsubscribe();
            _activityUnsubscribe.Clear();
        }

        private void IniciarEjecucionLeccion()
        {
            _gestorLeccion = new GestorLeccion();
            string leccionId = string.IsNullOrEmpty(ContextoNavegacionModel.LeccionId) ? "L01" : ContextoNavegacionModel.LeccionId;
            if (!_gestorLeccion.IniciarLeccion(leccionId))
            {
                Debug.LogError($"[LeccionBasePresenter] No se pudo iniciar '{leccionId}'.");
                VolverAlMapa();
                return;
            }

            view.ConfigurarHeader(
                _gestorLeccion.LeccionActual.titulo,
                _gestorLeccion.ProgresoNormalizado);
            RenderizarPasoActual();
        }

        private void RenderizarPasoActual()
        {
            DesuscribirActividadActual();
            PasoLeccionModel paso = _gestorLeccion.PasoActual;
            if (paso == null)
            {
                FinalizarLeccion();
                return;
            }

            _esperandoFeedback = false;
            _ejecutarAccionPrincipal = null;
            _puedeEjecutarAccionPrincipal = null;
            view.PresentarAccionPrincipal(LeccionBaseView.AccionPrincipal.Continuar, false, false);
            view.MostrarOmitirASR(false);
            view.ActualizarProgreso(_gestorLeccion.ProgresoNormalizado);
            view.OcultarFeedback();

            if (!TipoPasoLeccion.EsSoportado(paso.tipo))
            {
                Debug.LogError($"[LeccionBasePresenter] Tipo de paso no soportado: '{paso.tipo}'.");
                enabled = false;
                return;
            }

            if (TipoPasoLeccion.EsPresentacion(paso.tipo)) RenderizarPresentacion(paso);
            else if (TipoPasoLeccion.EsSeleccion(paso.tipo)) RenderizarSeleccion(paso);
            else if (paso.tipo == TipoPasoLeccion.MemoriaUchEspanol) RenderizarMemoria(paso);
            else if (TipoPasoLeccion.EsEmparejamiento(paso.tipo)) RenderizarEmparejamiento(paso);
            else if (TipoPasoLeccion.EsWordBank(paso.tipo)) RenderizarWordBank(paso);
            else if (paso.tipo == TipoPasoLeccion.EscuchaYRepiteAsr) RenderizarASR(paso);
        }

        private void RenderizarASR(PasoLeccionModel paso)
        {
            DesuscribirActividadActual();
            var actividad = view.InstanciarASR();
            PalabraModel palabra = CorpusRepository.ObtenerPalabra(paso.objetivo_id);
            ConfigurarPracticaVoz(actividad, palabra, AvanzarPaso);
            view.ConfigurarTextoOmitirASR("CONTINUAR SIN GRABAR");
        }

        private void ConfigurarPracticaVoz(ASRView actividad, PalabraModel palabra, Action alOmitir)
        {
            bool microfonoDetectado = _grabacionVoz.TieneDispositivo;
            bool disponible = microfonoDetectado && _reconocedorVoz.EstaDisponible();
            actividad.Configurar(palabra?.uch ?? string.Empty, disponible);
            Action handlerOnAudioClicked = () => ReproducirPalabra(palabra?.id);
            actividad.OnAudioClicked += handlerOnAudioClicked;
            _activityUnsubscribe.Add(() => actividad.OnAudioClicked -= handlerOnAudioClicked);
            Action handlerOnMicrofonoClicked = () => AlternarGrabacion(actividad, palabra?.uch ?? string.Empty);
            actividad.OnMicrofonoClicked += handlerOnMicrofonoClicked;
            _activityUnsubscribe.Add(() => actividad.OnMicrofonoClicked -= handlerOnMicrofonoClicked);
            Action handlerOnOmitirClicked = alOmitir;
            actividad.OnOmitirClicked += handlerOnOmitirClicked;
            _activityUnsubscribe.Add(() => actividad.OnOmitirClicked -= handlerOnOmitirClicked);
            view.MostrarOmitirASR(true);
        }
        private void RenderizarPresentacion(PasoLeccionModel paso)
        {
            DesuscribirActividadActual();
            var actividad = view.InstanciarPresentacion();
            if (!ValidarActividad(actividad, paso.tipo)) return;

            PalabraModel palabra = CorpusRepository.ObtenerPalabra(paso.objetivo_id);
            actividad.Configurar(paso, palabra);
            Action handlerOnAudioClicked = () => ReproducirPalabra(palabra?.id);
            actividad.OnAudioClicked += handlerOnAudioClicked;
            _activityUnsubscribe.Add(() => actividad.OnAudioClicked -= handlerOnAudioClicked);
            // La práctica de presentación es opcional y permanece en esta misma vista.
            Action handlerOnPronunciacionClicked = () => AlternarGrabacionPresentacion(
                actividad, palabra?.uch ?? string.Empty);
            actividad.OnPronunciacionClicked += handlerOnPronunciacionClicked;
            _activityUnsubscribe.Add(() => actividad.OnPronunciacionClicked -= handlerOnPronunciacionClicked);
            Action<string> handlerOnFormaAudioClicked = ReproducirPalabra;
            actividad.OnFormaAudioClicked += handlerOnFormaAudioClicked;
            _activityUnsubscribe.Add(() => actividad.OnFormaAudioClicked -= handlerOnFormaAudioClicked);
            Action<string> handlerOnFormaPronunciacionClicked = esperado =>
                AlternarGrabacionPresentacion(actividad, esperado);
            actividad.OnFormaPronunciacionClicked += handlerOnFormaPronunciacionClicked;
            _activityUnsubscribe.Add(() => actividad.OnFormaPronunciacionClicked -= handlerOnFormaPronunciacionClicked);
            ConfigurarAccionPrincipal(AccionAvance, AvanzarPaso, () => true);

            if (palabra != null && ConfiguracionRepository.Obtener().audio_automatico)
                ReproducirPalabra(palabra.id);
        }

        private void AlternarGrabacionPresentacion(PresentacionFamiliaView actividad, string esperado)
        {
            if (_grabacionVoz.EstaGrabando)
            {
                FinalizarGrabacionPresentacion(actividad);
                return;
            }

            if (string.IsNullOrWhiteSpace(esperado)) return;
            if (!PermisoMicrofonoService.TienePermiso())
            {
                PermisoMicrofonoService.SolicitarPermiso();
                view.MostrarFeedback("informacion", "Autoriza el micrófono y vuelve a tocar PRACTICAR.");
                return;
            }

            if (!_grabacionVoz.Iniciar())
            {
                view.MostrarFeedback("informacion", "No se pudo iniciar el micrófono.");
                return;
            }

            _objetivoGrabacionPresentacion = esperado;
            actividad.MostrarEstadoGrabacion(true);
            view.EstablecerInteraccionDuranteGrabacion(true);
            ConfigurarAccionPrincipal(AccionAvance,
                () => FinalizarGrabacionPresentacion(actividad),
                () => _grabacionVoz.EstaGrabando);
            view.MostrarDetenerGrabacion();
            view.MostrarFeedback("informacion", "Pronuncia “" + esperado + "” y toca DETENER al terminar.");
        }

        private void FinalizarGrabacionPresentacion(PresentacionFamiliaView actividad)
        {
            if (!_grabacionVoz.EstaGrabando) return;

            AudioClip clip = _grabacionVoz.Finalizar();
            actividad.MostrarEstadoGrabacion(false);
            view.EstablecerInteraccionDuranteGrabacion(false);
            ConfigurarAccionPrincipal(AccionAvance, AvanzarPaso, () => true);
            if (clip == null)
            {
                view.MostrarFeedback("informacion", "No se capturó audio. Inténtalo otra vez.");
                return;
            }

            StartCoroutine(ProcesarPronunciacionPresentacion(_objetivoGrabacionPresentacion, clip));
            _objetivoGrabacionPresentacion = null;
        }

        private IEnumerator ProcesarPronunciacionPresentacion(string esperado, AudioClip clip)
        {
            view.MostrarFeedback("informacion", "Analizando pronunciación...");
            yield return null;

            ResultadoEvaluacion resultado = EvaluadorVoz.EvaluarPronunciacion(esperado, clip, _reconocedorVoz);
            Destroy(clip);
            if (this == null || view == null) yield break;

            if (!resultado.evaluado)
            {
                view.MostrarFeedback("informacion",
                    "La práctica se grabó, pero no se pudo evaluar en este dispositivo.");
                yield break;
            }

            string detalle = "Dijiste: “" + resultado.textoIngresado + "”. ";
            detalle += resultado.colorSemaforo == "verde"
                ? "Muy buena pronunciación."
                : resultado.colorSemaforo == "amarillo"
                    ? "Vas bien. Escucha e inténtalo otra vez."
                    : "Escucha con atención e inténtalo otra vez.";
            view.MostrarFeedback(resultado.colorSemaforo, detalle,
                resultado.porcentajeSimilitud, resultado.textoEsperado);
        }

        private void AlternarGrabacion(ASRView actividad, string esperado)
        {
            if (_grabacionVoz.EstaGrabando)
            {
                AudioClip clip = _grabacionVoz.Finalizar();
                if (clip == null)
                {
                    actividad.MostrarEstado(ASRView.Estado.Error, "No se capturo audio. Intentalo otra vez.");
                    return;
                }
                StartCoroutine(ProcesarPronunciacion(actividad, esperado, clip));
                return;
            }

            if (!PermisoMicrofonoService.TienePermiso())
            {
                PermisoMicrofonoService.SolicitarPermiso();
                actividad.MostrarEstado(ASRView.Estado.Error, "Autoriza el microfono y vuelve a tocar PRACTICAR.");
                return;
            }

            bool grabando = _grabacionVoz.Iniciar();
            actividad.MostrarEstado(grabando ? ASRView.Estado.Grabando : ASRView.Estado.Error,
                grabando ? "Pronuncia la palabra y toca DETENER." : "No se pudo iniciar el microfono.");
        }

        private IEnumerator ProcesarPronunciacion(ASRView actividad, string esperado, AudioClip clip)
        {
            actividad.MostrarEstado(ASRView.Estado.Procesando, "Analizando pronunciacion...");
            yield return null;
            ResultadoEvaluacion resultado = EvaluadorVoz.EvaluarPronunciacion(esperado, clip, _reconocedorVoz);
            Destroy(clip);
            if (actividad == null) yield break;

            if (!resultado.evaluado)
                actividad.MostrarEstado(ASRView.Estado.Error, "No se pudo evaluar. Puedes volver a intentarlo.");
            else
            {
                string mensaje = resultado.colorSemaforo == "verde" ? "Muy buena pronunciacion." :
                    resultado.colorSemaforo == "amarillo" ? "Vas bien. Escucha e intentalo otra vez." :
                    "Escucha con atencion e intentalo otra vez.";
                actividad.MostrarResultado(resultado.textoIngresado, resultado.porcentajeSimilitud, mensaje);
            }
        }

        private void RenderizarSeleccion(PasoLeccionModel paso)
        {
            DesuscribirActividadActual();
            var actividad = view.InstanciarSeleccionMultiple();
            if (!ValidarActividad(actividad, paso.tipo)) return;

            ActividadSeleccionModel datos;
            try
            {
                datos = GestorActividadLeccion.CrearSeleccion(paso);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LeccionBasePresenter] Paso inválido '{paso.tipo}': {ex.Message}");
                enabled = false;
                return;
            }

            var opciones = new List<string>(datos.Opciones);
            Barajar(opciones);
            actividad.Configurar(datos.Tipo, datos.Prompt, opciones, datos.AudioPalabraId ?? datos.AudioOracionId);
            Action handlerOnAudioPreguntaClicked = () =>
            {
                if (!string.IsNullOrEmpty(datos.AudioOracionId)) ReproducirOracion(datos.AudioOracionId);
                else ReproducirPalabra(datos.AudioPalabraId);
            };
            actividad.OnAudioPreguntaClicked += handlerOnAudioPreguntaClicked;
            _activityUnsubscribe.Add(() => actividad.OnAudioPreguntaClicked -= handlerOnAudioPreguntaClicked);
            Action<string> handlerOnRespuestaEnviada = respuesta =>
            {
                ResultadoEvaluacion resultado = MotorEvaluacion.EvaluarSeleccionExacta(datos.RespuestaCorrecta, respuesta);
                actividad.MostrarResultado(resultado.aprobado);
                RegistrarYMostrarFeedback(resultado, resultado.aprobado ? "¡Correcto!" : "Revisa la opción correcta");
            };
            actividad.OnRespuestaEnviada += handlerOnRespuestaEnviada;
            _activityUnsubscribe.Add(() => actividad.OnRespuestaEnviada -= handlerOnRespuestaEnviada);
            Action handlerOnSeleccionCambiada = ActualizarAccionPrincipal;
            actividad.OnSeleccionCambiada += handlerOnSeleccionCambiada;
            _activityUnsubscribe.Add(() => actividad.OnSeleccionCambiada -= handlerOnSeleccionCambiada);
            ConfigurarAccionPrincipal(LeccionBaseView.AccionPrincipal.Comprobar,
                actividad.ConfirmarRespuesta, () => actividad.PuedeEnviarRespuesta);

            if (datos.Tipo.StartsWith("audio_", StringComparison.Ordinal))
            {
                if (!string.IsNullOrEmpty(datos.AudioOracionId)) ReproducirOracion(datos.AudioOracionId);
                else if (!string.IsNullOrEmpty(datos.AudioPalabraId)) ReproducirPalabra(datos.AudioPalabraId);
            }
        }

        private void RenderizarEmparejamiento(PasoLeccionModel paso)
        {
            DesuscribirActividadActual();
            var actividad = view.InstanciarEmparejamiento();
            if (!ValidarActividad(actividad, paso.tipo)) return;

            Dictionary<string, string> pares;
            try
            {
                pares = GestorActividadLeccion.CrearPares(paso);
            }
            catch (Exception ex)
            {
                DetenerPorPasoInvalido(paso, ex);
                return;
            }
            if (pares.Count == 0)
            {
                Debug.LogError($"[LeccionBasePresenter] El paso '{paso.tipo}' no produjo pares.");
                enabled = false;
                return;
            }

            bool audio = paso.tipo == TipoPasoLeccion.EmparejarAudioUch;
            actividad.Configurar(pares, audio);
            Action<string> handlerOnAudioClicked = ReproducirPalabra;
            actividad.OnAudioClicked += handlerOnAudioClicked;
            _activityUnsubscribe.Add(() => actividad.OnAudioClicked -= handlerOnAudioClicked);
            Action<string, string> handlerOnParPropuesto = (izquierda, derecha) => actividad.MostrarResultadoPar(
                pares.TryGetValue(izquierda, out string esperada) && MotorEvaluacion.EvaluarSeleccionExacta(esperada, derecha).aprobado);
            actividad.OnParPropuesto += handlerOnParPropuesto;
            _activityUnsubscribe.Add(() => actividad.OnParPropuesto -= handlerOnParPropuesto);
            Action<bool> handlerOnEmparejamientoCompletado = completado =>
            {
                var resultado = MotorEvaluacion.EvaluarSeleccionExacta("completo", completado ? "completo" : "incompleto");
                RegistrarYMostrarFeedback(resultado, "¡Pares emparejados correctamente!");
            };
            actividad.OnEmparejamientoCompletado += handlerOnEmparejamientoCompletado;
            _activityUnsubscribe.Add(() => actividad.OnEmparejamientoCompletado -= handlerOnEmparejamientoCompletado);
        }

        private void RenderizarWordBank(PasoLeccionModel paso)
        {
            DesuscribirActividadActual();
            var actividad = view.InstanciarWordBank();
            if (!ValidarActividad(actividad, paso.tipo)) return;

            ActividadWordBankModel datos;
            try
            {
                datos = GestorActividadLeccion.CrearWordBank(paso);
            }
            catch (Exception ex)
            {
                DetenerPorPasoInvalido(paso, ex);
                return;
            }
            var tokens = new List<string>(datos.Tokens);
            Barajar(tokens);
            actividad.Configurar(datos.Tipo, datos.Prompt, tokens, datos.AudioOracionId);
            Action handlerOnAudioPromptClicked = () => ReproducirOracion(datos.AudioOracionId);
            actividad.OnAudioPromptClicked += handlerOnAudioPromptClicked;
            _activityUnsubscribe.Add(() => actividad.OnAudioPromptClicked -= handlerOnAudioPromptClicked);
            Action<List<string>> handlerOnTokensComprobados = respuesta =>
            {
                ResultadoEvaluacion resultado = ValidadorOrden.EvaluarOrdenOracion(datos.RespuestaEsperada, respuesta);
                RegistrarYMostrarFeedback(
                    resultado,
                    resultado.aprobado ? "¡Oración ordenada correctamente!" : "El orden no es correcto.");
            };
            actividad.OnTokensComprobados += handlerOnTokensComprobados;
            _activityUnsubscribe.Add(() => actividad.OnTokensComprobados -= handlerOnTokensComprobados);
            Action handlerOnRespuestaCambiada = ActualizarAccionPrincipal;
            actividad.OnRespuestaCambiada += handlerOnRespuestaCambiada;
            _activityUnsubscribe.Add(() => actividad.OnRespuestaCambiada -= handlerOnRespuestaCambiada);
            ConfigurarAccionPrincipal(LeccionBaseView.AccionPrincipal.Comprobar,
                actividad.Comprobar, () => actividad.PuedeEnviarRespuesta);

            if (paso.tipo == TipoPasoLeccion.DictadoWordBank)
                ReproducirOracion(datos.AudioOracionId);
        }

        private void RenderizarMemoria(PasoLeccionModel paso)
        {
            DesuscribirActividadActual();
            var actividad = view.InstanciarMemoria();
            if (!ValidarActividad(actividad, paso.tipo)) return;
            Dictionary<string, string> pares;
            try
            {
                pares = GestorActividadLeccion.CrearPares(paso);
            }
            catch (Exception ex)
            {
                DetenerPorPasoInvalido(paso, ex);
                return;
            }
            if (pares.Count == 0)
            {
                Debug.LogError("[LeccionBasePresenter] La actividad de memoria no produjo pares.");
                enabled = false;
                return;
            }
            actividad.Configurar(pares);
            Action<string, string> handlerOnParPropuesto = (izquierda, derecha) => actividad.MostrarResultadoPar(
                MotorEvaluacion.EvaluarSeleccionExacta(izquierda, derecha).aprobado);
            actividad.OnParPropuesto += handlerOnParPropuesto;
            _activityUnsubscribe.Add(() => actividad.OnParPropuesto -= handlerOnParPropuesto);
            Action<bool> handlerOnMemoriaCompletada = completada =>
            {
                ResultadoEvaluacion resultado =
                    MotorEvaluacion.EvaluarSeleccionExacta("completo", completada ? "completo" : "incompleto");
                RegistrarYMostrarFeedback(resultado, "¡Memoria completada!");
            };
            actividad.OnMemoriaCompletada += handlerOnMemoriaCompletada;
            _activityUnsubscribe.Add(() => actividad.OnMemoriaCompletada -= handlerOnMemoriaCompletada);
        }

        private void RegistrarYMostrarFeedback(ResultadoEvaluacion resultado, string detalle)
        {
            if (_esperandoFeedback) return;
            _gestorLeccion.RegistrarResultadoPaso(resultado.aprobado);
            _esperandoFeedback = true;
            ConfigurarAccionPrincipal(AccionAvance, AvanzarTrasFeedback, () => _esperandoFeedback);
            view.MostrarFeedback(resultado.colorSemaforo, detalle, resultado.porcentajeSimilitud, resultado.textoEsperado);
        }

        private void AvanzarTrasFeedback()
        {
            if (!_esperandoFeedback) return;
            _esperandoFeedback = false;
            AvanzarPaso();
        }

        private void AvanzarPaso()
        {
            if (_gestorLeccion.AvanzarSiguientePaso()) RenderizarPasoActual();
            else FinalizarLeccion();
        }

        private void FinalizarLeccion()
        {
            _ejecutarAccionPrincipal = null;
            _puedeEjecutarAccionPrincipal = null;
            view.PresentarAccionPrincipal(LeccionBaseView.AccionPrincipal.Finalizar, false, false);
            view.MostrarOmitirASR(false);
            if (!_gestorLeccion.FinalizarYGuardar())
            {
                ConfigurarAccionPrincipal(LeccionBaseView.AccionPrincipal.Finalizar, FinalizarLeccion, () => true);
                return;
            }
            int total = _gestorLeccion.Aciertos + _gestorLeccion.Errores;
            double precision = total > 0 ? (double)_gestorLeccion.Aciertos / total * 100.0 : 100.0;
            int estrellas = precision >= MotorEvaluacion.UMBRAL_VERDE ? 3
                : precision >= MotorEvaluacion.UMBRAL_AMARILLO ? 2 : 1;
            view.MostrarResultado(_gestorLeccion.Aciertos, total, precision, estrellas);
        }

        private LeccionBaseView.AccionPrincipal AccionAvance =>
            _gestorLeccion.IndicePasoActual == _gestorLeccion.TotalPasos - 1
                ? LeccionBaseView.AccionPrincipal.Finalizar
                : LeccionBaseView.AccionPrincipal.Continuar;

        private void ConfigurarAccionPrincipal(LeccionBaseView.AccionPrincipal accion, Action ejecutar, Func<bool> puedeEjecutar)
        {
            _accionPrincipal = accion;
            _ejecutarAccionPrincipal = ejecutar;
            _puedeEjecutarAccionPrincipal = puedeEjecutar;
            ActualizarAccionPrincipal();
        }

        private void ActualizarAccionPrincipal() => view.PresentarAccionPrincipal(
            _accionPrincipal, enabled && (_puedeEjecutarAccionPrincipal?.Invoke() ?? false));

        private void EjecutarAccionPrincipal()
        {
            if (!enabled || !(_puedeEjecutarAccionPrincipal?.Invoke() ?? false)) return;
            _ejecutarAccionPrincipal?.Invoke();
        }

        private static void ReproducirPalabra(string id)
        {
            if (!string.IsNullOrEmpty(id)) AudioService.Instance.ReproducirPalabra(id);
        }

        private static void ReproducirOracion(string id)
        {
            if (!string.IsNullOrEmpty(id)) AudioService.Instance.ReproducirOracion(id);
        }

        private void SolicitarSalida() => view.MostrarConfirmacionSalida(true);

        private void CancelarSalida() => view.MostrarConfirmacionSalida(false);

        private void ConfirmarSalida()
        {
            if (_gestorLeccion != null && !_gestorLeccion.GuardarEstadoActual()) return;
            VolverAlMapa();
        }

        private void VolverAlMapa() => NavigationService.IrAMain(PaginaMain.Lecciones);

        private bool ValidarActividad(Component actividad, string tipoPaso)
        {
            if (actividad != null) return true;
            Debug.LogError($"[LeccionBasePresenter] No se pudo crear la vista para el paso '{tipoPaso}'.");
            enabled = false;
            return false;
        }

        private void DetenerPorPasoInvalido(PasoLeccionModel paso, Exception ex)
        {
            Debug.LogError($"[LeccionBasePresenter] Paso inválido '{paso?.tipo}': {ex.Message}");
            enabled = false;
        }

        private void Barajar<T>(IList<T> lista)
        {
            for (int n = lista.Count - 1; n > 0; n--)
            {
                int k = _random.Next(n + 1);
                (lista[n], lista[k]) = (lista[k], lista[n]);
            }
        }
    }
}
