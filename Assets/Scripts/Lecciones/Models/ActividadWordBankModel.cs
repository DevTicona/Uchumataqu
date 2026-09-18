using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;
using Assets.Scripts.PLN;

namespace Assets.Scripts.Lecciones
{
    public sealed class ActividadWordBankModel
    {
        public string Tipo { get; set; }
        public string Prompt { get; set; }
        public string RespuestaEsperada { get; set; }
        public string AudioOracionId { get; set; }
        public List<string> Tokens { get; set; } = new List<string>();
    }
}
