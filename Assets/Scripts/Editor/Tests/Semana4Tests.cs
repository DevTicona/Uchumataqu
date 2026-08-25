using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.Lecciones;
using Assets.Scripts.Persistencia;
using Assets.Scripts.PLN;
using Assets.Scripts.UI.Main;

namespace Assets.Scripts.Editor.Tests
{
    [TestFixture]
    public class Semana4Tests
    {
        [SetUp]
        public void PrepararCorpus()
        {
            CorpusRepository.LimpiarCache();
            Assert.IsTrue(CorpusRepository.Inicializar());
        }

        [Test]
        public void L01AL27_ConstruyenTodosLosPasosEvaluables()
        {
            var tiposEncontrados = new HashSet<string>();

            foreach (var leccion in CorpusRepository.ObtenerTodasLecciones())
            {
                Assert.IsNotEmpty(leccion.pasos, leccion.id);
                foreach (var paso in leccion.pasos)
                {
                    tiposEncontrados.Add(paso.tipo);
                    Assert.IsTrue(TipoPasoLeccion.EsSoportado(paso.tipo), $"{leccion.id}/{paso.tipo}");

                    if (TipoPasoLeccion.EsSeleccion(paso.tipo))
                        VerificarSeleccion(leccion.id, paso);
                    else if (TipoPasoLeccion.EsEmparejamiento(paso.tipo))
                        Assert.IsNotEmpty(ActividadLeccionService.CrearPares(paso), $"{leccion.id}/{paso.tipo}");
                    else if (TipoPasoLeccion.EsWordBank(paso.tipo))
                        VerificarWordBank(leccion.id, paso);
                }
            }

            CollectionAssert.AreEquivalent(TipoPasoLeccion.Todos, tiposEncontrados);
        }

        [Test]
        public void Inicio_PriorizaLeccionEnProgresoAunqueHayaHuecosCompletados()
        {
            var progreso = new ProgresoUsuario
            {
                ultima_leccion_id = "L05",
                lecciones = new List<ProgresoLeccionData>
                {
                    new ProgresoLeccionData { leccion_id = "L01", estado = "completada" },
                    new ProgresoLeccionData { leccion_id = "L05", estado = "en_progreso", paso_actual = 3 }
                }
            };

            InicioPresentacionData inicio = MainPresentacionService.CrearInicio(
                new PerfilUsuario { nombre = "Ana", avatar_id = "avatar_02" },
                progreso,
                CorpusRepository.ObtenerTodasLecciones(),
                CorpusRepository.ObtenerTodosEscenariosRa());

            Assert.AreEqual("L05", inicio.LeccionId);
            Assert.IsTrue(inicio.EsContinuacion);
            Assert.IsFalse(inicio.CurriculoCompletado);
            Assert.AreEqual("avatar_02", inicio.AvatarId);
        }

        [Test]
        public void Logros_UsanUmbralesCanonicosYProgresoAcotado()
        {
            var progreso = new ProgresoUsuario();
            for (int i = 1; i <= 6; i++)
                progreso.lecciones.Add(new ProgresoLeccionData { leccion_id = $"L{i:D2}", estado = "completada" });
            progreso.MarcarEscenarioVisitado("ra_encuentro_comunicacion");

            List<LogroPresentacionData> logros = MainPresentacionService.CrearLogros(progreso);

            CollectionAssert.AreEquivalent(
                new[] { "lecciones_1", "lecciones_5", "lecciones_14", "lecciones_27", "ra_1", "ra_3", "ra_6" },
                logros.Select(logro => logro.Id));
            Assert.IsTrue(logros.Single(logro => logro.Id == "lecciones_5").Desbloqueado);
            Assert.AreEqual(6, logros.Single(logro => logro.Id == "lecciones_14").ProgresoActual);
            Assert.IsTrue(logros.Single(logro => logro.Id == "ra_1").Desbloqueado);
        }

        private static void VerificarSeleccion(
            string leccionId,
            Assets.Scripts.Data.Corpus.PasoLeccionData paso)
        {
            ActividadSeleccionData datos = ActividadLeccionService.CrearSeleccion(paso);
            Assert.IsNotEmpty(datos.RespuestaCorrecta, $"{leccionId}/{paso.tipo}");
            Assert.Contains(datos.RespuestaCorrecta, datos.Opciones, $"{leccionId}/{paso.tipo}");

            int opcionesUnicas = datos.Opciones
                .Select(NormalizadorTexto.Normalizar)
                .Distinct()
                .Count();
            Assert.AreEqual(datos.Opciones.Count, opcionesUnicas, $"Distractores duplicados en {leccionId}/{paso.tipo}");

            if (paso.tipo == TipoPasoLeccion.CompletarOracion)
            {
                StringAssert.Contains("_____", datos.Prompt, $"{leccionId}/{paso.oracion_id}");
                Assert.AreEqual(1, datos.Opciones.Count(opcion =>
                    NormalizadorTexto.Normalizar(opcion) == NormalizadorTexto.Normalizar(datos.RespuestaCorrecta)));
            }
        }

        private static void VerificarWordBank(
            string leccionId,
            Assets.Scripts.Data.Corpus.PasoLeccionData paso)
        {
            ActividadWordBankData datos = ActividadLeccionService.CrearWordBank(paso);
            Assert.IsNotEmpty(datos.Tokens, $"{leccionId}/{paso.tipo}");
            CollectionAssert.AreEqual(MotorEvaluacion.Tokenizar(datos.RespuestaEsperada), datos.Tokens);

            if (paso.tipo == TipoPasoLeccion.DictadoWordBank)
                Assert.IsEmpty(datos.Prompt);
            else
                Assert.IsNotEmpty(datos.Prompt);
        }
    }
}
