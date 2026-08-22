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
using Assets.Scripts.UI.LeccionBase.Actividades;

namespace Assets.Scripts.UI.LeccionBase
{
    /// <summary>
    /// Presenter de la escena 03_LeccionBase.
    /// Orquesta los pasos pedagógicos continuos de cualquier lección compilada L01-L27.
    /// Especificado en Docs/UI/04-escena-leccion-base.md y Docs/Specifications/04-flujo-leccion-app.md.
    /// </summary>
    public class LeccionBasePresenter : MonoBehaviour
    {
        [SerializeField] private LeccionBaseView view;
        [SerializeField] private NavigationBackHandler backHandler;

        private GestorLeccion _gestorLeccion;
        private bool _esperandoFeedback = false;

        private void Start()
        {
            if (view == null) view = FindAnyObjectByType<LeccionBaseView>();
            if (backHandler == null) backHandler = FindAnyObjectByType<NavigationBackHandler>();

            if (backHandler != null)
            {
                backHandler.Configurar(VolverAlMapa);
            }

            if (view != null)
            {
                view.OnAtrasClick += VolverAlMapa;
                view.OnVolverAlMapaClick += VolverAlMapa;
                view.OnContinuarLeccionClick += VolverAlMapa;

                if (view.Feedback != null)
                {
                    view.Feedback.OnContinuarFeedbackClicked += AvanzarTrasFeedback;
                }
            }

            IniciarEjecucionLeccion();
        }

        private void OnDestroy()
        {
            if (view != null)
            {
                view.OnAtrasClick -= VolverAlMapa;
                view.OnVolverAlMapaClick -= VolverAlMapa;
                view.OnContinuarLeccionClick -= VolverAlMapa;

                if (view.Feedback != null)
                {
                    view.Feedback.OnContinuarFeedbackClicked -= AvanzarTrasFeedback;
                }
            }
        }

        private void IniciarEjecucionLeccion()
        {
            _gestorLeccion = new GestorLeccion();
            string leccionId = ContextoNavegacion.LeccionId;

            if (string.IsNullOrEmpty(leccionId))
            {
                leccionId = "L01"; // Fallback para pruebas directas en editor
            }

            if (!_gestorLeccion.IniciarLeccion(leccionId))
            {
                Debug.LogError($"[LeccionBasePresenter] No se pudo iniciar la lección '{leccionId}'. Volviendo al mapa...");
                VolverAlMapa();
                return;
            }

            if (view != null)
            {
                view.ConfigurarHeader(
                    _gestorLeccion.LeccionActual.titulo,
                    _gestorLeccion.IndicePasoActual + 1,
                    _gestorLeccion.TotalPasos,
                    _gestorLeccion.ProgresoNormalizado
                );
            }

            RenderizarPasoActual();
        }

        private void RenderizarPasoActual()
        {
            var paso = _gestorLeccion.PasoActual;
            if (paso == null)
            {
                FinalizarLeccion();
                return;
            }

            _esperandoFeedback = false;

            if (view != null)
            {
                view.ActualizarProgreso(_gestorLeccion.IndicePasoActual + 1, _gestorLeccion.TotalPasos, _gestorLeccion.ProgresoNormalizado);
                view.OcultarFeedback();
            }

            string tipo = paso.tipo ?? "";

            switch (tipo)
            {
                case "presentacion_palabra":
                    RenderizarPresentacion(paso);
                    break;

                case "seleccion_simple_uch_es":
                case "seleccion_simple_es_uch":
                case "seleccion_multiple_uch_es":
                case "seleccion_multiple_es_uch":
                case "audio_a_uch":
                case "audio_a_es":
                case "forma_a_raiz":
                case "forma_a_es":
                    RenderizarSeleccion(paso);
                    break;

                case "emparejamiento":
                case "memoria":
                    RenderizarEmparejamiento(paso);
                    break;

                case "ordenar_oracion":
                case "dictado_word_bank":
                case "traduccion_constructiva":
                case "completar_oracion":
                    RenderizarWordBank(paso);
                    break;

                default:
                    RenderizarSeleccion(paso);
                    break;
            }
        }

        private void RenderizarPresentacion(PasoLeccionData paso)
        {
            var actView = view.InstanciarPresentacion();
            if (actView == null) return;

            PalabraData palabra = !string.IsNullOrEmpty(paso.objetivo_id)
                ? CorpusRepository.ObtenerPalabraPorId(paso.objetivo_id)
                : null;

            actView.Configurar(paso, palabra);
            actView.OnContinuarClicked += () =>
            {
                _gestorLeccion.RegistrarResultadoPaso(true);
                AvanzarPaso();
            };
        }

