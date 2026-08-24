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
                : new GameObject($"Logro_{logro.Id}", typeof(RectTransform), typeof(Image));

            if (prefabLogroItem == null)
                item.transform.SetParent(contenedorLogros, false);

            var textos = item.GetComponentsInChildren<TMP_Text>();
            if (textos.Length >= 2)
            {
                textos[0].text = logro.Titulo;
                textos[1].text = logro.Descripcion;
            }
        }
    }
}
