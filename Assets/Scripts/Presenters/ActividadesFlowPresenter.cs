using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Activities;
using Assets.Scripts.Services;
using Assets.Scripts.Services.Activities;
using Assets.Scripts.Presenters.Activities;
using Assets.Scripts.Views.UI;

namespace Assets.Scripts.Presenters
{
    /// <summary>
    /// Orquesta la fase de actividades de una lección.
    /// No contiene datos descriptivos hardcodeados de actividades.
    /// Los títulos provienen del catálogo JSON.
    /// El avance se realiza únicamente con botones de navegación.
    /// </summary>
    public class ActividadesFlowPresenter : MonoBehaviour
    {
        // ===============================
        // UI
        // ===============================
        [Header("UI - Actividades")]
        [SerializeField] private TextMeshProUGUI txtTitulo;
        [SerializeField] private Transform actividadBase;
        [SerializeField] private LeccionBaseView view;

        [Header("UI - Contenedores")]
        [SerializeField] private GameObject practicaContainer;
        [SerializeField] private GameObject actividadContainer;

        // ===============================
        // Estado interno
        // ===============================
        private readonly List<PalabraData> poolPalabras = new();
        private List<TipoActividad> actividadesDisponibles;
        private Distractores distractoresLeccion;

        private ActividadPresenterBase actividadActual;
        private ActividadData actividadDataActual;

        private List<PalabraData> palabrasPendientesEliminar;

        private TipoActividad ultimaActividadEjecutada = null;
        private DireccionPregunta ultimaDireccionSeleccion = DireccionPregunta.UchumataquAEspanol;

        public event Action AlFinalizarActividades;

        // ===============================
        // Colores UI
        // ===============================
        private static readonly Color COLOR_NAV_FONDO =
            new Color32(0x3B, 0x6E, 0xF5, 0xFF); // #3B6EF5

        private static readonly Color COLOR_NAV_TEXTO = Color.white;

        // ===============================
        // Inicio del flujo
        // ===============================
        public void IniciarActividades(
            string idLeccion,
            List<PalabraData> palabrasLeccion
        )
        {
            if (string.IsNullOrEmpty(idLeccion) ||
                palabrasLeccion == null ||
                palabrasLeccion.Count == 0)
            {
                FinalizarFlujoActividades();
                return;
            }

            poolPalabras.Clear();
            poolPalabras.AddRange(palabrasLeccion);

            ActivarModoActividades();

            ActividadesCatalogoService.Cargar(() =>
            {
                ActividadesLeccionService.Cargar(lista =>
                {
                    var config = lista?.Find(l => l.leccionId == idLeccion);

                    if (config == null ||
                        config.actividades == null ||
                        config.actividades.Count == 0)
                    {
                        FinalizarFlujoActividades();
                        return;
                    }

                    actividadesDisponibles = new List<TipoActividad>(config.actividades);
                    distractoresLeccion = config.distractores ?? new Distractores
                    {
                        espanol = new List<string>(),
                        uchumataqu = new List<string>()
                    };

                    EjecutarSiguienteActividad();
                });
            });
        }

        // ===============================
        // Activación UI
        // ===============================
        private void ActivarModoActividades()
        {
            if (practicaContainer != null)
                practicaContainer.SetActive(false);

            if (actividadContainer != null)
                actividadContainer.SetActive(true);

            view.LimpiarBotones();
        }

        // ===============================
        // Ejecución principal
        // ===============================
        private void EjecutarSiguienteActividad()
        {
            if (poolPalabras.Count == 0)
            {
                MostrarPantallaFinal();
                return;
            }

            var actividadesValidas = ObtenerActividadesValidas();
            if (actividadesValidas.Count == 0)
            {
                MostrarPantallaFinal();
                return;
            }

            var tipoSeleccionado = ElegirActividadPonderada(actividadesValidas);
            actividadDataActual = ConstruirActividadData(tipoSeleccionado);

            // Registrar la actividad seleccionada para evitar repeticiones
            ultimaActividadEjecutada = tipoSeleccionado;

            ActualizarTituloDesdeCatalogo(tipoSeleccionado);

            LimpiarActividadBase();
            view.LimpiarBotones();

            LeccionBaseService.InstanciarActividad(
                tipoSeleccionado,
                actividadBase,
                OnActividadInstanciada,
                onError: () => EjecutarSiguienteActividad()
            );
        }

