using System.Collections;
using Assets.Scripts.Persistencia;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Core.UI.Components;

namespace Assets.Scripts.Core.UI.Ayuda
{
    public sealed class AyudaBrevePantalla : MonoBehaviour
    {
        [SerializeField] private Button acceso;
        [SerializeField] private Button cerrar;
        [SerializeField] private GameObject panel;
        [SerializeField, TextArea] private string descripcion;
        private AyudaCardLayout tarjeta;
        private Button anterior;
        private PasoGuia[] pasos;
        private int indice;
        private string titulo = "Ayuda";
        public bool Visible => panel != null && panel.activeSelf;

        private void Awake()
        {
            if (acceso == null) acceso = transform.Find("BtnAyuda").GetComponent<Button>();
            if (panel == null) panel = transform.Find("Panel").gameObject;
            tarjeta = panel.GetComponentInChildren<AyudaCardLayout>(true);
            cerrar = tarjeta.Buscar<Button>("BtnSiguiente");
            anterior = tarjeta.Buscar<Button>("BtnAnterior");
            pasos = new[] { new PasoGuia("Ayuda", descripcion) };
            foreach (string nombre in new[] { "OpcionesAyuda", "BtnAnterior", "BtnOmitir" })
                tarjeta.Buscar<RectTransform>(nombre).gameObject.SetActive(false);
            panel.SetActive(false);
            acceso.onClick.AddListener(Alternar);
            cerrar.onClick.AddListener(Avanzar);
            anterior.onClick.AddListener(Retroceder);
        }

        private IEnumerator Start()
        {
            yield return null;
            yield return new WaitForSecondsRealtime(0.3f);
            if (Visible) yield break;
            MostrarPrimeraVez("pantalla:" + gameObject.scene.name + ":v1", "Ayuda", pasos);
        }

        public static AyudaBrevePantalla BuscarEnEscena(UnityEngine.SceneManagement.Scene escena)
        {
            foreach (var raiz in escena.GetRootGameObjects())
            {
                var ayuda = raiz.GetComponentInChildren<AyudaBrevePantalla>(true);
                if (ayuda != null) return ayuda;
            }
            return null;
        }

        public bool MostrarPrimeraVez(string identificador, string encabezado, params PasoGuia[] contenido)
        {
            if (string.IsNullOrWhiteSpace(identificador) || contenido == null || contenido.Length == 0 ||
                AyudaRepository.FueVista(identificador)) return false;
            PrepararGuia(encabezado, contenido);
            MostrarPaso();
            AyudaRepository.MarcarVista(identificador);
            return true;
        }

        public void PrepararGuia(string encabezado, params PasoGuia[] contenido)
        {
            if (contenido == null || contenido.Length == 0) return;
            titulo = encabezado;
            pasos = contenido;
            indice = 0;
        }

        private void OnDestroy()
        {
            if (acceso != null) acceso.onClick.RemoveListener(Alternar);
            if (cerrar != null) cerrar.onClick.RemoveListener(Avanzar);
            if (anterior != null) anterior.onClick.RemoveListener(Retroceder);
        }

        private void Alternar()
        {
            if (Visible) Cerrar();
            else { indice = 0; MostrarPaso(); }
        }

        private void MostrarPaso()
        {
            panel.SetActive(true);
            tarjeta.Buscar<TMPro.TMP_Text>("Titulo").text = pasos[indice].titulo ?? titulo;
            tarjeta.Buscar<TMPro.TMP_Text>("Contador").text = "GUÍA · " + (indice + 1) + " / " + pasos.Length;
            tarjeta.Buscar<TMPro.TMP_Text>("Descripcion").text = pasos[indice].texto;
            cerrar.GetComponentInChildren<TMPro.TMP_Text>(true).text = indice == pasos.Length - 1 ? "ENTENDIDO" : "SIGUIENTE";
            anterior.gameObject.SetActive(indice > 0);
            var area = (RectTransform)panel.transform;
            ((RectTransform)tarjeta.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Min(720f, area.rect.width - 48f));
            tarjeta.Ajustar(area.rect.height - 48f);
            var scroll = tarjeta.GetComponentInChildren<ScrollRect>(true);
            if (scroll != null) { scroll.StopMovement(); scroll.verticalNormalizedPosition = 1f; }
        }

        private void Avanzar()
        {
            if (++indice < pasos.Length) MostrarPaso();
            else Cerrar();
        }

        private void Retroceder()
        {
            if (indice > 0) { indice--; MostrarPaso(); }
        }

        public bool CerrarSiAbierta()
        {
            if (!Visible) return false;
            Cerrar();
            return true;
        }

        public void Cerrar() => panel.SetActive(false);
    }
}
