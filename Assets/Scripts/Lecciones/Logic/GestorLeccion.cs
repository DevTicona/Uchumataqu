using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.Lecciones
{
    /// <summary>
    /// Orquestador del flujo de ejecución de una lección basada en pasos compilados.
    /// Consume el currículo activo y no genera actividades en runtime.
    /// Especificado en Docs/Specifications/04-flujo-leccion-app.md.
    /// </summary>
    public class GestorLeccion
    {
        public LeccionCompiladaModel LeccionActual { get; private set; }
        public int IndicePasoActual { get; private set; }
        public int Aciertos { get; private set; }
        public int Errores { get; private set; }
        public bool UltimoGuardadoExitoso { get; private set; } = true;
        private bool _finalizacionGuardada;
        public int TotalPasos => LeccionActual?.pasos?.Count ?? 0;
        public int TotalPasosObligatorios => LeccionActual?.pasos?.Count(TipoPasoLeccion.EsObligatorio) ?? 0;
        public int PasoObligatorioActual => LeccionActual?.pasos?
            .Take(IndicePasoActual + 1)
            .Count(TipoPasoLeccion.EsObligatorio) ?? 0;
        public float ProgresoNormalizado
        {
            get
            {
                if (TotalPasosObligatorios == 0) return 0f;
                int completados = LeccionActual.pasos.Take(IndicePasoActual)
                    .Count(TipoPasoLeccion.EsObligatorio);
                return (float)completados / TotalPasosObligatorios;
            }
        }

        public PasoLeccionModel PasoActual
        {
            get
            {
                if (LeccionActual == null || LeccionActual.pasos == null || IndicePasoActual >= LeccionActual.pasos.Count)
                    return null;
                return LeccionActual.pasos[IndicePasoActual];
            }
        }

        public bool IniciarLeccion(string leccionId)
        {
            LeccionActual = CorpusRepository.ObtenerLeccion(leccionId);
            if (LeccionActual == null || LeccionActual.pasos == null || LeccionActual.pasos.Count == 0)
            {
                Assets.Scripts.Utils.JsonUtilityHelper.LogError($"[GestorLeccion] No se encontraron pasos compilados para la lección '{leccionId}'.");
                return false;
            }

            IndicePasoActual = 0;
            Aciertos = 0;
            Errores = 0;
            _finalizacionGuardada = false;
            UltimoGuardadoExitoso = true;
            var progreso = ProgresoRepository.ObtenerProgreso();
            var previo = progreso.lecciones?.Find(l => l.leccion_id == leccionId &&
                (l.estado == "en_progreso" || l.sesion_en_progreso));
            // Los índices de otro currículo no identifican la misma actividad. Solo se reinicia la lección pendiente.
            if (previo != null && !string.IsNullOrEmpty(LeccionActual.RevisionContenido) &&
                previo.revision_contenido == LeccionActual.RevisionContenido)
            {
                IndicePasoActual = Mathf.Clamp(previo.paso_actual, 0, TotalPasos);
                Aciertos = Mathf.Max(0, previo.aciertos);
                Errores = Mathf.Max(0, previo.errores);
            }
            return true;
        }

        public void RegistrarResultadoPaso(bool aprobado)
        {
            if (PasoActual != null)
            {
                if (aprobado) Aciertos++;
                else Errores++;
            }
        }

        public bool AvanzarSiguientePaso()
        {
            if (LeccionActual == null || IndicePasoActual >= TotalPasos) return false;
            IndicePasoActual++;
            GuardarEstadoActual();
            return IndicePasoActual < TotalPasos;
        }

        /// <summary>
        /// Persiste la próxima posición reanudable. Un paso resuelto puede guardarse antes de
        /// avanzar la vista, para no repetir la respuesta al salir durante el feedback.
        /// </summary>
        public bool GuardarEstadoActual(bool pasoResuelto = false)
        {
            if (LeccionActual == null) return false;
            if (_finalizacionGuardada) return true;
            return UltimoGuardadoExitoso = GestorProgreso.GuardarLeccionEnProgreso(
                LeccionActual.id,
                Mathf.Min(IndicePasoActual + (pasoResuelto ? 1 : 0), TotalPasos),
                Aciertos,
                Errores,
                LeccionActual.RevisionContenido);
        }

        public bool FinalizarYGuardar()
        {
            if (LeccionActual == null || IndicePasoActual < TotalPasos) return false;
            if (_finalizacionGuardada) return true;
            int totalEvaluables = Aciertos + Errores;
            _finalizacionGuardada = UltimoGuardadoExitoso = GestorProgreso.CompletarLeccion(
                LeccionActual.id, Aciertos, Errores, totalEvaluables > 0 ? totalEvaluables : TotalPasos);
            return _finalizacionGuardada;
        }
    }
}
