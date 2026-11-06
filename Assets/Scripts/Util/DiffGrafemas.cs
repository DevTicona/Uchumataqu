using System.Collections.Generic;
using System.Text;
using Assets.Scripts.PLN;

namespace Assets.Scripts.Util
{
    public static class DiffGrafemas
    {
        private const string Correcto = "#2E8B57";
        private const string Incorrecto = "#D32F2F";
        private const string Faltante = "#E08A00";

        public static List<string> Tokenizar(string texto) => AlineadorEscritura.TokenizarUnidades(texto);

        public static (string usuario, string correcta) Comparar(string usuario, string correcta)
        {
            var resultadoUsuario = new StringBuilder();
            var resultadoCorrecto = new StringBuilder();
            foreach (var cambio in AlineadorEscritura.Comparar(correcta, usuario))
            {
                switch (cambio.Tipo)
                {
                    case TipoCambioEscritura.Igual:
                        resultadoUsuario.Append(Color(Correcto, cambio.Recibido));
                        resultadoCorrecto.Append(Color(Correcto, cambio.Esperado));
                        break;
                    case TipoCambioEscritura.Sustitucion:
                        resultadoUsuario.Append(Color(Incorrecto, cambio.Recibido));
                        resultadoCorrecto.Append(ResaltarFaltante(cambio.Esperado));
                        break;
                    case TipoCambioEscritura.Faltante:
                        resultadoUsuario.Append(Color(Faltante, "_"));
                        resultadoCorrecto.Append(ResaltarFaltante(cambio.Esperado));
                        break;
                    case TipoCambioEscritura.Sobrante:
                        resultadoUsuario.Append($"<color={Incorrecto}><s>{TextoSeguro(cambio.Recibido)}</s></color>");
                        break;
                }
            }
            return (resultadoUsuario.ToString(), resultadoCorrecto.ToString());
        }

        private static string Color(string color, string texto) => $"<color={color}>{TextoSeguro(texto)}</color>";
        private static string ResaltarFaltante(string texto) => $"<color={Faltante}><b><u>{TextoSeguro(texto)}</u></b></color>";
        private static string TextoSeguro(string texto) => texto.Replace("<", "‹").Replace(">", "›");
    }
}
