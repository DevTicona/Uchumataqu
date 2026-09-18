using System.Collections.Generic;
using NUnit.Framework;
using Assets.Scripts.Persistencia;
using Assets.Scripts.Data.Corpus;

namespace Assets.Scripts.Editor.Tests
{
    [TestFixture]
    public class CorpusIntegrityTests
    {
        [SetUp]
        public void Setup()
        {
            CorpusRepository.Inicializar();
        }

        [Test]
        public void Corpus_ContienePalabrasYOraciones()
        {
            var palabras = CorpusRepository.ObtenerTodasPalabras();
            Assert.IsNotNull(palabras, "La lista de palabras no debe ser nula.");
            Assert.IsTrue(palabras.Count > 0, "El corpus debe contener palabras cargadas.");

            var oraciones = CorpusRepository.ObtenerTodasOraciones();
            Assert.IsNotNull(oraciones, "La lista de oraciones no debe ser nula.");
            Assert.IsTrue(oraciones.Count > 0, "El corpus debe contener oraciones cargadas.");
        }

        [Test]
        public void LeccionesCompiladas_Contiene27LeccionesValidas()
        {
            var lecciones = CorpusRepository.ObtenerTodasLecciones();
            Assert.IsNotNull(lecciones, "La lista de lecciones no debe ser nula.");
            Assert.AreEqual(27, lecciones.Count, "Deben existir exactamente 27 lecciones compiladas oficiales.");

            for (int i = 1; i <= 27; i++)
            {
                string idEsperado = $"L{i:D2}";
                var leccion = CorpusRepository.ObtenerLeccionPorId(idEsperado);
                Assert.IsNotNull(leccion, $"La lección '{idEsperado}' debe existir.");
                Assert.IsNotNull(leccion.pasos, $"La lección '{idEsperado}' debe contener una lista de pasos.");
                Assert.IsTrue(leccion.pasos.Count > 0, $"La lección '{idEsperado}' debe contener al menos un paso.");
            }
        }

        [Test]
        public void EscenariosRA_Contiene6EscenariosOficiales()
        {
            var escenarios = CorpusRepository.ObtenerTodosEscenariosRa();
            Assert.IsNotNull(escenarios, "La lista de escenarios RA no debe ser nula.");
            Assert.AreEqual(6, escenarios.Count, "Deben existir exactamente 6 escenarios de Realidad Aumentada.");

            string[] idsEsperados = new string[]
            {
                "ra_encuentro_comunicacion",
                "ra_familia_vestimenta",
                "ra_casa_objetos",
                "ra_cocina_alimentos",
                "ra_animales_ecosistema",
                "ra_naturaleza_orientacion"
            };

            foreach (var id in idsEsperados)
            {
                var esc = CorpusRepository.ObtenerEscenarioRaPorId(id);
                Assert.IsNotNull(esc, $"El escenario RA '{id}' debe existir en el corpus.");
                Assert.IsNotNull(esc.lecciones_asociadas, $"El escenario '{id}' debe definir lecciones asociadas.");
                Assert.IsTrue(esc.lecciones_asociadas.Count > 0, $"El escenario '{id}' debe requerir al menos una lección completada.");
            }
        }
    }
}
