using Assets.Scripts.Data.Persistencia.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Assets.Scripts.ASR;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.Lecciones;
using Assets.Scripts.Persistencia;
using Assets.Scripts.PLN;
using Assets.Scripts.UI.Main;

namespace Assets.Scripts.Editor.Tests
{
    [TestFixture]
    public class AplicacionIntegrationTests
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
                        Assert.IsNotEmpty(GestorActividadLeccion.CrearPares(paso), $"{leccion.id}/{paso.tipo}");
                    else if (TipoPasoLeccion.EsWordBank(paso.tipo))
                        VerificarWordBank(leccion.id, paso);
                    else if (TipoPasoLeccion.EsEscritura(paso.tipo))
                    {
                        ActividadEscrituraModel escritura = GestorActividadLeccion.CrearEscritura(paso);
                        Assert.IsNotEmpty(escritura.Consigna, $"{leccion.id}/{paso.tipo}");
                        Assert.IsNotEmpty(escritura.RespuestaEsperada, $"{leccion.id}/{paso.tipo}");
                    }
                }
            }

            CollectionAssert.AreEquivalent(TipoPasoLeccion.Todos, tiposEncontrados);
        }

        [Test]
        public void Inicio_PriorizaLeccionEnProgresoAunqueHayaHuecosCompletados()
        {
            var progreso = new ProgresoUsuarioModel
            {
                ultima_leccion_id = "L05",
                lecciones = new List<ProgresoLeccionModel>
                {
                    new ProgresoLeccionModel { leccion_id = "L01", estado = "completada" },
                    new ProgresoLeccionModel { leccion_id = "L05", estado = "en_progreso", paso_actual = 3 }
                }
            };

            InicioModel inicio = MainModelFactory.CrearInicio(
                new PerfilUsuarioModel { nombre = "Ana", avatar_id = "avatar_02" },
                progreso,
                CorpusRepository.ObtenerTodasLecciones(),
                CorpusRepository.ObtenerTodosEscenariosRa());

            Assert.AreEqual("L05", inicio.LeccionId);
            Assert.IsTrue(inicio.EsContinuacion);
            Assert.Greater(inicio.PorcentajeLeccionActual, 0);
            Assert.IsFalse(inicio.CurriculoCompletado);
            Assert.AreEqual("avatar_02", inicio.AvatarId);
        }

        [Test]
        public void Logros_UsanUmbralesCanonicosYProgresoAcotado()
        {
            var progreso = new ProgresoUsuarioModel();
            for (int i = 1; i <= 6; i++)
                progreso.lecciones.Add(new ProgresoLeccionModel { leccion_id = $"L{i:D2}", estado = "completada" });
            progreso.MarcarEscenarioVisitado("ra_encuentro_comunicacion");

            List<LogroModel> logros = MainModelFactory.CrearLogros(progreso);

            CollectionAssert.AreEquivalent(
                new[] { "lecciones_1", "lecciones_5", "lecciones_14", "lecciones_27", "ra_1", "ra_3", "ra_6" },
                logros.Select(logro => logro.Id));
            Assert.IsTrue(logros.All(logro => !string.IsNullOrWhiteSpace(logro.IconoId)));
            Assert.AreEqual(logros.Count, logros.Select(logro => logro.IconoId).Distinct().Count(),
                "Cada logro debe tener un icono propio.");
            Assert.IsTrue(logros.Single(logro => logro.Id == "lecciones_5").Desbloqueado);
            Assert.AreEqual(6, logros.Single(logro => logro.Id == "lecciones_14").ProgresoActual);
            Assert.IsTrue(logros.Single(logro => logro.Id == "ra_1").Desbloqueado);
        }

        [Test]
        public void EdicionPerfil_ValidaTodosLosCamposSinAlterarProgreso()
        {
            var perfil = new PerfilUsuarioModel
            {
                nombre = "Inti",
                avatar_id = "avatar_01",
                onboarding_completado = true
            };
            var progreso = new ProgresoUsuarioModel
            {
                ultima_leccion_id = "L05",
                lecciones = new List<ProgresoLeccionModel>
                {
                    new ProgresoLeccionModel { leccion_id = "L05", estado = "en_progreso", paso_actual = 7 }
                }
            };

            Assert.IsFalse(PerfilValidator.AplicarEdicion(perfil, "A", "avatar_02").Valido);
            Assert.AreEqual("Inti", perfil.nombre, "Una edición inválida no debe alterar el perfil existente.");
            Assert.IsFalse(PerfilValidator.ValidarPerfilCompleto("Ana", "").Valido);
            Assert.IsTrue(PerfilValidator.AplicarEdicion(
                perfil,
                "  Ana  ",
                "avatar_02").Valido);

            Assert.AreEqual("Ana", perfil.nombre);
            Assert.AreEqual("avatar_02", perfil.avatar_id);
            Assert.IsTrue(perfil.onboarding_completado);
            Assert.AreEqual("L05", progreso.ultima_leccion_id);
            Assert.AreEqual(7, progreso.lecciones.Single().paso_actual);
        }

        [Test]
        public void AsrNoDisponible_DevuelveResultadoNeutralNoBloqueante()
        {
            var resultado = EvaluarConClip(new ReconocedorSimulado(disponible: false));
            Assert.IsFalse(resultado.evaluado);
            Assert.IsTrue(resultado.aprobado);
            Assert.AreEqual(0f, resultado.porcentajeSimilitud);
            Assert.AreEqual("neutral", resultado.colorSemaforo);
            Assert.AreEqual("voz", resultado.origenEntrada);
        }

        [Test]
        public void AsrDisponible_DelegaLaEvaluacionAlMotorDeTexto()
        {
            var resultado = EvaluarConClip(new ReconocedorSimulado(true, "palitak"));
            Assert.IsTrue(resultado.evaluado);
            Assert.IsTrue(resultado.aprobado);
            Assert.AreEqual(100f, resultado.porcentajeSimilitud);
            Assert.AreEqual("verde", resultado.colorSemaforo);
            Assert.AreEqual("GreedyDirecto", resultado.modoEvaluacion);
        }

        [TestCase("T\u00ef wiyanichay wakpacha", "TIIWIYAICHAYAKPCHA")]
        [TestCase("Thu\u00f1i seschay sits\u00f1i", "THNYISECHAYSITSNYI")]
        public void AsrEnZonaDeCoarticulacion_ReconstruyeLaOracionConElLexico(string esperado, string prediccion)
        {
            var resultado = EvaluarConClip(esperado, new ReconocedorSimulado(true, prediccion));

            Assert.IsTrue(resultado.aprobado);
            Assert.AreEqual(100f, resultado.porcentajeSimilitud);
            Assert.AreEqual("AsistidoPorLexico", resultado.modoEvaluacion);
            Assert.AreEqual(esperado.ToLowerInvariant(), resultado.textoReconocido);
        }

        [TestCase("Wiyantam palitak", "HOLABUENOSDIASAMIGO")]
        [TestCase("Wiyantam palitak", "")]
        public void AsrConEntradaFueraDelUmbral_NoUsaLaCorreccionLexica(string esperado, string prediccion)
        {
            var resultado = EvaluarConClip(esperado, new ReconocedorSimulado(true, prediccion));

            Assert.IsFalse(resultado.aprobado);
            Assert.AreEqual("RechazoAcustico", resultado.modoEvaluacion);
        }

        [TestCase("Tï wiyanichay", "TIIWIYANICHAY")]
        [TestCase("Thuñi seschay", "THUNYISESCHAY")]
        [TestCase("Wilk thö", "WILKTHOO")]
        [TestCase("Chhñi pichuchay", "CHHNYIPICHUCHAY")]
        [TestCase("Tï wiyanichay wakpacha", "TIIWIYANICHAYWAKPACHA")]
        [TestCase("Thuñi seschay sitsñi", "THUNYISESCHAYSITSNYI")]
        [TestCase("Wilk thö lorenzo", "WILKTHOOLORENSO")]
        public void AsrFonetico_ValidaExpresionesCompuestasYOracionesSinEspacios(string esperado, string prediccion)
        {
            Assert.AreEqual(prediccion, NormalizadorFoneticoAsr.ConvertirEsperado(esperado));

            var resultado = EvaluarConClip(esperado, new ReconocedorSimulado(true, "  " + prediccion + "  "));

            Assert.IsTrue(resultado.evaluado);
            Assert.IsTrue(resultado.aprobado);
            Assert.AreEqual(100f, resultado.porcentajeSimilitud);
            Assert.AreEqual(esperado, resultado.textoEsperado);
            Assert.AreEqual(prediccion, resultado.textoIngresado);
        }

        [TestCase("Palitak", "PALITAK")]
        [TestCase("Wiyantam palitak", "WIYANTAMPALITAK")]
        public void AsrFonetico_DecodificaLaSalidaParaMostrarTextoLegible(string esperado, string prediccion)
        {
            Assert.AreEqual(esperado, DecodificadorFoneticoAsr.Decodificar(prediccion, esperado));
        }

        [Test]
        public void FalloDelBindingAsr_NoPropagaLaExcepcionNiBloquea()
        {
            var resultado = EvaluarConClip(new ReconocedorSimulado(true, lanzarError: true));
            Assert.IsFalse(resultado.evaluado);
            Assert.IsTrue(resultado.aprobado);
            Assert.AreEqual("neutral", resultado.colorSemaforo);
        }

        private static ResultadoEvaluacion EvaluarConClip(IReconocedorVoz reconocedor) =>
            EvaluarConClip("palitak", reconocedor);

        private static ResultadoEvaluacion EvaluarConClip(string esperado, IReconocedorVoz reconocedor)
        {
            AudioClip clip = AudioClip.Create("prueba_asr", 160, 1, 16000, false);
            try
            {
                return EvaluadorVoz.EvaluarPronunciacion(esperado, clip, reconocedor);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(clip);
            }
        }

        private sealed class ReconocedorSimulado : IReconocedorVoz
        {
            private readonly bool _disponible;
            private readonly string _transcripcion;
            private readonly bool _lanzarError;

            public ReconocedorSimulado(bool disponible, string transcripcion = "", bool lanzarError = false)
            {
                _disponible = disponible;
                _transcripcion = transcripcion;
                _lanzarError = lanzarError;
            }

            public bool EstaDisponible() => _disponible;

            public string TranscribirAudio(AudioClip clip)
            {
                if (_lanzarError) throw new InvalidOperationException("Fallo nativo simulado");
                return _transcripcion;
            }
        }

        private static void VerificarSeleccion(
            string leccionId,
            Assets.Scripts.Data.Corpus.PasoLeccionModel paso)
        {
            ActividadSeleccionModel datos = GestorActividadLeccion.CrearSeleccion(paso);
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
            Assets.Scripts.Data.Corpus.PasoLeccionModel paso)
        {
            ActividadWordBankModel datos = GestorActividadLeccion.CrearWordBank(paso);
            Assert.IsNotEmpty(datos.Tokens, $"{leccionId}/{paso.tipo}");
            CollectionAssert.AreEqual(MotorEvaluacion.Tokenizar(datos.RespuestaEsperada), datos.Tokens);

            if (paso.tipo == TipoPasoLeccion.DictadoWordBank)
                Assert.IsEmpty(datos.Prompt);
            else
                Assert.IsNotEmpty(datos.Prompt);
        }
    }
}
