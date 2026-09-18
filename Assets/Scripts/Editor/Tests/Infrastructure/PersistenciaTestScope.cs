using System;
using System.IO;
using Assets.Scripts.Persistencia;
using UnityEngine;

namespace Assets.Scripts.Editor.Tests
{
    public sealed class PersistenciaTestScope : IDisposable
    {
        private readonly IDisposable _scope;
        public string Directorio { get; }

        public PersistenciaTestScope()
        {
            Directorio = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Temp", "PersistenciaTests", Guid.NewGuid().ToString("N")));
            _scope = JsonLocalService.UsarDirectorioPruebas(Directorio);
            LimpiarCaches();
        }

        public void Dispose()
        {
            _scope.Dispose();
            LimpiarCaches();
        }

        private static void LimpiarCaches()
        {
            PerfilRepository.LimpiarCache();
            ProgresoRepository.LimpiarCache();
            ConfiguracionRepository.LimpiarCache();
        }
    }

    [NUnit.Framework.SetUpFixture]
    public sealed class PersistenciaTestEnvironment
    {
        private PersistenciaTestScope _scope;

        [NUnit.Framework.OneTimeSetUp]
        public void Inicializar() => _scope = new PersistenciaTestScope();

        [NUnit.Framework.OneTimeTearDown]
        public void Restaurar() => _scope?.Dispose();
    }
}