        // ===============================
        // Instanciación
        // ===============================
        private void OnActividadInstanciada(GameObject actividadGO)
        {
            if (actividadGO == null)
            {
                EjecutarSiguienteActividad();
                return;
            }

            actividadActual =
                actividadGO.GetComponentInChildren<ActividadPresenterBase>();

            if (actividadActual == null)
            {
                EjecutarSiguienteActividad();
                return;
            }

            actividadActual.AlFinalizarActividad += OnActividadFinalizada;
            actividadActual.IniciarActividad(actividadDataActual);
        }

        // ===============================
        // Fin de actividad individual
        // ===============================
        private void OnActividadFinalizada(List<PalabraData> palabrasUsadas)
        {
            palabrasPendientesEliminar = palabrasUsadas;

            view.LimpiarBotones();
            view.CrearBoton(
                "Siguiente",
                COLOR_NAV_FONDO,
                COLOR_NAV_TEXTO,
                ContinuarConSiguienteActividad
            );
        }

        private void ContinuarConSiguienteActividad()
        {
            if (palabrasPendientesEliminar != null)
            {
                foreach (var palabra in palabrasPendientesEliminar)
                    poolPalabras.Remove(palabra);
            }

            palabrasPendientesEliminar = null;

            if (actividadActual != null)
            {
                Destroy(actividadActual.gameObject);
                actividadActual = null;
            }

            EjecutarSiguienteActividad();
        }

        // ===============================
        // Pantalla final
        // ===============================
        private void MostrarPantallaFinal()
        {
            LimpiarActividadBase();
            view.LimpiarBotones();

            view.SetTextoIndicacion(
                "¡Felicitaciones!\nTerminaste la fase de actividades."
            );

            view.CrearBoton(
                "Finalizar",
                COLOR_NAV_FONDO,
                COLOR_NAV_TEXTO,
                () => AlFinalizarActividades?.Invoke()
            );
        }

        private void FinalizarFlujoActividades()
        {
            if (actividadContainer != null)
                actividadContainer.SetActive(false);

            AlFinalizarActividades?.Invoke();
        }

        // ===============================
        // Helpers
        // ===============================
        private void ActualizarTituloDesdeCatalogo(TipoActividad tipo)
        {
            if (txtTitulo == null) return;

            // Usar directamente el ID del tipo de actividad como clave del catálogo
            txtTitulo.text = ActividadesCatalogoService.ObtenerTitulo(tipo.id);
        }

        private void LimpiarActividadBase()
        {
            for (int i = actividadBase.childCount - 1; i >= 0; i--)
                Destroy(actividadBase.GetChild(i).gameObject);
        }

        private List<TipoActividad> ObtenerActividadesValidas()
        {
            var validas = new List<TipoActividad>();

            foreach (var tipo in actividadesDisponibles)
            {
                // EmparejamientoPalabras ahora requiere solo 1 palabra (para 1 par mínimo)
                if (tipo.Es("EmparejamientoPalabras") && poolPalabras.Count >= 1)
                    validas.Add(tipo);
                else if (!tipo.Es("EmparejamientoPalabras") && poolPalabras.Count >= 1)
                    validas.Add(tipo);
            }

            return validas;
        }

        private TipoActividad ElegirActividadPonderada(List<TipoActividad> validas)
        {
            // Si solo hay una opción válida, usarla
            if (validas.Count == 1)
                return validas[0];

            // Si hay múltiples opciones y la última actividad está en la lista, 
            // intentar elegir una diferente
            if (ultimaActividadEjecutada != null && 
                validas.Count > 1)
            {
                var alternativas = validas
                    .Where(a => a.id != ultimaActividadEjecutada.id)
                    .ToList();

                if (alternativas.Count > 0)
                    return alternativas[UnityEngine.Random.Range(0, alternativas.Count)];
            }

            return validas[UnityEngine.Random.Range(0, validas.Count)];
        }

