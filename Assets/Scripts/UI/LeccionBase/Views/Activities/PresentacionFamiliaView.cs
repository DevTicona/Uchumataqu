using System;
using System.Collections.Generic;
using Assets.Scripts.Core.UI;
using Assets.Scripts.Data.Corpus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    public sealed class PresentacionFamiliaView : MonoBehaviour
    {
        [SerializeField] private TMP_Text txtBadge;
        [SerializeField] private TMP_Text txtRaizUch;
        [SerializeField] private TMP_Text txtSignificadoEs;
        [SerializeField] private Button btnAudio;
        [SerializeField] private Button btnPracticar;
        [SerializeField] private GameObject prefabFilaForma;
        [SerializeField] private GameObject contenedorFormas;
        [SerializeField] private Transform items;
        [SerializeField] private TMP_Text txtTituloFormas;
        [SerializeField] private Sprite iconoPracticar;
        [SerializeField] private Sprite iconoGrabando;
        public event Action OnAudioClicked;
        public event Action OnPronunciacionClicked;
        public event Action<string> OnFormaAudioClicked;
        public event Action<string> OnFormaPronunciacionClicked;

        private readonly List<GameObject> _filas = new List<GameObject>();
        private readonly UIEventSubscriptions _formaSubscriptions = new UIEventSubscriptions();
        private Button _botonPracticaActivo;

        private void Awake()
        {
            if (btnAudio != null) btnAudio.onClick.AddListener(Audio);
            if (btnPracticar != null) btnPracticar.onClick.AddListener(Pronunciacion);
        }


        private void OnDestroy()
        {
            _formaSubscriptions.Dispose();
            if (btnAudio != null) btnAudio.onClick.RemoveListener(Audio);
            if (btnPracticar != null) btnPracticar.onClick.RemoveListener(Pronunciacion);
        }

        public void Configurar(PasoLeccionModel paso, PalabraModel palabra)
        {
            _formaSubscriptions.Dispose();
            Transform contenedorItems = items != null ? items : contenedorFormas.transform;
            for (int i = contenedorItems.childCount - 1; i >= 0; i--)
                RuntimeActividadUIFactory.Retirar(contenedorItems.GetChild(i).gameObject);
            _filas.Clear();
            txtBadge.text = "Conoce una nueva palabra";
            txtRaizUch.text = palabra?.uch ?? string.Empty;
            txtSignificadoEs.text = palabra?.es ?? string.Empty;
            bool tieneFormas = palabra?.formas != null && palabra.formas.Count > 0;
            contenedorFormas.SetActive(tieneFormas);
            txtTituloFormas.text = "Formas documentadas";
            if (tieneFormas)
            {
                foreach (FormaModel forma in palabra.formas)
                {
                    GameObject fila = Instantiate(prefabFilaForma, contenedorItems, false);
                    fila.transform.Find("ColTexto/TxtUchForma").GetComponent<TMP_Text>().text = forma.uch;
                    fila.transform.Find("ColTexto/TxtEsForma").GetComponent<TMP_Text>().text = forma.es;
                    Button btnEscuchar = fila.transform.Find("BtnEscuchar")?.GetComponent<Button>();
                    Button btnPracticar = fila.transform.Find("BtnPracticar")?.GetComponent<Button>();
                    _formaSubscriptions.Add(btnEscuchar, () => OnFormaAudioClicked?.Invoke(forma.id));
                    _formaSubscriptions.Add(btnPracticar, () => PracticarForma(btnPracticar, forma.uch));
                    _filas.Add(fila);
                }

                ActualizarAltoFormas();
            }
            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        private void Audio() => OnAudioClicked?.Invoke();
        private void Pronunciacion()
        {
            _botonPracticaActivo = btnPracticar;
            OnPronunciacionClicked?.Invoke();
        }

        private void PracticarForma(Button boton, string esperado)
        {
            _botonPracticaActivo = boton;
            OnFormaPronunciacionClicked?.Invoke(esperado);
        }

        public void MostrarEstadoGrabacion(bool grabando)
        {
            foreach (Button boton in BotonesPresentacion())
            {
                bool esActivo = boton == _botonPracticaActivo;
                boton.interactable = !grabando;
                if (!esActivo) continue;

                EstablecerTextoBoton(boton, grabando
                    ? "GRABANDO"
                    : boton == btnPracticar ? "PRACTICAR\nPRONUNCIACIÓN" : "PRACTICAR");
                EstablecerIconoBoton(boton, grabando ? iconoGrabando : iconoPracticar);
            }
        }

        private IEnumerable<Button> BotonesPresentacion()
        {
            if (btnAudio != null) yield return btnAudio;
            if (btnPracticar != null) yield return btnPracticar;
            foreach (GameObject fila in _filas)
            {
                Button escuchar = fila.transform.Find("BtnEscuchar")?.GetComponent<Button>();
                Button practicar = fila.transform.Find("BtnPracticar")?.GetComponent<Button>();
                if (escuchar != null) yield return escuchar;
                if (practicar != null) yield return practicar;
            }
        }

        private static void EstablecerTextoBoton(Button boton, string texto)
        {
            if (boton == null) return;
            TMP_Text etiqueta = boton.GetComponentInChildren<TMP_Text>();
            if (etiqueta != null) etiqueta.text = texto;
        }

        private static void EstablecerIconoBoton(Button boton, Sprite icono)
        {
            if (boton == null || icono == null) return;
            Image imagen = boton.transform.Find("Icono")?.GetComponent<Image>();
            if (imagen != null) imagen.sprite = icono;
        }

        private void ActualizarAltoFormas()
        {
            RectTransform rect = contenedorFormas.GetComponent<RectTransform>();
            VerticalLayoutGroup layout = contenedorFormas.GetComponent<VerticalLayoutGroup>();
            LayoutElement element = contenedorFormas.GetComponent<LayoutElement>();
            if (rect == null || layout == null || element == null) return;

            ActualizarAltoItems();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            float alto = CalcularAlto(layout, rect);

            element.minHeight = alto;
            element.preferredHeight = alto;
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, alto);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            Canvas.ForceUpdateCanvases();

            for (RectTransform padre = rect.parent as RectTransform; padre != null; padre = padre.parent as RectTransform)
                LayoutRebuilder.ForceRebuildLayoutImmediate(padre);
        }

        private void ActualizarAltoItems()
        {
            if (items == null) return;

            RectTransform rectItems = items as RectTransform;
            VerticalLayoutGroup layoutItems = items.GetComponent<VerticalLayoutGroup>();
            LayoutElement elementoItems = items.GetComponent<LayoutElement>();
            if (rectItems == null || layoutItems == null || elementoItems == null) return;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectItems);
            float alto = CalcularAlto(layoutItems, rectItems);
            elementoItems.minHeight = alto;
            elementoItems.preferredHeight = alto;
            rectItems.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, alto);
        }

        private static float CalcularAlto(VerticalLayoutGroup layout, RectTransform contenedor)
        {
            float alto = layout.padding.top + layout.padding.bottom;
            int elementosIncluidos = 0;

            foreach (Transform transformHijo in contenedor)
            {
                if (!(transformHijo is RectTransform hijo)
                    || !hijo.gameObject.activeInHierarchy
                    || hijo.GetComponent<LayoutElement>()?.ignoreLayout == true)
                    continue;

                if (elementosIncluidos++ > 0)
                    alto += layout.spacing;

                alto += Mathf.Max(LayoutUtility.GetMinHeight(hijo), LayoutUtility.GetPreferredHeight(hijo));
            }

            return alto;
        }
    }
}
