using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class MisionRAModel
    {
        public string id;
        public List<FaseRAModel> fases = new List<FaseRAModel>();
    }

    [Serializable]
    public class FaseRAModel
    {
        public string id;
        public string tipo;
        public List<string> objetivos_objeto_ids = new List<string>();
        public bool contador_primera_interaccion;
        public List<PasoRAModel> pasos = new List<PasoRAModel>();
        public string resultado_audio_palabra_id;
        public string reaccion_al_completar;
    }

    [Serializable]
    public class PasoRAModel
    {
        public string id;
        public string tipo;
        public string estimulo_tipo;
        public string estimulo_id;
        public string objetivo_objeto_id;
        public string destino_id;
        public string reaccion_id;
    }
}
