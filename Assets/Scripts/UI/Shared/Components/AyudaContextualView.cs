using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Core.UI.Components;
using Assets.Scripts.Persistencia;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Core.UI.Ayuda
{
    [DisallowMultipleComponent]
    public sealed class AyudaContextualView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private RectTransform tarjeta;
        [SerializeField] private RectTransform marco;
        [SerializeField] private RectTransform[] sombras;
        [SerializeField] private TMP_Text titulo;
        [SerializeField] private TMP_Text cuerpo;
        [SerializeField] private RectTransform areaTexto;
        [SerializeField] private TMP_Text contador;
        [SerializeField] private TMP_Text textoSiguiente;
        [SerializeField] private Button siguiente;
        [SerializeField] private Button anterior;
        [SerializeField] private Button omitir;
        [SerializeField] private Button abrirAyuda;
        [SerializeField] private Button repetirGuia;
        [SerializeField] private Button verConceptos;
        [SerializeField] private GameObject opciones;
        [SerializeField] private Image iconoGuia;
        [SerializeField] private Sprite iconoInformacion;
        [SerializeField] private Sprite iconoConcepto;
        [SerializeField] private Image iconoSiguiente;
        [SerializeField] private Sprite iconoAvanzar;
        [SerializeField] private Sprite iconoListo;

        private readonly List<PasoGuia> pasos = new List<PasoGuia>();
        private readonly Vector3[] esquinas = new Vector3[4];
        private GuiaPantalla pantalla;
        private int indice;
        private bool esConcepto;
        private bool esMenu;
        private bool permitido = true;
        private bool conceptosAutomaticosPendientes;
        private Vector2 ultimoTamano;
        private FlechaGuia flecha;
        private Rect? ultimoObjetivo;
        private Vector2 ultimaArea;
        private bool actualizarUbicacion = true;
        public bool Visible => panel != null && panel.activeSelf;

        private void Awake()
        {
            flecha = panel.GetComponentInChildren<FlechaGuia>(true);
            siguiente.onClick.AddListener(Avanzar);
            anterior.onClick.AddListener(Retroceder);
            omitir.onClick.AddListener(Omitir);
            abrirAyuda.onClick.AddListener(AlternarMenu);
            abrirAyuda.transform.SetAsLastSibling();
            repetirGuia.onClick.AddListener(RepetirGuia);
            verConceptos.onClick.AddListener(VerConceptos);
            panel.SetActive(false);
            abrirAyuda.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            siguiente.onClick.RemoveListener(Avanzar);
            anterior.onClick.RemoveListener(Retroceder);
            omitir.onClick.RemoveListener(Omitir);
            abrirAyuda.onClick.RemoveListener(AlternarMenu);
            repetirGuia.onClick.RemoveListener(RepetirGuia);
            verConceptos.onClick.RemoveListener(VerConceptos);
        }

        public void Preparar(GuiaPantalla origen)
        {
            if (origen == null) return;
            if (pantalla != origen) Cerrar();
            pantalla = origen;
            abrirAyuda.gameObject.SetActive(true);
            abrirAyuda.interactable = permitido;
            if (!permitido) return;
            if (origen.EsPresentacionFamilia)
            {
                if (!origen.RegistrarPrimeraPresentacion())
                {
                    conceptosAutomaticosPendientes = false;
                    return;
                }
                conceptosAutomaticosPendientes = true;
                MostrarGuia();
                return;
            }
            conceptosAutomaticosPendientes = true;
            if (!origen.GuiaVista) MostrarGuia();
            else MostrarConceptos(true);
        }

        public void Retirar(GuiaPantalla origen)
        {
            if (pantalla != origen) return;
            Cerrar();
            pantalla = null;
            conceptosAutomaticosPendientes = false;
            abrirAyuda.gameObject.SetActive(false);
        }

        public void PermitirAyuda(bool valor)
        {
            permitido = valor;
            abrirAyuda.interactable = valor;
            if (!valor)
            {
                conceptosAutomaticosPendientes = false;
                Cerrar();
            }
        }

        public bool CerrarSiAbierta()
        {
            if (!Visible) return false;
            Omitir();
            return true;
        }

        private void MostrarMenu()
        {
            if (pantalla == null || !permitido) return;
            esMenu = true;
            esConcepto = false;
            pasos.Clear();
            pasos.Add(new PasoGuia("Ayuda", "Repasa esta pantalla y elige la ayuda que necesitas."));
            indice = 0;
            MostrarPaso();
        }

        private void AlternarMenu()
        {
            if (Visible) Omitir();
            else MostrarMenu();
        }

        private void RepetirGuia() => MostrarGuia();
        private void VerConceptos() => MostrarConceptos(false);

        private void MostrarGuia()
        {
            if (pantalla == null || !permitido) return;
            esMenu = false;
            esConcepto = false;
            pasos.Clear();
            pasos.AddRange(pantalla.Pasos.Where(p => p != null &&
                (p.objetivo == null || p.objetivo.gameObject.activeInHierarchy)));
            indice = 0;
            MostrarPaso();
        }

        private void MostrarConceptos(bool soloNuevos)
        {
            if (pantalla == null || !permitido) return;
            if (soloNuevos)
            {
                if (!conceptosAutomaticosPendientes) return;
                conceptosAutomaticosPendientes = false;
            }
            esMenu = false;
            esConcepto = true;
            pasos.Clear();
            var disponibles = pantalla.Conceptos.Where(p => !soloNuevos || !AyudaRepository.FueVista(pantalla.IdConcepto(p)));
            pasos.AddRange(soloNuevos ? disponibles.Take(1) : disponibles);
            indice = 0;
            MostrarPaso();
        }

        private void MostrarPaso()
        {
            if (pasos.Count == 0) { Cerrar(); return; }
            tarjeta.GetComponent<AyudaCardLayout>()?.OrganizarMenu(false);
            EventSystem.current?.SetSelectedGameObject(null);
            panel.SetActive(true);
            abrirAyuda.gameObject.SetActive(true);
            var paso = pasos[indice];
            titulo.text = paso.titulo;
            if (iconoGuia != null) iconoGuia.sprite = esConcepto ? iconoConcepto : iconoInformacion;
            cuerpo.text = paso.texto;
            contador.text = esMenu ? "AYUDA" : esConcepto ? "APRENDE · " + (indice + 1) + " / " + pasos.Count :
                "GUÍA · " + (indice + 1) + " / " + pasos.Count;
            textoSiguiente.text = esMenu ? "CERRAR" : indice == pasos.Count - 1 ? "ENTENDIDO" : "SIGUIENTE";
            anterior.gameObject.SetActive(!esMenu && indice > 0);
            omitir.gameObject.SetActive(!esMenu);
            opciones.SetActive(esMenu);
            verConceptos.gameObject.SetActive(esMenu && pantalla.Conceptos.Count > 0);
            if (iconoSiguiente != null) iconoSiguiente.sprite = esMenu || indice == pasos.Count - 1 ? iconoListo : iconoAvanzar;
            EnfocarObjetivo(paso.objetivo);
            AjustarTarjeta();
            tarjeta.GetComponent<MovimientoUI>()?.ReproducirEntrada();
        }

        private void AjustarTarjeta()
        {
            actualizarUbicacion = true;
            tarjeta.GetComponent<AyudaCardLayout>()?.Reconstruir();
            tarjeta.GetComponent<AyudaCardLayout>()?.OrganizarMenu(esMenu);
            if (!esMenu) tarjeta.GetComponent<AyudaCardLayout>()?.OrganizarGuia();
            var scroll = areaTexto.GetComponent<ScrollRect>();
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = 1f;
            ultimoTamano = ((RectTransform)panel.transform).rect.size;
        }

        private void Avanzar()
        {
            if (!Visible || pantalla == null || pasos.Count == 0) return;
            if (esMenu) { Cerrar(); return; }
            if (esConcepto) AyudaRepository.MarcarVista(pantalla.IdConcepto(pasos[indice]));
            if (++indice < pasos.Count) { MostrarPaso(); return; }
            bool continuar = !esConcepto;
            if (continuar) AyudaRepository.MarcarVista(pantalla.Identificador);
            Cerrar();
            if (continuar) MostrarConceptos(true);
        }

        private void Retroceder()
        {
            if (indice <= 0) return;
            indice--;
            MostrarPaso();
        }

        private void Omitir()
        {
            conceptosAutomaticosPendientes = false;
            if (pantalla != null && !esMenu && pasos.Count > 0)
            {
                if (esConcepto)
                {
                    foreach (var paso in pasos) AyudaRepository.MarcarVista(pantalla.IdConcepto(paso));
                }
                else AyudaRepository.MarcarVista(pantalla.Identificador);
            }
            Cerrar();
        }

        private void Cerrar()
        {
            if (panel != null) panel.SetActive(false);
            if (tarjeta != null) tarjeta.GetComponent<AyudaCardLayout>()?.OrganizarMenu(false);
            if (abrirAyuda != null) abrirAyuda.gameObject.SetActive(pantalla != null);
        }

        private static void EnfocarObjetivo(RectTransform objetivo)
        {
            if (objetivo == null) return;
            var scroll = objetivo.GetComponentInParent<ScrollRect>();
            if (scroll == null || scroll.content == null || !scroll.vertical) return;
            Canvas.ForceUpdateCanvases();
            var viewport = scroll.viewport != null ? scroll.viewport : (RectTransform)scroll.transform;
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, objetivo);
            float recorrido = scroll.content.rect.height - viewport.rect.height;
            if (recorrido <= 0f) return;
            float desplazamiento = bounds.center.y - viewport.rect.center.y;
            scroll.StopMovement();
            scroll.verticalNormalizedPosition = Mathf.Clamp01(scroll.verticalNormalizedPosition + desplazamiento / recorrido);
            Canvas.ForceUpdateCanvases();
        }

        private void LateUpdate()
        {
            if (!Visible || pasos.Count == 0) return;
            var area = (RectTransform)panel.transform;
            if (area.rect.size != ultimoTamano) AjustarTarjeta();
            var objetivo = pasos[indice].objetivo;
            if (objetivo == null || !objetivo.gameObject.activeInHierarchy)
            {
                marco.gameObject.SetActive(false);
                Colocar(sombras[0], area.rect);
                for (int i = 1; i < sombras.Length; i++) sombras[i].gameObject.SetActive(false);
                PosicionarTarjeta(null);
                return;
            }
            objetivo.GetWorldCorners(esquinas);
            Vector2 minimo = area.InverseTransformPoint(esquinas[0]);
            Vector2 maximo = area.InverseTransformPoint(esquinas[2]);
            Rect hueco = Rect.MinMaxRect(Mathf.Max(area.rect.xMin, minimo.x - 8f),
                Mathf.Max(area.rect.yMin, minimo.y - 8f), Mathf.Min(area.rect.xMax, maximo.x + 8f),
                Mathf.Min(area.rect.yMax, maximo.y + 8f));
            marco.gameObject.SetActive(true);
            Colocar(marco, hueco);
            Colocar(sombras[0], Rect.MinMaxRect(area.rect.xMin, hueco.yMax, area.rect.xMax, area.rect.yMax));
            Colocar(sombras[1], Rect.MinMaxRect(area.rect.xMin, area.rect.yMin, area.rect.xMax, hueco.yMin));
            Colocar(sombras[2], Rect.MinMaxRect(area.rect.xMin, hueco.yMin, hueco.xMin, hueco.yMax));
            Colocar(sombras[3], Rect.MinMaxRect(hueco.xMax, hueco.yMin, area.rect.xMax, hueco.yMax));
            PosicionarTarjeta(hueco);
        }

        private void PosicionarTarjeta(Rect? objetivo)
        {
            var layout = tarjeta.GetComponent<AyudaCardLayout>();
            var area = (RectTransform)panel.transform;
            if (!actualizarUbicacion && ultimoObjetivo.Equals(objetivo) && ultimaArea == area.rect.size) return;
            actualizarUbicacion = false;
            ultimoObjetivo = objetivo;
            ultimaArea = area.rect.size;
            Rect disponible = area.rect;
            disponible.xMin += 24f;
            disponible.xMax -= 24f;
            disponible.yMin += 24f;
            disponible.yMax -= 24f;
            float ancho = Mathf.Min(720f, disponible.width);
            tarjeta.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, ancho);
            layout?.Reconstruir();
            Rect region = ElegirRegion(disponible, objetivo, tarjeta.rect.size);
            if (region.width < ancho)
                tarjeta.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(1f, region.width));
            layout?.Ajustar(region.height);
            tarjeta.anchorMin = tarjeta.anchorMax = tarjeta.pivot = new Vector2(.5f, .5f);
            Vector2 centro = objetivo.HasValue ? region.center : new Vector2(region.center.x, region.yMin + tarjeta.rect.height * .5f);
            if (objetivo.HasValue)
            {
                Rect foco = objetivo.Value;
                if (region.yMin >= foco.yMax) centro.y = region.yMin + tarjeta.rect.height * .5f;
                else if (region.yMax <= foco.yMin) centro.y = region.yMax - tarjeta.rect.height * .5f;
                else if (region.xMax <= foco.xMin) centro.x = region.xMax - tarjeta.rect.width * .5f;
                else centro.x = region.xMin + tarjeta.rect.width * .5f;
            }
            centro.y = Mathf.Clamp(centro.y, region.yMin + tarjeta.rect.height * .5f, region.yMax - tarjeta.rect.height * .5f);
            tarjeta.anchoredPosition = centro - area.rect.center;
            if (flecha == null) return;
            flecha.gameObject.SetActive(objetivo.HasValue);
            if (!objetivo.HasValue) return;
            Rect card = new Rect(centro - tarjeta.rect.size * .5f, tarjeta.rect.size);
            Vector2 fin = PuntoCercano(objetivo.Value, centro);
            Vector2 desde = PuntoCercano(card, fin);
            flecha.Apuntar(desde, fin);
        }

        private static Vector2 PuntoCercano(Rect rect, Vector2 punto) =>
            new Vector2(Mathf.Clamp(punto.x, rect.xMin, rect.xMax), Mathf.Clamp(punto.y, rect.yMin, rect.yMax));

        public static Rect ElegirRegion(Rect area, Rect? objetivo, Vector2 tarjetaTamano)
        {
            if (!objetivo.HasValue) return area;
            Rect foco = objetivo.Value;
            var regiones = new[]
            {
                Rect.MinMaxRect(area.xMin, Mathf.Min(area.yMax, foco.yMax + 48f), area.xMax, area.yMax),
                Rect.MinMaxRect(area.xMin, area.yMin, area.xMax, Mathf.Max(area.yMin, foco.yMin - 48f)),
                Rect.MinMaxRect(area.xMin, area.yMin, Mathf.Max(area.xMin, foco.xMin - 48f), area.yMax),
                Rect.MinMaxRect(Mathf.Min(area.xMax, foco.xMax + 48f), area.yMin, area.xMax, area.yMax)
            };
            foreach (var region in regiones)
                if (region.width >= tarjetaTamano.x && region.height >= tarjetaTamano.y) return region;
            return regiones.OrderByDescending(r => r.width >= 360f ? r.width * r.height : 0f).First();
        }

        private static void Colocar(RectTransform rect, Rect limites)
        {
            rect.gameObject.SetActive(limites.width > 0f && limites.height > 0f);
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
            var area = (RectTransform)rect.parent;
            rect.anchoredPosition = limites.min - area.rect.min;
            rect.sizeDelta = limites.size;
        }
    }
}
