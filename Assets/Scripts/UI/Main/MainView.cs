using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Core;
using Assets.Scripts.UI.Main.Paginas;

namespace Assets.Scripts.UI.Main
{
    /// <summary>
    /// Vista contenedora de las 5 páginas internas de la escena 02_Main.
    /// Especificado en Docs/UI/03-escena-main.md.
    /// </summary>
    public class MainView : MonoBehaviour
    {
        [Header("Páginas")]
        [SerializeField] private GameObject pageInicio;
        [SerializeField] private GameObject pageLecciones;
        [SerializeField] private GameObject pageLogros;
        [SerializeField] private GameObject pageAjustes;
        [SerializeField] private GameObject pageAcerca;

        [Header("Navegación Inferior")]
        [SerializeField] private BottomNavigationView bottomNavigation;

        [Header("Modal Salida")]
        [SerializeField] private GameObject dialogoSalida;

        public BottomNavigationView BottomNav => bottomNavigation;
        public InicioView Inicio => pageInicio != null ? pageInicio.GetComponent<InicioView>() : null;
        public MapaLeccionesView MapaLecciones => pageLecciones != null ? pageLecciones.GetComponent<MapaLeccionesView>() : null;
        public LogrosView Logros => pageLogros != null ? pageLogros.GetComponent<LogrosView>() : null;
        public AjustesView Ajustes => pageAjustes != null ? pageAjustes.GetComponent<AjustesView>() : null;
        public AcercaView Acerca => pageAcerca != null ? pageAcerca.GetComponent<AcercaView>() : null;
        public bool DialogoSalidaVisible => dialogoSalida != null && dialogoSalida.activeSelf;

        private void Awake()
        {
            ConfigurarDialogoSalida();
            MostrarDialogoSalida(false);
            foreach (Button boton in GetComponentsInChildren<Button>(true))
            {
                ColorBlock colores = boton.colors;
                if (colores.highlightedColor.maxColorComponent <= 1f &&
                    colores.pressedColor.maxColorComponent <= 1f)
                    continue;

                Color normal = colores.normalColor;
                colores.highlightedColor = new Color(
                    Mathf.Min(1f, normal.r * 1.1f),
                    Mathf.Min(1f, normal.g * 1.1f),
                    Mathf.Min(1f, normal.b * 1.1f),
                    normal.a);
                colores.pressedColor = new Color(normal.r * 0.85f, normal.g * 0.85f, normal.b * 0.85f, normal.a);
                boton.colors = colores;
            }
        }

        public void MostrarPagina(PaginaMain pagina)
        {
            if (pageInicio != null) pageInicio.SetActive(pagina == PaginaMain.Inicio);
            if (pageLecciones != null) pageLecciones.SetActive(pagina == PaginaMain.Lecciones);
            if (pageLogros != null) pageLogros.SetActive(pagina == PaginaMain.Logros);
            if (pageAjustes != null) pageAjustes.SetActive(pagina == PaginaMain.Ajustes);
            if (pageAcerca != null) pageAcerca.SetActive(pagina == PaginaMain.Acerca);

            if (bottomNavigation != null)
                bottomNavigation.ResaltarPestana(pagina);
        }

        public void MostrarDialogoSalida(bool mostrar)
        {
            if (dialogoSalida != null)
                dialogoSalida.SetActive(mostrar);
        }

        public void ConfirmarSalirApp()
        {
            Application.Quit();
        }

        public void CancelarSalirApp() => MostrarDialogoSalida(false);

        private void ConfigurarDialogoSalida()
        {
            if (dialogoSalida == null) return;

            Button confirmar = BuscarDescendiente(dialogoSalida.transform, "BtnConfirmarSalir")?.GetComponent<Button>();
            Button cancelar = BuscarDescendiente(dialogoSalida.transform, "BtnCancelarSalir")?.GetComponent<Button>();
            if (confirmar != null) confirmar.onClick.AddListener(ConfirmarSalirApp);
            if (cancelar != null) cancelar.onClick.AddListener(CancelarSalirApp);
        }

        private static Transform BuscarDescendiente(Transform raiz, string nombre)
        {
            foreach (Transform descendiente in raiz.GetComponentsInChildren<Transform>(true))
                if (descendiente.name == nombre) return descendiente;
            return null;
        }
    }
}
