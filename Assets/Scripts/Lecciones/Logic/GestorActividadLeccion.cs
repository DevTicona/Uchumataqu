using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;
using Assets.Scripts.PLN;

namespace Assets.Scripts.Lecciones
{

    public static class GestorActividadLeccion
    {
        public static ActividadSeleccionModel CrearSeleccion(PasoLeccionModel paso)
        {
            if (paso == null || !TipoPasoLeccion.EsSeleccion(paso.tipo))
                throw new ArgumentException("El paso no corresponde a una actividad de selección.", nameof(paso));

            if (paso.tipo == TipoPasoLeccion.AudioAEspanolOracion)
                return CrearSeleccionOracion(paso);

            if (paso.tipo == TipoPasoLeccion.CompletarOracion)
                return CrearCompletarOracion(paso);

            if (paso.tipo == TipoPasoLeccion.FormaARaiz || paso.tipo == TipoPasoLeccion.FormaAEspanol ||
                paso.tipo == TipoPasoLeccion.EspanolAForma || paso.tipo == TipoPasoLeccion.AudioAForma)
                return CrearSeleccionForma(paso);

            return CrearSeleccionPalabra(paso);
        }

        public static Dictionary<string, string> CrearPares(PasoLeccionModel paso)
        {
            if (paso == null || !TipoPasoLeccion.EsEmparejamiento(paso.tipo))
                throw new ArgumentException("El paso no corresponde a una actividad de emparejamiento.", nameof(paso));

            var pares = new Dictionary<string, string>();
            IEnumerable<string> ids = paso.items_ids ?? Enumerable.Empty<string>();

            foreach (string id in ids)
            {
                PalabraModel palabra = CorpusRepository.ObtenerPalabra(id);
                if (palabra != null)
                    AgregarPar(pares, paso.tipo == TipoPasoLeccion.EmparejarAudioUch ? palabra.id : palabra.uch, paso.tipo == TipoPasoLeccion.EmparejarAudioUch ? palabra.uch : AlternativaNatural(palabra.es));
            }

            if (paso.tipo == TipoPasoLeccion.FamiliaCompleta)
            {
                PalabraModel raiz = CorpusRepository.ObtenerPalabra(paso.raiz_id);
                if (raiz != null)
                    AgregarPar(pares, raiz.uch, AlternativaNatural(raiz.es));

                foreach (string formaId in paso.formas_ids ?? Enumerable.Empty<string>())
                {
                    FormaModel forma = CorpusRepository.ObtenerForma(formaId);
                    if (forma != null)
                        AgregarPar(pares, forma.uch, AlternativaNatural(forma.es));
                }
            }

            return pares;
        }

        public static ActividadWordBankModel CrearWordBank(PasoLeccionModel paso)
        {
            if (paso == null || !TipoPasoLeccion.EsWordBank(paso.tipo))
                throw new ArgumentException("El paso no corresponde a Word Bank.", nameof(paso));

            OracionModel oracion = CorpusRepository.ObtenerOracion(paso.oracion_id)
                ?? throw new InvalidOperationException($"No existe la oración '{paso.oracion_id}'.");

            bool traduccion = paso.tipo == TipoPasoLeccion.TraduccionConstructiva;
            string respuesta = traduccion ? AlternativaNatural(oracion.es) : oracion.uch;
            var tokens = MotorEvaluacion.Tokenizar(respuesta);
            return new ActividadWordBankModel
            {
                Tipo = paso.tipo,
                Prompt = paso.tipo == TipoPasoLeccion.DictadoWordBank ? string.Empty : traduccion ? oracion.uch : AlternativaNatural(oracion.es),
                RespuestaEsperada = respuesta,
                AudioOracionId = oracion.id,
                Tokens = tokens
            };
        }

