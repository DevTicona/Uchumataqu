using Assets.Scripts.Data.Corpus;
using Assets.Scripts.PLN;
using Assets.Scripts.Persistencia;
using Assets.Scripts.RA;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public partial class RealidadAumentadaPresenter
    {
        private PasoRAModel pasoResultadoVoz;
        private ResultadoEvaluacion resultadoVoz;

        private void LimpiarResultadoVoz()
        {
            pasoResultadoVoz = null;
            resultadoVoz = null;
            feedbackActividad = string.Empty;
            estadoErrorGuiado = ExperienciaRAView.Estado.Instruccion;
        }

        private void RegistrarResultadoVoz(PasoRAModel paso, ResultadoEvaluacion resultado)
        {
            pasoResultadoVoz = paso;
            resultadoVoz = resultado;
            feedbackActividad = string.Empty;
            if (UsaFooterGuiado) RegistrarResultadoGuiado(resultado);
            else
            {
                if (resultado.evaluado && !ContenidoFeedbackRespuesta.SinDeteccion(resultado))
                    experiencia.ReproducirSonidoRespuesta(resultado.porcentajeSimilitud >= MotorEvaluacion.UMBRAL_AMARILLO
                        ? ExperienciaRAView.Estado.Reintento : ExperienciaRAView.Estado.Incorrecto);
                mision.RegistrarEvaluacion(resultado);
            }
            ActualizarMision();
        }

        private void AplicarResultadoVoz(PasoRAModel paso, ref string titulo, ref string instruccion,
            ref string accion, ref ExperienciaRAView.Estado estado)
        {
            if (paso.tipo != "repetir_voz" || pasoResultadoVoz != paso || resultadoVoz == null ||
                !resultadoVoz.evaluado || iniciandoMicrofono || grabacionVoz.EstaGrabando || procesandoVoz) return;

            var feedback = ContenidoFeedbackRespuesta.Pronunciacion(resultadoVoz,
                ContenidoActividadRA.TextoVisible(paso, PerfilRepository.ObtenerPerfil().nombre), mision.IntentosVoz);
            titulo = feedback.Titulo;
            estado = EstadoFeedback(feedback);
            instruccion = feedback.Texto;
            accion = mision.PuedeContinuarVoz ? "Siguiente" : feedback.Accion;
        }

        private static ExperienciaRAView.Estado EstadoFeedback(FeedbackRespuesta feedback) => feedback.Semaforo switch
        {
            "verde" => ExperienciaRAView.Estado.Correcto,
            "amarillo" => ExperienciaRAView.Estado.Reintento,
            "rojo" => ExperienciaRAView.Estado.Incorrecto,
            _ => ExperienciaRAView.Estado.Instruccion
        };

    }
}
