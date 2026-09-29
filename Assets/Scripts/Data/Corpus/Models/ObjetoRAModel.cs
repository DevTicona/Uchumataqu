using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class ObjetoRAModel
    {
        
        public string id;
        public string prioridad;
        public string palabra_id;
        public string modelo_3d;
        public PosicionOffsetModel posicion_offset;
    }
}
