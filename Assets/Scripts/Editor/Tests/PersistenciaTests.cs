using System.Collections.Generic;
using NUnit.Framework;
using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.Persistencia;
using Assets.Scripts.Lecciones;

namespace Assets.Scripts.Editor.Tests
{
    [TestFixture]
    public class PersistenciaTests
    {
        [SetUp]
        public void Setup()
        {
            CorpusRepository.Inicializar();
            ProgresoRepository.ReiniciarProgreso();
        }

        [Test]
        public void Perfil_GuardarYCargar_MantieneDatos()
        {
            var perfil = new PerfilUsuario
            {
                nombre = "Inti",
                rango_edad = "8-12",
                avatar_id = "avatar_02",
                onboarding_completado = true
            };

            PerfilRepository.GuardarPerfil(perfil);
            Assert.IsTrue(PerfilRepository.ExistePerfil());

            var cargado = PerfilRepository.CargarPerfil();
            Assert.IsNotNull(cargado);
            Assert.AreEqual("Inti", cargado.nombre);
            Assert.AreEqual("8-12", cargado.rango_edad);
            Assert.AreEqual("avatar_02", cargado.avatar_id);
            Assert.IsTrue(cargado.onboarding_completado);
        }

        [Test]
        public void Progreso_CompletarLeccion_ActualizaDesbloqueoRA()
        {
            // Inicialmente ningún escenario está desbloqueado
            Assert.IsFalse(ProgresoRepository.EstaEscenarioDesbloqueado("ra_encuentro_comunicacion"));

            // Completar lecciones L01 a L06 requeridas para el primer escenario RA
            for (int i = 1; i <= 6; i++)
            {
                ProgresoRepository.GuardarLeccionCompletada($"L{i:D2}", 10, 10, 100);
            }

            var progreso = ProgresoRepository.CargarProgreso();
            Assert.AreEqual(6, progreso.TotalLeccionesCompletadas);

            // Ahora el escenario 1 debe estar desbloqueado
            Assert.IsTrue(ProgresoRepository.EstaEscenarioDesbloqueado("ra_encuentro_comunicacion"));
            // El escenario 2 aún no debe estar desbloqueado
            Assert.IsFalse(ProgresoRepository.EstaEscenarioDesbloqueado("ra_familia_vestimenta"));
        }

        [Test]
        public void GestorLeccion_EjecutaFlujoCompleto()
        {
            GestorLeccion gestor = new GestorLeccion();
            bool iniciada = gestor.IniciarLeccion("L01");
            Assert.IsTrue(iniciada, "Debe ser posible iniciar la lección L01.");

            Assert.AreEqual("L01", gestor.LeccionActual.id);
            Assert.AreEqual(0, gestor.IndicePasoActual);
            Assert.IsTrue(gestor.TotalPasos > 0);

            // Simular respuesta correcta
            gestor.RegistrarResultadoPaso(true);
            Assert.AreEqual(1, gestor.Aciertos);
            Assert.AreEqual(0, gestor.Errores);

            // Simular avance hasta finalizar
            while (gestor.AvanzarSiguientePaso())
            {
                gestor.RegistrarResultadoPaso(true);
            }

            gestor.FinalizarYGuardar();

            var progreso = ProgresoRepository.CargarProgreso();
            Assert.IsTrue(progreso.LeccionCompletada("L01"), "L01 debe figurar en las lecciones completadas.");
        }
    }
}
