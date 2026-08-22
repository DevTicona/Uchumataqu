using UnityEngine;
using Assets.Scripts.Core;

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
    }
}
