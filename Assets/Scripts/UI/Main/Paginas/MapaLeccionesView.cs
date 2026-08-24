using System;
using System.Collections.Generic;
using UnityEngine;
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
        [SerializeField] private GameObject prefabIslaLeccion;
        [SerializeField] private GameObject prefabHitoRA;

        private readonly List<IslaItemView> _islasInstanciadas = new List<IslaItemView>();

        public event Action OnSolicitarActualizacion;
        public event Action<string> OnLeccionSeleccionada;
        public event Action<string> OnHitoRaSeleccionado;

        private void OnEnable() => OnSolicitarActualizacion?.Invoke();

        public void Mostrar(IReadOnlyList<IslaMapaPresentacionData> islas)
        {
            LimpiarIslas();
            if (islas == null || contenedorIslas == null) return;

            foreach (var isla in islas)
            {
                if (isla.EsHitoRa) CrearHitoRa(isla);
                else CrearLeccion(isla);
            }
        }

        private void CrearLeccion(IslaMapaPresentacionData datos)
        {
            GameObject go = CrearInstancia(prefabIslaLeccion, $"Isla_{datos.LeccionId}");
            var item = go.GetComponent<IslaItemView>();
            if (item == null) return;

            item.ConfigurarLeccion(datos.LeccionId, datos.Numero, datos.Titulo, datos.Estado);
            item.OnLeccionSeleccionada += NotificarLeccion;
            _islasInstanciadas.Add(item);
        }

        private void CrearHitoRa(IslaMapaPresentacionData datos)
        {
            GameObject prefab = prefabHitoRA != null ? prefabHitoRA : prefabIslaLeccion;
            GameObject go = CrearInstancia(prefab, $"HitoRA_{datos.EscenarioRaId}");
            var item = go.GetComponent<IslaItemView>();
            if (item == null) return;

            item.ConfigurarHitoRA(datos.EscenarioRaId, datos.Titulo, datos.Estado);
            item.OnHitoRaSeleccionado += NotificarHitoRa;
            _islasInstanciadas.Add(item);
        }

        private GameObject CrearInstancia(GameObject prefab, string nombre)
        {
            if (prefab != null) return Instantiate(prefab, contenedorIslas);
            var go = new GameObject(nombre, typeof(RectTransform), typeof(IslaItemView));
            go.transform.SetParent(contenedorIslas, false);
            return go;
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
        }
    }
}
