using UnityEngine;
using System.Collections;
using Assets.Scripts.Models;
using Assets.Scripts.Services;

namespace Assets.Scripts.Views.UI
{
    public class CarouselView : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private Transform root;

        private WordCardView cardActual;

        private const float DURACION = 0.35f;
        private const float OFFSET_X = 320f;
        private const float SCALE_REDUCIDO = 0.92f;

        public void Mostrar(PalabraData palabra, bool haciaDerecha)
        {
            if (root == null)
            {
                Debug.LogError("[CarouselView] Root no asignado.");
                return;
            }

            StartCoroutine(AnimarCambio(palabra, haciaDerecha));
        }

        // ===============================
        // Animación principal
        // ===============================
        private IEnumerator AnimarCambio(PalabraData palabra, bool haciaDerecha)
        {
            if (cardActual != null)
            {
                var rectSalida = cardActual.GetComponent<RectTransform>();
                var cgSalida = ObtenerCanvasGroup(cardActual.gameObject);

                yield return AnimarSalida(rectSalida, cgSalida, haciaDerecha);
                Destroy(cardActual.gameObject);
            }

            // Instanciar WordCard usando Addressables
            bool instanciaCompletada = false;
            GameObject go = null;

            LeccionBaseService.InstanciarWordCard(
                root,
                gameObject =>
                {
                    go = gameObject;
                    instanciaCompletada = true;
                },
                () =>
                {
                    Debug.LogError("[CarouselView] No se pudo instanciar WordCard.");
                    instanciaCompletada = true;
                }
            );

            // Esperar a que se instancie
            yield return new WaitUntil(() => instanciaCompletada);

            if (go == null)
                yield break;

            cardActual = go.GetComponent<WordCardView>();

            if (cardActual == null)
            {
                Debug.LogError("[CarouselView] El prefab instanciado no contiene WordCardView.");
                Destroy(go);
                yield break;
            }

            cardActual.Render(palabra);

            var rect = go.GetComponent<RectTransform>();
            var canvasGroup = ObtenerCanvasGroup(go);

            rect.anchoredPosition = new Vector2(
                haciaDerecha ? OFFSET_X : -OFFSET_X, 0
            );
            rect.localScale = Vector3.one * SCALE_REDUCIDO;
            canvasGroup.alpha = 0f;

            yield return AnimarEntrada(rect, canvasGroup);
        }

        // ===============================
        // Animaciones
        // ===============================
        private IEnumerator AnimarSalida(
            RectTransform rect,
            CanvasGroup canvasGroup,
            bool haciaDerecha
        )
        {
            float startX = rect.anchoredPosition.x;
            float endX = haciaDerecha ? -OFFSET_X : OFFSET_X;

            Vector3 startScale = rect.localScale;
            Vector3 endScale = Vector3.one * SCALE_REDUCIDO;

            float t = 0f;
            while (t < DURACION)
            {
                t += Time.deltaTime;
                float eased = EaseInOut(t / DURACION);

                rect.anchoredPosition =
                    new Vector2(Mathf.Lerp(startX, endX, eased), 0);
                rect.localScale =
                    Vector3.Lerp(startScale, endScale, eased);
                canvasGroup.alpha =
                    Mathf.Lerp(1f, 0f, eased);

                yield return null;
            }
        }

        private IEnumerator AnimarEntrada(
            RectTransform rect,
            CanvasGroup canvasGroup
        )
        {
            float startX = rect.anchoredPosition.x;

            Vector3 startScale = rect.localScale;
            Vector3 endScale = Vector3.one;

            float t = 0f;
            while (t < DURACION)
            {
                t += Time.deltaTime;
                float eased = EaseOut(t / DURACION);

                rect.anchoredPosition =
                    new Vector2(Mathf.Lerp(startX, 0f, eased), 0);
                rect.localScale =
                    Vector3.Lerp(startScale, endScale, eased);
                canvasGroup.alpha =
                    Mathf.Lerp(0f, 1f, eased);

                yield return null;
            }
        }

        // ===============================
        // Utilidades
        // ===============================
        private CanvasGroup ObtenerCanvasGroup(GameObject go)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                cg = go.AddComponent<CanvasGroup>();
            }
            return cg;
        }

        // ===============================
        // Easing
        // ===============================
        private float EaseOut(float t)
        {
            return 1f - Mathf.Pow(1f - t, 3f);
        }

        private float EaseInOut(float t)
        {
            return t < 0.5f
                ? 4f * t * t * t
                : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        }
    }
}
