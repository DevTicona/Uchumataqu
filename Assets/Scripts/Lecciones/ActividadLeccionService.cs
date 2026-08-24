using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;
using Assets.Scripts.PLN;

namespace Assets.Scripts.Lecciones
{
    public sealed class ActividadSeleccionData
    {
        public string Tipo { get; set; }
        public string Prompt { get; set; }
        public string RespuestaCorrecta { get; set; }
        public string AudioPalabraId { get; set; }
        public string AudioOracionId { get; set; }
        public List<string> Opciones { get; set; } = new List<string>();
    }

    public sealed class ActividadWordBankData
    {
        public string Tipo { get; set; }
        public string Prompt { get; set; }
        public string RespuestaEsperada { get; set; }
        public string AudioOracionId { get; set; }
        public List<string> Tokens { get; set; } = new List<string>();
    }

    public static class ActividadLeccionService
    {
        public static ActividadSeleccionData CrearSeleccion(PasoLeccionData paso)
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

        public static Dictionary<string, string> CrearPares(PasoLeccionData paso)
        {
            if (paso == null || !TipoPasoLeccion.EsEmparejamiento(paso.tipo))
                throw new ArgumentException("El paso no corresponde a una actividad de emparejamiento.", nameof(paso));

            var pares = new Dictionary<string, string>();
            IEnumerable<string> ids = paso.items_ids ?? Enumerable.Empty<string>();

            foreach (string id in ids)
            {
                PalabraData palabra = CorpusRepository.ObtenerPalabra(id);
                if (palabra != null)
                    AgregarPar(pares, palabra.uch, AlternativaNatural(palabra.es));
            }

            if (paso.tipo == TipoPasoLeccion.FamiliaCompleta)
            {
                PalabraData raiz = CorpusRepository.ObtenerPalabra(paso.raiz_id);
                if (raiz != null)
                    AgregarPar(pares, raiz.uch, AlternativaNatural(raiz.es));

                foreach (string formaId in paso.formas_ids ?? Enumerable.Empty<string>())
                {
                    FormaData forma = CorpusRepository.ObtenerForma(formaId);
                    if (forma != null)
                        AgregarPar(pares, forma.uch, AlternativaNatural(forma.es));
                }
            }

            return pares;
        }

        public static ActividadWordBankData CrearWordBank(PasoLeccionData paso)
        {
            if (paso == null || !TipoPasoLeccion.EsWordBank(paso.tipo))
                throw new ArgumentException("El paso no corresponde a Word Bank.", nameof(paso));

            OracionData oracion = CorpusRepository.ObtenerOracion(paso.oracion_id)
                ?? throw new InvalidOperationException($"No existe la oración '{paso.oracion_id}'.");

            var tokens = MotorEvaluacion.Tokenizar(oracion.uch);
            return new ActividadWordBankData
            {
                Tipo = paso.tipo,
                Prompt = paso.tipo == TipoPasoLeccion.DictadoWordBank ? string.Empty : AlternativaNatural(oracion.es),
                RespuestaEsperada = oracion.uch,
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

        private static ActividadSeleccionData CrearSeleccionPalabra(PasoLeccionData paso)
        {
            PalabraData objetivo = CorpusRepository.ObtenerPalabra(paso.objetivo_id)
                ?? throw new InvalidOperationException($"No existe la palabra '{paso.objetivo_id}'.");

            bool respuestaEspanol = paso.tipo == TipoPasoLeccion.UchAEspanol || paso.tipo == TipoPasoLeccion.AudioAEspanol;
            string respuesta = respuestaEspanol ? AlternativaNatural(objetivo.es) : objetivo.uch;
            string prompt = paso.tipo == TipoPasoLeccion.AudioAUch || paso.tipo == TipoPasoLeccion.AudioAEspanol
                ? string.Empty
                : respuestaEspanol ? objetivo.uch : AlternativaNatural(objetivo.es);

            return new ActividadSeleccionData
            {
                Tipo = paso.tipo,
                Prompt = prompt,
                RespuestaCorrecta = respuesta,
                AudioPalabraId = paso.tipo.StartsWith("audio_", StringComparison.Ordinal) ? objetivo.id : null,
                Opciones = ResolverOpcionesPalabra(paso.opciones_ids, respuestaEspanol, respuesta)
            };
        }

        private static ActividadSeleccionData CrearSeleccionForma(PasoLeccionData paso)
        {
            FormaData forma = CorpusRepository.ObtenerForma(paso.forma_id)
                ?? throw new InvalidOperationException($"No existe la forma '{paso.forma_id}'.");
            PalabraData raiz = CorpusRepository.ObtenerPalabra(paso.raiz_id);

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
                    FormaData candidata = CorpusRepository.ObtenerForma(id);
                    opcion = formaComoRespuesta ? candidata?.uch : AlternativaNatural(candidata?.es);
                }
                AgregarUnica(opciones, opcion);
            }

            return new ActividadSeleccionData
            {
                Tipo = paso.tipo,
                Prompt = prompt,
                RespuestaCorrecta = respuesta,
                AudioPalabraId = paso.tipo == TipoPasoLeccion.AudioAForma ? forma.id : null,
                Opciones = opciones
            };
        }

        private static ActividadSeleccionData CrearCompletarOracion(PasoLeccionData paso)
        {
            OracionData oracion = CorpusRepository.ObtenerOracion(paso.oracion_id)
                ?? throw new InvalidOperationException($"No existe la oración '{paso.oracion_id}'.");
            PalabraData oculta = CorpusRepository.ObtenerPalabra(paso.palabra_oculta_id)
                ?? throw new InvalidOperationException($"No existe la palabra oculta '{paso.palabra_oculta_id}'.");

            string prompt = Regex.Replace(oracion.uch, Regex.Escape(oculta.uch), "_____", RegexOptions.IgnoreCase);
            return new ActividadSeleccionData
            {
                Tipo = paso.tipo,
                Prompt = prompt,
                RespuestaCorrecta = oculta.uch,
                Opciones = ResolverOpcionesPalabra(paso.opciones_ids, false, oculta.uch)
            };
        }

        private static ActividadSeleccionData CrearSeleccionOracion(PasoLeccionData paso)
        {
            OracionData objetivo = CorpusRepository.ObtenerOracion(paso.oracion_id)
                ?? throw new InvalidOperationException($"No existe la oración '{paso.oracion_id}'.");
            string respuesta = AlternativaNatural(objetivo.es);
            var opciones = new List<string> { respuesta };
            foreach (string id in paso.opciones_oraciones_ids ?? Enumerable.Empty<string>())
                AgregarUnica(opciones, AlternativaNatural(CorpusRepository.ObtenerOracion(id)?.es));

            return new ActividadSeleccionData
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
                PalabraData palabra = CorpusRepository.ObtenerPalabra(id);
                AgregarUnica(opciones, usarEspanol ? AlternativaNatural(palabra?.es) : palabra?.uch);
            }
            return opciones;
        }

        private static void AgregarPar(IDictionary<string, string> pares, string izquierda, string derecha)
        {
            if (!string.IsNullOrWhiteSpace(izquierda) && !string.IsNullOrWhiteSpace(derecha) && !pares.ContainsKey(izquierda))
                pares.Add(izquierda, derecha);
        }

        private static void AgregarUnica(ICollection<string> opciones, string valor)
        {
            if (!string.IsNullOrWhiteSpace(valor) && !opciones.Contains(valor))
                opciones.Add(valor);
        }
    }
}
