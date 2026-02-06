using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using Assets.Scripts.Services;
using Assets.Scripts.Views.UI;

namespace Assets.Scripts.Views.RealidadAumentada
{
    public class RAView : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TextMeshProUGUI txtUchumataqu;
        [SerializeField] private TextMeshProUGUI txtEspanol;
        [SerializeField] private TextMeshProUGUI txtIndicacion;
        [SerializeField] private TextMeshProUGUI txtFeedback;

        [Header("Botones")]
        [SerializeField] private Button btnReproducir;
        [SerializeField] private Button btnMicrofono;

        [Header("Navegación")]
        [SerializeField] private Transform navegacionRoot;

        private BotonView botonSiguiente;
        private BotonView botonAnterior;

        // Eventos hacia el Presenter
        public event Action OnReproducirAudio;
        public event Action OnGrabarVoz;
        public event Action OnSiguiente;
        public event Action OnAnterior;

        private void Awake()
        {
            RegistrarEventosUI();
            LimpiarFeedback();
            InicializarNavegacion();
        }

        // ======================
        // UI
        // ======================

        private void RegistrarEventosUI()
        {
            if (btnReproducir != null)
                btnReproducir.onClick.AddListener(() => OnReproducirAudio?.Invoke());

            if (btnMicrofono != null)
                btnMicrofono.onClick.AddListener(() => OnGrabarVoz?.Invoke());
        }

        private void InicializarNavegacion()
        {
            if (navegacionRoot == null)
            {
                Debug.LogError("[RAView] NavegacionRoot no asignado.");
                return;
            }

            StartCoroutine(CrearBotonesNavegacion());
        }

        private IEnumerator CrearBotonesNavegacion()
        {
            // BOTÓN ANTERIOR
            yield return CrearBotonAsync(
                texto: "Anterior",
                onCreado: boton =>
                {
                    botonAnterior = boton;
                    botonAnterior.Configurar(
                        "Anterior",
                        Color.gray,
                        Color.white,
                        () => OnAnterior?.Invoke()
                    );
                }
            );

            // BOTÓN SIGUIENTE
            yield return CrearBotonAsync(
                texto: "Siguiente",
                onCreado: boton =>
                {
                    botonSiguiente = boton;
                    botonSiguiente.Configurar(
                        "Siguiente",
                        new Color32(0x3B, 0x6E, 0xF5, 0xFF),
                        Color.white,
                        () => OnSiguiente?.Invoke()
                    );
                }
            );
        }

        private IEnumerator CrearBotonAsync(
            string texto,
            Action<BotonView> onCreado
        )
        {
            bool instanciaCompletada = false;
            BotonView boton = null;

            LeccionBaseService.InstanciarBoton(
                navegacionRoot,
                gameObject =>
                {
                    boton = gameObject.GetComponent<BotonView>();
                    instanciaCompletada = true;
                },
                () =>
                {
                    Debug.LogError($"[RAView] No se pudo crear el botón {texto}");
                    instanciaCompletada = true;
                }
            );

            yield return new WaitUntil(() => instanciaCompletada);

            if (boton != null)
                onCreado?.Invoke(boton);
        }

        // ======================
        // Métodos públicos
        // ======================

        public void MostrarPalabra(string uchumataqu, string espanol)
        {
            if (txtUchumataqu != null)
                txtUchumataqu.text = uchumataqu;

            if (txtEspanol != null)
                txtEspanol.text = espanol;
        }

        public void MostrarIndicacion(string mensaje)
        {
            if (txtIndicacion != null)
                txtIndicacion.text = mensaje;
        }

        public void MostrarFeedback(string mensaje)
        {
            if (txtFeedback == null) return;

            txtFeedback.text = mensaje;
            txtFeedback.gameObject.SetActive(true);
        }

        public void LimpiarFeedback()
        {
            if (txtFeedback == null) return;

            txtFeedback.text = string.Empty;
            txtFeedback.gameObject.SetActive(false);
        }

        public void ActivarInteraccion(bool activa)
        {
            if (btnReproducir != null)
                btnReproducir.interactable = activa;

            if (btnMicrofono != null)
                btnMicrofono.interactable = activa;
        }

        public void ActualizarNavegacion(bool puedeIrAnterior, bool puedeIrSiguiente)
        {
            if (botonAnterior != null)
                botonAnterior.SetInteractable(puedeIrAnterior);

            if (botonSiguiente != null)
                botonSiguiente.SetInteractable(puedeIrSiguiente);
        }
    }
}
