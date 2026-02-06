using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Models;
using Assets.Scripts.Services;
using Assets.Scripts.Views.UI;
using Assets.Scripts.Core;

namespace Assets.Scripts.Presenters
{
    public class MenuLeccionesPresenter : MonoBehaviour
    {
        [Header("Views")]
        [SerializeField] private MenuLeccionesView menuView;
        [SerializeField] private HeaderView headerView;
        [SerializeField] private SeleccionModuloView seleccionModuloView;

        [Header("Sistema")]
        [SerializeField] private NavigationBackHandler backHandler;

        private List<Sprite> spritesIslas = new();
        private GameObject prefabIsla;

        private List<EstadoLeccionMapa> estadosMapa;
        private IslaLeccionView islaActual;

        // Leccion temporal para selección de módulo
        private LeccionData leccionSeleccionada;

        private void Start()
        {
            if (menuView == null || headerView == null || seleccionModuloView == null)
            {
                Debug.LogError("[MenuLeccionesPresenter] Views no asignadas.");
                enabled = false;
                return;
            }

            ConfigurarHeader();
            ConfigurarBackHandler();
            SuscribirseEventosSeleccion();

            // 🔑 Flujo correcto:
            // 1. Cargar prefab
            // 2. Cargar sprites
            // 3. Cargar lecciones
            CargarPrefabIsla();
        }

        private void OnDestroy()
        {
            // 🧹 Liberar Addressables
            MenuLeccionesService.LimpiarRecursos();
            DesuscribirseEventosSeleccion();
        }

        // -------------------------------------------------
        // CONFIGURACIÓN
        // -------------------------------------------------

        private void ConfigurarHeader()
        {
            headerView.BindBotonAtras(VolverAlMenuPrincipal);
            headerView.SetTitulo("Lecciones");
            headerView.SetSubtitulo("Elije qué aprender hoy");
            headerView.OcultarIcono();
        }

        private void ConfigurarBackHandler()
        {
            if (backHandler != null)
                backHandler.Configurar(VolverAlMenuPrincipal);
        }

        private void SuscribirseEventosSeleccion()
        {
            if (seleccionModuloView == null)
                return;

            seleccionModuloView.OnSeleccionarRA += OnSeleccionarRA;
            seleccionModuloView.OnSeleccionarDos2D += OnSeleccionarDos2D;
            seleccionModuloView.OnCancelar += OnCancelarSeleccion;
        }

        private void DesuscribirseEventosSeleccion()
        {
            if (seleccionModuloView == null)
                return;

            seleccionModuloView.OnSeleccionarRA -= OnSeleccionarRA;
            seleccionModuloView.OnSeleccionarDos2D -= OnSeleccionarDos2D;
            seleccionModuloView.OnCancelar -= OnCancelarSeleccion;
        }

        private void VolverAlMenuPrincipal()
        {
            SceneManager.LoadScene("MenuPrincipal");
        }

        // -------------------------------------------------
        // ADDRESSABLES
        // -------------------------------------------------

        private void CargarPrefabIsla()
        {
            MenuLeccionesService.CargarPrefabIsla(
                prefab =>
                {
                    prefabIsla = prefab;
                    menuView.SetPrefabIsla(prefabIsla);
                    CargarSpritesIslas();
                },
                onError: () =>
                {
                    Debug.LogError("[MenuLeccionesPresenter] No se pudo cargar el prefab IslaLeccion.");
                }
            );
        }

        private void CargarSpritesIslas()
        {
            MenuLeccionesService.CargarSpritesIslas(
                sprites =>
                {
                    spritesIslas = sprites;

                    if (spritesIslas.Count == 0)
                    {
                        Debug.LogError("[MenuLeccionesPresenter] No se encontraron sprites con label img_islas.");
                        return;
                    }

                    LeccionesService.ObtenerLecciones(InicializarMapa);
                },
                onError: () =>
                {
                    Debug.LogError("[MenuLeccionesPresenter] Error al cargar sprites de islas.");
                }
            );
        }

        // -------------------------------------------------
        // MAPA
        // -------------------------------------------------

