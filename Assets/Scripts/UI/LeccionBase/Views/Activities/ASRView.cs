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
        [SerializeField] private Button btnAudioReferencia;
        [SerializeField] private Button btnPracticar;
        [SerializeField] private Sprite iconoPracticar;
        [SerializeField] private Sprite iconoGrabando;

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


        public void Configurar(string objetivo, bool disponible, bool esOracion = false)
        {
            txtObjetivo.text = objetivo ?? string.Empty;
            EstablecerTexto(btnAudioReferencia, esOracion ? "ESCUCHAR ORACIÓN" : "ESCUCHAR PALABRA");
            MostrarEstado(disponible ? Estado.Listo : Estado.NoDisponible);
        }

        public void MostrarEstado(Estado estado)
        {
            bool grabando = estado == Estado.Grabando;
            bool procesando = estado == Estado.Procesando;
            bool disponible = estado != Estado.NoDisponible;

            btnAudioReferencia.interactable = !grabando && !procesando;
            btnPracticar.interactable = disponible && !grabando && !procesando;
            EstablecerTexto(btnPracticar, grabando ? "GRABANDO" : procesando ? "ANALIZANDO" :
                estado == Estado.Resultado ? "PRACTICAR DE NUEVO" : "PRACTICAR PRONUNCIACIÓN");
            EstablecerIcono(btnPracticar, grabando || procesando ? iconoGrabando : iconoPracticar);
        }

        private static void EstablecerTexto(Button boton, string texto)
        {
            TMP_Text etiqueta = boton != null ? boton.GetComponentInChildren<TMP_Text>(true) : null;
            if (etiqueta != null) etiqueta.text = texto;
        }

        private static void EstablecerIcono(Button boton, Sprite icono)
        {
            Image imagen = boton != null ? boton.transform.Find("Icono")?.GetComponent<Image>() : null;
            if (imagen != null && icono != null) imagen.sprite = icono;
        }

        private void NotificarAudio() => OnAudioClicked?.Invoke();
        private void NotificarPractica() => OnPracticarClicked?.Invoke();
    }
}
