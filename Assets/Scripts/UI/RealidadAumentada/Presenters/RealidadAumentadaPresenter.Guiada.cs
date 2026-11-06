using System.Collections;
using System.Linq;
using Assets.Scripts.Core.Assets;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;
using Assets.Scripts.PLN;
using Assets.Scripts.RA;
using UnityEngine;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public partial class RealidadAumentadaPresenter
    {
        private bool inicioGuiado;
        private bool audioGuiado;
        private Coroutine rutinaAudioGuiada;
        private PasoRAModel aciertoPendienteGuiado;
        private ResultadoEvaluacion evaluacionPendienteGuiada;
        private bool oracionApoyoMostrada;
        private string ultimaPalabraGuiada;
        private ExperienciaRAView.Estado estadoErrorGuiado;
        private bool exploracionTerrestreGuiada;
        private bool esperandoRespuestaEscrita;
        private void AplicarEscalaComun()
        {
            if (root == null || !EncuadreEscenarioRA.ObtenerLimites(root.transform, out var limites)) return;
            float anchoObjetivo = anchoEscenarioReferencia * EscalaEscenarioRA.FactorBase * multiplicadorEscala;
            if (!EscalaEscenarioRA.AjustarAncho(root.transform, limites, anchoObjetivo)) return;
            Transform superficie = root.transform.parent;
            Vector3 punto = superficie != null ? superficie.TransformPoint(puntoAnclajeLocal) : puntoAnclajeLocal;
            GeometriaColocacionRA.ApoyarEnSuperficie(root.transform, limites, punto, 0f);
        }

        private bool EsApoyoDePalabraGuiado(PasoRAModel paso) => UsaFooterGuiado && !EsRA1 &&
            paso != null && paso.tipo != "historia" && !paso.sin_evaluacion_linguistica;

        private string OracionApoyoGuiada => EsApoyoDePalabraGuiado(aciertoPendienteGuiado)
            ? aciertoPendienteGuiado.refuerzo_oracion_id : null;

        private bool EsRA2 => gestorRA?.EscenarioActual?.id == "ra_familia_vestimenta";
        private bool EsRA3 => gestorRA?.EscenarioActual?.id == "ra_casa_objetos";
        private bool UsaSeleccionGuiadaRA1 => EsRA1 && mision?.PasoActual?.tipo == "escuchar_seleccionar";
        private bool UsaFooterGuiado => experiencia != null && gestorRA?.EscenarioActual != null && (!EsRA1 || UsaSeleccionGuiadaRA1);
        private bool PuedeInteractuarGuiado => !destruido && isActiveAndEnabled && !pausadoRA1 &&
            root != null && root.activeInHierarchy && colocador.TieneSeguimiento;
        private bool OcupadoGuiado => audioGuiado || animandoColocacion || procesandoVoz || iniciandoMicrofono || grabacionVoz.EstaGrabando;

        private void ConectarExperienciaGuiada()
        {
            if (experiencia == null) return;
            experiencia.AccionPrincipal += EjecutarAccionGuiada;
            experiencia.RepetirDialogo += RepetirAudioGuiado;
            experiencia.RespuestaEditada += EditarRespuestaEscrita;
            if (experiencia.Tarjeta != null)
            {
                experiencia.Tarjeta.ArrastreIniciado += IniciarArrastreTarjeta;
                experiencia.Tarjeta.Soltada += SoltarTarjeta;
            }
        }

        private void DesconectarExperienciaGuiada()
        {
            if (experiencia == null) return;
            experiencia.AccionPrincipal -= EjecutarAccionGuiada;
            experiencia.RepetirDialogo -= RepetirAudioGuiado;
            experiencia.RespuestaEditada -= EditarRespuestaEscrita;
            if (experiencia.Tarjeta != null)
            {
                experiencia.Tarjeta.ArrastreIniciado -= IniciarArrastreTarjeta;
                experiencia.Tarjeta.Soltada -= SoltarTarjeta;
            }
        }

        private void ActualizarExperienciaGuiada()
        {
            if (!PuedeInteractuarGuiado) return;
            view.MostrarExperienciaGuiada();
            ActualizarDialogoPersistente();
            if (!inicioGuiado)
            {
                experiencia.Tarjeta?.Ocultar();
                experiencia.MostrarEscritura(false);
                experiencia.Mostrar("Instrucciones", ContenidoExperienciaRA.InstruccionBienvenida,
                    "Comenzar", true, bienvenida: true);
                return;
            }
            if (mision.Completada)
            {
                cantidades?.Restablecer();
                experiencia.Tarjeta?.Ocultar();
                experiencia.MostrarEscritura(false);
                bool otraZona = gestorRA.EscenarioActual.id == "ra_animales_ecosistema" && !exploracionTerrestreGuiada;
                experiencia.Mostrar(ContenidoExperienciaRA.TituloCompletado(gestorRA.EscenarioActual.id),
                    otraZona ? "Explora los animales acuáticos. Pulsa SIGUIENTE para volver a la zona terrestre."
                        : "Toca los elementos para explorar. Pulsa Finalizar para volver al mapa.",
                    otraZona ? "Siguiente" : "Finalizar", !OcupadoGuiado, ExperienciaRAView.Estado.Correcto, guiaVisual: IconoRA.Completada);
                return;
            }
            var paso = mision.PasoActual;
            if (paso == null) return;
            if (faseVisible != mision.FaseActual.id)
            {
                cantidades?.Restablecer();
                root.GetComponent<PresentacionFasesRA>()?.MostrarFase(mision.FaseActual.id);
                AplicarEscalaComun();
                faseVisible = mision.FaseActual.id;
            }
            bool nuevo = ultimoEstimulo != paso;
            if (nuevo)
            {
                DetenerAudioGuiado();
                cantidades?.Restablecer();
                ultimoEstimulo = paso;
                cantidades?.PrepararActividad(paso);
                feedbackActividad = string.Empty;
                esperandoRespuestaEscrita = false;
                estadoErrorGuiado = ExperienciaRAView.Estado.Instruccion;
                aciertoPendienteGuiado = null;
                evaluacionPendienteGuiada = null;
                oracionApoyoMostrada = false;
                LimpiarResultadoVoz();
                LimpiarMarcasObjetos();
                if (paso.tipo != "escribir")
                {
                    audioGuiado = true;
                    rutinaAudioGuiada = StartCoroutine(EscucharActividadGuiada(paso));
                }
            }
            bool escritura = paso.tipo == "escribir" && (aciertoPendienteGuiado == null ||
                (evaluacionPendienteGuiada?.aprobado == false && !oracionApoyoMostrada));
            string texto = ContenidoActividadRA.TextoVisible(paso, PerfilRepository.ObtenerPerfil().nombre);
            string instruccionActividad = ContenidoExperienciaRA.Instruccion(paso, texto, mision.EsperandoDestino, EsRA2);
            string diagnosticoMorfologico = null;
            string instruccion;
            string accion;
            string titulo;
            var estado = ExperienciaRAView.Estado.Instruccion;
            if (aciertoPendienteGuiado != null)
            {
                titulo = paso.EsHistoriaInteractiva ? "¡Listo!" : "¡Correcto!";
                var oracion = CorpusRepository.ObtenerOracion(OracionApoyoGuiada);
                if (oracionApoyoMostrada)
                {
                    titulo = "Oración";
                    instruccion = ContenidoExperienciaRA.RepetirOracion;
                    accion = "Siguiente";
                    estado = ExperienciaRAView.Estado.Instruccion;
                }
                else if (evaluacionPendienteGuiada != null && (paso.tipo == "repetir_voz" || paso.tipo == "escribir"))
                {
                    var feedback = paso.tipo == "repetir_voz"
                        ? ContenidoFeedbackRespuesta.Pronunciacion(evaluacionPendienteGuiada, texto, mision.IntentosVoz,
                            oracion != null, oracionApoyoMostrada)
                        : ContenidoFeedbackRespuesta.Escritura(evaluacionPendienteGuiada,
                            oracion != null, oracionApoyoMostrada, incluirCorreccion: true, requiereAprobacion: true);
                    if (paso.tipo == "escribir" && string.Equals(
                        NormalizadorTexto.NormalizarEscritura(experiencia.Respuesta),
                        NormalizadorTexto.NormalizarEscritura(evaluacionPendienteGuiada.textoIngresado),
                        System.StringComparison.OrdinalIgnoreCase))
                        diagnosticoMorfologico = feedback.DiagnosticoMorfologico;
                    titulo = feedback.Titulo;
                    instruccion = feedback.Texto;
                    accion = feedback.Accion;
                    estado = EstadoFeedback(feedback);
                }
                else
                {
                    instruccion = oracion != null && !oracionApoyoMostrada ? ContenidoFeedbackRespuesta.VerOracion
                        : ContenidoFeedbackRespuesta.Avanzar;
                    accion = oracion != null && !oracionApoyoMostrada ? "Continuar" : "Siguiente";
                    estado = paso.EsHistoriaInteractiva ? ExperienciaRAView.Estado.Instruccion : ExperienciaRAView.Estado.Correcto;
                }
            }
            else
            {
                titulo = ContenidoExperienciaRA.TituloActividad(paso);
                instruccion = instruccionActividad;
                accion = paso.tipo switch
                {
                    "repetir_voz" => iniciandoMicrofono ? "Preparando…" : procesandoVoz ? "Comprobando…" : grabacionVoz.EstaGrabando ? "Detener" :
                        mision.PuedeContinuarVoz ? "Siguiente" : "Hablar",
                    "escribir" => "Comprobar",
                    "historia" => !paso.EsHistoriaInteractiva || paso.continuar_permitido ? "Siguiente" : "Escuchar",
                    _ => "Escuchar"
                };
                if (paso.tipo != "repetir_voz" && estadoErrorGuiado != ExperienciaRAView.Estado.Instruccion) estado = estadoErrorGuiado;
                else if (paso.tipo != "repetir_voz" && !string.IsNullOrEmpty(feedbackActividad) && mision.Errores > 0)
                    estado = ExperienciaRAView.Estado.Incorrecto;
                if (estado == ExperienciaRAView.Estado.Incorrecto) titulo = "Incorrecto";
                else if (estado == ExperienciaRAView.Estado.Reintento && paso.tipo == "escribir") titulo = "Casi";
                if (estado != ExperienciaRAView.Estado.Instruccion)
                    instruccion = paso.tipo == "escribir" ? feedbackActividad : ContenidoExperienciaRA.Reintento(paso);
                else if (!string.IsNullOrEmpty(feedbackActividad)) instruccion = feedbackActividad;
                AplicarResultadoVoz(paso, ref titulo, ref instruccion, ref accion, ref estado);
                if (paso.tipo == "escribir" && evaluacionPendienteGuiada?.aprobado == false)
                {
                    var feedback = ContenidoFeedbackRespuesta.Escritura(evaluacionPendienteGuiada,
                        incluirCorreccion: true, requiereAprobacion: true);
                    titulo = feedback.Titulo;
                    instruccion = feedback.Texto;
                    feedbackActividad = feedback.Texto;
                    diagnosticoMorfologico = feedback.DiagnosticoMorfologico;
                    accion = feedback.Accion;
                    estado = EstadoFeedback(feedback);
                }
            }
            bool apoyo = aciertoPendienteGuiado != null && !string.IsNullOrEmpty(OracionApoyoGuiada);
            if (audioGuiado && !apoyo) accion = "Escuchando…";
            else if (animandoColocacion) accion = "Un momento…";
            bool disponible = (!audioGuiado || apoyo) && !animandoColocacion && !procesandoVoz && !iniciandoMicrofono;
            if (paso.EsHistoriaInteractiva && aciertoPendienteGuiado == null &&
                !paso.continuar_permitido && string.IsNullOrEmpty(paso.estimulo_id)) disponible = false;
            if (escritura && evaluacionPendienteGuiada?.aprobado == false) disponible = false;
            experiencia.Mostrar(titulo, instruccion, accion, disponible, estado, guiaVisual:
                oracionApoyoMostrada ? IconoRA.Mision : paso.tipo == "repetir_voz" ? IconoRA.Hablar :
                paso.tipo == "arrastrar" ? mision.EsperandoDestino ? IconoRA.Mover : IconoRA.Colocar : IconoRA.Objetivo,
                diagnosticoMorfologico: diagnosticoMorfologico);
            experiencia.MostrarEscritura(escritura, nuevo, !animandoColocacion && !procesandoVoz && !iniciandoMicrofono,
                instruccionActividad, aciertoPendienteGuiado != null ? instruccion : feedbackActividad,
                aciertoPendienteGuiado != null ? string.Empty : estado == ExperienciaRAView.Estado.Instruccion
                    ? instruccionActividad : string.Empty);
            if (paso.UsaTarjeta && aciertoPendienteGuiado == null)
                experiencia.Tarjeta?.Mostrar(paso.texto_tarjeta, !OcupadoGuiado);
            else experiencia.Tarjeta?.Ocultar();
            foreach (var objeto in objetos.Values)
                objeto.EstablecerResaltado((escritura || paso.tipo == "repetir_voz" || paso.EsHistoriaInteractiva)
                    && aciertoPendienteGuiado == null && objeto.EstadoActual == ObjetoRAView.EstadoSeleccion.Neutro && objeto.ObjetoId == paso.objetivo_objeto_id);
            foreach (var destino in destinos.Values)
                destino.MostrarIndicador(aciertoPendienteGuiado == null && mision.EsperandoDestino && destino.DestinoId == paso.destino_id);
        }

        // Despacha la acción según el paso curricular: comprobar escritura, practicar voz o continuar.
        // La confirmación pendiente permite leer el feedback antes de registrar el avance de la misión.
        private void EjecutarAccionGuiada()
        {
            if (AyudaBloqueaInteraccion) return;
            if (!UsaFooterGuiado || !PuedeInteractuarGuiado || animandoColocacion || procesandoVoz) return;
            if (!inicioGuiado) { DetenerAudioGuiado(); inicioGuiado = true; ActualizarMision(); return; }
            if (mision?.PasoActual?.tipo == "escribir" && evaluacionPendienteGuiada?.aprobado == false) return;
            if (audioGuiado && aciertoPendienteGuiado == null) return;
            if (mision.Completada)
            {
                if (gestorRA.EscenarioActual.id == "ra_animales_ecosistema" && !exploracionTerrestreGuiada)
                {
                    exploracionTerrestreGuiada = true;
                    experiencia.Dialogo.Ocultar();
                    root.GetComponent<PresentacionFasesRA>()?.MostrarFase("terrestre");
                    AplicarEscalaComun();
                    ActualizarMision();
                }
                else VolverAlMapa();
                return;
            }
            if (aciertoPendienteGuiado != null)
            {
                if (!oracionApoyoMostrada && CorpusRepository.ObtenerOracion(OracionApoyoGuiada) != null)
                {
                    MostrarOracionApoyoGuiada();
                    return;
                }
                DetenerAudioGuiado();
                var paso = aciertoPendienteGuiado;
                var resultado = evaluacionPendienteGuiada;
                aciertoPendienteGuiado = null;
                evaluacionPendienteGuiada = null;
                if (mision.PasoActual != paso) return;
                if (resultado != null && paso.tipo == "escribir") mision.ConfirmarEscritura(resultado);
                else if (resultado != null) mision.RegistrarEvaluacion(resultado);
                else if (paso.tipo == "arrastrar") mision.RegistrarDestino(paso.destino_id);
                else mision.RegistrarObjeto(paso.objetivo_objeto_id);
                return;
            }
            if (mision.PasoActual.tipo == "escribir") EvaluarEscrituraGuiada(experiencia.Respuesta);
            else if (mision.PasoActual.EsHistoriaPresentacion) mision.AvanzarNarrativa();
            else if (mision.PasoActual.EsHistoriaInteractiva && mision.PasoActual.continuar_permitido)
                StartCoroutine(ConfirmarAciertoGuiado(mision.PasoActual, null));
            else if (mision.PasoActual.tipo == "repetir_voz")
            {
                if (mision.PuedeContinuarVoz) mision.ContinuarVoz();
                else PracticarVoz();
            }
            else RepetirAudioGuiado();
        }

        private void TocarObjetoGuiado(ObjetoRAView objeto)
        {
            if (!PuedeInteractuarGuiado || animandoColocacion || procesandoVoz || iniciandoMicrofono || grabacionVoz.EstaGrabando || objeto == null ||
                !objetos.ContainsKey(objeto.ObjetoId)) return;
            if (aciertoPendienteGuiado != null)
            {
                if (oracionApoyoMostrada) RepetirAudioGuiado();
                else EscucharObjetoGuiado(objeto);
                return;
            }
            if (!inicioGuiado || mision.Completada)
            {
                EscucharObjetoGuiado(objeto);
                return;
            }
            DetenerAudioGuiado();
            var paso = mision.PasoActual;
            if (paso.tipo == "repetir_voz" || paso.tipo == "escribir" || paso.EsHistoriaPresentacion || paso.UsaTarjeta || mision.EsperandoDestino)
            {
                EscucharObjetoGuiado(objeto);
                return;
            }
            if (objeto.ObjetoId != paso.objetivo_objeto_id)
            {
                feedbackActividad = paso.objetivo_objeto_id == "obj_pez" &&
                    (objeto.ObjetoId == "obj_pez_amarillo" || objeto.ObjetoId == "obj_pejerrey" || objeto.ObjetoId == "obj_trucha")
                    ? "También es un pez. En este paso busca el modelo de pez genérico."
                    : ContenidoExperienciaRA.Reintento(paso);
                estadoErrorGuiado = ExperienciaRAView.Estado.Incorrecto;
                experiencia.ReproducirSonidoRespuesta(estadoErrorGuiado);
                objeto.MostrarEstadoSeleccion(ObjetoRAView.EstadoSeleccion.Incorrecto);
                mision.RegistrarObjeto(objeto.ObjetoId);
                EscucharObjetoGuiado(objeto);
                return;
            }
            feedbackActividad = string.Empty;
            estadoErrorGuiado = ExperienciaRAView.Estado.Instruccion;
            objeto.MostrarEstadoSeleccion(ObjetoRAView.EstadoSeleccion.Correcto);
            if (paso.tipo == "arrastrar")
            {
                mision.RegistrarObjeto(objeto.ObjetoId);
                EscucharObjetoGuiado(objeto);
            }
            else StartCoroutine(ConfirmarAciertoGuiado(paso, objeto));
        }

        private void EditarRespuestaEscrita(string respuesta)
        {
            if (mision?.PasoActual?.tipo == "escribir" && evaluacionPendienteGuiada?.aprobado == false)
            {
                evaluacionPendienteGuiada = null;
                aciertoPendienteGuiado = null;
                estadoErrorGuiado = ExperienciaRAView.Estado.Instruccion;
                feedbackActividad = string.Empty;
                esperandoRespuestaEscrita = false;
                ActualizarMision();
                return;
            }
            if (!esperandoRespuestaEscrita || string.IsNullOrWhiteSpace(respuesta)) return;
            esperandoRespuestaEscrita = false;
            feedbackActividad = string.Empty;
            ActualizarMision();
        }

        // Escritura contextualizada en RA: obtiene la referencia del paso, evalúa ortografía y agrega
        // diagnóstico morfológico. Solo un acierto pasa a la presentación y confirmación del avance.
        private void EvaluarEscrituraGuiada(string respuesta)
        {
            if (!PuedeInteractuarGuiado || !inicioGuiado || OcupadoGuiado || aciertoPendienteGuiado != null ||
                mision.PasoActual?.tipo != "escribir") return;
            if (string.IsNullOrWhiteSpace(respuesta))
            {
                esperandoRespuestaEscrita = true;
                feedbackActividad = "Escribe una respuesta antes de comprobar.";
                ActualizarMision();
                return;
            }
            var paso = mision.PasoActual;
            var resultado = MotorEvaluacion.EvaluarEscritura(ContenidoActividadRA.TextoEsperado(paso), respuesta.Trim(),
                CorpusRepository.Morfologia);
            if (resultado.aprobado)
            {
                StartCoroutine(ConfirmarAciertoGuiado(paso, objetos[paso.objetivo_objeto_id], resultado));
                return;
            }
            esperandoRespuestaEscrita = false;
            DetenerAudioGuiado();
            aciertoPendienteGuiado = null;
            evaluacionPendienteGuiada = resultado;
            oracionApoyoMostrada = false;
            estadoErrorGuiado = resultado.colorSemaforo == "amarillo" ? ExperienciaRAView.Estado.Reintento : ExperienciaRAView.Estado.Incorrecto;
            experiencia.ReproducirSonidoRespuesta(estadoErrorGuiado);
            ActualizarMision();
        }

        private void ColocarArrastradoGuiado(ObjetoRAView origen, DestinoRA destino)
        {
            if (AyudaBloqueaInteraccion) return;
            if (UsaFooterGuiado && origen != null && origen.ObjetoId == mision?.PasoActual?.objetivo_objeto_id)
            {
                if (destino == null)
                {
                    feedbackActividad = ContenidoExperienciaRA.Reintento(mision.PasoActual);
                    estadoErrorGuiado = ExperienciaRAView.Estado.Incorrecto;
                    experiencia.ReproducirSonidoRespuesta(estadoErrorGuiado);
                    mision.RegistrarDestino(null);
                    return;
                }
                TocarDestino(destino);
            }
        }

        private void IniciarArrastreTarjeta()
        {
            if (AyudaBloqueaInteraccion) return;
            var paso = mision?.PasoActual;
            if (!PuedeInteractuarGuiado || OcupadoGuiado || paso?.UsaTarjeta != true || aciertoPendienteGuiado != null) return;
            if (!mision.EsperandoDestino) mision.RegistrarObjeto(paso.objetivo_objeto_id);
        }

        private void SoltarTarjeta(Vector2 posicion)
        {
            if (AyudaBloqueaInteraccion) return;
            var paso = mision?.PasoActual;
            if (!PuedeInteractuarGuiado || OcupadoGuiado || paso?.UsaTarjeta != true || aciertoPendienteGuiado != null) return;
            DestinoRA destino = null;
            if (interaccion.ResolverImpacto(interaccion.Camara.ScreenPointToRay(posicion), out var impacto))
            {
                destino = impacto.collider.GetComponentInParent<DestinoRA>();
                if (impacto.collider.GetComponentInParent<ObjetoRAView>()?.ObjetoId == paso.objetivo_objeto_id &&
                    destinos.TryGetValue(paso.destino_id, out var receptor)) destino = receptor;
            }
            if (destinos.TryGetValue(paso.destino_id, out var esperado) &&
                (esperado.ContieneEnPantalla(posicion, interaccion.Camara) ||
                 esperado.SolapaTarjetaEnPantalla(experiencia.Tarjeta.UltimaAreaSoltada, interaccion.Camara)))
                destino = esperado;
            if (destino == null || destino.DestinoId != paso.destino_id)
            {
                feedbackActividad = ContenidoExperienciaRA.Reintento(paso);
                estadoErrorGuiado = ExperienciaRAView.Estado.Incorrecto;
                experiencia.ReproducirSonidoRespuesta(estadoErrorGuiado);
                mision.RegistrarDestino(destino?.DestinoId);
                return;
            }
            feedbackActividad = string.Empty;
            estadoErrorGuiado = ExperienciaRAView.Estado.Instruccion;
            ConservarDialogoResuelto(objetos[paso.objetivo_objeto_id]);
            experiencia.Tarjeta?.Ocultar();
            StartCoroutine(ConfirmarAciertoGuiado(paso, objetos[paso.objetivo_objeto_id]));
        }

        private IEnumerator ConfirmarAciertoGuiado(PasoRAModel paso, ObjetoRAView actor, ResultadoEvaluacion resultado = null)
        {
            if (mision.PasoActual != paso || aciertoPendienteGuiado != null) yield break;
            DetenerAudioGuiado();
            aciertoPendienteGuiado = paso;
            oracionApoyoMostrada = false;
            if (paso.tipo != "historia" && !paso.sin_evaluacion_linguistica)
                experiencia.ReproducirSonidoRespuesta(ExperienciaRAView.Estado.Correcto);
            evaluacionPendienteGuiada = resultado;
            animandoColocacion = true;
            var palabra = actor != null ? CorpusRepository.ObtenerPalabra(actor.PalabraId) : null;
            actor?.MostrarEstadoSeleccion(ObjetoRAView.EstadoSeleccion.Correcto);
            if (EsRA3 && paso.reaccion_id == "ventanas_cerrar") MostrarDialogoActorGuiado(paso);
            else if (palabra != null) ConservarDialogoResuelto(actor);
            experiencia.MostrarEscritura(false);
            ActualizarMision();
            if (palabra != null && paso.tipo == "escuchar_seleccionar")
                yield return ReproducirAudioGuiado(palabra.id, "palabra");
            if (actor != null && !string.IsNullOrEmpty(paso.destino_acierto_id) &&
                destinos.TryGetValue(paso.destino_acierto_id, out var destinoAcierto) &&
                root.GetComponent<IAccionEscenarioRA>()?.SustituyeColocacion(actor.ObjetoId) != true)
                yield return AnimarColocacionObjeto(actor, destinoAcierto, paso.colocacion_vertical);
            yield return PresentarAcierto(paso, actor);
            while (!PuedeInteractuarGuiado && mision.PasoActual == paso) yield return null;
            animandoColocacion = false;
            if (paso.tipo == "arrastrar") RetroalimentacionHapticaService.ConfirmarColocacion();
            if (mision.PasoActual == paso) ActualizarMision();
        }

        private IEnumerator EscucharActividadGuiada(PasoRAModel paso)
        {
            // Esperar un frame permite terminar la composición del footer antes de reproducir.
            yield return null;
            if (!PuedeInteractuarGuiado || mision.PasoActual != paso) yield break;
            if (paso.mostrar_card_escenario && gestorRA.EscenarioActual.card_escenario is CardEscenarioRAModel card)
            {
                var ancla = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == card.ancla);
                var palabra = CorpusRepository.ObtenerPalabra(card.palabra_id);
                if (ancla != null && palabra != null)
                    experiencia.Dialogo.Mostrar(ancla, interaccion.Camara, palabra.uch, true, palabra.es);
            }
            if (paso.tipo == "repetir_voz")
                MostrarDialogoGuiado(objetos[paso.objetivo_objeto_id], ContenidoActividadRA.TextoVisible(paso,
                    PerfilRepository.ObtenerPerfil().nombre), ContenidoActividadRA.Traduccion(paso,
                    PerfilRepository.ObtenerPerfil().nombre), true);
            if (!string.IsNullOrEmpty(paso.contexto_oracion_id))
                yield return ReproducirAudioGuiado(paso.contexto_oracion_id, "oracion");
            if (paso.EsHistoriaInteractiva) MostrarDialogoActorGuiado(paso);
            if (!string.IsNullOrEmpty(paso.estimulo_id))
                yield return ReproducirAudioGuiado(paso.estimulo_id, paso.estimulo_tipo);
            if (paso.EsHistoriaPresentacion)
            {
                animandoColocacion = true;
                ActualizarMision();
                objetos.TryGetValue(paso.objetivo_objeto_id ?? string.Empty, out var actor);
                yield return root.GetComponent<PresentacionReaccionesRA>().Ejecutar(paso.reaccion_id, actor?.transform);
                while (!PuedeInteractuarGuiado && mision.PasoActual == paso) yield return null;
                animandoColocacion = false;
            }
            audioGuiado = false;
            rutinaAudioGuiada = null;
            if (mision.PasoActual == paso) ActualizarMision();
        }

        private IEnumerator ReproducirAudioGuiado(string id, string tipo)
        {
            if (tipo == "oracion") LimpiarMarcasObjetos();
            audioGuiado = true;
            ActualizarMision();
            var tarea = tipo == "oracion" ? AudioService.Instance.ReproducirOracionAsync(id)
                : AudioService.Instance.ReproducirPalabraAsync(id);
            while (!tarea.IsCompleted) yield return null;
            bool disponible = !tarea.IsFaulted && !tarea.IsCanceled && AudioService.Instance.EstaReproduciendo;
            while (AudioService.Instance.EstaReproduciendo) yield return null;
            audioGuiado = false;
            if (mision.Completada) rutinaAudioGuiada = null;
            if (!disponible) feedbackActividad = "Audio no disponible. Reintentar.";
            ActualizarMision();
        }

        private void RepetirAudioGuiado()
        {
            if (!UsaFooterGuiado || !inicioGuiado || !PuedeInteractuarGuiado ||
                animandoColocacion || procesandoVoz || iniciandoMicrofono || grabacionVoz.EstaGrabando) return;
            if (aciertoPendienteGuiado != null)
            {
                if (oracionApoyoMostrada)
                {
                    DetenerAudioGuiado();
                    rutinaAudioGuiada = StartCoroutine(EscucharApoyoGuiado(OracionApoyoGuiada));
                }
                else if (objetos.TryGetValue(aciertoPendienteGuiado.objetivo_objeto_id, out var actor))
                    EscucharObjetoGuiado(actor);
                return;
            }
            if (audioGuiado) return;
            feedbackActividad = string.Empty;
            if (mision.Completada)
            {
                if (!string.IsNullOrEmpty(ultimaPalabraGuiada))
                    rutinaAudioGuiada = StartCoroutine(ReproducirAudioGuiado(ultimaPalabraGuiada, "palabra"));
            }
            else if (mision.PasoActual.tipo != "escribir")
            {
                audioGuiado = true;
                rutinaAudioGuiada = StartCoroutine(EscucharActividadGuiada(mision.PasoActual));
            }
        }

        private void MostrarOracionApoyoGuiada()
        {
            var oracion = CorpusRepository.ObtenerOracion(OracionApoyoGuiada);
            if (oracion == null) return;
            DetenerAudioGuiado();
            oracionApoyoMostrada = true;
            LimpiarMarcasObjetos();
            cantidades?.Mostrar(aciertoPendienteGuiado);
            string objetoId = string.IsNullOrEmpty(aciertoPendienteGuiado.objeto_oracion_id)
                ? aciertoPendienteGuiado.objetivo_objeto_id : aciertoPendienteGuiado.objeto_oracion_id;
            if (objetos.TryGetValue(objetoId, out var actor))
                MostrarDialogoGuiado(actor, oracion.uch, oracion.es, true);
            rutinaAudioGuiada = StartCoroutine(EscucharApoyoGuiado(oracion.id));
            ActualizarMision();
        }

        private IEnumerator EscucharApoyoGuiado(string oracionId)
        {
            yield return null;
            yield return ReproducirAudioGuiado(oracionId, "oracion");
            rutinaAudioGuiada = null;
        }

        private bool RegistrarResultadoGuiado(ResultadoEvaluacion resultado)
        {
            estadoErrorGuiado = ContenidoFeedbackRespuesta.SinDeteccion(resultado) ? ExperienciaRAView.Estado.Instruccion : resultado.porcentajeSimilitud >= 60
                ? ExperienciaRAView.Estado.Reintento : ExperienciaRAView.Estado.Incorrecto;
            if (resultado.evaluado) experiencia.ReproducirSonidoRespuesta(estadoErrorGuiado);
            return mision.RegistrarEvaluacion(resultado);
        }

        private void MostrarDialogoGuiado(ObjetoRAView actor, string uch, string espanol, bool conAudio)
        {
            experiencia.Dialogo.Mostrar(actor.AnclaDialogo, interaccion.Camara, uch, conAudio, espanol);
        }

        private void MostrarDialogoActorGuiado(PasoRAModel paso)
        {
            if (EsRA3 && paso.reaccion_id == "ventanas_cerrar" &&
                objetos.TryGetValue(paso.objetivo_objeto_id, out var ventana))
            {
                MostrarDialogoGuiado(ventana, ContenidoActividadRA.TextoVisible(paso, string.Empty),
                    ContenidoActividadRA.Traduccion(paso, string.Empty), true);
                return;
            }
            foreach (var actor in root.GetComponentsInChildren<ActorNarrativoRA>(true))
                if (actor.ObjetoId == paso.objetivo_objeto_id)
                    experiencia.Dialogo.Mostrar(actor.AnclaDialogo, interaccion.Camara,
                        ContenidoActividadRA.TextoVisible(paso, string.Empty), true,
                        ContenidoActividadRA.Traduccion(paso, string.Empty));
        }

        private void DetenerAudioGuiado()
        {
            if (rutinaAudioGuiada != null) StopCoroutine(rutinaAudioGuiada);
            rutinaAudioGuiada = null;
            audioGuiado = false;
            if (UsaFooterGuiado) AudioService.DetenerSiExiste();
        }

        private void SuspenderExperienciaGuiada()
        {
            bool preparacionInterrumpida = rutinaAudioGuiada != null && aciertoPendienteGuiado == null && !mision.Completada;
            DetenerAudioGuiado();
            if (preparacionInterrumpida)
            {
                ultimoEstimulo = null;
                animandoColocacion = false;
            }
            experiencia.Ocultar(conservarDialogo: true);
        }

        private void ReiniciarEstadoGuiado()
        {
            LimpiarDialogoPersistente();
            inicioGuiado = false;
            aciertoPendienteGuiado = null;
            evaluacionPendienteGuiada = null;
            oracionApoyoMostrada = false;
            ultimaPalabraGuiada = null;
            exploracionTerrestreGuiada = false;
            DetenerAudioGuiado();
            if (!destruido && UsaFooterGuiado)
            {
                experiencia.MostrarEscritura(false, true);
                experiencia.Ocultar();
            }
        }
    }
}
