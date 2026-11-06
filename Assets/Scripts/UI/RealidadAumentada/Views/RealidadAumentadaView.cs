using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.RealidadAumentada
{
    /// <summary>Cabecera y estado de escaneo. Las actividades utilizan ExperienciaRAView.</summary>
    public class RealidadAumentadaView : MonoBehaviour
    {
        [Header("Progreso y navegación")]
        [SerializeField] private Slider sliderProgreso;
        [SerializeField] private TMP_Text txtProgreso;
        [SerializeField] private TMP_Text txtPorcentajeProgreso;
        [SerializeField] private Button btnVolver;
        [SerializeField] private Button btnRecolocar;

        [Header("Confirmación de salida")]
        [SerializeField] private GameObject panelConfirmacionSalida;
        [SerializeField] private Button btnSeguirAprendiendo;
        [SerializeField] private Button btnConfirmarSalida;

        [Header("Estado de cámara y superficie")]
        [SerializeField] private GameObject panelEscaneo;
        [SerializeField] private TextMeshProUGUI txtEstadoRA;
        [SerializeField] private IconografiaRAView iconografia;

        public event Action OnVolverClick;
        public event Action OnRecolocarClick;
        public event Action OnConfirmarSalidaClick;
        public event Action OnCancelarSalidaClick;
        public bool ConfirmacionSalidaVisible => panelConfirmacionSalida != null && panelConfirmacionSalida.activeSelf;

        private void Awake()
        {
            if (txtEstadoRA != null) txtEstadoRA.raycastTarget = false;
            if (btnVolver != null) btnVolver.onClick.AddListener(Volver);
            if (btnRecolocar != null) btnRecolocar.onClick.AddListener(Recolocar);
            if (btnSeguirAprendiendo != null) btnSeguirAprendiendo.onClick.AddListener(CancelarSalida);
            if (btnConfirmarSalida != null) btnConfirmarSalida.onClick.AddListener(ConfirmarSalida);
            MostrarConfirmacionSalida(false);
        }

        private void OnDestroy()
        {
            if (btnVolver != null) btnVolver.onClick.RemoveListener(Volver);
            if (btnRecolocar != null) btnRecolocar.onClick.RemoveListener(Recolocar);
            if (btnSeguirAprendiendo != null) btnSeguirAprendiendo.onClick.RemoveListener(CancelarSalida);
            if (btnConfirmarSalida != null) btnConfirmarSalida.onClick.RemoveListener(ConfirmarSalida);
        }

        private void Volver() => OnVolverClick?.Invoke();
        private void Recolocar() => OnRecolocarClick?.Invoke();
        private void ConfirmarSalida() => OnConfirmarSalidaClick?.Invoke();
        private void CancelarSalida() => OnCancelarSalidaClick?.Invoke();

        public void MostrarConfirmacionSalida(bool mostrar)
        {
            if (panelConfirmacionSalida == null) return;
            if (mostrar) panelConfirmacionSalida.transform.SetAsLastSibling();
            panelConfirmacionSalida.SetActive(mostrar);
        }

        public void ActualizarProgreso(float progreso)
        {
            float valor = Mathf.Clamp01(progreso);
            if (sliderProgreso != null) sliderProgreso.SetValueWithoutNotify(valor);
            if (txtProgreso != null) txtProgreso.text = "Progreso";
            if (txtPorcentajeProgreso != null) txtPorcentajeProgreso.text = $"{Mathf.RoundToInt(valor * 100f)}%";
        }

        public void MostrarEscaneo(bool poseValida, string mensaje = null)
        {
            iconografia?.MostrarEscaneo(poseValida,
                UnityEngine.XR.ARFoundation.ARSession.state == UnityEngine.XR.ARFoundation.ARSessionState.SessionTracking);
            if (panelEscaneo != null) panelEscaneo.SetActive(true);
            if (btnRecolocar != null) btnRecolocar.gameObject.SetActive(false);
            if (txtEstadoRA != null) txtEstadoRA.text = mensaje ?? (poseValida
                ? "Superficie detectada en el centro. Toca para colocar."
                : "Mueve lentamente el dispositivo para detectar una superficie.");
        }

        public void MostrarExperienciaGuiada()
        {
            if (panelEscaneo != null) panelEscaneo.SetActive(false);
            if (btnRecolocar != null) btnRecolocar.gameObject.SetActive(true);
        }

        public void MostrarError(string mensaje, IconoRA icono = IconoRA.RealidadAumentada)
        {
            iconografia?.MostrarEstado(icono);
            if (panelEscaneo != null) panelEscaneo.SetActive(true);
            if (txtEstadoRA != null) txtEstadoRA.text = mensaje;
        }
    }
}
