using System.Linq;
using NUnit.Framework;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.Lecciones;
using Assets.Scripts.Persistencia;
using Assets.Scripts.UI.Main;
using Assets.Scripts.UI.Main.Paginas;

namespace Assets.Scripts.Editor.Tests
{
    [TestFixture]
    public class Semana3Tests
    {
        [SetUp]
        public void PrepararCorpus()
        {
            CorpusRepository.LimpiarCache();
            Assert.IsTrue(CorpusRepository.Inicializar());
        }

        [Test]
        public void Onboarding_ValidaNombreUnicodeYAvatarExplicito()
        {
            Assert.IsTrue(PerfilOnboardingService.ValidarNombre("Ñusta").Valido);
            Assert.IsFalse(PerfilOnboardingService.ValidarNombre("A").Valido);
            Assert.IsFalse(PerfilOnboardingService.ValidarNombre(new string('a', 31)).Valido);
            Assert.IsFalse(PerfilOnboardingService.ValidarAvatar(null).Valido);
            Assert.IsFalse(PerfilOnboardingService.ValidarAvatar("avatar_99").Valido);
            Assert.IsTrue(PerfilOnboardingService.ValidarAvatar("avatar_04").Valido);
        }

        [Test]
        public void Mapa_Crea27LeccionesSeisHitosYSeisRegiones()
        {
            var mapa = MapaLeccionesService.Crear(
                CorpusRepository.ObtenerTodasLecciones(),
                CorpusRepository.ObtenerTodosEscenariosRa(),
                new ProgresoUsuario());

            Assert.AreEqual(33, mapa.Count);
            Assert.AreEqual(27, mapa.Count(item => !item.EsHitoRa));
            Assert.AreEqual(6, mapa.Count(item => item.EsHitoRa));
            Assert.AreEqual(6, mapa.Select(item => item.Region).Distinct().Count());
            Assert.AreEqual(EstadoIsla.Disponible, mapa.Single(item => item.LeccionId == "L01").Estado);
            Assert.AreEqual(EstadoIsla.Bloqueada, mapa.Single(item => item.LeccionId == "L02").Estado);
            Assert.IsTrue(mapa.Single(item => item.LeccionId == "L01").EsActual);
        }

        [Test]
        public void Mapa_DesbloqueaRa1YL07TrasCompletarL01AL06()
        {
            var progreso = new ProgresoUsuario();
            for (int i = 1; i <= 6; i++)
            {
                progreso.lecciones.Add(new ProgresoLeccionData
                {
                    leccion_id = $"L{i:D2}",
                    estado = "completada"
                });
            }

            var mapa = MapaLeccionesService.Crear(
                CorpusRepository.ObtenerTodasLecciones(),
                CorpusRepository.ObtenerTodosEscenariosRa(),
                progreso);

            Assert.AreEqual(
                EstadoIsla.Disponible,
                mapa.Single(item => item.EscenarioRaId == "ra_encuentro_comunicacion").Estado);
            Assert.AreEqual(EstadoIsla.Disponible, mapa.Single(item => item.LeccionId == "L07").Estado);
            Assert.IsTrue(mapa.Single(item => item.LeccionId == "L07").EsActual);
        }

        [Test]
        public void L01AL05_ConstruyenTodosSusPasosEvaluables()
        {
            foreach (var leccion in CorpusRepository.ObtenerTodasLecciones().Where(l => l.orden <= 5))
            {
                foreach (var paso in leccion.pasos)
                {
                    Assert.IsTrue(TipoPasoLeccion.EsSoportado(paso.tipo), $"{leccion.id}/{paso.tipo}");
                    if (TipoPasoLeccion.EsSeleccion(paso.tipo))
                        Assert.IsNotEmpty(ActividadLeccionService.CrearSeleccion(paso).Opciones);
                    else if (TipoPasoLeccion.EsEmparejamiento(paso.tipo))
                        Assert.IsNotEmpty(ActividadLeccionService.CrearPares(paso));
                    else if (TipoPasoLeccion.EsWordBank(paso.tipo))
                        Assert.IsNotEmpty(ActividadLeccionService.CrearWordBank(paso).Tokens);
                }
            }
        }

        [Test]
        public void NombresPropios_NoSeEnsenanComoFamiliasNiObjetivosCompilados()
        {
            string palabras = System.IO.File.ReadAllText("Assets/Content/Data/palabras.json");
            string lecciones = System.IO.File.ReadAllText("Assets/Content/Data/lecciones_compiladas.json");

            foreach (string nombreId in new[]
                     {
                         "irohito_urus", "juana", "lorenzo", "maria", "pedro", "urus", "waki"
                     })
            {
                StringAssert.DoesNotContain($"\"{nombreId}\"", palabras,
                    $"{nombreId} no debe registrarse como familia léxica.");
                StringAssert.DoesNotContain($"\"{nombreId}\"", lecciones,
                    $"{nombreId} no debe convertirse en objetivo compilado.");
            }
        }

        [Test]
        public void EscenasSemana3_TienenComposicionSerializada()
        {
            string bootstrap = System.IO.File.ReadAllText("Assets/Scenes/00_Bootstrap.unity");
            StringAssert.Contains("BootstrapView", bootstrap);
            StringAssert.Contains("view: {fileID: 744994581}", bootstrap);
            StringAssert.Contains("appInitializer: {fileID: 1453209405}", bootstrap);

            string onboarding = System.IO.File.ReadAllText("Assets/Scenes/01_Onboarding.unity");
            StringAssert.Contains("backHandler: {fileID: 1167188420}", onboarding);

            string rutaPrefab = System.IO.File.Exists("Assets/Content/Prefabs/ContenedorIsla.prefab")
                ? "Assets/Content/Prefabs/ContenedorIsla.prefab"
                : "Assets/Content/Prefabs/IslaLeccion.prefab";
            string prefab = System.IO.File.ReadAllText(rutaPrefab);
            StringAssert.Contains("UI.Main.Paginas.IslaItemView", prefab);
            StringAssert.DoesNotContain("Assets.Scripts.Views.UI.IslaLeccionView", prefab);
        }
    }
}
