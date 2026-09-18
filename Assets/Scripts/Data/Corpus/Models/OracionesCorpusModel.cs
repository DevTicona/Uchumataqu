using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class OracionesCorpusModel
    {
        public int version;
        public List<OracionModel> oraciones = new List<OracionModel>();
    }
}
