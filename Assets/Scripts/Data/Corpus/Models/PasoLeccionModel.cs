using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class RefuerzoPasoModel
    {
        public string tipo;
        public string objetivo_id;
        public string oracion_id;

        public PasoLeccionModel CrearPaso()
        {
            var paso = new PasoLeccionModel
            {
                tipo = tipo,
                objetivo_id = objetivo_id,
                oracion_id = oracion_id,
                seccion = "refuerzo"
            };

            if (tipo == "audio_a_forma" || tipo == "es_a_forma")
                paso.forma_id = objetivo_id;
            else if (tipo == "ordenar_oracion" || tipo == "dictado_word_bank")
                paso.oracion_id = string.IsNullOrWhiteSpace(oracion_id) ? objetivo_id : oracion_id;

            return paso;
        }
    }

    [Serializable]
    public class PasoLeccionModel
    {
        public string tipo;
        public string objetivo_id;
        public bool tiene_formas;
        public List<string> opciones_ids = new List<string>();
        public string forma_id;
        public string raiz_id;
        public List<string> formas_ids = new List<string>();
        public List<string> items_ids = new List<string>();
        public string oracion_id;
        public List<string> opciones_oraciones_ids = new List<string>();
        public string palabra_oculta_id;
        public bool opcional;
        public string respuesta_esperada_uch;
        public string consigna;
        public string pista_es;
        public string fuente_estimulo;
        public string seccion;
        public RefuerzoPasoModel refuerzo_al_fallar;
    }
}