        // ===============================
        // Construcción de datos de actividades
        // ===============================
        private ActividadData ConstruirActividadData(TipoActividad tipo)
        {
            return tipo.id switch
            {
                "SeleccionSimple" => CrearSeleccionSimpleData(),
                "EmparejamientoPalabras" => CrearEmparejamientoData(),
                "CompletarTexto" => CrearCompletarTextoData(),
                _ => null
            };
        }

        private ActividadData CrearSeleccionSimpleData()
        {
            var correcta = poolPalabras[0];
            var distractoresData = new List<PalabraData>();

            var externos = (distractoresLeccion?.espanol ?? new List<string>())
                .Where(d => d != correcta.uchumataqu && d != correcta.espanol)
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(3)
                .ToList();

            foreach (var d in externos)
            {
                distractoresData.Add(new PalabraData
                {
                    id = d,
                    uchumataqu = d,
                    espanol = d
                });
            }

            if (distractoresData.Count < 3)
            {
                var complemento = poolPalabras
                    .Where(p => p.id != correcta.id)
                    .OrderBy(_ => UnityEngine.Random.value)
                    .Take(3 - distractoresData.Count)
                    .ToList();

                distractoresData.AddRange(complemento);
            }

            // Alternar dirección de pregunta
            var direccionActual = ultimaDireccionSeleccion;
            ultimaDireccionSeleccion = direccionActual == DireccionPregunta.UchumataquAEspanol
                ? DireccionPregunta.EspanolAUchumataqu
                : DireccionPregunta.UchumataquAEspanol;

            return new ActividadData
            {
                palabrasCorrectas = new List<PalabraData> { correcta },
                palabrasIncorrectas = distractoresData,
                direccion = direccionActual
            };
        }

        private ActividadData CrearEmparejamientoData()
        {
            // Determinar dinámicamente cuántos pares crear (1 o 2)
            int numPares = Mathf.Min(2, poolPalabras.Count);
            if (numPares < 1) numPares = 1;

            var correctas = poolPalabras
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(numPares)
                .ToList();

            var distractoresData = new List<PalabraData>();

            var distGrupo = distractoresLeccion ?? new Distractores
            {
                espanol = new List<string>(),
                uchumataqu = new List<string>()
            };

            // Crear distractores emparejados desde español y uchumataqu
            var count = Mathf.Min(distGrupo.espanol?.Count ?? 0, distGrupo.uchumataqu?.Count ?? 0);
            
            // Necesitamos al menos tantos distractores como pares correctos
            var distractoresNecesarios = numPares;
            
            for (int i = 0; i < count && distractoresData.Count < distractoresNecesarios; i++)
            {
                var esp = distGrupo.espanol[i];
                var uch = distGrupo.uchumataqu[i];

                // Asegurar que el distractor no coincida con palabras correctas
                if (!correctas.Any(c => c.espanol == esp || c.uchumataqu == uch))
                {
                    distractoresData.Add(new PalabraData
                    {
                        id = esp + "_" + uch,
                        espanol = esp,
                        uchumataqu = uch
                    });
                }
            }

            // Si no hay suficientes distractores externos, complementar con palabras del pool
            if (distractoresData.Count < distractoresNecesarios)
            {
                var complemento = poolPalabras
                    .Where(p => !correctas.Any(c => c.id == p.id))
                    .OrderBy(_ => UnityEngine.Random.value)
                    .Take(distractoresNecesarios - distractoresData.Count)
                    .ToList();

                distractoresData.AddRange(complemento);
            }

            return new ActividadData
            {
                palabrasCorrectas = correctas,
                palabrasIncorrectas = distractoresData
            };
        }

        private ActividadData CrearCompletarTextoData()
        {
            return new ActividadData
            {
                palabrasCorrectas = new List<PalabraData> { poolPalabras[0] },
                palabrasIncorrectas = new List<PalabraData>()
            };
        }

        private void OnDestroy()
        {
            LeccionBaseService.LimpiarRecursos();
        }
    }
}