        public static string AlternativaNatural(string traduccion)
        {
            if (string.IsNullOrWhiteSpace(traduccion)) return string.Empty;
            string texto = traduccion.Trim();
            int separador = texto.IndexOf('/');
            if (separador < 0) return texto;

            string primera = texto.Substring(0, separador).Trim();
            string resto = texto.Substring(separador + 1).Trim();
            int primerEspacio = resto.IndexOf(' ');
            string sufijo = primerEspacio >= 0 ? resto.Substring(primerEspacio) : string.Empty;
            return (primera + sufijo).Trim();
        }

        private static ActividadSeleccionModel CrearSeleccionPalabra(PasoLeccionModel paso)
        {
            PalabraModel objetivo = CorpusRepository.ObtenerPalabra(paso.objetivo_id)
                ?? throw new InvalidOperationException($"No existe la palabra '{paso.objetivo_id}'.");

            bool respuestaEspanol = paso.tipo == TipoPasoLeccion.UchAEspanol || paso.tipo == TipoPasoLeccion.AudioAEspanol;
            string respuesta = respuestaEspanol ? AlternativaNatural(objetivo.es) : objetivo.uch;
            string prompt = paso.tipo == TipoPasoLeccion.AudioAUch || paso.tipo == TipoPasoLeccion.AudioAEspanol
                ? string.Empty
                : respuestaEspanol ? objetivo.uch : AlternativaNatural(objetivo.es);

            return new ActividadSeleccionModel
            {
                Tipo = paso.tipo,
                Prompt = prompt,
                RespuestaCorrecta = respuesta,
                AudioPalabraId = paso.tipo.StartsWith("audio_", StringComparison.Ordinal) ||
                    paso.tipo == TipoPasoLeccion.UchAEspanol ? objetivo.id : null,
                Opciones = ResolverOpcionesPalabra(paso.opciones_ids, respuestaEspanol, respuesta)
            };
        }

        private static ActividadSeleccionModel CrearSeleccionForma(PasoLeccionModel paso)
        {
            FormaModel forma = CorpusRepository.ObtenerForma(paso.forma_id)
                ?? throw new InvalidOperationException($"No existe la forma '{paso.forma_id}'.");
            PalabraModel raiz = CorpusRepository.ObtenerPalabra(paso.raiz_id);

            if (paso.tipo == TipoPasoLeccion.FormaARaiz && raiz == null)
                throw new InvalidOperationException($"No existe la raíz '{paso.raiz_id}'.");

            bool formaComoRespuesta = paso.tipo == TipoPasoLeccion.EspanolAForma || paso.tipo == TipoPasoLeccion.AudioAForma;
            bool raizComoRespuesta = paso.tipo == TipoPasoLeccion.FormaARaiz;
            string respuesta = raizComoRespuesta ? raiz?.uch : formaComoRespuesta ? forma.uch : AlternativaNatural(forma.es);
            string prompt = paso.tipo == TipoPasoLeccion.AudioAForma
                ? string.Empty
                : formaComoRespuesta ? AlternativaNatural(forma.es) : forma.uch;

            var opciones = new List<string> { respuesta };
            foreach (string id in paso.opciones_ids ?? Enumerable.Empty<string>())
            {
                string opcion;
                if (raizComoRespuesta)
                {
                    opcion = CorpusRepository.ObtenerPalabra(id)?.uch;
                }
                else
                {
                    FormaModel candidata = CorpusRepository.ObtenerForma(id);
                    opcion = formaComoRespuesta ? candidata?.uch : AlternativaNatural(candidata?.es);
                }
                AgregarUnica(opciones, opcion);
            }

            return new ActividadSeleccionModel
            {
                Tipo = paso.tipo,
                Prompt = prompt,
                RespuestaCorrecta = respuesta,
                AudioPalabraId = paso.tipo == TipoPasoLeccion.AudioAForma ||
                    paso.tipo == TipoPasoLeccion.FormaARaiz || paso.tipo == TipoPasoLeccion.FormaAEspanol ? forma.id : null,
                Opciones = opciones
            };
        }

