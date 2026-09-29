using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class EscenarioRAModel
    {
        public string id;
        public string prefab_key;
        public float escala_base = 1f;
        public MisionRAModel mision;
        public string titulo;
        public List<string> lecciones_asociadas = new List<string>();
        public List<ObjetoRAModel> objetos = new List<ObjetoRAModel>();
        public bool avatar_requerido;
    }
}
