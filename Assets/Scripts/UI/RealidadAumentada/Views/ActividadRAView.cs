using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.RealidadAumentada
{
    public sealed class ActividadRAView : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text txtContenido;
        [SerializeField] private TMP_Text txtFeedback;
        [SerializeField] private Button btnVoz;
        [SerializeField] private Button btnContinuar;
        [SerializeField] private Button btnEnviar;
        [SerializeField] private TMP_InputField entrada;

        public event Action VozClick;
        public event Action ContinuarClick;
        public event Action<string> EscrituraEnviada;

        private void Awake()
        {
            btnVoz.onClick.AddListener(NotificarVoz);
            btnContinuar.onClick.AddListener(NotificarContinuar);
            btnEnviar.onClick.AddListener(NotificarEscritura);
        }

        private void OnDestroy()
        {
            btnVoz.onClick.RemoveListener(NotificarVoz);
            btnContinuar.onClick.RemoveListener(NotificarContinuar);
            btnEnviar.onClick.RemoveListener(NotificarEscritura);
        }

        private void NotificarVoz() => VozClick?.Invoke();
        private void NotificarContinuar() => ContinuarClick?.Invoke();
        private void NotificarEscritura() => EscrituraEnviada?.Invoke(entrada.text);

        public void Mostrar(string tipo, string contenido, bool nuevoPaso)
        {
            panel.SetActive(tipo == "repetir_voz" || tipo == "escribir" || tipo == "historia");
            if (!panel.activeSelf) return;
            txtContenido.text = contenido;
            btnVoz.gameObject.SetActive(tipo == "repetir_voz");
            btnContinuar.gameObject.SetActive(tipo == "historia");
            btnEnviar.gameObject.SetActive(tipo == "escribir");
            entrada.gameObject.SetActive(tipo == "escribir");
            if (nuevoPaso)
            {
                txtFeedback.text = string.Empty;
                entrada.SetTextWithoutNotify(string.Empty);
            }
        }

        public void Estado(string mensaje, bool grabando, bool ocupado, bool continuar)
        {
            txtFeedback.text = mensaje;
            btnVoz.interactable = !ocupado;
            btnVoz.GetComponentInChildren<TMP_Text>().text = grabando ? "TERMINAR" : "PRONUNCIAR";
            btnEnviar.interactable = !ocupado;
            btnContinuar.gameObject.SetActive(continuar);
            btnContinuar.interactable = !ocupado;
        }

        public void Ocultar() => panel.SetActive(false);
    }
}
