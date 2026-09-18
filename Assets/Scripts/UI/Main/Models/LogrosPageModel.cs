using System.Collections.Generic;

namespace Assets.Scripts.UI.Main
{
    public sealed class LogrosPageModel
    {
        public IReadOnlyList<LogroModel> Obtenidos { get; set; }
        public IReadOnlyList<LogroModel> EnProgreso { get; set; }
        public IReadOnlyList<LogroModel> PorDescubrir { get; set; }
    }
}
