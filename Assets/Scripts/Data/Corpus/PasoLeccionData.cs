using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class PasoLeccionData
    {
        public string tipo;
        public string objetivo_id;
        public bool tiene_formas;
        public List<string> opciones_ids = new List<string>();
        public string forma_id;
        public string raiz_id;
        public List<string> items_ids = new List<string>();
        public string oracion_id;
        public List<string> opciones_oraciones_ids = new List<string>();
        public string palabra_oculta_id;
        public bool opcional;
    }
}
