using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI.LeccionBase.Componentes
{
    /// <summary>
    /// Modal de resultado final de lección completada.
    /// Especificación: Docs/Specifications/04-flujo-leccion-app.md.
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
        [SerializeField] private Button btnVolverMapa;

        public event Action OnVolverMapaClicked;

        public bool ValidarConfiguracion(out string error)
        {
            bool completa = txtTitulo != null && txtAciertos != null && txtPrecision != null &&
                estrella1 != null && estrella2 != null && estrella3 != null &&
                btnVolverMapa != null;
            error = completa ? null : "Faltan referencias obligatorias del modal de resultados en el Inspector.";
            return completa;
        }

        private void Awake()
        {
            if (!ValidarConfiguracion(out string error))
            {
                Debug.LogError($"[ModalResultadoView] {error}", this);
                enabled = false;
                return;
            }
            if (btnVolverMapa != null)
                btnVolverMapa.onClick.AddListener(NotificarVolverMapa);
        }

        private void OnDestroy()
        {
            if (btnVolverMapa != null) btnVolverMapa.onClick.RemoveListener(NotificarVolverMapa);
        }

        private void NotificarVolverMapa() => OnVolverMapaClicked?.Invoke();

        public void Mostrar(int aciertos, int totalPasos, double precision, int estrellas)
        {
            if (txtTitulo != null)
                txtTitulo.text = "¡Lección Completada!";

            if (txtAciertos != null)
                txtAciertos.text = $"{aciertos} / {totalPasos} aciertos";

            if (txtPrecision != null)
                txtPrecision.text = $"{precision:F0} % de precisión";

            if (estrella1 != null) estrella1.SetActive(estrellas >= 1);
            if (estrella2 != null) estrella2.SetActive(estrellas >= 2);
            if (estrella3 != null) estrella3.SetActive(estrellas >= 3);

            gameObject.SetActive(true);
        }

        public void Ocultar()
        {
            gameObject.SetActive(false);
        }
    }
}
