using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data.Corpus
{
    [Serializable]
    public class ReglaAsrRAModel
    {
        public int umbral_aprobacion = 85;
        public int max_intentos_evaluados = 3;
        public bool continuar_tras_fallo_tecnico = true;
    }

    [Serializable]
    public class InteraccionPosteriorRAModel
    {
        public string estimulo_id;
        public string reaccion_id;
    }

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
        public List<PasoRAModel> pasos = new List<PasoRAModel>();
        public string resultado_audio_palabra_id;
        public string resultado_objeto_id;
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
        public List<string> opciones_ayuda_objeto_ids = new List<string>();
        public string origen;
        public string texto_tarjeta;
        public string accion_historia;
        public bool sufijo_nombre;

        public bool EsHistoriaInteractiva => tipo == "historia" &&
            (accion_historia == "tocar_objeto" || accion_historia == "tocar_o_continuar" || trayecto?.Count > 0);
        public bool EsHistoriaPresentacion => tipo == "historia" && !EsHistoriaInteractiva;
        public bool UsaTarjeta => tipo == "arrastrar" && origen == "tarjeta";
        public string destino_id;
        public string destino_acierto_id;
        public bool colocacion_vertical;
        public bool mostrar_card_escenario;
        public string instruccion;
        public string instruccion_destino;
        public string reaccion_id;
        public string respuesta_id;
        public string contexto_oracion_id;
        public string refuerzo_oracion_id;
        public string hablante;
        public string plantilla_uch;
        public string plantilla_es;
        public string comparar_texto;
        public string prefijo_voz_valido;
        public bool nombre_perfil;
        public bool sin_evaluacion_linguistica;
        public bool continuar_permitido;
        public int cantidad;
        public List<string> unidades_cantidad = new List<string>();
        // Puntos de apoyo expresados en el espacio del padre del objeto.
        public List<UnityEngine.Vector3> posiciones_cantidad = new List<UnityEngine.Vector3>();
        public List<float> escalas_cantidad = new List<float>();
        public string objeto_oracion_id;
        public int umbral_aprobacion;
        public int max_intentos_evaluados;
        public bool continuar_tras_fallo_tecnico;
        public List<string> trayecto = new List<string>();
        public InteraccionPosteriorRAModel interaccion_posterior;
    }
}
