using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class LeccionesCompiladasCorpusModel
    {
        public int version;
        public MetricasDistractoresModel metricas_distractores;
        public List<LeccionCompiladaModel> lecciones = new List<LeccionCompiladaModel>();
    }
}
