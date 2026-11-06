using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class LeccionCompiladaModel
    {
        public string id;
        public string titulo;
        public int orden;
        public List<string> familias_nuevas = new List<string>();
        public List<string> familias_repaso = new List<string>();
        public List<string> oraciones_ids = new List<string>();
        public List<PasoLeccionModel> pasos = new List<PasoLeccionModel>();
        [NonSerialized] public string RevisionContenido;
    }
}
