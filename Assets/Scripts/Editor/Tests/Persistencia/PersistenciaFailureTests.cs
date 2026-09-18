using System.IO;
using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.Persistencia;
using Assets.Scripts.Lecciones;
using NUnit.Framework;

namespace Assets.Scripts.Editor.Tests
{
    public class PersistenciaFailureTests
    {
        [Test]
        public void GuardadoFallido_NoConfirmaPerfilConfiguracionNiFinalizacion()
        {
            bool anterior = UnityEngine.Debug.unityLogger.logEnabled;
            UnityEngine.Debug.unityLogger.logEnabled = false;
            try
            {
                using (var scope = new PersistenciaTestScope())
                {
                    Directory.CreateDirectory(scope.Directorio);
                    Directory.CreateDirectory(Path.Combine(scope.Directorio, "PerfilUsuario.json"));
                    Directory.CreateDirectory(Path.Combine(scope.Directorio, "ConfiguracionUsuario.json"));
                    Directory.CreateDirectory(Path.Combine(scope.Directorio, "ProgresoUsuario.json"));
                    Assert.IsFalse(PerfilRepository.GuardarPerfil(new PerfilUsuarioModel()));
                    Assert.IsFalse(ConfiguracionRepository.Guardar(new ConfiguracionUsuarioModel()));
                    Assert.IsFalse(ProgresoRepository.Guardar());
                    CorpusRepository.Inicializar();
                    var gestor = new GestorLeccion();
                    Assert.IsTrue(gestor.IniciarLeccion("L01"));
                    while (gestor.AvanzarSiguientePaso()) { }
                    Assert.IsFalse(gestor.FinalizarYGuardar());
                    Assert.IsFalse(gestor.UltimoGuardadoExitoso);
                    ProgresoRepository.LimpiarCache();
                    Assert.IsFalse(ProgresoRepository.EstaLeccionCompletada("L01"));
                }
            }
            finally { UnityEngine.Debug.unityLogger.logEnabled = anterior; }
        }

        [Test]
        public void GuardadoExitoso_SePuedeRecargarSinCache()
        {
            using (var scope = new PersistenciaTestScope())
            {
                Assert.IsTrue(PerfilRepository.GuardarPerfil(new PerfilUsuarioModel { nombre = "Inti", avatar_id = "avatar_02" }));
                PerfilRepository.LimpiarCache();
                Assert.AreEqual("Inti", PerfilRepository.CargarPerfil().nombre);
                Assert.IsTrue(GestorProgreso.CompletarLeccion("L01", 1, 4, 5));
                ProgresoRepository.LimpiarCache();
                Assert.IsTrue(ProgresoRepository.EstaLeccionCompletada("L01"));
            }
        }
    }
}