        private void RenderizarSeleccion(PasoLeccionData paso)
        {
            var actView = view.InstanciarSeleccionMultiple();
            if (actView == null) return;

            PalabraData palabraObjetivo = !string.IsNullOrEmpty(paso.objetivo_id)
                ? CorpusRepository.ObtenerPalabraPorId(paso.objetivo_id)
                : null;

            bool esUchHaciaEs = paso.tipo == "seleccion_simple_uch_es" || paso.tipo == "seleccion_multiple_uch_es" || paso.tipo == "audio_a_es" || paso.tipo == "forma_a_es";
            string respuestaCorrecta = palabraObjetivo != null ? (esUchHaciaEs ? palabraObjetivo.es : palabraObjetivo.uch) : "";

            List<string> opciones = new List<string>();
            if (!string.IsNullOrEmpty(respuestaCorrecta))
            {
                opciones.Add(respuestaCorrecta);
            }

            if (paso.opciones_ids != null)
            {
                foreach (var id in paso.opciones_ids)
                {
                    if (id == paso.objetivo_id) continue;
                    var p = CorpusRepository.ObtenerPalabraPorId(id);
                    if (p != null)
                    {
                        string op = esUchHaciaEs ? p.es : p.uch;
                        if (!opciones.Contains(op)) opciones.Add(op);
                    }
                }
            }

            Barajar(opciones);

            string prompt = palabraObjetivo != null ? (esUchHaciaEs ? palabraObjetivo.uch : palabraObjetivo.es) : "";
            actView.Configurar(paso.tipo, prompt, opciones, palabraObjetivo?.id);
            actView.OnRespuestaEnviada += (respuesta) =>
            {
                EvaluarRespuestaSeleccion(respuestaCorrecta, respuesta);
            };
        }

        private void EvaluarRespuestaSeleccion(string esperada, string respuestaUsuario)
        {
            ResultadoEvaluacion resultado = MotorEvaluacion.EvaluarTexto(esperada, respuestaUsuario, "seleccion");
            _gestorLeccion.RegistrarResultadoPaso(resultado.aprobado);

            _esperandoFeedback = true;
            if (view != null)
            {
                view.MostrarFeedback(resultado.aprobado, resultado.aprobado ? "¡Correcto!" : "Revisa la opción correcta", resultado.porcentajeSimilitud, esperada);
            }
        }

        private void RenderizarEmparejamiento(PasoLeccionData paso)
        {
            var actView = view.InstanciarEmparejamiento();
            if (actView == null) return;

            Dictionary<string, string> pares = new Dictionary<string, string>();
            if (paso.items_ids != null)
            {
                foreach (var id in paso.items_ids)
                {
                    var p = CorpusRepository.ObtenerPalabraPorId(id);
                    if (p != null && !string.IsNullOrEmpty(p.uch) && !string.IsNullOrEmpty(p.es))
                        pares[p.uch] = p.es;
                }
            }

            actView.Configurar(pares);
            actView.OnEmparejamientoCompletado += (completado) =>
            {
                _gestorLeccion.RegistrarResultadoPaso(completado);
                _esperandoFeedback = true;
                if (view != null)
                {
                    view.MostrarFeedback(true, "¡Pares emparejados correctamente!");
                }
            };
        }

        private void RenderizarWordBank(PasoLeccionData paso)
        {
            var actView = view.InstanciarWordBank();
            if (actView == null) return;

            OracionData oracion = !string.IsNullOrEmpty(paso.oracion_id)
                ? CorpusRepository.ObtenerOracionPorId(paso.oracion_id)
                : null;

            string oracionUch = oracion != null ? oracion.uch : "";
            string promptEs = oracion != null ? oracion.es : "";
            List<string> tokens = MotorEvaluacion.Tokenizar(oracionUch);

            Barajar(tokens);

            actView.Configurar(paso.tipo, promptEs, tokens, oracion?.id);
            actView.OnTokensComprobados += (tokensUsuario) =>
            {
                EvaluarTokensWordBank(oracionUch, tokensUsuario);
            };
        }

        private void EvaluarTokensWordBank(string oracionEsperada, List<string> tokensUsuario)
        {
            ResultadoEvaluacion resultado = ValidadorOrden.EvaluarOrdenOracion(oracionEsperada, tokensUsuario);
            _gestorLeccion.RegistrarResultadoPaso(resultado.aprobado);

            _esperandoFeedback = true;
            if (view != null)
            {
                view.MostrarFeedback(
                    resultado.aprobado,
                    resultado.aprobado ? "¡Oración ordenada perfectamente!" : "El orden de las palabras no es el correcto.",
                    resultado.porcentajeSimilitud,
                    oracionEsperada
                );
            }
        }

        private void AvanzarTrasFeedback()
        {
            if (!_esperandoFeedback) return;
            _esperandoFeedback = false;
            AvanzarPaso();
        }

        private void AvanzarPaso()
        {
            if (_gestorLeccion.AvanzarSiguientePaso())
            {
                RenderizarPasoActual();
            }
            else
            {
                FinalizarLeccion();
            }
        }

        private void FinalizarLeccion()
        {
            _gestorLeccion.FinalizarYGuardar();
            int total = _gestorLeccion.Aciertos + _gestorLeccion.Errores;
            double precision = total > 0 ? ((double)_gestorLeccion.Aciertos / total) * 100.0 : 100.0;

            Debug.Log($"[LeccionBasePresenter] Lección completada: {_gestorLeccion.Aciertos}/{total} ({precision:F0}%)");

            if (view != null)
            {
                view.MostrarResultado(_gestorLeccion.Aciertos, total, precision);
            }
        }

        private void VolverAlMapa()
        {
            NavigationService.IrAMain(PaginaMain.Lecciones);
        }

        private void Barajar<T>(List<T> lista)
        {
            System.Random rng = new System.Random();
            int n = lista.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = lista[k];
                lista[k] = lista[n];
                lista[n] = value;
            }
        }
    }
}
