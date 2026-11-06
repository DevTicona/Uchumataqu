using System;
using System.Collections;
using Assets.Scripts.Core.Assets;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;
using Assets.Scripts.RA;
using UnityEngine;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public partial class RealidadAumentadaPresenter
    {
        private bool inicioRA1;
        private bool audioRA1;
        private bool pausadoRA1;
        private bool reanudarDialogoRA1;
        private int solicitudesMicrofonoRA1;
        private Coroutine rutinaAudioRA1;
        private Coroutine rutinaTurnoRA1;
        private Coroutine rutinaAnimacionRA1;
        private Action repetirExploracionRA1;
        private PasoRAModel pasoCorrectoRA1;
        private string respuestaCorrectaRA1;

        private bool EsRA1 => experiencia != null &&
            gestorRA?.EscenarioActual?.id == "ra_encuentro_comunicacion";
        private bool PuedeEjecutarTurnoRA1 => !destruido && isActiveAndEnabled && !pausadoRA1 &&
            root != null && root.activeInHierarchy && colocador.TieneSeguimiento;
        private bool InteraccionRA1Bloqueada => !inicioRA1 || rutinaTurnoRA1 != null ||
            animandoColocacion || audioRA1 || grabacionVoz.EstaGrabando || procesandoVoz || iniciandoMicrofono;

        private void ConectarExperienciaRA1()
        {
            if (experiencia == null) return;
            experiencia.AccionPrincipal += EjecutarAccionRA1;
            experiencia.RepetirDialogo += RepetirAudioRA1;
        }

        private void DesconectarExperienciaRA1()
        {
            if (experiencia == null) return;
            experiencia.AccionPrincipal -= EjecutarAccionRA1;
            experiencia.RepetirDialogo -= RepetirAudioRA1;
            DetenerTurnoRA1();
        }

        private void ReiniciarEstadoRA1()
        {
            LimpiarResultadoVoz();
            repetirExploracionRA1 = null;
            pasoCorrectoRA1 = null;
            reanudarDialogoRA1 = false;
            DetenerTurnoRA1();
            if (!destruido) experiencia?.Ocultar();
        }

        private void ActualizarExperienciaRA1()
        {
            if (!PuedeEjecutarTurnoRA1) return;
            view.MostrarExperienciaGuiada();
            ActualizarDialogoPersistente();
            if (!inicioRA1)
            {
                experiencia.Mostrar("Instrucciones",
                    ContenidoExperienciaRA.InstruccionBienvenida,
                    "Comenzar", true, bienvenida: true);
                return;
            }
            if (mision.Completada)
            {
                experiencia.Mostrar("¡Encuentro completado!",
                    "Toca los personajes para explorar. Pulsa Finalizar para volver al mapa.",
                    "Finalizar", !audioRA1, ExperienciaRAView.Estado.Correcto, guiaVisual: IconoRA.Completada);
                return;
            }
            var paso = mision.PasoActual;
            if (paso == null) return;
            if (pasoCorrectoRA1 == paso)
            {
                var feedback = ContenidoFeedbackRespuesta.Pronunciacion(resultadoVoz, respuestaCorrectaRA1, mision.IntentosVoz);
                experiencia.Mostrar(feedback.Titulo, feedback.Texto, animandoColocacion ? "Un momento…" : feedback.Accion,
                    !animandoColocacion, ExperienciaRAView.Estado.Correcto);
                return;
            }
            if (ultimoEstimulo != paso)
            {
                ultimoEstimulo = paso;
                feedbackActividad = string.Empty;
                solicitudesMicrofonoRA1 = 0;
                reanudarDialogoRA1 = false;
                rutinaTurnoRA1 = StartCoroutine(EjecutarTurnoRA1(paso));
            }
            else if (reanudarDialogoRA1 && rutinaTurnoRA1 == null && !animandoColocacion &&
                !grabacionVoz.EstaGrabando && !procesandoVoz)
            {
                reanudarDialogoRA1 = false;
                if (!mision.PuedeContinuarVoz) feedbackActividad = string.Empty;
                rutinaTurnoRA1 = StartCoroutine(EjecutarTurnoRA1(paso));
            }
            string texto = ContenidoActividadRA.TextoVisible(paso, PerfilRepository.ObtenerPerfil().nombre);
            bool ocupado = audioRA1 || animandoColocacion || procesandoVoz || rutinaTurnoRA1 != null;
            string accion;
            string titulo;
            string instruccion;
            switch (paso.tipo)
            {
                case "repetir_voz":
                    titulo = "Tu turno";
                    instruccion = "Responde diciendo: “" + texto + "”";
                    accion = iniciandoMicrofono ? "Preparando…" : procesandoVoz ? "Comprobando…" : grabacionVoz.EstaGrabando ? "Terminar" :
                        mision.PuedeContinuarVoz ? "Siguiente" : "Hablar";
                    ocupado |= iniciandoMicrofono;
                    if (!string.IsNullOrEmpty(feedbackActividad)) instruccion += "\n" + feedbackActividad;
                    break;
                case "historia" when paso.EsHistoriaInteractiva:
                    titulo = "Llega el profesor";
                    instruccion = "El profesor se acerca para conversar contigo.";
                    accion = "Acercándose…";
                    ocupado = true;
                    break;
                default:
                    titulo = "Conversación";
                    instruccion = "Escucha. En tu turno, pulsa Hablar.";
                    accion = "Escuchando…";
                    ocupado = true;
                    if (!string.IsNullOrEmpty(feedbackActividad))
                    {
                        instruccion = feedbackActividad;
                        accion = "Siguiente";
                        ocupado = audioRA1 || animandoColocacion || rutinaTurnoRA1 != null;
                    }
                    break;
            }
            var estado = paso.tipo == "repetir_voz" && mision.PuedeContinuarVoz
                ? ExperienciaRAView.Estado.Reintento : ExperienciaRAView.Estado.Instruccion;
            AplicarResultadoVoz(paso, ref titulo, ref instruccion, ref accion, ref estado);
            experiencia.Mostrar(titulo, instruccion, accion, !ocupado, estado,
                guiaVisual: paso.tipo == "repetir_voz" ? IconoRA.Hablar : IconoRA.Mision);
        }

        private IEnumerator EjecutarTurnoRA1(PasoRAModel paso)
        {
            // Deja que la instrucción se dibuje antes de iniciar el turno.
            yield return null;
            if (!PuedeEjecutarTurnoRA1 || mision.PasoActual != paso) yield break;
            if (paso.tipo == "repetir_voz")
            {
                rutinaTurnoRA1 = null;
                ActualizarMision();
                yield break;
            }
            if (paso.EsHistoriaInteractiva)
            {
                if (!objetos.TryGetValue(paso.objetivo_objeto_id, out var profesor)) yield break;
                profesor.gameObject.SetActive(true);
                animandoColocacion = true;
                ActualizarMision();
                yield return PresentarAcierto(paso, profesor);
                while (!PuedeEjecutarTurnoRA1) yield return null;
                animandoColocacion = false;
                rutinaTurnoRA1 = null;
                reanudarDialogoRA1 = false;
                if (PuedeEjecutarTurnoRA1 && mision.PasoActual == paso) mision.RegistrarObjeto(profesor.ObjetoId);
                yield break;
            }
            rutinaAnimacionRA1 = StartCoroutine(PresentarNarrativa(paso));
            MostrarFraseRA1(paso);
            bool conAudio = !string.IsNullOrEmpty(paso.estimulo_id);
            if (conAudio) yield return ReproducirDialogoRA1(paso.estimulo_id, paso.estimulo_tipo);
            else
            {
                float duracion = Mathf.Max(3f, ContenidoActividadRA.TextoVisible(paso,
                    PerfilRepository.ObtenerPerfil().nombre).Length * 0.09f);
                yield return new WaitForSecondsRealtime(duracion);
            }
            while (animandoColocacion) yield return null;
            rutinaAnimacionRA1 = null;
            rutinaTurnoRA1 = null;
            if (!PuedeEjecutarTurnoRA1 || mision.PasoActual != paso) yield break;
            if (conAudio && !string.IsNullOrEmpty(feedbackActividad)) ActualizarMision();
            else mision.AvanzarNarrativa();
        }

        private void EjecutarAccionRA1()
        {
            if (AyudaBloqueaInteraccion) return;
            if (!EsRA1 || UsaSeleccionGuiadaRA1 || !PuedeEjecutarTurnoRA1 || audioRA1 || rutinaTurnoRA1 != null ||
                animandoColocacion || procesandoVoz) return;
            if (!inicioRA1) { inicioRA1 = true; ActualizarMision(); return; }
            if (mision.Completada) { VolverAlMapa(); return; }
            if (pasoCorrectoRA1 == mision.PasoActual)
            {
                var resultado = resultadoVoz;
                pasoCorrectoRA1 = null;
                mision.RegistrarEvaluacion(resultado);
                return;
            }
            if (mision.PasoActual?.tipo == "historia" && !string.IsNullOrEmpty(feedbackActividad))
                mision.AvanzarNarrativa();
            else if (mision.PuedeContinuarVoz) mision.ContinuarVoz();
            else if (mision.PasoActual?.tipo == "repetir_voz") PracticarVoz();
        }

        private void SuspenderExperienciaRA1()
        {
            if (mision?.PasoActual?.EsHistoriaInteractiva == true) DetenerAudioRA1();
            else DetenerTurnoRA1();
            experiencia.Ocultar(conservarDialogo: true);
            reanudarDialogoRA1 = true;
        }

        private void RepetirAudioRA1()
        {
            if (!EsRA1 || UsaSeleccionGuiadaRA1 || !inicioRA1 || !PuedeEjecutarTurnoRA1 || InteraccionRA1Bloqueada) return;
            if (mision.Completada) { repetirExploracionRA1?.Invoke(); return; }
            var paso = mision.PasoActual;
            if (paso?.tipo != "historia") return;
            feedbackActividad = string.Empty;
            rutinaTurnoRA1 = StartCoroutine(EjecutarTurnoRA1(paso));
            ActualizarMision();
        }

        private void MostrarFraseRA1(PasoRAModel paso)
        {
            if (!objetos.TryGetValue(paso.objetivo_objeto_id, out var actor)) return;
            string nombre = PerfilRepository.ObtenerPerfil().nombre;
            MostrarDialogoRA1(actor, ContenidoActividadRA.TextoVisible(paso, nombre),
                !string.IsNullOrEmpty(paso.estimulo_id), ContenidoActividadRA.Traduccion(paso, nombre));
        }

        private void MostrarDialogoRA1(ObjetoRAView actor, string texto, bool audio, string espanol)
        {
            experiencia.Dialogo.Mostrar(actor.AnclaDialogo, interaccion.Camara, texto, audio, espanol);
        }

        private IEnumerator ReproducirDialogoRA1(string id, string tipo)
        {
            if (tipo == "oracion") LimpiarMarcasObjetos();
            audioRA1 = true;
            var tarea = tipo == "oracion" ? AudioService.Instance.ReproducirOracionAsync(id)
                : AudioService.Instance.ReproducirPalabraAsync(id);
            ActualizarMision();
            while (!tarea.IsCompleted) yield return null;
            bool reproduciendo = AudioService.Instance.EstaReproduciendo;
            while (AudioService.Instance.EstaReproduciendo) yield return null;
            audioRA1 = false;
            rutinaAudioRA1 = null;
            if (!reproduciendo) feedbackActividad = "El audio no está disponible. Lee el diálogo; toca al personaje para reintentar o pulsa Continuar.";
            ActualizarMision();
        }

        private void DetenerAudioRA1(bool ocultar = true)
        {
            if (rutinaAudioRA1 != null) StopCoroutine(rutinaAudioRA1);
            rutinaAudioRA1 = null;
            audioRA1 = false;
            if (EsRA1) AudioService.DetenerSiExiste();
            if (!destruido && ocultar) experiencia?.Dialogo.Suspender(pausadoRA1);
        }

        private void DetenerTurnoRA1()
        {
            if (rutinaTurnoRA1 != null) StopCoroutine(rutinaTurnoRA1);
            if (rutinaAnimacionRA1 != null) StopCoroutine(rutinaAnimacionRA1);
            rutinaTurnoRA1 = rutinaAnimacionRA1 = null;
            // Una confirmación de ASR usa su propia corrutina y conserva su bloqueo.
            if (ultimoEstimulo?.tipo != "repetir_voz") animandoColocacion = false;
            DetenerAudioRA1();
        }

        private void ExplorarObjetoRA1(ObjetoRAView actor)
        {
            if (animandoColocacion || !PuedeEjecutarTurnoRA1 || !objetos.ContainsKey(actor.ObjetoId)) return;
            var palabra = CorpusRepository.ObtenerPalabra(actor.PalabraId);
            if (palabra == null) return;
            repetirExploracionRA1 = () =>
            {
                if (actor == null) return;
                MostrarDialogoRA1(actor, palabra.uch, true, palabra.es);
                rutinaAudioRA1 = StartCoroutine(ReproducirDialogoRA1(palabra.id, "palabra"));
            };
            repetirExploracionRA1();
        }
    }
}
