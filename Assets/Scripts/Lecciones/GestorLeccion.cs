using System;
using System.Collections.Generic;
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
        public LeccionCompiladaData LeccionActual { get; private set; }
        public int IndicePasoActual { get; private set; }
        public int Aciertos { get; private set; }
        public int Errores { get; private set; }
        public int TotalPasos => LeccionActual?.pasos?.Count ?? 0;
        public float ProgresoNormalizado => TotalPasos > 0 ? (float)IndicePasoActual / TotalPasos : 0f;

        public PasoLeccionData PasoActual
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
            return IndicePasoActual < TotalPasos;
        }

        public void FinalizarYGuardar()
        {
            if (LeccionActual == null) return;
            int totalEvaluables = Aciertos + Errores;
            ProgresoRepository.CompletarLeccion(LeccionActual.id, Aciertos, Errores, totalEvaluables > 0 ? totalEvaluables : TotalPasos);
        }
    }
}
