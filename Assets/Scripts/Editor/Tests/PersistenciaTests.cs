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
                avatar_id = "avatar_02",
                onboarding_completado = true
            };

            PerfilRepository.GuardarPerfil(perfil);
            Assert.IsTrue(PerfilRepository.ExistePerfil());

            var cargado = PerfilRepository.CargarPerfil();
            Assert.IsNotNull(cargado);
            Assert.AreEqual("Inti", cargado.nombre);
            Assert.AreEqual("avatar_02", cargado.avatar_id);
            Assert.IsTrue(cargado.onboarding_completado);
        }

        [Test]
        public void Progreso_CompletarLeccion_ActualizaDesbloqueoRA()
        {
            // Inicialmente ningún escenario está desbloqueado
            Assert.IsFalse(ProgresoRepository.EstaEscenarioDesbloqueado("ra_encuentro_comunicacion"));

            // Completar lecciones L01 a L06 requeridas para el primer escenario RA (Sección 13: RA 1 -> L01-L06)
            for (int i = 1; i <= 5; i++)
            {
                ProgresoRepository.GuardarLeccionCompletada($"L{i:D2}", 10, 10, 100);
            }
            // Con 5 lecciones aún no debe desbloquearse
            Assert.IsFalse(ProgresoRepository.EstaEscenarioDesbloqueado("ra_encuentro_comunicacion"));

            // Al completar la 6ta lección se desbloquea
            ProgresoRepository.GuardarLeccionCompletada("L06", 10, 10, 100);
            Assert.IsTrue(ProgresoRepository.EstaEscenarioDesbloqueado("ra_encuentro_comunicacion"));

            // El escenario 2 (L07-L11) aún no debe estar desbloqueado
            Assert.IsFalse(ProgresoRepository.EstaEscenarioDesbloqueado("ra_familia_vestimenta"));

            // Completar L07 a L11 para desbloquear RA 2
            for (int i = 7; i <= 11; i++)
            {
                ProgresoRepository.GuardarLeccionCompletada($"L{i:D2}", 10, 10, 100);
            }
            Assert.IsTrue(ProgresoRepository.EstaEscenarioDesbloqueado("ra_familia_vestimenta"));
        }

        [Test]
        public void GestorLeccion_FinalizaSinPrecisionMinimaNiASR()
        {
            // Especificación Sección 11 y 23:
            // "Una lección queda completada cuando el usuario recorrió todos los pasos obligatorios,
            // independientemente de que la precisión sea baja (ej. 40%) o de que no se haya realizado ASR."
            GestorLeccion gestor = new GestorLeccion();
            bool iniciada = gestor.IniciarLeccion("L01");
            Assert.IsTrue(iniciada, "Debe ser posible iniciar la lección L01.");

            // Simular respuestas incorrectas (precisión baja)
            gestor.RegistrarResultadoPaso(false);

            while (gestor.AvanzarSiguientePaso())
            {
                gestor.RegistrarResultadoPaso(false);
            }

            // Finalizar tras recorrer todos los pasos
            gestor.FinalizarYGuardar();

            var progreso = ProgresoRepository.CargarProgreso();
            Assert.IsTrue(progreso.LeccionCompletada("L01"), "L01 debe marcarse completada aunque la precisión sea baja.");
        }
    }
}
