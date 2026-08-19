using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts.Features.Lecciones.Models;

namespace Assets.Scripts.Features.Lecciones.Views
{
    public class MenuLeccionesView : MonoBehaviour
    {
        [Header("Mapa - Contenedores")]
        [SerializeField] private RectTransform rioContent;
        [SerializeField] private RectTransform islasRoot;

        [Header("Distribución automática")]
        [SerializeField] private float distanciaVertical  = 220f;
        [SerializeField] private float amplitudHorizontal = 240f;
        [SerializeField] private int   curvasVisibles     = 2;

        private GameObject prefabIsla;
        private readonly Dictionary<string, IslaLeccionView> islasPorId = new();

        public void SetPrefabIsla(GameObject prefab) => prefabIsla = prefab;

        public void LimpiarIslas()
        {
            if (islasRoot == null) return;
            for (int i = islasRoot.childCount - 1; i >= 0; i--)
                Destroy(islasRoot.GetChild(i).gameObject);
            islasPorId.Clear();
        }

        public void PrepararLayout(int totalIslas)
        {
            if (totalIslas <= 0 || rioContent == null || prefabIsla == null) return;

            var viewport = rioContent.parent as RectTransform;
            if (viewport == null) return;

            float alturaIsla = prefabIsla.TryGetComponent(out RectTransform rt) ? rt.rect.height : 0f;
            float margen     = alturaIsla * 0.8f;
            float alturaTotal = Mathf.Max(
                (totalIslas - 1) * distanciaVertical + alturaIsla + margen,
                viewport.rect.height
            );

            rioContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, alturaTotal);
        }

        public void CrearIsla(
            IslaMapaData isla,
            Sprite spriteBloqueado,
            Sprite spriteEnProgreso,
            Sprite spriteCompletado,
            int indice,
            int totalIslas,
            System.Action<IslaMapaData> onClick
        )
        {
            if (prefabIsla == null || islasRoot == null)
            {
                Debug.LogError("[MenuLeccionesView] PrefabIsla no inyectado.");
                return;
            }

            var islaGO = Instantiate(prefabIsla, islasRoot);
            if (!islaGO.TryGetComponent(out IslaLeccionView islaView))
            {
                Destroy(islaGO);
                return;
            }

            islaGO.GetComponent<RectTransform>().anchoredPosition =
                CalcularPosicionIsla(indice, totalIslas);

            islaView.Configurar(isla, isla.Estado, spriteBloqueado, spriteEnProgreso, spriteCompletado, onClick);
            islasPorId[isla.Id] = islaView;
        }

        public IslaLeccionView ObtenerIsla(string id) =>
            islasPorId.TryGetValue(id, out var v) ? v : null;

        private Vector2 CalcularPosicionIsla(int index, int total)
        {
            float y      = index * distanciaVertical;
            float t      = (float)index / Mathf.Max(1, total - 1);
            float angulo = t * Mathf.PI * 2f * curvasVisibles;
            float x      = Mathf.Sin(angulo) * amplitudHorizontal;
            return new Vector2(x, y);
        }
    }
}