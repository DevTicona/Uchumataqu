using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Core;

namespace Assets.Scripts.UI.Main
{
    /// <summary>
    /// Barra de navegación inferior fija para la escena 02_Main.
    /// Especificado en Docs/UI/00-arquitectura-ui-ux-general.md y 03-escena-main.md.
    /// </summary>
    public class BottomNavigationView : MonoBehaviour
    {
        [Header("Botones de Navegación")]
        [SerializeField] private Button btnInicio;
        [SerializeField] private Button btnLecciones;
        [SerializeField] private Button btnLogros;
        [SerializeField] private Button btnAjustes;
        [SerializeField] private Button btnAcerca;

        public event Action<PaginaMain> OnTabSelected;

        private void Awake()
        {
            if (btnInicio != null) btnInicio.onClick.AddListener(() => OnTabSelected?.Invoke(PaginaMain.Inicio));
            if (btnLecciones != null) btnLecciones.onClick.AddListener(() => OnTabSelected?.Invoke(PaginaMain.Lecciones));
            if (btnLogros != null) btnLogros.onClick.AddListener(() => OnTabSelected?.Invoke(PaginaMain.Logros));
            if (btnAjustes != null) btnAjustes.onClick.AddListener(() => OnTabSelected?.Invoke(PaginaMain.Ajustes));
            if (btnAcerca != null) btnAcerca.onClick.AddListener(() => OnTabSelected?.Invoke(PaginaMain.Acerca));
        }

        public void ResaltarPestana(PaginaMain pagina)
        {
            // Opcional: feedback visual de pestaña seleccionada
        }
    }
}
