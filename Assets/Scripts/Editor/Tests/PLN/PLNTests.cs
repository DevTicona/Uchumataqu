using System.Collections.Generic;
using NUnit.Framework;
using Assets.Scripts.PLN;

namespace Assets.Scripts.Editor.Tests
{
    [TestFixture]
    public class PLNTests
    {
        [Test]
        public void NormalizadorTexto_NormalizaEspaciosYMinsuculas()
        {
            string original = "  Chhaq  ";
            string normalizado = NormalizadorTexto.Normalizar(original);
            Assert.AreEqual("chhaq", normalizado);
        }

        [Test]
        public void NormalizadorTexto_RemuevePuntuacionComun()
        {
            string original = "¡Tek, tek, tek! ¿Chis?";
            string normalizado = NormalizadorTexto.Normalizar(original);
            Assert.AreEqual("tek tek tek chis", normalizado);
        }

        [Test]
        public void NormalizadorTexto_ConservaGrafiasPropiasYVocalesLargas()
        {
            // Conserva apóstrofe y grafías propias del uchumataqu
            string original = "ch'amaka k'ata";
            string normalizado = NormalizadorTexto.Normalizar(original);
            Assert.AreEqual("ch'amaka k'ata", normalizado);

            // Cadena vacía
            Assert.AreEqual("", NormalizadorTexto.Normalizar(""));
            Assert.AreEqual("", NormalizadorTexto.Normalizar("   "));
        }

        [Test]
        public void Levenshtein_CalculaDistanciaCorrectamente()
        {
            Assert.AreEqual(0, MotorEvaluacion.DistanciaLevenshtein("chhaq", "chhaq"));
            Assert.AreEqual(1, MotorEvaluacion.DistanciaLevenshtein("chhaq", "chaq"));
            Assert.AreEqual(2, MotorEvaluacion.DistanciaLevenshtein("chhaq", "chhok"));
            Assert.AreEqual(0, MotorEvaluacion.DistanciaLevenshtein("", ""));
            Assert.AreEqual(4, MotorEvaluacion.DistanciaLevenshtein("test", ""));
            // Simetría
            Assert.AreEqual(
                MotorEvaluacion.DistanciaLevenshtein("viru", "chaki"),
                MotorEvaluacion.DistanciaLevenshtein("chaki", "viru")
            );
        }

        [Test]
        public void Similitud_CalculaPorcentajeCorrectamente()
        {
            float simExacta = MotorEvaluacion.PorcentajeSimilitud("chhaq", "chhaq");
            Assert.AreEqual(100.0f, simExacta, 0.001f);

            float simVacia = MotorEvaluacion.PorcentajeSimilitud("", "");
            Assert.AreEqual(100.0f, simVacia, 0.001f);

            float simCero = MotorEvaluacion.PorcentajeSimilitud("abc", "xyz");
            Assert.AreEqual(0.0f, simCero, 0.001f);
        }

        [Test]
        public void EvaluarTexto_AplicaUmbralesSemaforo()
        {
            // 100% exacto -> Aprobado (verde)
            var resExacto = MotorEvaluacion.EvaluarTexto("grande", "Grande");
            Assert.IsTrue(resExacto.aprobado);
            Assert.AreEqual("verde", resExacto.colorSemaforo);

            // Similitud alta (>= 85%) -> Aprobado (verde)
            var resCasiAlto = MotorEvaluacion.EvaluarTexto("chhaqchay", "chhaqchaye");
            Assert.IsTrue(resCasiAlto.aprobado);
            Assert.AreEqual("verde", resCasiAlto.colorSemaforo);

            // Similitud media (60% a 84%) -> Casi (amarillo), no aprobado
            var resCasi = MotorEvaluacion.EvaluarTexto("chhaq", "chhok");
            Assert.IsFalse(resCasi.aprobado);
            Assert.AreEqual("amarillo", resCasi.colorSemaforo);

            // Similitud baja (<60%) -> Incorrecto (rojo)
            var resIncorrecto = MotorEvaluacion.EvaluarTexto("grande", "pequeño");
            Assert.IsFalse(resIncorrecto.aprobado);
            Assert.AreEqual("rojo", resIncorrecto.colorSemaforo);
        }

        [Test]
        public void AnalizadorErrorEscritura_DescribeLetraPalabraYExceso()
        {
            Assert.AreEqual("Te faltó la letra “a” en “pala”.",
                AnalizadorErrorEscritura.Describir("pala", "pal"));
            Assert.AreEqual("Te faltó la palabra “suma”.",
                AnalizadorErrorEscritura.Describir("pala suma", "pala"));
            Assert.AreEqual("La palabra “extra” está de más.",
                AnalizadorErrorEscritura.Describir("pala", "pala extra"));
        }

        [Test]
        public void ValidadorOrden_ValidaSecuenciaTokens()
        {
            List<string> tokensCorrectos = new List<string> { "viru", "chaki", "khesay" };
            List<string> tokensEsperados = new List<string> { "viru", "chaki", "khesay" };

            // Mismo número y orden -> true
            Assert.IsTrue(ValidadorOrden.ValidarOrden(tokensCorrectos, tokensEsperados));

            // Distinto orden -> false
            List<string> tokensDesordenados = new List<string> { "khesay", "viru", "chaki" };
            Assert.IsFalse(ValidadorOrden.ValidarOrden(tokensDesordenados, tokensEsperados));

            // Token faltante -> false
            List<string> tokensIncompletos = new List<string> { "viru", "chaki" };
            Assert.IsFalse(ValidadorOrden.ValidarOrden(tokensIncompletos, tokensEsperados));

            // Token extra -> false
            List<string> tokensExtra = new List<string> { "viru", "chaki", "khesay", "mas" };
            Assert.IsFalse(ValidadorOrden.ValidarOrden(tokensExtra, tokensEsperados));

            var resultadoEval = ValidadorOrden.EvaluarOrdenOracion("Viru chaki khesay", tokensCorrectos);
            Assert.IsTrue(resultadoEval.aprobado);
            Assert.AreEqual("verde", resultadoEval.colorSemaforo);
        }
    }
}
