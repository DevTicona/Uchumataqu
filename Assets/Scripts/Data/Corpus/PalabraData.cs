using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class FormaData
    {
        public string id;
        public string uch;
        public string es;
    }

    [Serializable]
    public class PalabraData
    {
        public string id;
        public string uch;
        public string es;
        public string categoria_gramatical;
        public List<FormaData> formas = new List<FormaData>();
    }

    [Serializable]
    public class PalabrasWrapperData
    {
        public int version;
        public List<PalabraData> palabras = new List<PalabraData>();
    }
}
