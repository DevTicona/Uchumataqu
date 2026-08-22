using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Core.Services;

namespace Assets.Scripts.UI.LeccionBase.Componentes
{
    /// <summary>
    /// Modal de resultado final de lección completada.
    /// Especificación: Docs/UI/04-escena-leccion-base.md §12
    /// </summary>
    public class ModalResultadoView : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TMP_Text txtTitulo;
        [SerializeField] private TMP_Text txtAciertos;
        [SerializeField] private TMP_Text txtPrecision;

        [Header("Estrellas")]
        [SerializeField] private GameObject estrella1;
        [SerializeField] private GameObject estrella2;
        [SerializeField] private GameObject estrella3;

        [Header("Botones")]
        [SerializeField] private Button btnContinuar;
        [SerializeField] private Button btnVolverMapa;

        public event Action OnContinuarClicked;
        public event Action OnVolverMapaClicked;

        private void Awake()
        {
            if (btnContinuar != null)
                btnContinuar.onClick.AddListener(() => OnContinuarClicked?.Invoke());

            if (btnVolverMapa != null)
                btnVolverMapa.onClick.AddListener(() => OnVolverMapaClicked?.Invoke());
        }

        public void Mostrar(int aciertos, int totalPasos, double precision)
        {
            if (txtTitulo != null)
                txtTitulo.text = "✓ ¡Lección Completada!";

            if (txtAciertos != null)
                txtAciertos.text = $"{aciertos} / {totalPasos} aciertos";

            if (txtPrecision != null)
                txtPrecision.text = $"{precision:F0} % de precisión";

            // Calcular estrellas obtenidas
            int estrellas = 1;
            if (precision >= 85) estrellas = 3;
            else if (precision >= 60) estrellas = 2;

            if (estrella1 != null) estrella1.SetActive(estrellas >= 1);
            if (estrella2 != null) estrella2.SetActive(estrellas >= 2);
            if (estrella3 != null) estrella3.SetActive(estrellas >= 3);

            AudioService.Instance?.ReproducirEfecto("leccion_completada");

            gameObject.SetActive(true);
        }

        public void Ocultar()
        {
            gameObject.SetActive(false);
        }
    }
}
