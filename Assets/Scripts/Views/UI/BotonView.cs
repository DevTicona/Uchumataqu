using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace Assets.Scripts.Views.UI
{
    public class BotonView : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private Button boton;
        [SerializeField] private Image fondo;
        [SerializeField] private TextMeshProUGUI texto;

        /// <summary>
        /// Configura completamente el botón.
        /// </summary>
        public void Configurar(
            string textoBoton,
            Color colorFondo,
            Color colorTexto,
            Action onClick
        )
        {
            if (texto != null)
            {
                texto.text = textoBoton;
                texto.color = colorTexto;
            }

            if (fondo != null)
                fondo.color = colorFondo;

            if (boton != null)
            {
                boton.onClick.RemoveAllListeners();
                boton.onClick.AddListener(() => onClick?.Invoke());
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (boton != null)
                boton.interactable = interactable;
        }
    }
}
