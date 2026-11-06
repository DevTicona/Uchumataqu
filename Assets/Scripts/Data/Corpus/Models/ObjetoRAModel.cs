using System;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class AnclajeCardRAModel
    {
        public string[] referencias = new string[0];
        public float margen_superior = 0.04f;
    }

    [Serializable]
    public class CardEscenarioRAModel
    {
        public string palabra_id;
        public string referencia;
        public string ancla;
        public PosicionOffsetModel posicion_relativa;
        public float margen_superior = 0.04f;
    }

    [Serializable]
    public class ObjetoRAModel
    {
        public string id;
        public string palabra_id;
        public AnclajeCardRAModel anclaje_card;
        public float escala_visual = 1f;
    }
}
