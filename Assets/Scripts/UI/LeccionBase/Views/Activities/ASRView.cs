using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    public sealed class ASRView : MonoBehaviour
    {
        public enum Estado { Listo, Grabando, Procesando, Resultado, NoDisponible, Error }

        [SerializeField] private TMP_Text txtObjetivo;
        [SerializeField] private TMP_Text txtEstado;
        [SerializeField] private TMP_Text txtTranscripcion;
        [SerializeField] private TMP_Text txtPorcentaje;
        [SerializeField] private GameObject panelResultado;
        [SerializeField] private GameObject indicadorGrabacion;
        [SerializeField] private Button btnAudioReferencia;
        [SerializeField] private Button btnMicrofono;
        [SerializeField] private Button btnOmitir;

        public event Action OnAudioClicked;
        public event Action OnMicrofonoClicked;
        public event Action OnOmitirClicked;

        private void Awake()
        {
            btnAudioReferencia.onClick.AddListener(Audio);
            btnMicrofono.onClick.AddListener(Microfono);
            btnOmitir.onClick.AddListener(Omitir);
        }

        private void OnDestroy()
        {
            btnAudioReferencia.onClick.RemoveListener(Audio);
            btnMicrofono.onClick.RemoveListener(Microfono);
            btnOmitir.onClick.RemoveListener(Omitir);
        }


        public void Configurar(string objetivo, bool disponible)
        {
            txtObjetivo.text = objetivo;
            MostrarEstado(disponible ? Estado.Listo : Estado.NoDisponible,
                disponible ? "Escucha y después toca el micrófono." :
                "La práctica de voz no está disponible en este dispositivo. Puedes escuchar y continuar.");
        }

        public void MostrarResultado(string transcripcion, float porcentaje, string mensaje)
        {
            MostrarEstado(Estado.Resultado, mensaje, transcripcion, $"Similitud: {porcentaje:0}%");
        }

        public void MostrarEstado(Estado estado, string mensaje, string transcripcion = "", string porcentaje = "")
        {
            txtEstado.text = mensaje;
            txtTranscripcion.text = transcripcion;
            txtPorcentaje.text = porcentaje;
            panelResultado.SetActive(estado == Estado.Resultado);
            indicadorGrabacion.SetActive(estado == Estado.Grabando);
            btnMicrofono.interactable = estado != Estado.NoDisponible && estado != Estado.Procesando;
            btnMicrofono.GetComponentInChildren<TMP_Text>().text = estado == Estado.Grabando ? "DETENER" : "PRACTICAR";
        }

        private void Audio() => OnAudioClicked?.Invoke();
        private void Microfono() => OnMicrofonoClicked?.Invoke();
        private void Omitir() => OnOmitirClicked?.Invoke();
    }
}