        private void InicializarMapa(List<LeccionData> lecciones)
        {
            if (lecciones == null || lecciones.Count == 0)
            {
                Debug.LogError("[MenuLeccionesPresenter] No hay lecciones.");
                return;
            }

            menuView.LimpiarIslas();

            var ordenadas = lecciones
                .Where(l => l != null)
                .OrderBy(l => l.orden)
                .ToList();

            estadosMapa = ConstruirEstadosMapa(ordenadas);

            int totalIslas = estadosMapa.Count;

            // 1️⃣ Preparar layout
            menuView.PrepararLayout(totalIslas);

            // 2️⃣ Crear islas
            for (int i = 0; i < totalIslas; i++)
            {
                var item = estadosMapa[i];

                menuView.CrearIsla(
                    item.Leccion,
                    item.Estado,
                    i,
                    totalIslas,
                    OnLeccionSeleccionada
                );

                var isla = menuView.ObtenerIsla(item.Leccion.id);
                if (isla == null)
                    continue;

                // 🔁 Alternar sprite de isla
                Sprite spriteIsla = spritesIslas[i % spritesIslas.Count];
                isla.SetIslaSprite(spriteIsla);
            }

            // 3️⃣ Forzar layout
            Canvas.ForceUpdateCanvases();

            // 4️⃣ Posicionar balsa
            PosicionarBalsaInicial();
        }

        private List<EstadoLeccionMapa> ConstruirEstadosMapa(List<LeccionData> lecciones)
        {
            var resultado = new List<EstadoLeccionMapa>(lecciones.Count);

            for (int i = 0; i < lecciones.Count; i++)
            {
                var leccion = lecciones[i];
                EstadoLeccion estado;

                if (ProgresoService.EstaCompletada(leccion.id))
                    estado = EstadoLeccion.Completada;
                else if (i == 0 || ProgresoService.EstaCompletada(lecciones[i - 1].id))
                    estado = EstadoLeccion.EnProgreso;
                else
                    estado = EstadoLeccion.Bloqueada;

                resultado.Add(new EstadoLeccionMapa(leccion, estado, i));
            }

            return resultado;
        }

        private void PosicionarBalsaInicial()
        {
            if (estadosMapa == null || estadosMapa.Count == 0)
                return;

            var objetivo =
                estadosMapa.LastOrDefault(e => e.Estado == EstadoLeccion.Completada) ??
                estadosMapa.FirstOrDefault(e => e.Estado == EstadoLeccion.EnProgreso);

            if (objetivo == null)
                return;

            var isla = menuView.ObtenerIsla(objetivo.Leccion.id);
            if (isla == null)
                return;

            menuView.PosicionarBalsaEnIsla(isla);
            islaActual = isla;
        }

        // -------------------------------------------------
        // INTERACCIÓN
        // -------------------------------------------------

        private void OnLeccionSeleccionada(LeccionData leccion)
        {
            if (leccion == null)
                return;

            leccionSeleccionada = leccion;
            seleccionModuloView.MostrarDialogo(leccion.titulo);
        }

        private void OnSeleccionarRA()
        {
            if (leccionSeleccionada == null)
                return;

            PlayerPrefs.SetString("LECCION_ACTUAL", leccionSeleccionada.id);
            PlayerPrefs.Save();

            SceneManager.LoadScene("RealidadAumentada");
        }

        private void OnSeleccionarDos2D()
        {
            if (leccionSeleccionada == null)
                return;

            PlayerPrefs.SetString("LECCION_ACTUAL", leccionSeleccionada.id);
            PlayerPrefs.Save();

            SceneManager.LoadScene("LeccionBase");
        }

        private void OnCancelarSeleccion()
        {
            leccionSeleccionada = null;
            // Opcionalmente mostrar algún feedback
        }

        public void RefrescarBalsa()
        {
            PosicionarBalsaInicial();
        }

        // -------------------------------------------------
        // MODELO INTERNO
        // -------------------------------------------------

        private sealed class EstadoLeccionMapa
        {
            public LeccionData Leccion { get; }
            public EstadoLeccion Estado { get; }
            public int Indice { get; }

            public EstadoLeccionMapa(
                LeccionData leccion,
                EstadoLeccion estado,
                int indice
            )
            {
                Leccion = leccion;
                Estado = estado;
                Indice = indice;
            }
        }
    }
}
