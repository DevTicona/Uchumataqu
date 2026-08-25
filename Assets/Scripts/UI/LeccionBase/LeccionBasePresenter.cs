using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.Sistema;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Lecciones;
using Assets.Scripts.PLN;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.UI.LeccionBase
{
    public class LeccionBasePresenter : MonoBehaviour
    {
        [SerializeField] private LeccionBaseView view;
        [SerializeField] private NavigationBackHandler backHandler;

        private readonly System.Random _random = new System.Random();
        private GestorLeccion _gestorLeccion;
        private bool _esperandoFeedback;

        private void Start()
        {
            if (view == null || backHandler == null)
            {
                Debug.LogError("[LeccionBasePresenter] Faltan referencias de composición de la escena.");
                enabled = false;
                return;
            }

            backHandler.Configurar(SolicitarSalida);
            view.OnAtrasClick += SolicitarSalida;
            view.OnVolverAlMapaClick += VolverAlMapa;
            view.OnContinuarLeccionClick += VolverAlMapa;
            view.OnConfirmarSalidaClick += ConfirmarSalida;
            view.OnCancelarSalidaClick += CancelarSalida;
            if (view.Feedback != null)
                view.Feedback.OnContinuarFeedbackClicked += AvanzarTrasFeedback;

            IniciarEjecucionLeccion();
        }

        private void OnDestroy()
        {
            if (view == null) return;
            view.OnAtrasClick -= SolicitarSalida;
            view.OnVolverAlMapaClick -= VolverAlMapa;
            view.OnContinuarLeccionClick -= VolverAlMapa;
            view.OnConfirmarSalidaClick -= ConfirmarSalida;
            view.OnCancelarSalidaClick -= CancelarSalida;
            if (view.Feedback != null)
                view.Feedback.OnContinuarFeedbackClicked -= AvanzarTrasFeedback;
        }

        private void IniciarEjecucionLeccion()
        {
            _gestorLeccion = new GestorLeccion();
            string leccionId = string.IsNullOrEmpty(ContextoNavegacion.LeccionId) ? "L01" : ContextoNavegacion.LeccionId;
            if (!_gestorLeccion.IniciarLeccion(leccionId))
            {
                Debug.LogError($"[LeccionBasePresenter] No se pudo iniciar '{leccionId}'.");
                VolverAlMapa();
                return;
            }

            view.ConfigurarHeader(
                _gestorLeccion.LeccionActual.titulo,
                Mathf.Max(1, _gestorLeccion.PasoObligatorioActual),
                _gestorLeccion.TotalPasosObligatorios,
                _gestorLeccion.ProgresoNormalizado);
            RenderizarPasoActual();
        }

        private void RenderizarPasoActual()
        {
            PasoLeccionData paso = _gestorLeccion.PasoActual;
            if (paso == null)
            {
                FinalizarLeccion();
                return;
            }

            _esperandoFeedback = false;
            view.ActualizarProgreso(
                Mathf.Max(1, _gestorLeccion.PasoObligatorioActual),
                _gestorLeccion.TotalPasosObligatorios,
                _gestorLeccion.ProgresoNormalizado);
            view.OcultarFeedback();

            if (!TipoPasoLeccion.EsSoportado(paso.tipo))
            {
                Debug.LogError($"[LeccionBasePresenter] Tipo de paso no soportado: '{paso.tipo}'.");
                enabled = false;
                return;
            }

            if (TipoPasoLeccion.EsPresentacion(paso.tipo)) RenderizarPresentacion(paso, false);
            else if (TipoPasoLeccion.EsSeleccion(paso.tipo)) RenderizarSeleccion(paso);
            else if (paso.tipo == TipoPasoLeccion.MemoriaUchEspanol) RenderizarMemoria(paso);
            else if (TipoPasoLeccion.EsEmparejamiento(paso.tipo)) RenderizarEmparejamiento(paso);
            else if (TipoPasoLeccion.EsWordBank(paso.tipo)) RenderizarWordBank(paso);
            else if (paso.tipo == TipoPasoLeccion.EscuchaYRepiteAsr) RenderizarPresentacion(paso, true);
        }

        private void RenderizarPresentacion(PasoLeccionData paso, bool practicaOpcional)
        {
            var actividad = view.InstanciarPresentacion();
            if (actividad == null) return;

            PalabraData palabra = CorpusRepository.ObtenerPalabra(paso.objetivo_id);
            actividad.Configurar(paso, palabra);
            actividad.OnAudioClicked += () => ReproducirPalabra(palabra?.id);
            actividad.OnFormaAudioClicked += ReproducirPalabra;
            actividad.OnPracticarVozClicked += () =>
                Debug.Log("[LeccionBasePresenter] La práctica ASR es opcional y se integrará en la Semana 6.");
            actividad.OnContinuarClicked += AvanzarPaso;

            if (!practicaOpcional && palabra != null && ConfiguracionRepository.Obtener().audio_automatico)
                ReproducirPalabra(palabra.id);
        }

        private void RenderizarSeleccion(PasoLeccionData paso)
        {
            var actividad = view.InstanciarSeleccionMultiple();
            if (actividad == null) return;

            ActividadSeleccionData datos;
            try
            {
                datos = ActividadLeccionService.CrearSeleccion(paso);
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
            actividad.OnAudioPreguntaClicked += () =>
            {
                if (!string.IsNullOrEmpty(datos.AudioOracionId)) ReproducirOracion(datos.AudioOracionId);
                else ReproducirPalabra(datos.AudioPalabraId);
            };
            actividad.OnRespuestaEnviada += respuesta => EvaluarSeleccion(datos.RespuestaCorrecta, respuesta);

            if (!string.IsNullOrEmpty(datos.AudioOracionId)) ReproducirOracion(datos.AudioOracionId);
            else if (!string.IsNullOrEmpty(datos.AudioPalabraId)) ReproducirPalabra(datos.AudioPalabraId);
        }

        private void EvaluarSeleccion(string esperada, string respuesta)
        {
            ResultadoEvaluacion resultado = MotorEvaluacion.EvaluarSeleccionExacta(esperada, respuesta);
            RegistrarYMostrarFeedback(resultado, resultado.aprobado ? "¡Correcto!" : "Revisa la opción correcta");
        }

        private void RenderizarEmparejamiento(PasoLeccionData paso)
        {
            var actividad = view.InstanciarEmparejamiento();
            if (actividad == null) return;

            Dictionary<string, string> pares = ActividadLeccionService.CrearPares(paso);
            if (pares.Count == 0)
            {
                Debug.LogError($"[LeccionBasePresenter] El paso '{paso.tipo}' no produjo pares.");
                enabled = false;
                return;
            }

            actividad.Configurar(pares);
            actividad.OnEmparejamientoCompletado += completado =>
            {
                var resultado = MotorEvaluacion.EvaluarSeleccionExacta("completo", completado ? "completo" : "incompleto");
                RegistrarYMostrarFeedback(resultado, "¡Pares emparejados correctamente!");
            };
        }

        private void RenderizarWordBank(PasoLeccionData paso)
        {
            var actividad = view.InstanciarWordBank();
            if (actividad == null) return;

            ActividadWordBankData datos = ActividadLeccionService.CrearWordBank(paso);
            var tokens = new List<string>(datos.Tokens);
            Barajar(tokens);
            actividad.Configurar(datos.Tipo, datos.Prompt, tokens, datos.AudioOracionId);
            actividad.OnAudioPromptClicked += () => ReproducirOracion(datos.AudioOracionId);
            actividad.OnTokensComprobados += respuesta =>
            {
                ResultadoEvaluacion resultado = ValidadorOrden.EvaluarOrdenOracion(datos.RespuestaEsperada, respuesta);
                RegistrarYMostrarFeedback(
                    resultado,
                    resultado.aprobado ? "¡Oración ordenada correctamente!" : "El orden no es correcto.");
            };

            if (paso.tipo == TipoPasoLeccion.DictadoWordBank)
                ReproducirOracion(datos.AudioOracionId);
        }

        private void RenderizarMemoria(PasoLeccionData paso)
        {
            var actividad = view.InstanciarMemoria();
            if (actividad == null) return;
            Dictionary<string, string> pares = ActividadLeccionService.CrearPares(paso);
            if (pares.Count == 0)
            {
                Debug.LogError("[LeccionBasePresenter] La actividad de memoria no produjo pares.");
                enabled = false;
                return;
            }
            actividad.Configurar(pares);
            actividad.OnMemoriaCompletada += completada =>
            {
                ResultadoEvaluacion resultado =
                    MotorEvaluacion.EvaluarSeleccionExacta("completo", completada ? "completo" : "incompleto");
                RegistrarYMostrarFeedback(resultado, "¡Memoria completada!");
            };
        }

        private void RegistrarYMostrarFeedback(ResultadoEvaluacion resultado, string detalle)
        {
            if (_esperandoFeedback) return;
            _gestorLeccion.RegistrarResultadoPaso(resultado.aprobado);
            _esperandoFeedback = true;
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
            _gestorLeccion.FinalizarYGuardar();
            int total = _gestorLeccion.Aciertos + _gestorLeccion.Errores;
            double precision = total > 0 ? (double)_gestorLeccion.Aciertos / total * 100.0 : 100.0;
            view.MostrarResultado(_gestorLeccion.Aciertos, total, precision);
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
            _gestorLeccion?.GuardarEstadoActual();
            VolverAlMapa();
        }

        private void VolverAlMapa() => NavigationService.IrAMain(PaginaMain.Lecciones);

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
