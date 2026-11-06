using System.Collections;
using Assets.Scripts.Persistencia;
using Assets.Scripts.RA;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public partial class RealidadAumentadaPresenter
    {
        private void ConservarDialogoResuelto(ObjetoRAView objeto)
        {
            if (objeto == null || experiencia?.Dialogo == null || interaccion?.Camara == null) return;
            // Las intervenciones de RA1 muestran su propio diálogo.
            if (EsRA1 && !UsaSeleccionGuiadaRA1) return;
            var palabra = CorpusRepository.ObtenerPalabra(objeto.PalabraId);
            if (palabra != null && experiencia.Dialogo.TieneContenido && experiencia.Dialogo.Ancla == objeto.AnclaDialogo &&
                experiencia.Dialogo.Texto == palabra.uch && experiencia.Dialogo.Traduccion == palabra.es) return;
            if (palabra != null) MostrarDialogoGuiado(objeto, palabra.uch, palabra.es, true);
        }

        private void ActualizarDialogoPersistente()
        {
            experiencia?.Dialogo.Suspender(pausadoRA1);
        }

        private void LimpiarDialogoPersistente()
        {
            experiencia?.Dialogo.Ocultar();
        }

        private void LimpiarMarcasObjetos()
        {
            foreach (var objeto in objetos.Values) objeto.LimpiarMarca();
        }

        private void EscucharObjetoGuiado(ObjetoRAView objeto)
        {
            DetenerAudioGuiado();
            ultimaPalabraGuiada = objeto.PalabraId;
            var palabra = CorpusRepository.ObtenerPalabra(objeto.PalabraId);
            if (palabra == null) return;
            MostrarDialogoGuiado(objeto, palabra.uch, palabra.es, true);
            rutinaAudioGuiada = StartCoroutine(EscucharNombreGuiado(palabra.id));
        }

        private IEnumerator EscucharNombreGuiado(string palabra)
        {
            yield return ReproducirAudioGuiado(palabra, "palabra");
            rutinaAudioGuiada = null;
        }
    }
}
