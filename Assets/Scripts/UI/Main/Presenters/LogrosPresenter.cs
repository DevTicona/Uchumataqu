using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.UI.Main
{
    /// <summary>Prepara las secciones de logros para el Presenter principal de Main.</summary>
    public static class LogrosPresenter
    {
        public static LogrosPageModel CrearModel(IReadOnlyList<LogroModel> logros)
        {
            if (logros == null) throw new ArgumentNullException(nameof(logros));
            return new LogrosPageModel
            {
                Obtenidos = logros.Where(x => x.Desbloqueado).ToList(),
                EnProgreso = logros.Where(x => !x.Desbloqueado && x.ProgresoActual > 0).ToList(),
                PorDescubrir = logros.Where(x => !x.Desbloqueado && x.ProgresoActual == 0).ToList()
            };
        }
    }
}