        private static ActividadSeleccionModel CrearCompletarOracion(PasoLeccionModel paso)
        {
            OracionModel oracion = CorpusRepository.ObtenerOracion(paso.oracion_id)
                ?? throw new InvalidOperationException($"No existe la oración '{paso.oracion_id}'.");
            string textoOculto = CorpusRepository.ObtenerPalabra(paso.palabra_oculta_id)?.uch
                ?? CorpusRepository.ObtenerForma(paso.palabra_oculta_id)?.uch;
            if (string.IsNullOrWhiteSpace(textoOculto))
                throw new InvalidOperationException($"No existe la palabra o forma oculta '{paso.palabra_oculta_id}'.");

            string prompt = Regex.Replace(oracion.uch, Regex.Escape(textoOculto), "_____", RegexOptions.IgnoreCase);
            if (string.Equals(prompt, oracion.uch, StringComparison.Ordinal))
                throw new InvalidOperationException(
                    $"La respuesta '{textoOculto}' no aparece en la oración '{oracion.id}'.");

            return new ActividadSeleccionModel
            {
                Tipo = paso.tipo,
                Prompt = prompt,
                RespuestaCorrecta = textoOculto,
                Opciones = ResolverOpcionesPalabraOForma(paso.opciones_ids, textoOculto)
            };
        }

        private static ActividadSeleccionModel CrearSeleccionOracion(PasoLeccionModel paso)
        {
            OracionModel objetivo = CorpusRepository.ObtenerOracion(paso.oracion_id)
                ?? throw new InvalidOperationException($"No existe la oración '{paso.oracion_id}'.");
            string respuesta = AlternativaNatural(objetivo.es);
            var opciones = new List<string> { respuesta };
            foreach (string id in paso.opciones_oraciones_ids ?? Enumerable.Empty<string>())
                AgregarUnica(opciones, AlternativaNatural(CorpusRepository.ObtenerOracion(id)?.es));

            return new ActividadSeleccionModel
            {
                Tipo = paso.tipo,
                Prompt = string.Empty,
                RespuestaCorrecta = respuesta,
                AudioOracionId = objetivo.id,
                Opciones = opciones
            };
        }

        private static List<string> ResolverOpcionesPalabra(IEnumerable<string> ids, bool usarEspanol, string respuesta)
        {
            var opciones = new List<string> { respuesta };
            foreach (string id in ids ?? Enumerable.Empty<string>())
            {
                PalabraModel palabra = CorpusRepository.ObtenerPalabra(id);
                AgregarUnica(opciones, usarEspanol ? AlternativaNatural(palabra?.es) : palabra?.uch);
            }
            return opciones;
        }

        private static List<string> ResolverOpcionesPalabraOForma(IEnumerable<string> ids, string respuesta)
        {
            var opciones = new List<string>();
            AgregarUnica(opciones, respuesta);
            foreach (string id in ids ?? Enumerable.Empty<string>())
            {
                string opcion = CorpusRepository.ObtenerPalabra(id)?.uch
                    ?? CorpusRepository.ObtenerForma(id)?.uch;
                AgregarUnica(opciones, opcion);
            }
            return opciones;
        }

        private static void AgregarPar(IDictionary<string, string> pares, string izquierda, string derecha)
        {
            bool derechaDuplicada = pares.Values.Any(valor =>
                string.Equals(valor, derecha, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(izquierda) && !string.IsNullOrWhiteSpace(derecha) &&
                !pares.ContainsKey(izquierda) && !derechaDuplicada)
                pares.Add(izquierda, derecha);
        }

        private static void AgregarUnica(ICollection<string> opciones, string valor)
        {
            bool duplicada = opciones.Any(opcion =>
                string.Equals(
                    NormalizadorTexto.Normalizar(opcion),
                    NormalizadorTexto.Normalizar(valor),
                    StringComparison.Ordinal));
            if (!string.IsNullOrWhiteSpace(valor) && !duplicada)
                opciones.Add(valor);
        }
    }
}
