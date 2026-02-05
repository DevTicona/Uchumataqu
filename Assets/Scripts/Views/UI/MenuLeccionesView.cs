using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Models;
using Assets.Scripts.Services;

namespace Assets.Scripts.Views.UI
{
    public class MenuLeccionesView : MonoBehaviour
    {
        [Header("Mapa - Contenedores")]
        [SerializeField] private RectTransform rioContent;      // Content del ScrollRect
        [SerializeField] private RectTransform islasContainer;  // Contenedor visual (si lo usas para fondo)
        [SerializeField] private RectTransform islasRoot;       // SOLO islas

        [Header("Distribución automática")]
        [SerializeField] private float distanciaVertical = 220f;
        [SerializeField] private float amplitudHorizontal = 240f;
        [SerializeField] private int curvasVisibles = 2;

        [Header("Balsa")]
        [SerializeField] private BalsaProgresoView balsaProgreso;

        // Prefab inyectado (Addressable)
        private GameObject prefabIsla;

        // Sprites de estado cargados desde Addressables
        private Sprite spriteEstadoBloqueada;
        private Sprite spriteEstadoEnProgreso;
        private Sprite spriteEstadoCompletada;

        private readonly Dictionary<string, IslaLeccionView> islasPorId = new();

        private void Awake()
        {
            // Cargar sprites de estado al iniciar
            CargarSpritesEstado();
        }

        private void CargarSpritesEstado()
        {
            MenuLeccionesService.CargarSpritesEstado(
                (bloqueada, enProgreso, completada) =>
                {
                    spriteEstadoBloqueada = bloqueada;
                    spriteEstadoEnProgreso = enProgreso;
                    spriteEstadoCompletada = completada;
                },
                () =>
                {
                    Debug.LogWarning("[MenuLeccionesView] No se pudieron cargar sprites de estado.");
                }
            );
        }

        // -------------------------------------------------
        // INYECCIÓN DEPENDENCIAS
        // -------------------------------------------------

        /// <summary>
        /// Inyecta el prefab de isla (cargado por Addressables).
        /// </summary>
        public void SetPrefabIsla(GameObject prefab)
        {
            prefabIsla = prefab;
        }

        // -------------------------------------------------
        // ISLAS
        // -------------------------------------------------

        public void LimpiarIslas()
        {
            if (islasRoot == null)
                return;

            // SOLO limpiar IslasRoot
            for (int i = islasRoot.childCount - 1; i >= 0; i--)
                Destroy(islasRoot.GetChild(i).gameObject);

            islasPorId.Clear();
        }

        /// <summary>
        /// Calcula y ajusta la altura REAL del scroll (RioContent)
        /// </summary>
        public void PrepararLayout(int totalIslas)
        {
            if (totalIslas <= 0 || rioContent == null || prefabIsla == null)
                return;

            RectTransform viewport = rioContent.parent as RectTransform;
            if (viewport == null)
                return;

            float alturaSeparaciones = (totalIslas - 1) * distanciaVertical;

            float alturaIsla = 0f;
            if (prefabIsla.TryGetComponent(out RectTransform islaRect))
                alturaIsla = islaRect.rect.height;

            float margen = alturaIsla * 0.8f;
            float alturaTotal = alturaSeparaciones + alturaIsla + margen;

            alturaTotal = Mathf.Max(alturaTotal, viewport.rect.height);

            rioContent.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                alturaTotal
            );
        }

        public void CrearIsla(
            LeccionData leccion,
            EstadoLeccion estado,
            int indice,
            int totalIslas,
            System.Action<LeccionData> onClick
        )
        {
            if (prefabIsla == null || islasRoot == null)
            {
                Debug.LogError("[MenuLeccionesView] PrefabIsla no inyectado.");
                return;
            }

            var islaGO = Instantiate(prefabIsla, islasRoot);

            if (!islaGO.TryGetComponent(out IslaLeccionView isla))
            {
                Destroy(islaGO);
                return;
            }

            RectTransform rect = islaGO.GetComponent<RectTransform>();
            rect.anchoredPosition = CalcularPosicionIsla(indice, totalIslas);

            isla.Configurar(
                leccion,
                null,
                estado,
                spriteEstadoBloqueada,
                spriteEstadoEnProgreso,
                spriteEstadoCompletada,
                onClick
            );
            islasPorId[leccion.id] = isla;

            if (!string.IsNullOrEmpty(leccion.id))
                CargarIcono(leccion.id, isla);
        }

        public IslaLeccionView ObtenerIsla(string id) =>
            islasPorId.TryGetValue(id, out var isla) ? isla : null;

        // -------------------------------------------------
        // POSICIONAMIENTO ISLAS
        // -------------------------------------------------

        private Vector2 CalcularPosicionIsla(int index, int total)
        {
            float y = index * distanciaVertical;

            float t = (float)index / Mathf.Max(1, total - 1);
            float angulo = t * Mathf.PI * 2f * curvasVisibles;
            float x = Mathf.Sin(angulo) * amplitudHorizontal;

            return new Vector2(x, y);
        }

        // -------------------------------------------------
        // BALSA
        // -------------------------------------------------

        public void PosicionarBalsaEnIsla(IslaLeccionView isla)
        {
            if (isla == null || balsaProgreso == null)
                return;

            RectTransform islaRect = isla.GetComponent<RectTransform>();
            balsaProgreso.PosicionarEnIsla(islaRect);
        }

        // -------------------------------------------------
        // ICONOS (Addressables)
        // -------------------------------------------------

        private void CargarIcono(string key, IslaLeccionView isla)
        {
            LeccionesService.CargarIconoLeccion(
                key,
                sprite => isla.ActualizarIcono(sprite),
                onError: () =>
                {
                    Debug.LogWarning($"[MenuLeccionesView] No se pudo cargar ícono: {key}");
                }
            );
        }

        private void OnDestroy()
        {
            MenuLeccionesService.LimpiarRecursos();
            LeccionesService.LimpiarRecursosIconos();
        }
    }
}
