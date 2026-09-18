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
    /// Consume lecciones_compiladas.json y no genera currículo en runtime.
    /// Especificado en Docs/Specifications/04-flujo-leccion-app.md y Docs/UI/04-escena-leccion-base.md.
    /// </summary>
    public class GestorLeccion
    {
        public LeccionCompiladaModel LeccionActual { get; private set; }
        public int IndicePasoActual { get; private set; }
        public int Aciertos { get; private set; }
        public int Errores { get; private set; }
        public bool UltimoGuardadoExitoso { get; private set; } = true;
        public int TotalPasos => LeccionActual?.pasos?.Count ?? 0;
        public int TotalPasosObligatorios => LeccionActual?.pasos?.Count(p => !p.opcional) ?? 0;
        public int PasoObligatorioActual => LeccionActual?.pasos?
            .Take(IndicePasoActual + 1)
            .Count(p => !p.opcional) ?? 0;
        public float ProgresoNormalizado
        {
            get
            {
                if (TotalPasosObligatorios == 0) return 0f;
                int completados = LeccionActual.pasos.Take(IndicePasoActual).Count(p => !p.opcional);
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
            var progreso = ProgresoRepository.ObtenerProgreso();
            var previo = progreso.lecciones?.Find(l => l.leccion_id == leccionId && l.estado == "en_progreso");
            if (previo != null)
            {
                IndicePasoActual = Mathf.Clamp(previo.paso_actual, 0, TotalPasos - 1);
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
            IndicePasoActual++;
            GuardarEstadoActual();
            return IndicePasoActual < TotalPasos;
        }

        public bool GuardarEstadoActual()
        {
            if (LeccionActual == null) return false;
            if (IndicePasoActual >= TotalPasos) return true;
            return UltimoGuardadoExitoso = GestorProgreso.GuardarLeccionEnProgreso(
                LeccionActual.id,
                IndicePasoActual,
                Aciertos,
                Errores);
        }

        public bool FinalizarYGuardar()
        {
            if (LeccionActual == null || IndicePasoActual < TotalPasos - 1) return false;
            int totalEvaluables = Aciertos + Errores;
            return UltimoGuardadoExitoso = GestorProgreso.CompletarLeccion(
                LeccionActual.id, Aciertos, Errores, totalEvaluables > 0 ? totalEvaluables : TotalPasos);
        }
    }
}
