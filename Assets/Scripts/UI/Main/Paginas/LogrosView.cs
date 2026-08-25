using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.UI.Main;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista de la pestaña de Logros en 02_Main.
    /// Especificado en Docs/UI/08-perfil-logros-ajustes-acerca.md §3.
    /// </summary>
    public class LogrosView : MonoBehaviour
    {
        [Header("Contenedor de Lista")]
        [SerializeField] private Transform contenedorLogros;
        [SerializeField] private GameObject prefabLogroItem;

        [Header("Resumen")]
        [SerializeField] private TMP_Text txtTotalLogros;

        private void OnEnable()
        {
            OnSolicitarActualizacion?.Invoke();
        }

        public event Action OnSolicitarActualizacion;

        public void Mostrar(IReadOnlyList<LogroPresentacionData> logros)
        {
            if (logros == null) return;

            int desbloqueados = 0;
            foreach (var logro in logros) if (logro.Desbloqueado) desbloqueados++;
            if (txtTotalLogros != null)
            {
                txtTotalLogros.text = $"{desbloqueados} / {logros.Count} logros conseguidos";
            }

            if (contenedorLogros != null)
            {
                foreach (Transform child in contenedorLogros)
                {
                    Destroy(child.gameObject);
                }

                foreach (var logro in logros)
                {
                    CrearElementoLogro(logro);
                }
            }
        }

        private void CrearElementoLogro(LogroPresentacionData logro)
        {
            GameObject item = (prefabLogroItem != null)
                ? Instantiate(prefabLogroItem, contenedorLogros)
                : CrearElementoRuntime(logro.Id);

            Image fondo = item.GetComponent<Image>();
            if (fondo != null)
                fondo.color = logro.Desbloqueado
                    ? new Color(0.15f, 0.55f, 0.34f, 1f)
                    : new Color(0.24f, 0.28f, 0.34f, 0.9f);

            var textos = item.GetComponentsInChildren<TMP_Text>();
            if (textos.Length >= 2)
            {
                textos[0].text = logro.Titulo;
                textos[1].text = logro.Desbloqueado
                    ? $"{logro.Descripcion}  Completado"
                    : $"{logro.Descripcion}  {logro.ProgresoActual}/{logro.Objetivo}";
            }
        }

        private GameObject CrearElementoRuntime(string id)
        {
            var item = new GameObject(
                $"Logro_{id}",
                typeof(RectTransform),
                typeof(Image),
                typeof(VerticalLayoutGroup),
                typeof(LayoutElement));
            item.transform.SetParent(contenedorLogros, false);

            var layout = item.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 14, 14);
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            item.GetComponent<LayoutElement>().preferredHeight = 112f;

            CrearTexto(item.transform, "Titulo", 24f, FontStyles.Bold);
            CrearTexto(item.transform, "Descripcion", 18f, FontStyles.Normal);
            return item;
        }

        private static void CrearTexto(Transform padre, string nombre, float tamano, FontStyles estilo)
        {
            var objeto = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI));
            objeto.transform.SetParent(padre, false);
            var texto = objeto.GetComponent<TextMeshProUGUI>();
            texto.fontSize = tamano;
            texto.fontStyle = estilo;
            texto.color = Color.white;
            texto.textWrappingMode = TextWrappingModes.Normal;
        }
    }
}
