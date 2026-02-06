using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Assets.Scripts.Core;
using Assets.Scripts.Views.RealidadAumentada;
using Assets.Scripts.Views.UI;
using Assets.Scripts.Models;
using Assets.Scripts.Services;

namespace Assets.Scripts.Presenters.RealidadAumentada
{
    /// <summary>
    /// Presenter de la escena de Realidad Aumentada.
    /// Orquesta la interacción entre la UI (RAView),
    /// la navegación y el flujo de palabras.
    /// Carga dinámicamente los datos de la lección desde el servicio.
    /// </summary>
    public class RAPresenter : MonoBehaviour
    {
        // ======================
        // CONSTANTES Y TIPOS
        // ======================
        private const string KEY_LECCION_ACTUAL = "LECCION_ACTUAL";
        private const string ESCENA_MENU_LECCIONES = "MenuLecciones";
        private const string INDICACION_INICIAL = "Escucha la palabra y luego repítela";

        private enum EstadoRA
        {
            Idle,
            Escuchando, // Reproduciendo audio
            Grabando,   // Usuario hablando
            Feedback    // Mostrando resultado
        }

        // ======================
        // REFERENCIAS
        // ======================

        [Header("Views")]
        [SerializeField] private RAView raView;
        [SerializeField] private HeaderView headerView;

        [Header("Navegación")]
        [SerializeField] private NavigationBackHandler backHandler;

        // ======================
        // ESTADO
        // ======================

        private string idLeccion;
        private int indicePalabraActual = 0;
        private List<PalabraData> listaPalabras;
        private PalabraData palabraActual;
        
        // Estado interno para claridad del flujo
        private EstadoRA estadoActual = EstadoRA.Idle;

        // ======================
        // CICLO DE VIDA
        // ======================

        private void Awake()
        {
            ValidarReferencias();
            listaPalabras = new List<PalabraData>();
        }

        private void Start()
        {
            InicializarLeccion();
        }

        private void OnDestroy()
        {
            DesuscribirseEventosView();
            LimpiarRecursos();
        }

        // ======================
        // INICIALIZACIÓN
        // ======================

        // ======================
        // INICIALIZACIÓN
        // ======================

        private void InicializarLeccion()
        {
            if (!IntentarObtenerLeccion(out idLeccion))
            {
                RegresarAlMenu();
                return;
            }

            ConfigurarHeaderInicial();
            ConfigurarBackHandler();
            SuscribirseEventosView();

            CargarDatosLeccion();
            CargarPalabras();
        }

        private void ValidarReferencias()
        {
            if (raView == null || headerView == null)
            {
                Debug.LogError("[RAPresenter] Referencias obligatorias no asignadas en el Inspector.");
                enabled = false;
            }
        }

        private void ConfigurarHeaderInicial()
        {
            headerView.BindBotonAtras(RegresarAlMenu);
            headerView.SetTitulo(string.Empty); // Limpiar mientras carga
            headerView.OcultarIcono();
        }

