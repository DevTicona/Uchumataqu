using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.PLN;

namespace Assets.Scripts.RA
{
    /// <summary>
    /// Máquina de estados de la misión RA. Avanza fases y pasos a partir de acciones validadas;
    /// emite eventos para que el presentador actualice la escena sin guardar reglas visuales aquí.
    /// </summary>
    public sealed class GestorMisionRA
    {
        public const int ErroresParaAyuda = 2;
        private readonly MisionRAModel mision;
        private readonly HashSet<string> descubiertos = new HashSet<string>();
        private int faseIndice;
        private int pasoIndice;
        private int errores;
        private bool esperandoDestino;
        private bool completada;
        private int intentosVoz;
        private bool falloTecnicoVoz;
        private int erroresLinguisticos;

        public event Action EstadoCambio;
        public event Action<PasoRAModel> PasoAcertado;
        public event Action MisionCompletada;

        public GestorMisionRA(MisionRAModel datos)
        {
            mision = datos ?? throw new ArgumentNullException(nameof(datos));
            Reiniciar();
        }

        public FaseRAModel FaseActual => !completada && faseIndice < mision.fases.Count ? mision.fases[faseIndice] : null;
        public PasoRAModel PasoActual => FaseActual != null && FaseActual.pasos != null && pasoIndice < FaseActual.pasos.Count ? FaseActual.pasos[pasoIndice] : null;
        public int Descubiertos => descubiertos.Count;
        public int TotalExploracion => FaseActual?.objetivos_objeto_ids?.Count ?? 0;
        public int NumeroPaso => pasoIndice + 1;
        public int TotalPasos => FaseActual?.pasos?.Count ?? 0;
        public int TotalPasosMision => mision.fases?.Sum(ContarPasos) ?? 0;
        public int PasosCompletados => completada ? TotalPasosMision :
            (mision.fases?.Take(faseIndice).Sum(ContarPasos) ?? 0) +
            (FaseActual?.tipo == "exploracion_libre"
                ? FaseActual.objetivos_objeto_ids?.Count(descubiertos.Contains) ?? 0 : pasoIndice);
        public float Progreso => TotalPasosMision > 0 ? (float)PasosCompletados / TotalPasosMision : 0f;
        public bool EsperandoDestino => esperandoDestino;
        public bool Completada => completada;
        public bool MostrarAyuda => errores >= ErroresParaAyuda;
        public int Errores => errores;
        public int IntentosVoz => intentosVoz;
        public int ErroresLinguisticos => erroresLinguisticos;
        public bool PuedeContinuarVoz => PasoActual?.tipo == "repetir_voz" &&
            (intentosVoz >= MaxIntentosVoz || (falloTecnicoVoz && PasoActual.continuar_tras_fallo_tecnico));
        private int MaxIntentosVoz => PasoActual?.max_intentos_evaluados > 0
            ? PasoActual.max_intentos_evaluados : 3;

        private static int ContarPasos(FaseRAModel fase) => fase.tipo == "exploracion_libre"
            ? fase.objetivos_objeto_ids?.Count ?? 0 : fase.pasos?.Count ?? 0;

        public void Reiniciar()
        {
            faseIndice = 0;
            pasoIndice = 0;
            errores = 0;
            esperandoDestino = false;
            completada = mision.fases == null || mision.fases.Count == 0;
            descubiertos.Clear();
            intentosVoz = 0;
            falloTecnicoVoz = false;
            erroresLinguisticos = 0;

            EstadoCambio?.Invoke();
        }

