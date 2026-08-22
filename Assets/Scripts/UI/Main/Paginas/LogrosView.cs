using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Persistencia;

namespace Assets.Scripts.UI.Main.Paginas
{
    public class LogroItem
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public bool Desbloqueado { get; set; }
    }

    /// <summary>
    /// Vista de la pestaña de Logros en 02_Main.
    /// Especificado en Docs/UI/08-perfil-logros-ajustes-acerca.md §3.
    /// </summary>
    public class LogrosView : MonoBehaviour
    {
        [Header("Contenedor de Lista")]
        [SerializeField] private Transform contenedorLogros;
        [SerializeField] private GameObject prefabLogroItem;

        [Header("Resumen")]
        [SerializeField] private TMP_Text txtTotalLogros;

        private void OnEnable()
        {
            RefrescarLogros();
        }

        public void RefrescarLogros()
        {
            var progreso = ProgresoRepository.CargarProgreso();
            int completadas = progreso != null ? progreso.TotalLeccionesCompletadas : 0;
            int raDesbloqueados = progreso != null ? progreso.TotalEscenariosVisitados : 0;

            List<LogroItem> logros = new List<LogroItem>
            {
                new LogroItem { Id = "l1", Titulo = "Primer Paso", Descripcion = "Completa tu primera lección de Uchumataqu.", Desbloqueado = completadas >= 1 },
                new LogroItem { Id = "l5", Titulo = "Caminante del Lago", Descripcion = "Completa 5 lecciones del currículo.", Desbloqueado = completadas >= 5 },
                new LogroItem { Id = "l10", Titulo = "Guardián de la Palabra", Descripcion = "Completa 10 lecciones consecutivas.", Desbloqueado = completadas >= 10 },
                new LogroItem { Id = "ra1", Titulo = "Visión Aumentada", Descripcion = "Desbloquea tu primer escenario de Realidad Aumentada.", Desbloqueado = raDesbloqueados >= 1 },
                new LogroItem { Id = "ra3", Titulo = "Explorador Uru", Descripcion = "Desbloquea 3 escenarios de Realidad Aumentada.", Desbloqueado = raDesbloqueados >= 3 },
                new LogroItem { Id = "l27", Titulo = "Heredero de la Lengua", Descripcion = "Completa las 27 lecciones oficiales del corpus.", Desbloqueado = completadas >= 27 }
            };

            int desbloqueados = logros.FindAll(l => l.Desbloqueado).Count;
            if (txtTotalLogros != null)
            {
                txtTotalLogros.text = $"{desbloqueados} / {logros.Count} logros conseguidos";
            }

            if (contenedorLogros != null)
            {
                foreach (Transform child in contenedorLogros)
                {
                    Destroy(child.gameObject);
                }

                foreach (var logro in logros)
                {
                    CrearElementoLogro(logro);
                }
            }
        }

        private void CrearElementoLogro(LogroItem logro)
        {
            GameObject item = (prefabLogroItem != null)
                ? Instantiate(prefabLogroItem, contenedorLogros)
                : new GameObject($"Logro_{logro.Id}", typeof(RectTransform), typeof(Image));

            if (prefabLogroItem == null)
                item.transform.SetParent(contenedorLogros, false);

            var textos = item.GetComponentsInChildren<TMP_Text>();
            if (textos.Length >= 2)
            {
                textos[0].text = logro.Titulo;
                textos[1].text = logro.Descripcion;
            }
        }
    }
}
