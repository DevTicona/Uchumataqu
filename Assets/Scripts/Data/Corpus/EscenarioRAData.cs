using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class PosicionOffsetData
    {
        public float x;
        public float y;
        public float z;

        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    [Serializable]
    public class ObjetoRAData
    {
        public string palabra_id;
        public string modelo_3d;
        public PosicionOffsetData posicion_offset;
    }

    [Serializable]
    public class EscenarioRAData
    {
        public string id;
        public string titulo;
        public List<string> lecciones_asociadas = new List<string>();
        public List<ObjetoRAData> objetos = new List<ObjetoRAData>();
        public bool avatar_requerido;
    }

    [Serializable]
    public class EscenariosRAWrapperData
    {
        public int version;
        public List<EscenarioRAData> escenarios = new List<EscenarioRAData>();
    }
}
