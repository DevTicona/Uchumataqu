using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.UI.Main;
using TMPro;
using System.Collections;

namespace Assets.Scripts.UI.Main.Paginas
{
    public class MapaLeccionesView : MonoBehaviour
    {
        [Header("Contenedor Scroll")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform contenedorIslas;

        [Header("Prefabs")]
        [SerializeField] private GameObject prefabIslaLeccion;
        [SerializeField] private GameObject prefabHitoRA;

        private readonly List<IslaItemView> _islasInstanciadas = new List<IslaItemView>();
        private readonly List<GameObject> _elementosInstanciados = new List<GameObject>();

        public event Action OnSolicitarActualizacion;
        public event Action<string> OnLeccionSeleccionada;
        public event Action<string> OnHitoRaSeleccionado;

        private void OnEnable() => OnSolicitarActualizacion?.Invoke();

        public void Mostrar(IReadOnlyList<IslaMapaPresentacionData> islas)
        {
            LimpiarIslas();
            if (islas == null || contenedorIslas == null) return;

            string regionActual = null;
            int indiceActual = -1;
            for (int i = 0; i < islas.Count; i++)
            {
                IslaMapaPresentacionData isla = islas[i];
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

        private void CrearLeccion(IslaMapaPresentacionData datos)
        {
            GameObject go = CrearInstancia(prefabIslaLeccion, $"Isla_{datos.LeccionId}");
            var item = go.GetComponent<IslaItemView>() ?? go.AddComponent<IslaItemView>();
            item.InicializarRuntime();

            item.ConfigurarLeccion(datos.LeccionId, datos.Numero, datos.Titulo, datos.Estado);
            item.OnLeccionSeleccionada += NotificarLeccion;
            _islasInstanciadas.Add(item);
            _elementosInstanciados.Add(go);
        }

        private void CrearHitoRa(IslaMapaPresentacionData datos)
        {
            GameObject prefab = prefabHitoRA != null ? prefabHitoRA : prefabIslaLeccion;
            GameObject go = CrearInstancia(prefab, $"HitoRA_{datos.EscenarioRaId}");
            var item = go.GetComponent<IslaItemView>() ?? go.AddComponent<IslaItemView>();
            item.InicializarRuntime();

            item.ConfigurarHitoRA(datos.EscenarioRaId, datos.Titulo, datos.Estado);
            item.OnHitoRaSeleccionado += NotificarHitoRa;
            _islasInstanciadas.Add(item);
            _elementosInstanciados.Add(go);
        }

        private GameObject CrearInstancia(GameObject prefab, string nombre)
        {
            if (prefab != null) return Instantiate(prefab, contenedorIslas);
            var go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(contenedorIslas, false);
            return go;
        }

        private void CrearEncabezadoRegion(string titulo)
        {
            var go = new GameObject($"Region_{titulo}", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(contenedorIslas, false);
            var texto = go.GetComponent<TextMeshProUGUI>();
            texto.text = titulo;
            texto.fontSize = 28f;
            texto.fontStyle = FontStyles.Bold;
            texto.color = new Color(0.92f, 0.76f, 0.35f);
            texto.alignment = TextAlignmentOptions.MidlineLeft;
            go.GetComponent<LayoutElement>().preferredHeight = 64f;
            _elementosInstanciados.Add(go);
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
                Destroy(item.gameObject);
            }
            _islasInstanciadas.Clear();
            foreach (GameObject elemento in _elementosInstanciados)
            {
                if (elemento != null && elemento.GetComponent<IslaItemView>() == null)
                    Destroy(elemento);
            }
            _elementosInstanciados.Clear();
        }
    }
}
