using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Assets.Scripts.UI.Main;

namespace Assets.Scripts.UI.Main.Paginas
{
    public class MapaLeccionesView : MonoBehaviour
    {
        [Header("Contenedor Scroll")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contenedorIslas;

        [Header("Prefabs")]
        [FormerlySerializedAs("prefabIslaLeccion")]
        [SerializeField] private GameObject prefabContenedorIsla;

        [Header("Camino Sinuoso en S")]
        [Tooltip("Amplitud máxima horizontal en px de la curva")]
        [SerializeField] private float amplitudCurva = 240f;

        [Tooltip("Cantidad de islas consecutivas por cada semiciclo de la curva (4 para formar una S de 8 islas)")]
        [SerializeField] private int islasPorCurva = 4;

        private readonly List<IslaItemView> _islasInstanciadas = new List<IslaItemView>();
        private readonly List<GameObject> _elementosInstanciados = new List<GameObject>();

        public event Action OnSolicitarActualizacion;
        public event Action<string> OnLeccionSeleccionada;
        public event Action<string> OnHitoRaSeleccionado;

        private void OnEnable() => OnSolicitarActualizacion?.Invoke();

        public void Mostrar(IReadOnlyList<IslaMapaData> islas)
        {
            LimpiarIslas();
            if (islas == null || contenedorIslas == null) return;
            if (!ValidarPrefabs()) return;

            string regionActual = null;
            int indiceActual = -1;
            for (int i = 0; i < islas.Count; i++)
            {
                IslaMapaData isla = islas[i];
                if (!string.Equals(regionActual, isla.Region, StringComparison.Ordinal))
                {
                    regionActual = isla.Region;
                    CrearEncabezadoRegion(regionActual);
                }

                if (isla.EsHitoRa) CrearHitoRa(isla);
                else CrearLeccion(isla);
                if (isla.EsActual) indiceActual = _elementosInstanciados.Count - 1;
            }

            if (indiceActual >= 0)
                StartCoroutine(PosicionarEnElemento(indiceActual));
        }

        private void CrearLeccion(IslaMapaData datos)
        {
            GameObject fila = CrearFilaIsla($"FilaIsla_{datos.LeccionId}");
            GameObject go = CrearInstanciaIsla(fila.transform, $"Isla_{datos.LeccionId}");
            var item = go.GetComponent<IslaItemView>();
            item.InicializarRuntime();

            item.ConfigurarLeccion(datos.LeccionId, datos.Estado, datos.EsActual);
            item.OnLeccionSeleccionada += NotificarLeccion;
            _islasInstanciadas.Add(item);
            _elementosInstanciados.Add(fila);
        }

        private void CrearHitoRa(IslaMapaData datos)
        {
            GameObject fila = CrearFilaIsla($"FilaHitoRA_{datos.EscenarioRaId}");
            GameObject go = CrearInstanciaIsla(fila.transform, $"HitoRA_{datos.EscenarioRaId}");
            var item = go.GetComponent<IslaItemView>();
            item.InicializarRuntime();

            item.ConfigurarHitoRA(datos.EscenarioRaId, datos.Estado, datos.EsActual);
            item.OnHitoRaSeleccionado += NotificarHitoRa;
            _islasInstanciadas.Add(item);
            _elementosInstanciados.Add(fila);
        }

        private GameObject CrearFilaIsla(string nombre)
        {
            var fila = new GameObject(nombre, typeof(RectTransform), typeof(LayoutElement));
            fila.transform.SetParent(contenedorIslas, false);

            var layout = fila.GetComponent<LayoutElement>();
            layout.minHeight = 300f;
            layout.preferredHeight = 300f;
            layout.flexibleHeight = 0f;
            layout.flexibleWidth = 1f;
            return fila;
        }

        private GameObject CrearInstanciaIsla(Transform fila, string nombre)
        {
            GameObject isla = Instantiate(prefabContenedorIsla, fila);
            isla.name = nombre;

            var rect = isla.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(CalcularOffsetCamino(), 0f);
            return isla;
        }

        private bool ValidarPrefabs()
        {
            if (prefabContenedorIsla == null)
            {
                Debug.LogError("[MapaLeccionesView] Falta asignar el prefab ContenedorIsla.", this);
                return false;
            }

            if (prefabContenedorIsla.GetComponent<IslaItemView>() == null ||
                prefabContenedorIsla.GetComponent<LayoutElement>() == null)
            {
                Debug.LogError(
                    "[MapaLeccionesView] ContenedorIsla debe contener IslaItemView y LayoutElement.",
                    this);
                return false;
            }

            if (prefabSeparadorSeccion == null)
            {
                Debug.LogError("[MapaLeccionesView] Falta asignar el prefab SeparadorSeccion.", this);
                return false;
            }

            if (prefabSeparadorSeccion.GetComponent<SeparadorSeccionView>() == null)
            {
                Debug.LogError("[MapaLeccionesView] SeparadorSeccion no contiene SeparadorSeccionView.", this);
                return false;
            }

            return true;
        }

        private float CalcularOffsetCamino()
        {
            int indice = _islasInstanciadas.Count;
            int k = Mathf.Max(1, islasPorCurva);
            float fase = (indice + 0.5f) / (float)k * Mathf.PI;
            return Mathf.Sin(fase) * amplitudCurva;
        }

        [Header("Separadores de Región")]
        [SerializeField] private GameObject prefabSeparadorSeccion;
        [SerializeField] private Color colorSeparadorRegion = new Color(0.40f, 0.22f, 0.06f, 1f);

        private void CrearEncabezadoRegion(string titulo)
        {
            string textoSeccion = ObtenerTextoSeccion(titulo);

            GameObject separador = Instantiate(prefabSeparadorSeccion, contenedorIslas);
            separador.name = $"Region_{titulo}";
            var vistaSeparador = separador.GetComponent<SeparadorSeccionView>();
            vistaSeparador.Configurar(textoSeccion);
            vistaSeparador.ConfigurarColorLineas(colorSeparadorRegion);
            _elementosInstanciados.Add(separador);
        }

        private static string ObtenerTextoSeccion(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                return "¡Aprende y avanza!";

            string lower = titulo.ToLowerInvariant();
            if (lower.Contains("encuentro") || lower.Contains("comunicaci"))
                return "¡Primeros pasos y saludos!";
            if (lower.Contains("familia") || lower.Contains("vestimenta"))
                return "¡Conoce a tu familia y vestimenta!";
            if (lower.Contains("casa") || lower.Contains("objetos"))
                return "¡El hogar y la vida diaria!";
            if (lower.Contains("cocina") || lower.Contains("alimentos"))
                return "¡Sabores de la cocina tradicional!";
            if (lower.Contains("animal") || lower.Contains("ecosistema"))
                return "¡Amigos del reino animal!";
            if (lower.Contains("naturaleza") || lower.Contains("orientaci"))
                return "¡Caminos y secretos de la naturaleza!";

            return $"¡{titulo}!";
        }

        private IEnumerator PosicionarEnElemento(int indice)
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            if (scrollRect == null || _elementosInstanciados.Count <= 1) yield break;
            float posicion = 1f - (float)indice / (_elementosInstanciados.Count - 1);
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(posicion);
        }

        private void NotificarLeccion(string id) => OnLeccionSeleccionada?.Invoke(id);
        private void NotificarHitoRa(string id) => OnHitoRaSeleccionado?.Invoke(id);

        private void LimpiarIslas()
        {
            foreach (var item in _islasInstanciadas)
            {
                if (item == null) continue;
                item.OnLeccionSeleccionada -= NotificarLeccion;
                item.OnHitoRaSeleccionado -= NotificarHitoRa;
            }
            _islasInstanciadas.Clear();

            if (contenedorIslas != null)
            {
                for (int i = contenedorIslas.childCount - 1; i >= 0; i--)
                {
                    Transform child = contenedorIslas.GetChild(i);
                    if (child != null)
                    {
                        child.gameObject.SetActive(false);
                        Destroy(child.gameObject);
                    }
                }
            }

            _elementosInstanciados.Clear();
        }
    }
}
