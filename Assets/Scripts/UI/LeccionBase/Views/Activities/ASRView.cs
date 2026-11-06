using System;
using Assets.Scripts.Core.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    public sealed class ASRView : MonoBehaviour
    {
        public enum Estado { Listo, Grabando, Procesando, Resultado, NoDisponible, Error, Preparando }

        [SerializeField] private TMP_Text txtObjetivo;
        [SerializeField] private Button btnAudioReferencia;
        [SerializeField] private Button btnPracticar;

        public event Action OnAudioClicked;
        public event Action OnPracticarClicked;

        private void Awake()
        {
            if (btnAudioReferencia != null)
                btnAudioReferencia.onClick.AddListener(NotificarAudio);
            if (btnPracticar != null)
                btnPracticar.onClick.AddListener(NotificarPractica);
        }

        private void OnDestroy()
        {
            if (btnAudioReferencia != null)
                btnAudioReferencia.onClick.RemoveListener(NotificarAudio);
            if (btnPracticar != null)
                btnPracticar.onClick.RemoveListener(NotificarPractica);
        }


        public void Preparar(string objetivo)
        {
            txtObjetivo.text = objetivo ?? string.Empty;
            MostrarEstado(Estado.Preparando);
        }

        public void Configurar(string objetivo, bool disponible, bool esOracion = false)
        {
            txtObjetivo.text = objetivo ?? string.Empty;
            MostrarEstado(disponible ? Estado.Listo : Estado.NoDisponible);
        }

        public void MostrarEstado(Estado estado)
        {
            bool grabando = estado == Estado.Grabando;
            bool procesando = estado == Estado.Procesando;
            bool preparando = estado == Estado.Preparando;
            bool disponible = estado != Estado.NoDisponible;

            btnAudioReferencia.interactable = !grabando && !procesando && !preparando;
            btnPracticar.interactable = disponible && !grabando && !procesando && !preparando;
            btnAudioReferencia.GetComponent<ThemedButtonView>()?.AplicarTema();
            btnPracticar.GetComponent<ThemedButtonView>()?.AplicarTema();
        }

        public void PermitirReintento(bool permitido) => btnPracticar.interactable = permitido;

        private void NotificarAudio() => OnAudioClicked?.Invoke();
        private void NotificarPractica() => OnPracticarClicked?.Invoke();
    }
}
