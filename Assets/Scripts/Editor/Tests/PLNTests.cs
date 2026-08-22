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
        public void Levenshtein_CalculaDistanciaCorrectamente()
        {
            Assert.AreEqual(0, MotorEvaluacion.DistanciaLevenshtein("chhaq", "chhaq"));
            Assert.AreEqual(1, MotorEvaluacion.DistanciaLevenshtein("chhaq", "chaq"));
            Assert.AreEqual(2, MotorEvaluacion.DistanciaLevenshtein("chhaq", "chhok"));
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

            // Similitud media (60% a 84%) -> Aprobado bajo criterio pedagógico suave (amarillo)
            var resCasi = MotorEvaluacion.EvaluarTexto("chhaq", "chhok");
            Assert.IsTrue(resCasi.aprobado);
            Assert.AreEqual("amarillo", resCasi.colorSemaforo);

            // Similitud baja (<60%) -> Incorrecto (rojo)
            var resIncorrecto = MotorEvaluacion.EvaluarTexto("grande", "pequeño");
            Assert.IsFalse(resIncorrecto.aprobado);
            Assert.AreEqual("rojo", resIncorrecto.colorSemaforo);
        }

        [Test]
        public void ValidadorOrden_ValidaSecuenciaTokens()
        {
            List<string> tokensCorrectos = new List<string> { "viru", "chaki", "khesay" };
            List<string> tokensEsperados = new List<string> { "viru", "chaki", "khesay" };

            Assert.IsTrue(ValidadorOrden.ValidarOrden(tokensCorrectos, tokensEsperados));

            List<string> tokensDesordenados = new List<string> { "khesay", "viru", "chaki" };
            Assert.IsFalse(ValidadorOrden.ValidarOrden(tokensDesordenados, tokensEsperados));

            List<string> tokensIncompletos = new List<string> { "viru", "chaki" };
            Assert.IsFalse(ValidadorOrden.ValidarOrden(tokensIncompletos, tokensEsperados));

            var resultadoEval = ValidadorOrden.EvaluarOrdenOracion("Viru chaki khesay", tokensCorrectos);
            Assert.IsTrue(resultadoEval.aprobado);
            Assert.AreEqual("verde", resultadoEval.colorSemaforo);
        }
    }
}
