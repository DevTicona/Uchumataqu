using System.Collections.Generic;
using Assets.Scripts.UI.Main;
using NUnit.Framework;

namespace Assets.Scripts.Editor.Tests
{
    public class LogrosPresenterTests
    {
        [Test]
        public void CrearModel_SeparaLogrosSinDuplicarlosNiModificarLosDatos()
        {
            var obtenido = new LogroModel { Desbloqueado = true, ProgresoActual = 5 };
            var enProgreso = new LogroModel { ProgresoActual = 2 };
            var porDescubrir = new LogroModel { ProgresoActual = 0 };
            var datos = new List<LogroModel> { obtenido, enProgreso, porDescubrir };

            LogrosPageModel model = LogrosPresenter.CrearModel(datos);

            CollectionAssert.AreEqual(new[] { obtenido }, model.Obtenidos);
            CollectionAssert.AreEqual(new[] { enProgreso }, model.EnProgreso);
            CollectionAssert.AreEqual(new[] { porDescubrir }, model.PorDescubrir);
            Assert.AreEqual(3, datos.Count);
            Assert.AreEqual(2, enProgreso.ProgresoActual);
        }
    }
}