        private bool IntentarObtenerLeccion(out string id)
        {
            id = ObtenerIdLeccionGuardada();
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("[RAPresenter] No se encontró lección activa.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Encapsula el acceso a la persistencia local.
        /// </summary>
        private string ObtenerIdLeccionGuardada()
        {
            return PlayerPrefs.GetString(KEY_LECCION_ACTUAL);
        }

        // ======================
        // LOGICA DE CARGA
        // ======================

        /// <summary>
        /// Carga los datos de la lección desde el servicio y configura el header.
        /// </summary>
        private void CargarDatosLeccion()
        {
            LeccionesService.ObtenerLeccionPorId(idLeccion, leccion =>
            {
                if (leccion == null)
                {
                    Debug.LogError($"[RAPresenter] No se encontró la lección {idLeccion}");
                    RegresarAlMenu();
                    return;
                }
                ActualizarUIHeader(leccion);
            });
        }

        /// <summary>
        /// Configura el header con el título de la lección y carga el ícono.
        /// </summary>
        private void ActualizarUIHeader(LeccionData leccion)
        {
            headerView.SetTitulo(leccion.titulo);
            headerView.SetSubtitulo(null); // Sin subtítulo en RA

            CargarIcono(leccion.id);
        }

        /// <summary>
        /// Carga el ícono de la lección desde el servicio.
        /// </summary>
        private void CargarIcono(string idIcono)
        {
            LeccionesService.CargarIconoLeccion(
                idIcono,
                sprite =>
                {
                    if (sprite != null)
                        headerView.SetIcono(sprite);
                },
                onError: () =>
                {
                    Debug.LogWarning($"[RAPresenter] No se pudo cargar el ícono: {idIcono}");
                    headerView.OcultarIcono();
                }
            );
        }

        /// <summary>
        /// Carga las palabras de la lección actual desde el servicio.
        /// </summary>
        private void CargarPalabras()
        {
            PalabrasService.ObtenerPalabrasPorLeccion(idLeccion, palabras =>
            {
                if (palabras == null || palabras.Count == 0)
                {
                    Debug.LogWarning($"[RAPresenter] No hay palabras para la lección {idLeccion}");
                    // Aquí se podría mostrar mensaje de error al usuario
                    return;
                }

                listaPalabras = palabras;
                indicePalabraActual = 0;
                MostrarPalabraActual();
            });
        }

        // ======================
        // CONFIGURACIÓN VIEW & EVENTOS
        // ======================

        private void ConfigurarBackHandler()
        {
            if (backHandler != null)
                backHandler.Configurar(RegresarAlMenu);
        }

        private void SuscribirseEventosView()
        {
            raView.OnReproducirAudio += OnReproducirAudio;
            raView.OnGrabarVoz += OnGrabarVoz;
            raView.OnSiguiente += OnSiguiente;
            raView.OnAnterior += OnAnterior;
        }

        private void DesuscribirseEventosView()
        {
            if (raView != null)
            {
                raView.OnReproducirAudio -= OnReproducirAudio;
                raView.OnGrabarVoz -= OnGrabarVoz;
                raView.OnSiguiente -= OnSiguiente;
                raView.OnAnterior -= OnAnterior;
            }
        }

        /// <summary>
        /// Limpia los recursos cargados (Addressables, sprites, etc.)
        /// </summary>
        private void LimpiarRecursos()
        {
            LeccionBaseService.LimpiarRecursos();
            LeccionesService.LimpiarRecursosIconos();
        }

        // ======================
        // FLUJO PRINCIPAL
        // ======================

        /// <summary>
        /// Muestra la palabra actual en la vista y actualiza el estado.
        /// </summary>
        private void MostrarPalabraActual()
        {
            if (!EsIndiceValido(indicePalabraActual)) return;

            palabraActual = listaPalabras[indicePalabraActual];
            estadoActual = EstadoRA.Idle;

            ActualizarUIView();
        }

        /// <summary>
        /// Verifica si un índice es válido dentro de la lista de palabras.
        /// </summary>
        private bool EsIndiceValido(int indice)
        {
            return listaPalabras != null && indice >= 0 && indice < listaPalabras.Count;
        }

        /// <summary>
        /// Actualiza la interfaz de usuario con la palabra actual.
        /// </summary>
        private void ActualizarUIView()
        {
            if (estadoActual != EstadoRA.Idle)
                return;
                
            raView.MostrarPalabra(palabraActual.uchumataqu, palabraActual.espanol);
            raView.MostrarIndicacion(INDICACION_INICIAL);
            raView.LimpiarFeedback();
            raView.ActivarInteraccion(true);
            
            raView.ActualizarNavegacion(
                indicePalabraActual > 0,
                indicePalabraActual < listaPalabras.Count - 1
            );
        }

        // ======================
        // MANEJADORES DE EVENTOS
        // ======================

        /// <summary>
        /// Maneja el evento de reproducir audio de la palabra.
        /// </summary>
        private void OnReproducirAudio()
        {
            estadoActual = EstadoRA.Escuchando;
            Debug.Log($"[RAPresenter] Reproduciendo: {palabraActual?.uchumataqu}");
            
            raView.LimpiarFeedback();
            // TODO: Integrar AudioService.Reproducir(palabraActual)
            // Cuando termine el audio -> estadoActual = EstadoRA.Idle
        }

        /// <summary>
        /// Maneja el evento de grabar voz del usuario.
        /// </summary>
        private void OnGrabarVoz()
        {
            estadoActual = EstadoRA.Grabando;
            Debug.Log("[RAPresenter] Grabando voz usuario...");

            raView.MostrarFeedback("Escuchando...");
            raView.ActivarInteraccion(false);

            // TODO: Integrar VoiceRecognitionService
            // Al recibir resultado -> OnResultadoGrabacion(...)
        }

        /// <summary>
        /// Navega a la siguiente palabra.
        /// </summary>
        private void OnSiguiente()
        {
            if (!EsIndiceValido(indicePalabraActual + 1))
            {
                FinDeLeccionRA();
                return;
            }

            indicePalabraActual++;
            MostrarPalabraActual();
        }

        /// <summary>
        /// Navega a la palabra anterior.
        /// </summary>
        private void OnAnterior()
        {
            if (!EsIndiceValido(indicePalabraActual - 1)) return;

            indicePalabraActual--;
            MostrarPalabraActual();
        }

        /// <summary>
        /// Maneja el fin de la lección de Realidad Aumentada.
        /// </summary>
        private void FinDeLeccionRA()
        {
            Debug.Log("[RAPresenter] Fin de palabras en RA.");
            // TODO: Implementar transición a Actividades 2D.
            // 1. Mostrar modal de completado o feedback final.
            // 2. Notificar al LeccionBasePresenter (si existiera un coordinador superior) o cargar escena.
            
            // Temporalmente nos quedamos en la última palabra o volvemos al menú
            // RegresarAlMenu(); 
        }

        // ======================
        // NAVEGACIÓN
        // ======================

        /// <summary>
        /// Vuelve al menú de lecciones.
        /// </summary>
        private void RegresarAlMenu()
        {
            SceneManager.LoadScene(ESCENA_MENU_LECCIONES);
        }
    }
}
