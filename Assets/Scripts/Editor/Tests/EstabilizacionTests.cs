using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Lecciones;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.Editor.Tests
{
    [TestFixture]
    public class EstabilizacionTests
    {
        private static readonly IReadOnlyDictionary<string, string[]> LeccionesPorEscenario =
            new Dictionary<string, string[]>
            {
                { "ra_encuentro_comunicacion", new[] { "L01", "L02", "L03", "L04", "L05", "L06" } },
                { "ra_familia_vestimenta", new[] { "L07", "L08", "L09", "L10", "L11" } },
                { "ra_casa_objetos", new[] { "L12", "L13", "L14", "L15" } },
                { "ra_cocina_alimentos", new[] { "L16", "L17", "L18", "L19" } },
                { "ra_animales_ecosistema", new[] { "L20", "L21", "L22", "L23", "L24" } },
                { "ra_naturaleza_orientacion", new[] { "L25", "L26", "L27" } }
            };

        [SetUp]
        public void PrepararCorpus()
        {
            CorpusRepository.LimpiarCache();
            Assert.IsTrue(CorpusRepository.Inicializar(), "El corpus de Editor debe inicializar correctamente.");
        }

        [Test]
        public void Contrato_CubreLos19TiposCompilados()
        {
            var tiposCompilados = CorpusRepository.ObtenerTodasLecciones()
                .SelectMany(leccion => leccion.pasos)
                .Select(paso => paso.tipo)
                .Distinct()
                .OrderBy(tipo => tipo)
                .ToArray();

            Assert.AreEqual(19, TipoPasoLeccion.Todos.Count);
            CollectionAssert.AreEquivalent(TipoPasoLeccion.Todos, tiposCompilados);
        }

        [Test]
        public void TodasLasLecciones_PuedenConstruirSusActividades()
        {
            foreach (LeccionCompiladaData leccion in CorpusRepository.ObtenerTodasLecciones())
            {
                foreach (PasoLeccionData paso in leccion.pasos)
                {
                    Assert.IsTrue(TipoPasoLeccion.EsSoportado(paso.tipo),
                        $"{leccion.id} contiene el tipo no soportado '{paso.tipo}'.");

                    if (TipoPasoLeccion.EsSeleccion(paso.tipo))
                        ValidarSeleccion(leccion.id, ActividadLeccionService.CrearSeleccion(paso));
                    else if (TipoPasoLeccion.EsEmparejamiento(paso.tipo))
                        Assert.IsNotEmpty(ActividadLeccionService.CrearPares(paso),
                            $"{leccion.id}/{paso.tipo} debe producir pares.");
                    else if (TipoPasoLeccion.EsWordBank(paso.tipo))
                        Assert.IsNotEmpty(ActividadLeccionService.CrearWordBank(paso).Tokens,
                            $"{leccion.id}/{paso.tipo} debe producir tokens.");
                }
            }
        }

        [Test]
        public void L01_TodosSusPasosTienenContratoValido()
        {
            LeccionCompiladaData leccion = CorpusRepository.ObtenerLeccion("L01");
            Assert.IsNotNull(leccion);
            Assert.IsNotEmpty(leccion.pasos);
            Assert.IsTrue(leccion.pasos.All(paso => TipoPasoLeccion.EsSoportado(paso.tipo)));
        }

        [Test]
        public void EscenariosRa_RespetanLosHitosCanonicos()
        {
            foreach (var esperado in LeccionesPorEscenario)
            {
                var escenario = CorpusRepository.ObtenerEscenarioRA(esperado.Key);
                Assert.IsNotNull(escenario, $"Falta el escenario {esperado.Key}.");
                CollectionAssert.AreEqual(esperado.Value, escenario.lecciones_asociadas);
            }
        }

        [Test]
        public void Escenas_TienenUnaUnicaViewPrincipal()
        {
            AssertUnaView("Assets/Scenes/01_Onboarding.unity", "UI.Onboarding.OnboardingView");
            AssertUnaView("Assets/Scenes/02_Main.unity", "UI.Main.MainView");
            AssertUnaView("Assets/Scenes/03_LeccionBase.unity", "UI.LeccionBase.LeccionBaseView");
            AssertUnaView("Assets/Scenes/04_RealidadAumentada.unity", "UI.RealidadAumentada.RealidadAumentadaView");
        }

        [Test]
        public void Addressables_ContieneDatosYTodosLosAudiosDisponibles()
        {
            string datos = File.ReadAllText("Assets/AddressableAssetsData/AssetGroups/Global_Data.asset");
            foreach (string direccion in new[] { "json_palabras", "json_oraciones", "json_lecciones", "json_escenarios_ra" })
                StringAssert.Contains($"m_Address: {direccion}", datos);

            string audios = File.ReadAllText("Assets/AddressableAssetsData/AssetGroups/Audios.asset");
            Assert.AreEqual(386, audios.Split(new[] { "m_Address: audio_" }, System.StringSplitOptions.None).Length - 1,
                "El catálogo debe contener exactamente los 386 clips requeridos por el corpus actual.");
        }

        private static void ValidarSeleccion(string leccionId, ActividadSeleccionData actividad)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(actividad.RespuestaCorrecta),
                $"{leccionId}/{actividad.Tipo} debe tener respuesta.");
            CollectionAssert.Contains(actividad.Opciones, actividad.RespuestaCorrecta,
                $"{leccionId}/{actividad.Tipo} debe incluir la respuesta entre sus opciones.");
            Assert.AreEqual(actividad.Opciones.Count, actividad.Opciones.Distinct().Count(),
                $"{leccionId}/{actividad.Tipo} no debe contener opciones duplicadas.");
        }

        private static void AssertUnaView(string ruta, string identificador)
        {
            string escena = File.ReadAllText(ruta);
            int cantidad = escena.Split(new[] { identificador }, System.StringSplitOptions.None).Length - 1;
            Assert.AreEqual(1, cantidad, $"{ruta} debe contener exactamente una {identificador}.");
        }
    }
}
