using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.PLN;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Persistencia
{
    public static class MorfologiaJsonService
    {
        // Entrada de la integración HFST: deserializa las dos exportaciones locales y entrega su
        // validación cruzada al analizador antes de permitir consultas lingüísticas.
        public static AnalizadorMorfologico CrearAnalizador(string analisisJson, string indiceJson)
        {
            if (string.IsNullOrWhiteSpace(analisisJson) || string.IsNullOrWhiteSpace(indiceJson))
                throw new InvalidDataException("Faltan los datos HFST locales.");
            var entradas = JArray.Parse(analisisJson);
            foreach (JObject entrada in entradas)
            {
                // La exportación utiliza un ID en palabras y una lista de IDs en oraciones.
                JToken fuente = entrada["source_id"];
                if (fuente?.Type == JTokenType.String) entrada["source_id"] = new JArray(fuente.Value<string>());
            }
            var configuracion = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None };
            var analisis = JsonConvert.DeserializeObject<List<AnalisisMorfologicoModel>>(entradas.ToString(), configuracion);
            var indice = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(indiceJson, configuracion);
            try { return new AnalizadorMorfologico(analisis, indice); }
            catch (ArgumentException ex) { throw new InvalidDataException("Los archivos HFST no son consistentes entre sí.", ex); }
        }
    }
}
