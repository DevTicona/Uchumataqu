using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista del mapa de 27 lecciones con scroll vertical e hitos de Realidad Aumentada intercalados.
    /// Especificado en Docs/UI/03-escena-main.md §5 y Docs/Specifications/03-modulo-ra.md §4.
    /// </summary>
    public class MapaLeccionesView : MonoBehaviour
    {
        [Header("Contenedor Scroll")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contenedorIslas;

        [Header("Prefabs")]
        [SerializeField] private GameObject prefabIslaLeccion;
        [SerializeField] private GameObject prefabHitoRA;

        private List<IslaItemView> _islasInstanciadas = new List<IslaItemView>();

        private void OnEnable()
        {
            RefrescarMapa();
        }

        public void RefrescarMapa()
        {
            LimpiarIslas();

            var lecciones = CorpusRepository.ObtenerTodasLecciones();
            if (lecciones == null || lecciones.Count == 0)
            {
                Debug.LogWarning("[MapaLeccionesView] No hay lecciones cargadas en CorpusRepository.");
                return;
            }

            var progreso = ProgresoRepository.CargarProgreso();
            int index = 0;

            foreach (var leccion in lecciones)
            {
                index++;
                bool completada = progreso != null && progreso.LeccionCompletada(leccion.id);
                bool disponible = (index == 1) || (progreso != null && progreso.LeccionCompletada($"L{index - 1:D2}"));

                EstadoIsla estado = completada ? EstadoIsla.Completada : (disponible ? EstadoIsla.Disponible : EstadoIsla.Bloqueada);

                CrearIslaLeccion(leccion, index, estado);

                // Comprobar si corresponde un hito de RA tras ciertos hitos curriculares
                // Escenarios RA según Docs/Specifications/03-modulo-ra.md:
                // Hito 1 tras L06, Hito 2 tras L10, Hito 3 tras L14, Hito 4 tras L18, Hito 5 tras L22, Hito 6 tras L27
                string hitoRaId = ObtenerHitoRaTrasLeccion(index);
                if (!string.IsNullOrEmpty(hitoRaId))
                {
                    bool raDesbloqueado = ProgresoRepository.EstaEscenarioDesbloqueado(hitoRaId);
                    CrearIslaHitoRA(hitoRaId, raDesbloqueado ? EstadoIsla.Disponible : EstadoIsla.Bloqueada);
                }
            }
        }

        private void CrearIslaLeccion(LeccionCompiladaData leccion, int numero, EstadoIsla estado)
        {
            if (contenedorIslas == null) return;

            GameObject go = prefabIslaLeccion != null
                ? Instantiate(prefabIslaLeccion, contenedorIslas)
                : new GameObject($"Isla_{leccion.id}", typeof(RectTransform), typeof(IslaItemView));

            if (prefabIslaLeccion == null)
                go.transform.SetParent(contenedorIslas, false);

            var item = go.GetComponent<IslaItemView>();
            if (item != null)
            {
                item.ConfigurarLeccion(leccion.id, numero, leccion.titulo, estado);
                item.OnLeccionSeleccionada += NavegarALeccion;
                _islasInstanciadas.Add(item);
            }
        }

        private void CrearIslaHitoRA(string escenarioId, EstadoIsla estado)
        {
            if (contenedorIslas == null) return;

            var escData = CorpusRepository.ObtenerEscenarioRaPorId(escenarioId);
            string titulo = escData != null ? escData.titulo : "Escenario RA";

            GameObject go = prefabHitoRA != null
                ? Instantiate(prefabHitoRA, contenedorIslas)
                : (prefabIslaLeccion != null ? Instantiate(prefabIslaLeccion, contenedorIslas) : new GameObject($"HitoRA_{escenarioId}", typeof(RectTransform), typeof(IslaItemView)));

            if (prefabHitoRA == null && prefabIslaLeccion == null)
                go.transform.SetParent(contenedorIslas, false);

            var item = go.GetComponent<IslaItemView>();
            if (item != null)
            {
                item.ConfigurarHitoRA(escenarioId, titulo, estado);
                item.OnHitoRaSeleccionado += NavegarAR;
                _islasInstanciadas.Add(item);
            }
        }

        private void NavegarALeccion(string leccionId)
        {
            NavigationService.IrALeccion(leccionId);
        }

        private void NavegarAR(string escenarioRaId)
        {
            NavigationService.IrARealidadAumentada(escenarioRaId);
        }

        private string ObtenerHitoRaTrasLeccion(int indexLeccion)
        {
            switch (indexLeccion)
            {
                case 6: return "ra_encuentro_comunicacion";
                case 10: return "ra_familia_vestimenta";
                case 14: return "ra_casa_objetos";
                case 18: return "ra_cocina_alimentos";
                case 22: return "ra_animales_ecosistema";
                case 27: return "ra_naturaleza_orientacion";
                default: return null;
            }
        }

        private void LimpiarIslas()
        {
            foreach (var item in _islasInstanciadas)
            {
                if (item != null)
                {
                    item.OnLeccionSeleccionada -= NavegarALeccion;
                    item.OnHitoRaSeleccionado -= NavegarAR;
                    Destroy(item.gameObject);
                }
            }
            _islasInstanciadas.Clear();
        }
    }
}
