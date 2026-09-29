using System;
using System.Collections.Generic;
using Assets.Scripts.Data.Corpus;

namespace Assets.Scripts.RA
{
    public sealed class GestorMisionRA
    {
        private readonly MisionRAModel mision;
        private readonly HashSet<string> descubiertos = new HashSet<string>();
        private int faseIndice;
        private int pasoIndice;
        private int errores;
        private bool esperandoDestino;
        private bool completada;

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
        public bool EsperandoDestino => esperandoDestino;
        public bool Completada => completada;
        public bool MostrarAyuda => errores >= 2;
        public int Errores => errores;

        public void Reiniciar()
        {
            faseIndice = 0;
            pasoIndice = 0;
            errores = 0;
            esperandoDestino = false;
            completada = mision.fases == null || mision.fases.Count == 0;
            descubiertos.Clear();
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
            if (paso.objetivo_objeto_id != objetoId)
            {
                errores++;
                EstadoCambio?.Invoke();
                return false;
            }

            errores = 0;
            if (paso.tipo == "seleccionar_colocar")
            {
                esperandoDestino = true;
                EstadoCambio?.Invoke();
                return true;
            }

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
    }
}
