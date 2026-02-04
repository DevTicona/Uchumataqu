using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models;
using Assets.Scripts.Models.Activities;
using Assets.Scripts.Views.Activities;

namespace Assets.Scripts.Presenters.Activities
{
    /// <summary>
    /// Presenter de la actividad de Emparejamiento.
    /// Columna izquierda: Español
    /// Columna derecha: Uchumataqu
    /// </summary>
    public class EmparejamientoPresenter : ActividadPresenterBase
    {
        [Header("Vista")]
        [SerializeField] private EmparejamientoView vista;

        [Header("Timing")]
        [SerializeField] private float tiempoFeedbackExito = 2f;

        private ActividadData actividadData;

        private int? indiceSeleccionado1;
        private int? indiceSeleccionado2;

        private readonly HashSet<int> indicesParejados = new();
        private readonly Dictionary<int, string> mapeoIndiceAIdPalabra = new();
        private readonly HashSet<string> idsPalabrasCorrectas = new();

        // =========================
        // Inicialización
        // =========================
        public override void IniciarActividad(ActividadData data)
        {
            if (vista == null)
            {
                Debug.LogError("[EmparejamientoPresenter] Vista no asignada.");
                return;
            }

            actividadData = data;

            indiceSeleccionado1 = null;
            indiceSeleccionado2 = null;
            indicesParejados.Clear();
            mapeoIndiceAIdPalabra.Clear();
            idsPalabrasCorrectas.Clear();

            foreach (var palabra in actividadData.palabrasCorrectas)
                idsPalabrasCorrectas.Add(palabra.id);

            vista.AlSeleccionarOpcion -= OnOpcionSeleccionada;
            vista.AlSeleccionarOpcion += OnOpcionSeleccionada;

            GenerarOpcionesSeparadas(
                out List<string> columnaEspanol,
                out List<string> columnaUchumataqu
            );

            vista.Renderizar(
                "Empareja Español con Uchumataqu",
                columnaEspanol,
                columnaUchumataqu
            );
        }

        private void OnDestroy()
        {
            if (vista != null)
                vista.AlSeleccionarOpcion -= OnOpcionSeleccionada;
        }

        // =========================
        // Generación correcta de opciones
        // =========================
        private void GenerarOpcionesSeparadas(
            out List<string> espanoles,
            out List<string> uchumataqu
        )
        {
            var palabrasCorrectas = actividadData.palabrasCorrectas;
            var distractores = actividadData.palabrasIncorrectas;

            espanoles = new List<string>();
            uchumataqu = new List<string>();

            int index = 0;

            // Columna izquierda: Español (palabras correctas en español)
            foreach (var palabra in palabrasCorrectas)
            {
                espanoles.Add(palabra.espanol);
                mapeoIndiceAIdPalabra[index] = palabra.id;
                index++;
            }

            // Agregar distractores en español
            foreach (var distractor in distractores)
            {
                espanoles.Add(distractor.espanol);
                mapeoIndiceAIdPalabra[index] = distractor.id;
                index++;
            }

            // Barajar columna español para no tener patrones
            espanoles = espanoles
                .Select((item, idx) => new { item, idx })
                .OrderBy(_ => UnityEngine.Random.value)
                .Select(x => x.item)
                .ToList();

            // Recrear mapeo con índices barajados
            mapeoIndiceAIdPalabra.Clear();
            index = 0;
            foreach (var item in espanoles)
            {
                var palabra = palabrasCorrectas.FirstOrDefault(p => p.espanol == item) 
                           ?? distractores.FirstOrDefault(d => d.espanol == item);
                if (palabra != null)
                {
                    mapeoIndiceAIdPalabra[index] = palabra.id;
                }
                index++;
            }

            // Columna derecha: Uchumataqu (palabras correctas en uchumataqu)
            foreach (var palabra in palabrasCorrectas)
            {
                uchumataqu.Add(palabra.uchumataqu);
                mapeoIndiceAIdPalabra[index] = palabra.id;
                index++;
            }

            // Agregar distractores en uchumataqu
            foreach (var distractor in distractores)
            {
                uchumataqu.Add(distractor.uchumataqu);
                mapeoIndiceAIdPalabra[index] = distractor.id;
                index++;
            }

            // Barajar columna uchumataqu para no tener patrones
            uchumataqu = uchumataqu
                .Select((item, idx) => new { item, idx })
                .OrderBy(_ => UnityEngine.Random.value)
                .Select(x => x.item)
                .ToList();

            // Recrear mapeo con índices barajados
            index = palabrasCorrectas.Count + distractores.Count;
            var newMapeo = new Dictionary<int, string>(mapeoIndiceAIdPalabra);
            
            foreach (var item in uchumataqu)
            {
                var palabra = palabrasCorrectas.FirstOrDefault(p => p.uchumataqu == item) 
                           ?? distractores.FirstOrDefault(d => d.uchumataqu == item);
                if (palabra != null)
                {
                    newMapeo[index] = palabra.id;
                }
                index++;
            }

            mapeoIndiceAIdPalabra.Clear();
            foreach (var kvp in newMapeo)
                mapeoIndiceAIdPalabra[kvp.Key] = kvp.Value;
        }

        // =========================
        // Lógica de emparejamiento
        // =========================
        private void OnOpcionSeleccionada(int indice)
        {
            if (indicesParejados.Contains(indice))
                return;

            if (indiceSeleccionado1 == null)
            {
                indiceSeleccionado1 = indice;
                vista.MarcarSeleccion(indice);
                return;
            }

            if (indiceSeleccionado1 == indice)
                return;

            indiceSeleccionado2 = indice;
            vista.MarcarSeleccion(indice);

            ValidarPar(indiceSeleccionado1.Value, indice);
        }

        private void ValidarPar(int indice1, int indice2)
        {
            string id1 = mapeoIndiceAIdPalabra[indice1];
            string id2 = mapeoIndiceAIdPalabra[indice2];

            bool esCorrecto =
                id1 == id2 &&
                idsPalabrasCorrectas.Contains(id1);

            if (esCorrecto)
                OnParCorrecto(indice1, indice2);
            else
                OnParIncorrecto(indice1, indice2);
        }

        private void OnParCorrecto(int i1, int i2)
        {
            indicesParejados.Add(i1);
            indicesParejados.Add(i2);

            vista.MarcarParCorrecto(i1, i2);

            indiceSeleccionado1 = null;
            indiceSeleccionado2 = null;

            if (indicesParejados.Count ==
                actividadData.palabrasCorrectas.Count * 2)
            {
                Invoke(nameof(FinalizarActividad), tiempoFeedbackExito);
            }
        }

        private void OnParIncorrecto(int i1, int i2)
        {
            vista.MostrarParIncorrecto(i1, i2);
            indiceSeleccionado1 = null;
            indiceSeleccionado2 = null;
        }

        private void FinalizarActividad()
        {
            NotificarFinalizacion(
                new List<PalabraData>(actividadData.palabrasCorrectas)
            );
        }
    }
}
