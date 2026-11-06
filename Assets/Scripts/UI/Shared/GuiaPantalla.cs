using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;
using UnityEngine;

namespace Assets.Scripts.Core.UI.Ayuda
{
    [DisallowMultipleComponent]
    public sealed class GuiaPantalla : MonoBehaviour
    {
        [SerializeField] private string identificador;
        [SerializeField] private AyudaContextualView ayuda;
        [SerializeField] private PasoGuia[] pasos;
        private readonly List<PasoGuia> conceptos = new List<PasoGuia>();
        private bool esPresentacionFamilia;
        private string idPrimeraPresentacion;
        public bool EsPresentacionFamilia => esPresentacionFamilia;
        public string Identificador => identificador;
        public IReadOnlyList<PasoGuia> Pasos => pasos ?? System.Array.Empty<PasoGuia>();
        public IReadOnlyList<PasoGuia> Conceptos => conceptos;

        private void OnEnable()
        {
            if (Application.isPlaying) StartCoroutine(PresentarCuandoEsteLista());
        }

        private IEnumerator PresentarCuandoEsteLista()
        {
            // Las vistas construyen sus filas y recalculan el layout al activarse.
            yield return null;
            yield return new WaitForSecondsRealtime(0.3f);
            ResolverObjetivos();
            if (ayuda != null) ayuda.Preparar(this);
        }

        private void ResolverObjetivos()
        {
            var elementos = GetComponentsInChildren<RectTransform>(true);
            foreach (var paso in Pasos)
            {
                if (paso == null || paso.objetivo != null || string.IsNullOrEmpty(paso.nombreObjetivo)) continue;
                foreach (var elemento in elementos)
                    if (elemento.name == paso.nombreObjetivo) { paso.objetivo = elemento; break; }
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            ayuda?.Retirar(this);
            conceptos.Clear();
            esPresentacionFamilia = false;
            idPrimeraPresentacion = null;
        }

        public void ConfigurarFormas(PalabraModel palabra)
        {
            conceptos.Clear();
            esPresentacionFamilia = true;
            idPrimeraPresentacion = !string.IsNullOrWhiteSpace(palabra?.id) && palabra.formas?.Count > 0
                ? "familia:" + palabra.id + ":presentacion:v1" : identificador;
            if (Application.isPlaying && isActiveAndEnabled)
            {
                StopAllCoroutines();
                ayuda?.Retirar(this);
                StartCoroutine(PresentarCuandoEsteLista());
            }
            if (palabra?.formas == null || CorpusRepository.Morfologia == null) return;
            var agregados = new HashSet<string>();
            foreach (var forma in palabra.formas)
            {
                foreach (var analisis in CorpusRepository.Morfologia.Consultar(forma.uch))
                {
                    if (analisis.source != "palabras.json" || analisis.family_id != palabra.id) continue;
                    foreach (string sufijo in analisis.suffixes ?? System.Array.Empty<string>())
                    {
                        if ((sufijo != "chay" && sufijo != "naka" && sufijo != "pini") || !agregados.Add(sufijo))
                            continue;
                        // El corpus documenta traducciones, pero su segmentación sigue siendo una hipótesis.
                        conceptos.Add(new PasoGuia("Observa «-" + sufijo + "»",
                            "Compara estas formas documentadas:\n\n" + palabra.uch + " — " + palabra.es +
                            "\n" + forma.uch + " — " + forma.es +
                            "\n\nAprende el significado de la forma completa. La separación en sufijos está en revisión.")
                            { identificador = "concepto:" + sufijo + ":v1" });
                    }
                }
            }
        }

        public string IdConcepto(PasoGuia paso) => paso.identificador;
        public bool RegistrarPrimeraPresentacion()
        {
            if (!esPresentacionFamilia || string.IsNullOrEmpty(idPrimeraPresentacion) ||
                AyudaRepository.FueVista(idPrimeraPresentacion)) return false;
            // Se registra al abrir: volver o interrumpir no debe repetir la guía de esta familia.
            return AyudaRepository.MarcarVista(idPrimeraPresentacion);
        }

        public bool GuiaVista => AyudaRepository.FueVista(identificador);
    }
}
