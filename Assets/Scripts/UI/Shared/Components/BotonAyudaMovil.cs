using Assets.Scripts.Persistencia;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Core.UI.Ayuda
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class BotonAyudaMovil : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private RectTransform boton;
        private RectTransform area;
        private Vector2 desplazamiento;
        private Vector2 ultimoTamano;
        private bool arrastrando;
        private string Pantalla => gameObject.scene.name;

        private void Awake()
        {
            boton = (RectTransform)transform;
            area = transform.parent as RectTransform;
        }

        private void OnEnable()
        {
            if (area == null) return;
            ultimoTamano = area.rect.size;
            Restaurar();
        }

        private void LateUpdate()
        {
            if (area == null || area.rect.size == ultimoTamano || arrastrando) return;
            ultimoTamano = area.rect.size;
            Restaurar();
        }

        private void Restaurar()
        {
            // Sin preferencia del usuario se conserva la posición diseñada en la escena.
            if (!AyudaRepository.ObtenerPosicion(Pantalla, out var posicion)) return;
            var limites = Limites();
            Colocar(new Vector2(Mathf.Lerp(limites.xMin, limites.xMax, posicion.x),
                Mathf.Lerp(limites.yMin, limites.yMax, posicion.y)));
        }

        public void OnBeginDrag(PointerEventData evento)
        {
            if (evento.button != PointerEventData.InputButton.Left || area == null ||
                !GetComponent<Button>().IsInteractable()) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(area, evento.position,
                evento.pressEventCamera, out var punto)) return;
            desplazamiento = (Vector2)area.InverseTransformPoint(boton.position) - punto;
            arrastrando = true;
            evento.eligibleForClick = false;
        }

        public void OnDrag(PointerEventData evento)
        {
            if (!arrastrando) return;
            evento.eligibleForClick = false;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(area, evento.position,
                evento.pressEventCamera, out var punto)) Colocar(punto + desplazamiento);
        }

        public void OnEndDrag(PointerEventData evento)
        {
            if (!arrastrando) return;
            arrastrando = false;
            evento.eligibleForClick = false;
            var limites = Limites();
            Vector2 punto = area.InverseTransformPoint(boton.position);
            AyudaRepository.GuardarPosicion(Pantalla, new Vector2(
                Mathf.InverseLerp(limites.xMin, limites.xMax, punto.x),
                Mathf.InverseLerp(limites.yMin, limites.yMax, punto.y)));
        }

        private void OnDisable() => arrastrando = false;

        private Rect Limites()
        {
            Vector2 tamano = Vector2.Scale(boton.rect.size, boton.localScale);
            Vector2 minimo = area.rect.min + Vector2.Scale(tamano, boton.pivot);
            Vector2 maximo = area.rect.max - Vector2.Scale(tamano, Vector2.one - boton.pivot);
            if (maximo.x < minimo.x) minimo.x = maximo.x = area.rect.center.x;
            if (maximo.y < minimo.y) minimo.y = maximo.y = area.rect.center.y;
            return Rect.MinMaxRect(minimo.x, minimo.y, maximo.x, maximo.y);
        }

        private void Colocar(Vector2 punto)
        {
            var limites = Limites();
            punto.x = Mathf.Clamp(punto.x, limites.xMin, limites.xMax);
            punto.y = Mathf.Clamp(punto.y, limites.yMin, limites.yMax);
            boton.position = area.TransformPoint(new Vector3(punto.x, punto.y, boton.localPosition.z));
        }
    }
}
