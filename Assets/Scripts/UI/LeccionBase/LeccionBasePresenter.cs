using System.Threading.Tasks;
using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.Sistema;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Lecciones;

namespace Assets.Scripts.UI.LeccionBase
{
    /// <summary>
    /// Presenter de la escena 03_LeccionBase.
    /// Orquesta los pasos compilados de la lección seleccionada mediante GestorLeccion.
    /// Especificado en Docs/UI/04-escena-leccion-base.md.
    /// </summary>
    public class LeccionBasePresenter : MonoBehaviour
    {
        [SerializeField] private LeccionBaseView view;
        [SerializeField] private NavigationBackHandler backHandler;

        private GestorLeccion _gestorLeccion;

        private void Start()
        {
            if (view == null) view = FindFirstObjectByType<LeccionBaseView>();
            if (backHandler == null) backHandler = FindFirstObjectByType<NavigationBackHandler>();

            if (backHandler != null)
            {
                backHandler.Configurar(VolverAlMapa);
            }

            if (view != null)
            {
                view.OnAtrasClick += VolverAlMapa;
                view.OnVolverAlMapaClick += VolverAlMapa;
            }

            IniciarEjecucionLeccion();
        }

        private void OnDestroy()
        {
            if (view != null)
            {
                view.OnAtrasClick -= VolverAlMapa;
                view.OnVolverAlMapaClick -= VolverAlMapa;
            }
        }

        private void IniciarEjecucionLeccion()
        {
            _gestorLeccion = new GestorLeccion();
            string leccionId = ContextoNavegacion.LeccionId;

            if (!_gestorLeccion.IniciarLeccion(leccionId))
            {
                Debug.LogError($"[LeccionBasePresenter] No se pudo iniciar la lección '{leccionId}'. Volviendo al mapa...");
                VolverAlMapa();
                return;
            }

            if (view != null)
            {
                view.ConfigurarHeader(_gestorLeccion.LeccionActual.titulo, _gestorLeccion.ProgresoNormalizado);
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

            if (view != null)
            {
                view.ActualizarProgreso(_gestorLeccion.ProgresoNormalizado);
                view.OcultarFeedback();
            }

            Debug.Log($"[LeccionBasePresenter] Ejecutando paso {_gestorLeccion.IndicePasoActual + 1}/{_gestorLeccion.TotalPasos} (Tipo: {paso.tipo})");
        }

        public async void ResponderPaso(bool aprobado, string mensajeFeedback = null)
        {
            _gestorLeccion.RegistrarResultadoPaso(aprobado);

            if (view != null)
            {
                view.MostrarFeedback(aprobado, mensajeFeedback ?? (aprobado ? "¡Correcto!" : "Revisa la respuesta"));
            }

            await Task.Delay(1200);

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
            float precision = total > 0 ? ((float)_gestorLeccion.Aciertos / total) * 100f : 100f;

            Debug.Log($"[LeccionBasePresenter] Lección '{_gestorLeccion.LeccionActual.id}' completada: {_gestorLeccion.Aciertos}/{total} ({precision:F0}%)");

            if (view != null)
            {
                view.MostrarResultado(_gestorLeccion.Aciertos, total, precision);
            }
        }

        private void VolverAlMapa()
        {
            NavigationService.IrAMain(PaginaMain.Lecciones);
        }
    }
}
