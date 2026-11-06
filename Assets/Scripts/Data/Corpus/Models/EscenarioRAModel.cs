using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class EscenarioRAModel
    {
        public string id;
        public string prefab_key;
        public MisionRAModel mision;
        public string titulo;
        public List<string> lecciones_asociadas = new List<string>();
        public List<ObjetoRAModel> objetos = new List<ObjetoRAModel>();
        public List<string> objetos_contexto_ids = new List<string>();
        public List<string> objetos_opcionales_exploracion_ids = new List<string>();
        public bool exploracion_posterior;
        public CardEscenarioRAModel card_escenario;
    }
}
