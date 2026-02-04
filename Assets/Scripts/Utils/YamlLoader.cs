using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using Assets.Scripts.Models.Activities;

namespace Assets.Scripts.Utils
{
    // Deserializador personalizado para TipoActividad
    public class TipoActividadYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(System.Type type)
        {
            return type == typeof(TipoActividad);
        }

        public object ReadYaml(IParser parser, System.Type type)
        {
            // Si es un valor scalar (string simple), convertirlo a TipoActividad
            if (parser.Current is Scalar scalar)
            {
                parser.MoveNext();
                return new TipoActividad(scalar.Value);
            }

            // Si es un mapping (objeto), dejar que el deserializador normal lo maneje
            return null;
        }

        public void WriteYaml(IEmitter emitter, object value, System.Type type)
        {
            var actividad = value as TipoActividad;
            emitter.Emit(new Scalar(null, null, actividad?.id ?? "", ScalarStyle.Plain, true, false));
        }
    }

    public static class YamlLoader
    {
        private static readonly IDeserializer deserializer =
            new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .WithTypeConverter(new TipoActividadYamlConverter())
                .Build();

        // Método correcto: carga desde TEXTO
        public static T LoadFromText<T>(string yamlText)
        {
            return deserializer.Deserialize<T>(yamlText);
        }
    }
}
