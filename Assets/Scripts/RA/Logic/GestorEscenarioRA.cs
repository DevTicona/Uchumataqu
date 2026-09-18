using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.RA
{
    /// <summary>
    /// Gestor principal de la escena 04_RealidadAumentada.
    /// Especificado en Docs/Specifications/03-modulo-ra.md y Docs/UI/05-escena-realidad-aumentada.md.
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.RA", sourceAssembly: "Assembly-CSharp", sourceClassName: "GestorEscenaRA")]
    public class GestorEscenarioRA : MonoBehaviour
    {
        public EscenarioRAModel EscenarioActual { get; private set; }

        public bool CargarEscenario(string escenarioId)
        {
            EscenarioActual = CorpusRepository.ObtenerEscenarioRA(escenarioId);
            if (EscenarioActual == null)
            {
                Debug.LogError($"[GestorEscenaRA] Escenario RA '{escenarioId}' no encontrado en corpus.");
                return false;
            }

            return true;
        }

        public bool EstaDesbloqueado()
        {
            if (EscenarioActual == null) return false;
            return GestorProgreso.EstaEscenarioDesbloqueado(EscenarioActual.id);
        }

        public bool RegistrarVisita()
        {
            if (EscenarioActual != null)
            {
                var progreso = ProgresoRepository.ObtenerProgreso();
                progreso.MarcarEscenarioVisitado(EscenarioActual.id);
                return ProgresoRepository.Guardar();
            }
            return false;
        }
    }
}
