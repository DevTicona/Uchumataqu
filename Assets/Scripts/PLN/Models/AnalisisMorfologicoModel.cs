using System;
using System.Collections.Generic;

namespace Assets.Scripts.PLN
{
    [Serializable]
    public sealed class AnalisisMorfologicoModel
    {
        public string id;
        public string surface;
        public string original;
        public string lemma;
        public string family_id;
        public string pos;
        public string translation;
        public string[] segments;
        public string[] suffixes;
        public Dictionary<string, string> features;
        public string source;
        public string[] source_id;
        public string[] context_ids;
        public string review_status;
        public string segmentation_status;
    }

    [Serializable]
    public sealed class AnalisisTokenModel
    {
        public string texto;
        public List<AnalisisMorfologicoModel> analisis = new List<AnalisisMorfologicoModel>();
        public bool Reconocido => analisis.Count > 0;
    }
}
