using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class OracionData
    {
        public string id;
        public string uch;
        public string es;
    }

    [Serializable]
    public class OracionesWrapperData
    {
        public int version;
        public List<OracionData> oraciones = new List<OracionData>();
    }
}
