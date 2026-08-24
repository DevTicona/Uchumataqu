using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        private void CrearControlesRuntime()
        {
            var layout = gameObject.GetComponent<VerticalLayoutGroup>() ?? gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.padding = new RectOffset(20, 20, 20, 20);
            txtTitulo = CrearTexto("Titulo", 34f);
            txtAciertos = CrearTexto("Aciertos", 26f);
            txtPrecision = CrearTexto("Precision", 26f);
            estrella1 = CrearTexto("Estrella1", 32f).gameObject;
            estrella2 = CrearTexto("Estrella2", 32f).gameObject;
            estrella3 = CrearTexto("Estrella3", 32f).gameObject;
            estrella1.GetComponent<TMP_Text>().text = "★";
            estrella2.GetComponent<TMP_Text>().text = "★";
            estrella3.GetComponent<TMP_Text>().text = "★";
            btnContinuar = CrearBoton("Continuar");
            btnVolverMapa = CrearBoton("Volver al mapa");
        }

        private void Awake()
        {
            if (btnContinuar == null || btnVolverMapa == null) CrearControlesRuntime();
            if (btnContinuar != null)
                btnContinuar.onClick.AddListener(() => OnContinuarClicked?.Invoke());

            if (btnVolverMapa != null)
                btnVolverMapa.onClick.AddListener(() => OnVolverMapaClicked?.Invoke());
        }

        private TMP_Text CrearTexto(string nombre, float tamano)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            go.transform.SetParent(transform, false);
            var texto = go.GetComponent<TextMeshProUGUI>();
            texto.fontSize = tamano;
            texto.alignment = TextAlignmentOptions.Center;
            go.GetComponent<LayoutElement>().preferredHeight = tamano * 1.7f;
            return texto;
        }

        private Button CrearBoton(string etiqueta)
        {
            var go = new GameObject($"Btn{etiqueta}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(transform, false);
            go.GetComponent<LayoutElement>().preferredHeight = 60f;
            TMP_Text texto = CrearTexto(etiqueta, 23f);
            texto.transform.SetParent(go.transform, false);
            return go.GetComponent<Button>();
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

            gameObject.SetActive(true);
        }

        public void Ocultar()
        {
            gameObject.SetActive(false);
        }
    }
}
