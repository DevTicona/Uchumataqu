using UnityEngine;
using UnityEngine.SceneManagement;
using Assets.Scripts.Views.UI;
using Assets.Scripts.Core;
using Assets.Scripts.Services;
using Assets.Scripts.Models;

namespace Assets.Scripts.Presenters
{
    /// <summary>
    /// Orquesta el flujo completo de una lección:
    /// Práctica → Actividades → Fin
    /// </summary>
    public class LeccionBasePresenter : MonoBehaviour
    {
        private const string KEY_LECCION_ACTUAL = "LECCION_ACTUAL";
        private const string ESCENA_MENU_LECCIONES = "MenuLecciones";

        [Header("UI")]
        [SerializeField] private LeccionBaseView view;
        [SerializeField] private HeaderView headerView;

        [Header("Fases")]
        [SerializeField] private PracticaPresenter practicaPresenter;
        [SerializeField] private ActividadesFlowPresenter actividadesPresenter;

        [Header("Navegación")]
        [SerializeField] private NavigationBackHandler backHandler;

        private string idLeccion;

        private void Start()
        {
            if (view == null ||
                practicaPresenter == null ||
                actividadesPresenter == null ||
                headerView == null)
            {
                Debug.LogError(
                    "[LeccionBasePresenter] Referencias no asignadas en el Inspector."
                );
                return;
            }

            idLeccion = PlayerPrefs.GetString(KEY_LECCION_ACTUAL);

            if (string.IsNullOrEmpty(idLeccion))
            {
                SceneManager.LoadScene(ESCENA_MENU_LECCIONES);
                return;
            }

            ConfigurarEventos();
            ConfigurarBackHandler();

            // 🔹 Cargar datos de la lección (título e ícono)
            CargarDatosLeccion();

            practicaPresenter.IniciarPractica(idLeccion);
        }

        private void OnDestroy()
        {
            practicaPresenter.AlFinalizarPractica -= OnPracticaFinalizada;
            actividadesPresenter.AlFinalizarActividades -= OnActividadesFinalizadas;
            LeccionBaseService.LimpiarRecursos();
            LeccionesService.LimpiarRecursosIconos();
        }

        // ======================
        // Carga de datos de la lección
        // ======================
        private void CargarDatosLeccion()
        {
            LeccionesService.ObtenerLeccionPorId(idLeccion, leccion =>
            {
                if (leccion == null)
                {
                    Debug.LogError(
                        $"[LeccionBasePresenter] No se encontró la lección {idLeccion}"
                    );
                    return;
                }

                // Configurar header
                ConfigurarHeader(leccion);

                // Ícono (usando ID de lección → Sprite)
                CargarIcono(leccion.id);
            });
        }

        private void ConfigurarHeader(LeccionData leccion)
        {
            headerView.BindBotonAtras(VolverAlMenu);
            headerView.SetTitulo(leccion.titulo);
            headerView.SetSubtitulo(null); // Sin subtítulo
        }

        private void ConfigurarBackHandler()
        {
            if (backHandler != null)
                backHandler.Configurar(VolverAlMenu);
        }

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
                    Debug.LogWarning($"[LeccionBasePresenter] No se pudo cargar el ícono: {idIcono}");
                }
            );
        }

        // ======================
        // Configuración de eventos
        // ======================
        private void ConfigurarEventos()
        {
            practicaPresenter.AlFinalizarPractica += OnPracticaFinalizada;
            actividadesPresenter.AlFinalizarActividades += OnActividadesFinalizadas;

            view.BindBotonAtras(VolverAlMenu);
        }

        // ======================
        // Flujo
        // ======================
        private void OnPracticaFinalizada()
        {
            actividadesPresenter.IniciarActividades(
                idLeccion,
                practicaPresenter.ObtenerPalabras()
            );
        }

        private void OnActividadesFinalizadas()
        {
            VolverAlMenu();
        }

        private void VolverAlMenu()
        {
            SceneManager.LoadScene(ESCENA_MENU_LECCIONES);
        }
    }
}
