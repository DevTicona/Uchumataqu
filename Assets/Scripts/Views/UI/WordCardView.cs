using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Models;
using System;

namespace Assets.Scripts.Views.UI
{
    public class WordCardView : MonoBehaviour
    {
        [Header("Texto")]
        [SerializeField] private TextMeshProUGUI txtUchumataqu;
        [SerializeField] private TextMeshProUGUI txtEspanol;

        [Header("Botones")]
        [SerializeField] private Button btnMicrofono;
        [SerializeField] private Button btnReproducir;

        // Eventos expuestos al Presenter
        public event Action OnMicrofonoPressed;
        public event Action OnReproducirPressed;

        public void Render(PalabraData palabra)
        {
            txtUchumataqu.text = palabra.uchumataqu;
            txtEspanol.text = palabra.espanol;

            ConfigureButtons();
        }

        private void ConfigureButtons()
        {
            if (btnMicrofono != null)
            {
                btnMicrofono.onClick.RemoveAllListeners();
                btnMicrofono.onClick.AddListener(() => OnMicrofonoPressed?.Invoke());
            }

            if (btnReproducir != null)
            {
                btnReproducir.onClick.RemoveAllListeners();
                btnReproducir.onClick.AddListener(() => OnReproducirPressed?.Invoke());
            }
        }

        // Métodos de UI controlados por el Presenter
        public void SetReproducirInteractable(bool value)
        {
            if (btnReproducir != null)
                btnReproducir.interactable = value;
        }

        public void SetMicrofonoInteractable(bool value)
        {
            if (btnMicrofono != null)
                btnMicrofono.interactable = value;
        }
    }
}
