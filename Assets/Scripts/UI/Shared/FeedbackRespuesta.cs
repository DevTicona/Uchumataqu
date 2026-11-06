using System;
using Assets.Scripts.PLN;
using Assets.Scripts.Util;

namespace Assets.Scripts.UI
{
    public sealed class FeedbackRespuesta
    {
        public string Titulo { get; }
        public string Texto { get; }
        public string Semaforo { get; }
        public string Accion { get; }
        public bool PermiteAvanzar { get; }
        public string DiagnosticoMorfologico { get; }

        internal FeedbackRespuesta(string titulo, string texto, string semaforo, string accion, bool permiteAvanzar,
            string diagnosticoMorfologico = null)
        {
            Titulo = titulo;
            Texto = texto;
            Semaforo = semaforo;
            Accion = accion;
            PermiteAvanzar = permiteAvanzar;
            DiagnosticoMorfologico = diagnosticoMorfologico;
        }
    }

    public static class ContenidoFeedbackRespuesta
    {
        public const int MaxIntentosVoz = 3;
        public const string Avanzar = "Pulsa SIGUIENTE para avanzar";
        public const string VerOracion = "Pulsa CONTINUAR para ver y escuchar la oración";

        public static bool SinDeteccion(ResultadoEvaluacion resultado) => resultado != null && !resultado.aprobado &&
            (string.IsNullOrWhiteSpace(resultado.textoReconocido) ||
             resultado.textoReconocido == "No se identificó una palabra con claridad");

        public static FeedbackRespuesta Pronunciacion(ResultadoEvaluacion resultado, string esperado, int intentosFallidos,
            bool tieneOracion = false, bool oracionVista = false)
        {
            bool correcto = resultado.aprobado;
            bool sinDeteccion = SinDeteccion(resultado);
            bool casi = !correcto && resultado.porcentajeSimilitud >= MotorEvaluacion.UMBRAL_AMARILLO;
            int intentos = Math.Max(0, Math.Min(MaxIntentosVoz, intentosFallidos));
            bool avanzar = correcto || intentos >= MaxIntentosVoz;
            bool verOracion = correcto && tieneOracion && !oracionVista;
            string primera = sinDeteccion ? "No se detectó nada"
                : "Dijiste “" + TextoSeguro(correcto ? esperado : resultado.textoReconocido) + "”";
            string cierre = verOracion ? VerOracion : avanzar ? Avanzar : "Vuelve a escuchar y repetir";
            return new FeedbackRespuesta(sinDeteccion ? "Sin detección" : correcto ? "¡Correcto!" : casi ? "Casi" : "Incorrecto",
                primera + "\nReintentar " + intentos + "/3\n" + cierre,
                sinDeteccion ? "informacion" : correcto ? "verde" : casi ? "amarillo" : "rojo",
                verOracion ? "Continuar" : avanzar ? "Siguiente" : "Reintentar", avanzar);
        }

        public static FeedbackRespuesta Escritura(ResultadoEvaluacion resultado, bool tieneOracion = false,
            bool oracionVista = false, bool incluirCorreccion = false, bool requiereAprobacion = false)
        {
            bool verOracion = tieneOracion && !oracionVista;
            string primera = "Escribiste " + TextoSeguro(resultado.aprobado ? resultado.textoEsperado : resultado.textoIngresado);
            if (incluirCorreccion)
            {
                var diferencia = DiffGrafemas.Comparar(resultado.textoIngresado, resultado.textoEsperado);
                primera = "Escribiste " + diferencia.usuario;
                if (!resultado.aprobado) primera += "\nLo correcto es " + diferencia.correcta;
            }
            string cierre = resultado.aprobado ? verOracion ? VerOracion : Avanzar
                : "Corrige tu respuesta y pulsa " + (requiereAprobacion ? "COMPROBAR" : verOracion ? "CONTINUAR" : "SIGUIENTE");
            if (!resultado.aprobado && !incluirCorreccion && !string.IsNullOrWhiteSpace(resultado.detalleLinguistico))
                primera += "\n" + TextoSeguro(resultado.detalleLinguistico);
            return new FeedbackRespuesta(resultado.aprobado ? "¡Correcto!" : resultado.colorSemaforo == "amarillo" ? "Casi" : "Incorrecto",
                primera + "\n" + cierre, resultado.aprobado ? "verde" : resultado.colorSemaforo,
                !resultado.aprobado && requiereAprobacion ? "Comprobar" : verOracion ? "Continuar" : "Siguiente",
                resultado.aprobado || !requiereAprobacion,
                incluirCorreccion && !resultado.aprobado ? resultado.detalleLinguistico : null);
        }

        private static string TextoSeguro(string texto) => (texto ?? string.Empty).Replace("<", "‹").Replace(">", "›")
            .Replace("\r", " ").Replace("\n", " ");
    }
}
