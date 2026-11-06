using Assets.Scripts.Core.UI.Ayuda;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public static class ContenidoGuiaActividadesRA
    {
        public static PasoGuia[] Obtener(string tipo) => tipo switch
        {
            "escuchar_seleccionar" => new[]
            {
                new PasoGuia("Escucha y encuentra", "Escucha el audio y busca en el escenario el elemento que corresponde. Puedes volver a escucharlo con el botón Escuchar."),
                new PasoGuia("Selecciona el elemento", "Toca el elemento en la cámara. Revisa el resultado; si no corresponde, vuelve a escuchar e inténtalo de nuevo. Continúa cuando la actividad lo indique.")
            },
            "repetir_voz" => new[]
            {
                new PasoGuia("Practica con tu voz", "Escucha el ejemplo. Pulsa Hablar y concede el permiso de micrófono si se solicita. Repite la palabra o frase indicada en un lugar con poco ruido."),
                new PasoGuia("Termina y revisa", "Pulsa Detener o Terminar cuando acabes. Espera la evaluación y revisa la indicación para volver a intentarlo o continuar.")
            },
            "escribir" => new[]
            {
                new PasoGuia("Escribe en Uchumataqu", "Observa el elemento resaltado y lee la consigna. Toca el campo de respuesta para abrir el teclado y escribir lo que se solicita."),
                new PasoGuia("Comprueba tu escritura", "Pulsa Comprobar. Revisa las letras marcadas y corrige tu respuesta si hace falta. Cuando sea correcta, sigue las indicaciones para continuar.")
            },
            "arrastrar" => new[]
            {
                new PasoGuia("Arrastra al destino", "Lee la consigna y localiza el elemento de origen y el destino resaltado. El origen puede ser un objeto del escenario o una tarjeta con una palabra."),
                new PasoGuia("Mantén, mueve y suelta", "Mantén el dedo sobre el origen, arrástralo y suéltalo sobre el destino indicado. Revisa el resultado y vuelve a intentarlo si no corresponde.")
            },
            _ => System.Array.Empty<PasoGuia>()
        };
    }
}
