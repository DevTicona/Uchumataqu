using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class PalabraModel
    {
        public string id;
        public string uch;
        public string es;
        public string categoria_gramatical;
        public List<FormaModel> formas = new List<FormaModel>();
    }
}
