using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class LeccionCompiladaData
    {
        public string id;
        public string titulo;
        public int orden;
        public List<string> familias_nuevas = new List<string>();
        public List<string> familias_repaso = new List<string>();
        public List<string> oraciones_ids = new List<string>();
        public List<PasoLeccionData> pasos = new List<PasoLeccionData>();
    }

    [Serializable]
    public class MetricasItemData
    {
        public int P1;
        public int P2;
        public int P3;
        public int P4;
    }

    [Serializable]
    public class MetricasDistractoresData
    {
        public MetricasItemData palabras;
        public MetricasItemData formas;
        public MetricasItemData oraciones;
    }

    [Serializable]
    public class LeccionesCompiladasWrapperData
    {
        public int version;
        public MetricasDistractoresData metricas_distractores;
        public List<LeccionCompiladaData> lecciones = new List<LeccionCompiladaData>();
    }
}
