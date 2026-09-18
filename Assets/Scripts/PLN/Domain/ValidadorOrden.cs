using System.Collections.Generic;

namespace Assets.Scripts.PLN
{
    /// <summary>
    /// Validación determinística del orden de tokens contra una oración canónica del corpus.
    /// Especificado en Docs/Specifications/01-modulo-pln-texto.md (Sección 8).
    /// </summary>
    public static class ValidadorOrden
    {
        public static bool ValidarOrden(List<string> tokensUsuario, List<string> tokensEsperados)
        {
            if (tokensUsuario == null || tokensEsperados == null) return false;
            if (tokensUsuario.Count != tokensEsperados.Count) return false;

            for (int i = 0; i < tokensUsuario.Count; i++)
            {
                if (NormalizadorTexto.Normalizar(tokensUsuario[i]) != NormalizadorTexto.Normalizar(tokensEsperados[i]))
                    return false;
            }

            return true;
        }

        public static ResultadoEvaluacion EvaluarOrdenOracion(string oracionEsperada, List<string> tokensUsuario)
        {
            var tokensEsperados = MotorEvaluacion.Tokenizar(oracionEsperada);
            bool correcto = ValidarOrden(tokensUsuario, tokensEsperados);
            string construida = tokensUsuario != null ? string.Join(" ", tokensUsuario) : "";

            return new ResultadoEvaluacion
            {
                aprobado = correcto,
                porcentajeSimilitud = correcto ? 100f : 0f,
                colorSemaforo = correcto ? "verde" : "rojo",
                textoEsperado = oracionEsperada,
                textoIngresado = construida,
                origenEntrada = "orden_oracion"
            };
        }
    }
}
