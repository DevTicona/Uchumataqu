using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Main
{
    public class BottomNavigationView : MonoBehaviour
    {
        [Header("Botones de navegación")]
        [SerializeField] private Button btnInicio;
        [SerializeField] private Button btnLecciones;
        [SerializeField] private Button btnLogros;
        [SerializeField] private Button btnAjustes;
        [SerializeField] private Button btnAcerca;

        public event Action<PaginaMain> OnTabSelected;

        private readonly Dictionary<PaginaMain, Button> _botones = new Dictionary<PaginaMain, Button>();
        private readonly Dictionary<Button, Image> _iconos = new Dictionary<Button, Image>();
        private readonly Dictionary<Button, Image> _indicadores = new Dictionary<Button, Image>();
        private static readonly Color ColorActivo = new Color(0.85f, 0.47f, 0.04f, 1f);
        private static readonly Color ColorInactivo = new Color(0.42f, 0.45f, 0.50f, 1f);

        private void Awake()
        {
            ConfigurarBoton(btnInicio, PaginaMain.Inicio, "icono_inicio");
            ConfigurarBoton(btnLecciones, PaginaMain.Lecciones, "icono_lecciones");
            ConfigurarBoton(btnLogros, PaginaMain.Logros, "icono_logros");
            ConfigurarBoton(btnAjustes, PaginaMain.Ajustes, "icono_ajustes");
            ConfigurarBoton(btnAcerca, PaginaMain.Acerca, "icono_acerca");
        }

        public void ResaltarPestana(PaginaMain pagina)
        {
            foreach (KeyValuePair<PaginaMain, Button> item in _botones)
            {
                bool activa = item.Key == pagina;
                Button boton = item.Value;
                TMP_Text texto = boton.GetComponentInChildren<TMP_Text>(true);
                if (texto != null)
                {
                    texto.color = activa ? ColorActivo : ColorInactivo;
                    texto.fontStyle = activa ? FontStyles.Bold : FontStyles.Normal;
                }
                if (_iconos.TryGetValue(boton, out Image icono))
                    icono.color = activa ? ColorActivo : ColorInactivo;
                if (_indicadores.TryGetValue(boton, out Image indicador))
                    indicador.gameObject.SetActive(activa);
            }
        }

        private void ConfigurarBoton(Button boton, PaginaMain pagina, string nombreIcono)
        {
            if (boton == null) return;
            _botones[pagina] = boton;
            boton.onClick.AddListener(() => OnTabSelected?.Invoke(pagina));

            TMP_Text etiqueta = boton.GetComponentInChildren<TMP_Text>(true);
            if (etiqueta != null)
            {
                RectTransform rectEtiqueta = etiqueta.rectTransform;
                rectEtiqueta.anchorMin = Vector2.zero;
                rectEtiqueta.anchorMax = new Vector2(1f, 0.43f);
                rectEtiqueta.offsetMin = Vector2.zero;
                rectEtiqueta.offsetMax = Vector2.zero;
                etiqueta.fontSize = Mathf.Min(etiqueta.fontSize, 18f);
            }

            Image icono = CrearImagen(boton.transform, "Icono");
            RectTransform rectIcono = icono.rectTransform;
            rectIcono.anchorMin = new Vector2(0.5f, 0.68f);
            rectIcono.anchorMax = new Vector2(0.5f, 0.68f);
            rectIcono.sizeDelta = new Vector2(30f, 30f);
            icono.preserveAspect = true;
            _iconos[boton] = icono;

            Image indicador = CrearImagen(boton.transform, "IndicadorActivo");
            RectTransform rectIndicador = indicador.rectTransform;
            rectIndicador.anchorMin = new Vector2(0.25f, 1f);
            rectIndicador.anchorMax = new Vector2(0.75f, 1f);
            rectIndicador.pivot = new Vector2(0.5f, 1f);
            rectIndicador.sizeDelta = new Vector2(0f, 4f);
            indicador.color = ColorActivo;
            _indicadores[boton] = indicador;

            _ = CargarIconoAsync(icono, nombreIcono);
        }

        private static Image CrearImagen(Transform padre, string nombre)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            objeto.transform.SetParent(padre, false);
            Image imagen = objeto.GetComponent<Image>();
            imagen.raycastTarget = false;
            return imagen;
        }

        private static async Task CargarIconoAsync(Image destino, string nombre)
        {
            try
            {
                Sprite sprite = await AddressablesService.CargarAssetAsync<Sprite>(
                    ContentAddress.Icono("navegacion", nombre));
                if (destino != null) destino.sprite = sprite;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BottomNavigationView] No se pudo cargar {nombre}: {ex.Message}");
            }
        }
    }
}
