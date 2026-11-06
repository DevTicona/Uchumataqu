using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class EscenariosRACorpusModel
    {
        public int version;
        public ReglaAsrRAModel regla_asr;
        public List<EscenarioRAModel> escenarios = new List<EscenarioRAModel>();
    }
}
