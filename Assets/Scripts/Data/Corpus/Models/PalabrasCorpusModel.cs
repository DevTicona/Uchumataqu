using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class PalabrasCorpusModel
    {
        public int version;
        public List<PalabraModel> palabras = new List<PalabraModel>();
    }
}
