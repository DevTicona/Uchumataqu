using System.Collections;
using Assets.Scripts.Core.UI.Ayuda;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;
using UnityEngine;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public partial class RealidadAumentadaPresenter
    {
        [SerializeField] private AyudaBrevePantalla ayuda;
        private Coroutine rutinaAyuda;
        private PasoRAModel pasoAyuda;
        private bool esperandoAyudaActividad;
        private bool AyudaBloqueaInteraccion => ayuda != null && (ayuda.Visible || esperandoAyudaActividad);

        private void PrepararAyudaActividad()
        {
            var paso = mision?.PasoActual;
            if (ayuda == null || paso == pasoAyuda || paso == null || mision.Completada ||
                !(UsaFooterGuiado ? inicioGuiado : inicioRA1)) return;
            if (rutinaAyuda != null) StopCoroutine(rutinaAyuda);
            esperandoAyudaActividad = false;
            pasoAyuda = paso;
            rutinaAyuda = StartCoroutine(PresentarAyudaActividad(paso));
        }

        private IEnumerator PresentarAyudaActividad(PasoRAModel paso)
        {
            string id = "ra:actividad:" + paso.tipo + ":v1";
            var pasos = ContenidoGuiaActividadesRA.Obtener(paso.tipo);
            if (pasos.Length == 0) yield break;
            esperandoAyudaActividad = !AyudaRepository.FueVista(id);
            yield return null;
            while (!destruido && mision?.PasoActual == paso &&
                (!PuedeInteractuarGuiado || OcupadoGuiado || audioRA1 || rutinaTurnoRA1 != null ||
                 ayuda.Visible || view.ConfirmacionSalidaVisible)) yield return null;
            if (!destruido && mision?.PasoActual == paso)
            {
                ayuda.PrepararGuia("Actividad RA", pasos);
                ayuda.MostrarPrimeraVez(id, "Actividad RA", pasos);
            }
            esperandoAyudaActividad = false;
            rutinaAyuda = null;
        }

        private void CancelarAyudaActividad()
        {
            if (rutinaAyuda != null) StopCoroutine(rutinaAyuda);
            rutinaAyuda = null;
            pasoAyuda = null;
            esperandoAyudaActividad = false;
        }
    }
}
