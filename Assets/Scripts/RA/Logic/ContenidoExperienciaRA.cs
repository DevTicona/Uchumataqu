using Assets.Scripts.Data.Corpus;

namespace Assets.Scripts.RA
{
    public static class ContenidoExperienciaRA
    {
        public const string InstruccionBienvenida = "Sigue las instrucciones de este cuadro. Pulsa comenzar";
        public const string InstruccionEscritura = "Escribe en uchumataqu el nombre del objeto resaltado";
        public const string VerOracion = "Pulsa continuar para ver y escuchar la oración";
        public const string RepetirOracion = "Vuelve a tocar para escuchar la oración\nPulsa SIGUIENTE para avanzar";

        public static string Reintento(PasoRAModel paso) => paso.tipo switch
        {
            "escuchar_seleccionar" => "Vuelve a escuchar y toca el elemento que corresponde",
            "repetir_voz" => "Vuelve a escuchar y repite la palabra",
            "escribir" => "Corrige las letras marcadas y vuelve a comprobar",
            "arrastrar" => paso.UsaTarjeta ? "Vuelve a intentarlo: suelta la tarjeta sobre su destino"
                : "Vuelve a intentarlo: suelta el objeto sobre su destino",
            _ => "Vuelve a escuchar y sigue la instrucción"
        };

        public static string Bienvenida(string escenario) => escenario switch
        {
            "ra_encuentro_comunicacion" => "Conversa con Roberto y el profesor. Escucha sus frases y responde en tu turno.",
            "ra_familia_vestimenta" => "Escucha, reconoce a la familia, habla, escribe y coloca las prendas. Arrastra la tarjeta hasta la abuela.",
            "ra_casa_objetos" => "Descubre los objetos de la casa. Abre las ventanas y la puerta, limpia la mesa y prepara la mochila para salir.",
            "ra_cocina_alimentos" => "Reconoce los alimentos y utensilios. Organiza la cocina, prepara el arroz, sirve la leche y ayuda a dejar la mesa lista.",
            "ra_animales_ecosistema" => "Explora primero la fauna terrestre y después la acuática. Escucha, encuentra a los animales y practica sus nombres hablando o escribiendo.",
            "ra_naturaleza_orientacion" => "Conoce las plantas, el sol y la luna. Aprende las direcciones del escenario y coloca la vela para preparar el viaje de la balsa.",
            _ => "Escucha y sigue las actividades del escenario."
        };

        public static string TituloCompletado(string escenario) => escenario switch
        {
            "ra_familia_vestimenta" => "¡Familia lista!",
            "ra_casa_objetos" => "¡Mochila lista!",
            "ra_cocina_alimentos" => "¡Mesa lista!",
            "ra_naturaleza_orientacion" => "¡Buen viaje!",
            _ => "¡Exploración completa!"
        };

        public static string TituloActividad(PasoRAModel paso) => paso.tipo switch
        {
            "escribir" => "Escribe",
            "repetir_voz" => "Tu turno",
            "arrastrar" => paso.UsaTarjeta ? "Arrastra la palabra" : "Coloca el objeto",
            "escuchar_seleccionar" => "Escucha y encuentra",
            "historia" => paso.EsHistoriaInteractiva ? "Continúa la historia" : "Observa y escucha",
            _ => "Escucha y observa"
        };

        public static string Instruccion(PasoRAModel paso, string texto, bool esperandoDestino, bool familia)
        {
            if (paso.tipo == "escribir")
            {
                string traduccion = ContenidoActividadRA.Traduccion(paso, string.Empty);
                return string.IsNullOrWhiteSpace(traduccion) ? InstruccionEscritura
                    : "Escribe " + traduccion + " en Uchumataqu";
            }
            if (paso.tipo == "escuchar_seleccionar") return "Escucha y toca el elemento que corresponde";
            if (paso.tipo == "repetir_voz") return "Escucha la palabra. Pulsa Hablar para repetirla y Detener cuando acabes";
            if (esperandoDestino && !string.IsNullOrWhiteSpace(paso.instruccion_destino)) return paso.instruccion_destino;
            if (!string.IsNullOrWhiteSpace(paso.instruccion)) return paso.instruccion;
            return paso.tipo switch
            {
                "arrastrar" => esperandoDestino ? "Suelta el origen sobre el círculo resaltado." : "Arrastra el origen hasta su destino.",
                "historia" => paso.EsHistoriaInteractiva ? "Toca el elemento resaltado para continuar la historia." : "Escucha y observa. Después pulsa Continuar.",
                _ => "Sigue las indicaciones de la actividad."
            };
        }
    }
}
