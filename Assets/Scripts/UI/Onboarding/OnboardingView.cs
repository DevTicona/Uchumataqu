using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Assets.Scripts.UI.Onboarding
{
    public class OnboardingView : MonoBehaviour
    {
        [Header("Paneles de Pasos")]
        [SerializeField] private GameObject pasoBienvenida;
        [SerializeField] private GameObject pasoPerfil;
        [SerializeField] private GameObject pasoAvatar;

        [Header("Campos Perfil")]
        [SerializeField] private TMP_InputField inputNombre;
        [SerializeField] private TMP_Dropdown dropdownEdad;

        [Header("Navegación")]
        [SerializeField] private Button botonContinuar;
        [SerializeField] private Button botonAtras;
        [SerializeField] private TextMeshProUGUI textoBotonContinuar;

        public event Action OnContinuarClick;
        public event Action OnAtrasClick;

        private string _avatarSeleccionado = "avatar_01";

        private void Awake()
        {
            if (botonContinuar != null)
                botonContinuar.onClick.AddListener(() => OnContinuarClick?.Invoke());

            if (botonAtras != null)
                botonAtras.onClick.AddListener(() => OnAtrasClick?.Invoke());
        }

        public void MostrarPaso(int paso)
        {
            if (pasoBienvenida != null) pasoBienvenida.SetActive(paso == 1);
            if (pasoPerfil != null) pasoPerfil.SetActive(paso == 2);
            if (pasoAvatar != null) pasoAvatar.SetActive(paso == 3);

            if (botonAtras != null)
                botonAtras.gameObject.SetActive(paso > 1);

            if (textoBotonContinuar != null)
                textoBotonContinuar.text = paso == 3 ? "COMENZAR" : (paso == 1 ? "EMPEZAR" : "CONTINUAR");
        }

        public string ObtenerNombre() => inputNombre != null ? inputNombre.text.Trim() : "";
        public string ObtenerRangoEdad() => dropdownEdad != null && dropdownEdad.options.Count > 0 ? dropdownEdad.options[dropdownEdad.value].text : "18+";
        public string ObtenerAvatarId() => _avatarSeleccionado;

        public void SeleccionarAvatar(string avatarId)
        {
            _avatarSeleccionado = avatarId;
        }
    }
}