        public bool RegistrarObjeto(string objetoId)
        {
            if (completada || string.IsNullOrEmpty(objetoId)) return false;
            var fase = FaseActual;
            if (fase.tipo == "exploracion_libre")
            {
                if (!fase.objetivos_objeto_ids.Contains(objetoId)) return false;
                if (descubiertos.Add(objetoId))
                {
                    if (descubiertos.Count >= fase.objetivos_objeto_ids.Count) SiguienteFase();
                    else EstadoCambio?.Invoke();
                }
                return true;
            }

            var paso = PasoActual;
            if (paso == null || esperandoDestino) return false;
            if (paso.tipo != "escuchar_seleccionar" && paso.tipo != "arrastrar" &&
                !paso.EsHistoriaInteractiva) return false;
            if (paso.objetivo_objeto_id != objetoId)
            {
                if (paso.EsHistoriaInteractiva) return false;
                errores++;
                EstadoCambio?.Invoke();
                return false;
            }

            errores = 0;
            if (paso.tipo == "arrastrar")
            {
                esperandoDestino = true;
                EstadoCambio?.Invoke();
                return true;
            }

            PasoAcertado?.Invoke(paso);
            SiguientePaso();
            return true;
        }

        public bool AvanzarNarrativa()
        {
            var paso = PasoActual;
            if (completada || paso == null || !paso.EsHistoriaPresentacion) return false;
            PasoAcertado?.Invoke(paso);
            SiguientePaso();
            return true;
        }

        public bool RegistrarDestino(string destinoId)
        {
            var paso = PasoActual;
            if (!esperandoDestino || paso == null) return false;
            if (paso.destino_id != destinoId)
            {
                errores++;
                EstadoCambio?.Invoke();
                return false;
            }

            esperandoDestino = false;
            errores = 0;
            PasoAcertado?.Invoke(paso);
            SiguientePaso();
            return true;
        }

        private void SiguientePaso()
        {
            intentosVoz = 0;
            falloTecnicoVoz = false;
            errores = 0;

            pasoIndice++;
            if (pasoIndice >= FaseActual.pasos.Count) SiguienteFase();
            else EstadoCambio?.Invoke();
        }

        private void SiguienteFase()
        {
            faseIndice++;
            pasoIndice = 0;
            errores = 0;
            esperandoDestino = false;
            if (faseIndice >= mision.fases.Count)
            {
                completada = true;
                MisionCompletada?.Invoke();
            }
            EstadoCambio?.Invoke();
        }

        // Regla de avance de la misión: distingue fallos técnicos de respuestas evaluadas.
        // Voz aplica el umbral del paso; escritura conserva la aprobación exacta decidida por el motor PLN.
        public bool RegistrarEvaluacion(ResultadoEvaluacion resultado)
        {
            var paso = PasoActual;
            if (completada || paso == null || resultado == null ||
                (paso.tipo != "repetir_voz" && paso.tipo != "escribir")) return false;
            if (!resultado.evaluado)
            {
                if (paso.tipo == "repetir_voz") falloTecnicoVoz = true;
                EstadoCambio?.Invoke();
                return false;
            }
            if (paso.tipo == "repetir_voz") falloTecnicoVoz = false;
            bool aprobado = paso.tipo == "repetir_voz"
                ? resultado.porcentajeSimilitud >= (paso.umbral_aprobacion > 0 ? paso.umbral_aprobacion : 85)
                : resultado.aprobado;
            if (aprobado)
            {
                PasoAcertado?.Invoke(paso);
                SiguientePaso();
                return true;
            }
            if (paso.tipo == "repetir_voz") intentosVoz++;
            errores++;
            EstadoCambio?.Invoke();
            return false;
        }

        public bool ConfirmarEscritura(ResultadoEvaluacion resultado)
        {
            if (PasoActual?.tipo != "escribir" || resultado == null || !resultado.evaluado || !resultado.aprobado)
                return false;
            return RegistrarEvaluacion(resultado);
        }

        // Omitir tras agotar intentos o ante un fallo técnico permitido no equivale a aprobar la pronunciación.
        public bool ContinuarVoz()
        {
            if (!PuedeContinuarVoz) return false;
            if (intentosVoz >= MaxIntentosVoz) erroresLinguisticos++;
            SiguientePaso();
            return true;
        }
    }
}
