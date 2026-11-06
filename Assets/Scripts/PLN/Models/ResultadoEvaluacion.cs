using System;
using System.Collections.Generic;

namespace Assets.Scripts.PLN
{
    /// <summary>
    /// Contrato de salida único para todas las evaluaciones (texto, voz, orden).
    /// Especificado en Docs/Specifications/01-modulo-pln-texto.md (Sección 3).
    /// </summary>
    [Serializable]
    public class ResultadoEvaluacion
    {
        public bool evaluado = true;
        public bool aprobado;
        public float porcentajeSimilitud;
        public string colorSemaforo; // "verde" (>=85%) | "amarillo" (60-84%) | "rojo" (<60%)
        public string textoEsperado;
        public string textoIngresado;
        public string textoReconocido;
        public string origenEntrada; // "texto" | "voz" | "orden_oracion" | "morfologia" | "seleccion"
        public string modoEvaluacion;
        public bool consultaMorfologicaRealizada;
        public List<AnalisisTokenModel> analisisEsperado;
        public List<AnalisisTokenModel> analisisIngresado;
        public string detalleLinguistico;
    }
}
