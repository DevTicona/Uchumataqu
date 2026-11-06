using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;


namespace Assets.Scripts.RA
{
    public static class ContenidoActividadRA
    {
        public static string TextoVisible(PasoRAModel paso, string nombre)
        {
            if (paso == null) return string.Empty;
            if (!string.IsNullOrWhiteSpace(paso.plantilla_uch))
                return paso.plantilla_uch.Replace("[nombre]", nombre ?? string.Empty);
            return TextoCorpus(paso.estimulo_id);
        }

        public static string TextoEsperado(PasoRAModel paso)
        {
            if (paso == null) return string.Empty;
            if (!string.IsNullOrWhiteSpace(paso.comparar_texto)) return paso.comparar_texto;
            return TextoCorpus(string.IsNullOrEmpty(paso.respuesta_id) ? paso.estimulo_id : paso.respuesta_id);
        }

        public static string Traduccion(PasoRAModel paso, string nombre)
        {
            if (paso == null) return string.Empty;
            if (!string.IsNullOrWhiteSpace(paso.plantilla_es))
                return paso.plantilla_es.Replace("[nombre]", nombre ?? string.Empty);
            return CorpusRepository.ObtenerPalabra(paso.estimulo_id)?.es ??
                CorpusRepository.ObtenerForma(paso.estimulo_id)?.es ??
                CorpusRepository.ObtenerOracion(paso.estimulo_id)?.es ?? string.Empty;
        }

        private static string TextoCorpus(string id)
        {
            if (string.IsNullOrEmpty(id)) return string.Empty;
            return CorpusRepository.ObtenerPalabra(id)?.uch ?? CorpusRepository.ObtenerForma(id)?.uch ??
                CorpusRepository.ObtenerOracion(id)?.uch ?? string.Empty;
        }
    }
}
