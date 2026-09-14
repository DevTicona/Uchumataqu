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
            int clipsDisponibles = Directory.GetFiles("Assets/Content/Audio", "*.wav", SearchOption.AllDirectories).Length;
            int clipsCatalogados = audios.Split(new[] { "m_Address: audio_" }, System.StringSplitOptions.None).Length - 1;
            Assert.AreEqual(clipsDisponibles, clipsCatalogados,
                "El catalogo debe contener todos los clips disponibles, sin depender de una cifra obsoleta.");

            string imagenes = File.ReadAllText("Assets/AddressableAssetsData/AssetGroups/Imagenes.asset");
            foreach (string id in Enumerable.Range(1, 27).Select(numero => $"l{numero:D2}"))
                StringAssert.Contains($"m_Address: imagen/isla/{id}", imagenes);
            foreach (string escenarioId in LeccionesPorEscenario.Keys)
                StringAssert.Contains($"m_Address: imagen/isla/{escenarioId}", imagenes);

            string ui = File.ReadAllText("Assets/AddressableAssetsData/AssetGroups/UI.asset");
            foreach (string icono in new[]
                     {
                         "icono_logro_primer_paso", "icono_logro_caminante_lago",
                         "icono_logro_guardian_palabra", "icono_logro_heredero_lengua",
                         "icono_logro_primera_exploracion", "icono_logro_explorador_uru",
                         "icono_logro_guia_territorios"
                     })
            {
                StringAssert.DoesNotContain($"m_Address: ui/icon/logros/{icono}", ui);
            }

            foreach (string direccion in new[]
                     {
                         "ui/icon/estados/icono_candado",
                         "ui/icon/estados/icono_actual",
                         "ui/icon/logros/icono_estrella"
                     })
            {
                StringAssert.DoesNotContain($"m_Address: {direccion}", ui);
            }
        }

        [Test]
        public void Main_ContieneLasCincoPestanasYDialogosOperativos()
        {
            string escena = File.ReadAllText("Assets/Scenes/02_Main.unity");
            foreach (string nombre in new[]
                     {
                         "PageInicio", "PageLecciones", "PageLogros", "PageAjustes", "PageAcerca",
                         "BtnInicio", "BtnLecciones", "BtnLogros", "BtnAjustes", "BtnAcerca",
                         "BottomNav", "ScrollMap", "ScrollLogros", "DialogoConfirmacionReinicio", "DialogoSalida",
                         "BtnConfirmarReinicio", "BtnCancelarReinicio", "BtnConfirmarSalir", "BtnCancelarSalir"
                     })
            {
                int cantidad = escena.Split(new[] { $"m_Name: {nombre}" }, System.StringSplitOptions.None).Length - 1;
                Assert.AreEqual(1, cantidad, $"02_Main debe contener exactamente un objeto {nombre}.");
            }

            StringAssert.Contains("prefabContenedorIsla: {fileID: 3065785605997044057, guid: 0f12b12c52491ee4582713ee92dc3b6b", escena);
            StringAssert.Contains("prefabSeparadorSeccion: {fileID: 5798873016202020357, guid: b19c0b51f15ada94f903acd2d1387ff6", escena);

            string prefabIsla = File.ReadAllText("Assets/Content/Prefabs/ContenedorIsla.prefab");
            StringAssert.Contains("Assets.Scripts.UI.Main.Paginas.IslaItemView", prefabIsla);
            StringAssert.Contains("UnityEngine.UI::UnityEngine.UI.LayoutElement", prefabIsla);
            StringAssert.Contains("UnityEngine.UI::UnityEngine.UI.Button", prefabIsla);

            string prefabSeparador = File.ReadAllText("Assets/Content/Prefabs/SeparadorSeccion.prefab");
            StringAssert.Contains("Assets.Scripts.UI.Main.Paginas.SeparadorSeccionView", prefabSeparador);
            StringAssert.Contains("UnityEngine.UI::UnityEngine.UI.LayoutElement", prefabSeparador);

            string vistaIsla = File.ReadAllText("Assets/Scripts/UI/Main/Paginas/IslaItemView.cs");
            StringAssert.Contains("iconoBloqueado", vistaIsla);
            StringAssert.Contains("iconoActual", vistaIsla);
            StringAssert.Contains("iconoCompletado", vistaIsla);
            StringAssert.DoesNotContain("ContentAddress.Icono", vistaIsla);

            StringAssert.Contains("iconoBloqueado: {fileID: 21300000, guid: 119638e3aa59b5846918cb264da91aa7", prefabIsla);
            StringAssert.Contains("iconoActual: {fileID: 21300000, guid: 7002d298cd661f443aeb0cb9e9ce06d5", prefabIsla);
            StringAssert.Contains("iconoCompletado: {fileID: 21300000, guid: 263a7ead482fae24a99d0a6b6a7b2745", prefabIsla);

            string vistaMapa = File.ReadAllText("Assets/Scripts/UI/Main/Paginas/MapaLeccionesView.cs");
            StringAssert.Contains("CrearFilaIsla", vistaMapa);
            StringAssert.DoesNotContain("SetOffsetZigzag", vistaMapa);
            StringAssert.DoesNotContain("SECCIÓN", vistaMapa.ToUpperInvariant());
            StringAssert.DoesNotContain("spriteSeparadorRegion", vistaMapa);

            string vistaSeparador = File.ReadAllText("Assets/Scripts/UI/Main/Paginas/SeparadorSeccionView.cs");
            StringAssert.DoesNotContain(".sprite =", vistaSeparador);

            int inicioLayoutMapa = escena.IndexOf("--- !u!114 &1980467303", System.StringComparison.Ordinal);
            int finLayoutMapa = escena.IndexOf("--- !u!", inicioLayoutMapa + 1, System.StringComparison.Ordinal);
            Assert.GreaterOrEqual(inicioLayoutMapa, 0, "No se encontró el VerticalLayoutGroup del mapa.");
            string layoutMapa = escena.Substring(inicioLayoutMapa, finLayoutMapa - inicioLayoutMapa);
            StringAssert.Contains("m_ChildForceExpandWidth: 1", layoutMapa);
            StringAssert.Contains("m_ChildControlWidth: 1", layoutMapa);
            StringAssert.Contains("m_ChildControlHeight: 1", layoutMapa);
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
