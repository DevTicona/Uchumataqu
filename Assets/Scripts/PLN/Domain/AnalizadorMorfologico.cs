using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.PLN
{
    /// <summary>Consulta las formas exportadas por HFST; no genera análisis para palabras desconocidas.</summary>
    public sealed class AnalizadorMorfologico
    {
        private readonly Dictionary<string, AnalisisMorfologicoModel[]> formas;
        public int CantidadFormas => formas.Count;

        public AnalizadorMorfologico(IEnumerable<AnalisisMorfologicoModel> analisis,
            IDictionary<string, string[]> indice)
        {
            if (analisis == null) throw new ArgumentNullException(nameof(analisis));
            if (indice == null || indice.Count == 0) throw new ArgumentException("El índice HFST está vacío.", nameof(indice));
            var porId = new Dictionary<string, AnalisisMorfologicoModel>(StringComparer.Ordinal);
            foreach (var entrada in analisis)
            {
                if (entrada == null || string.IsNullOrWhiteSpace(entrada.id) ||
                    string.IsNullOrWhiteSpace(entrada.surface) || string.IsNullOrWhiteSpace(entrada.lemma) ||
                    porId.ContainsKey(entrada.id))
                    throw new ArgumentException("El corpus HFST contiene análisis inválidos o IDs duplicados.");
                porId.Add(entrada.id, entrada);
            }

            // Vincula lookup-index.json con analyses.json y valida ID, lema, categoría y sufijos.
            // En el móvil se consultan estas exportaciones; aquí no se ejecuta un transductor HFST nativo.
            formas = new Dictionary<string, AnalisisMorfologicoModel[]>(StringComparer.Ordinal);
            var referenciados = new HashSet<string>(StringComparer.Ordinal);
            foreach (var par in indice)
            {
                string clave = NormalizarForma(par.Key);
                if (string.IsNullOrEmpty(clave) || formas.ContainsKey(clave) || par.Value == null || par.Value.Length == 0)
                    throw new ArgumentException($"Forma HFST inválida o duplicada: '{par.Key}'.");
                var encontrados = new List<AnalisisMorfologicoModel>();
                foreach (string etiqueta in par.Value)
                {
                    string[] partes = etiqueta?.Split('+') ?? Array.Empty<string>();
                    string[] ids = partes.Where(p => p.StartsWith("ID=", StringComparison.Ordinal)).ToArray();
                    string id = ids.Length == 1 ? ids[0].Substring(3) : null;
                    if (partes.Length < 3 || id == null || !porId.TryGetValue(id, out var entrada) ||
                        NormalizarForma(entrada.surface) != clave || partes[0] != entrada.lemma || partes[1] != entrada.pos ||
                        !partes.Where(p => p.StartsWith("SUF=", StringComparison.Ordinal)).Select(p => p.Substring(4))
                            .SequenceEqual(entrada.suffixes ?? Array.Empty<string>()))
                        throw new ArgumentException($"Referencia HFST inconsistente: '{par.Key}/{etiqueta}'.");
                    if (!encontrados.Contains(entrada)) encontrados.Add(entrada);
                    referenciados.Add(id);
                }
                formas.Add(clave, encontrados.ToArray());
            }
            if (referenciados.Count != porId.Count)
                throw new ArgumentException("Hay análisis HFST sin referencia en el índice.");
        }

        // Una forma desconocida devuelve cero análisis: el índice no infiere palabras ni sufijos nuevos.
        public IReadOnlyList<AnalisisMorfologicoModel> Consultar(string forma) =>
            formas.TryGetValue(NormalizarForma(forma), out var encontrados)
                ? Array.AsReadOnly(encontrados) : Array.Empty<AnalisisMorfologicoModel>();

        public List<AnalisisTokenModel> AnalizarTexto(string texto) => MotorEvaluacion.Tokenizar(texto)
            .Select(token => new AnalisisTokenModel { texto = token, analisis = Consultar(token).ToList() }).ToList();

        public void CompletarEvaluacion(ResultadoEvaluacion resultado)
        {
            if (resultado == null) throw new ArgumentNullException(nameof(resultado));
            resultado.detalleLinguistico = null;
            resultado.analisisEsperado = AnalizarTexto(resultado.textoEsperado);
            resultado.analisisIngresado = AnalizarTexto(resultado.textoIngresado);
            resultado.consultaMorfologicaRealizada = true;
            if (resultado.aprobado) return;

            foreach (var cambio in AlineadorEscritura.CompararPalabras(resultado.textoEsperado, resultado.textoIngresado))
            {
                if (cambio.Tipo != TipoCambioEscritura.Sustitucion) continue;
                // Describe únicamente traducciones documentadas de una misma familia, sin afirmar sufijos hipotéticos.
                var pares = (from e in Consultar(cambio.Esperado)
                    from r in Consultar(cambio.Recibido)
                    where e.source == "palabras.json" && r.source == "palabras.json" &&
                        !string.IsNullOrEmpty(e.family_id) && e.family_id == r.family_id &&
                        !string.IsNullOrWhiteSpace(e.translation) && !string.IsNullOrWhiteSpace(r.translation)
                    select new { Esperado = e, Recibido = r }).ToArray();
                if (pares.Length != 1) continue;
                resultado.detalleLinguistico = $"Usaste “{cambio.Recibido}” ({pares[0].Recibido.translation}); " +
                    $"se esperaba “{cambio.Esperado}” ({pares[0].Esperado.translation}).";
                return;
            }
        }

        private static string NormalizarForma(string texto) =>
            NormalizadorTexto.Normalizar(NormalizadorTexto.NormalizarEscritura(texto));
    }
}
