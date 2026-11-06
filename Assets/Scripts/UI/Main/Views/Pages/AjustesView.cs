using System;
using System.Collections.Generic;
using Assets.Scripts.Core.UI;
using Assets.Scripts.Core.UI.Components;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Core;
using Assets.Scripts.Core.UI.Theme;
using Assets.Scripts.UI.Avatares;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista de la pestaña de Ajustes en 02_Main.
    /// Sigue Docs/UI/00-arquitectura-ui-ux-general.md y Docs/UI/06-design-system.md.
    /// </summary>
    public class AjustesView : MonoBehaviour
    {
        private const float MargenKnob = 5f;

        [Header("Cabecera de Perfil (Vista Principal)")]
        [SerializeField] private Image imgAvatarPrincipal;
        [SerializeField] private CatalogoAvataresModel catalogoAvatares;
        [SerializeField] private TMP_Text txtNombrePrincipal;
        [SerializeField] private Button btnEditarPerfil;

        [Header("Modal de Edición de Perfil")]
        [SerializeField] private GameObject modalEditarPerfil;
        [SerializeField] private TMP_InputField inputNombreModal;
        [SerializeField] private Transform contenedorBotonesAvatarModal;
        [SerializeField] private Button btnGuardarModalPerfil;
        [SerializeField] private Button btnCancelarModalPerfil;
        [SerializeField] private Button btnCerrarModalPerfil;
        [SerializeField] private TMP_Text textoErrorModal;

        [Header("Audio")]
        [SerializeField] private Slider sliderVolumenGeneral;
        [SerializeField] private Toggle toggleAudioAutomatico;
        [SerializeField] private Toggle toggleVibracion;
        [SerializeField] private TMP_Text txtVersionApp;

        [Header("Zona de Peligro")]
        [SerializeField] private Button btnReiniciarProgreso;
        [SerializeField] private GameObject dialogoConfirmacionReinicio;
        [SerializeField] private Button btnConfirmarReinicio;
        [SerializeField] private Button btnCancelarReinicio;

        public event Action<string, string> OnGuardarPerfil;
        public event Action<float> OnCambiarVolumen;
        public event Action<bool> OnCambiarAudioAutomatico;
        public event Action<bool> OnCambiarVibracion;
        public event Action OnConfirmarReinicio;
        public event Action OnSolicitarActualizacion;

        private readonly List<Button> _botonesAvatarModal = new List<Button>();
        private string _nombreActual = string.Empty;
        private string _avatarActual = "avatar_01";
        private string _avatarModalSeleccionado = "avatar_01";
        private readonly UIEventSubscriptions _subscriptions = new UIEventSubscriptions();
        private readonly UIEventSubscriptions _avatarSubscriptions = new UIEventSubscriptions();

        public void SetVersion(string version)
        {
            if (txtVersionApp != null)
                txtVersionApp.text = $"Uchumataqu · Versión {version} · Sin conexión";
        }

        private void OnDestroy()
        {
            _subscriptions.Dispose();
            _avatarSubscriptions.Dispose();
        }

        private void Awake()
        {
            MobileInputFieldStyle.Aplicar(inputNombreModal);
            InicializarBotonesAvatarModal();

            if (btnEditarPerfil != null)
                _subscriptions.Add(btnEditarPerfil, AbrirModalEditarPerfil);

            // Botones del modal de edición
            if (btnGuardarModalPerfil != null)
                _subscriptions.Add(btnGuardarModalPerfil, GuardarPerfilDesdeModal);

            if (btnCancelarModalPerfil != null)
                _subscriptions.Add(btnCancelarModalPerfil, CerrarModalEditarPerfil);

            if (btnCerrarModalPerfil != null)
                _subscriptions.Add(btnCerrarModalPerfil, CerrarModalEditarPerfil);

            // Audio y Háptica
            if (sliderVolumenGeneral != null)
                _subscriptions.Add(sliderVolumenGeneral.onValueChanged, OnVolumenChanged);

            if (toggleAudioAutomatico != null)
            {
                _subscriptions.Add(toggleAudioAutomatico.onValueChanged, valor =>
                {
                    ActualizarVisualSwitch(toggleAudioAutomatico, valor);
                    OnCambiarAudioAutomatico?.Invoke(valor);
                });
            }

            if (toggleVibracion != null)
            {
                _subscriptions.Add(toggleVibracion.onValueChanged, valor =>
                {
                    ActualizarVisualSwitch(toggleVibracion, valor);
                    OnCambiarVibracion?.Invoke(valor);
                });
            }

            // Reinicio de progreso
            if (btnReiniciarProgreso != null)
                _subscriptions.Add(btnReiniciarProgreso, () => MostrarDialogoReinicio(true));

            if (btnCancelarReinicio != null)
                _subscriptions.Add(btnCancelarReinicio, () => MostrarDialogoReinicio(false));

            if (btnConfirmarReinicio != null)
                _subscriptions.Add(btnConfirmarReinicio, EjecutarReinicio);
        }

        private void OnEnable()
        {
            MostrarDialogoReinicio(false);
            CerrarModalEditarPerfil();
            OnSolicitarActualizacion?.Invoke();
        }

        public void Mostrar(
            string nombre,
            string avatarId,
            float volumen,
            bool audioAutomatico,
            bool vibracion)
        {
            _nombreActual = string.IsNullOrWhiteSpace(nombre) ? "Usuario" : nombre.Trim();
            _avatarActual = string.IsNullOrEmpty(avatarId) ? "avatar_01" : avatarId;

            if (txtNombrePrincipal != null)
                txtNombrePrincipal.text = _nombreActual;

            ActualizarAvatarPrincipal(_avatarActual);

            if (sliderVolumenGeneral != null)
                sliderVolumenGeneral.SetValueWithoutNotify(volumen);

            if (toggleAudioAutomatico != null)
            {
                toggleAudioAutomatico.SetIsOnWithoutNotify(audioAutomatico);
                ActualizarVisualSwitch(toggleAudioAutomatico, audioAutomatico);
            }

            if (toggleVibracion != null)
            {
                toggleVibracion.SetIsOnWithoutNotify(vibracion);
                ActualizarVisualSwitch(toggleVibracion, vibracion);
            }
        }

        public void MostrarErrorPerfil(string mensaje)
        {
            if (textoErrorModal != null)
            {
                textoErrorModal.text = mensaje ?? string.Empty;
                textoErrorModal.gameObject.SetActive(!string.IsNullOrWhiteSpace(mensaje));
            }
        }

        public void MostrarPerfilGuardado()
        {
            CerrarModalEditarPerfil();
        }

        private void AbrirModalEditarPerfil()
        {
            if (modalEditarPerfil != null)
                modalEditarPerfil.SetActive(true);

            if (inputNombreModal != null)
            {
                inputNombreModal.text = _nombreActual;
                MobileInputFieldStyle.Aplicar(inputNombreModal);
            }

            _avatarModalSeleccionado = _avatarActual;
            ActualizarSeleccionAvatarModal(_avatarModalSeleccionado);

            if (textoErrorModal != null)
                textoErrorModal.gameObject.SetActive(false);
        }

        private void CerrarModalEditarPerfil()
        {
            if (modalEditarPerfil != null)
                modalEditarPerfil.SetActive(false);

            if (textoErrorModal != null)
                textoErrorModal.gameObject.SetActive(false);
        }

        private void GuardarPerfilDesdeModal()
        {
            string nuevoNombre = inputNombreModal != null ? inputNombreModal.text?.Trim() : string.Empty;
            OnGuardarPerfil?.Invoke(nuevoNombre, _avatarModalSeleccionado);
        }

        private void SeleccionarAvatarModal(string avatarId)
        {
            if (string.IsNullOrEmpty(avatarId))
                avatarId = "avatar_01";

            _avatarModalSeleccionado = avatarId;
            ActualizarSeleccionAvatarModal(_avatarModalSeleccionado);
        }

        private void ActualizarSeleccionAvatarModal(string seleccionado)
        {
            foreach (Button boton in _botonesAvatarModal)
            {
                if (boton == null) continue;
                bool esSeleccionado = string.Equals(boton.name, seleccionado, StringComparison.OrdinalIgnoreCase);

                Transform selectorBorde = boton.transform.Find("Seleccionado") ?? boton.transform.Find("Seleccion");
                if (selectorBorde != null)
                {
                    selectorBorde.gameObject.SetActive(esSeleccionado);
                }
            }
        }

        private void ActualizarAvatarPrincipal(string avatarId)
        {
            if (imgAvatarPrincipal == null || catalogoAvatares == null) return;
            Sprite sprite = catalogoAvatares.ObtenerSprite(avatarId);
            if (sprite != null)
            {
                imgAvatarPrincipal.sprite = sprite;
                imgAvatarPrincipal.color = Color.white;
            }
        }

        private void InicializarBotonesAvatarModal()
        {
            _avatarSubscriptions.Dispose();
            _botonesAvatarModal.Clear();
            if (contenedorBotonesAvatarModal == null) return;

            // Busca los botones existentes en la grilla
            var botones = contenedorBotonesAvatarModal.GetComponentsInChildren<Button>(true);
            foreach (var btn in botones)
            {
                string avId = btn.name;
                _avatarSubscriptions.Add(btn, () => SeleccionarAvatarModal(avId));
                _botonesAvatarModal.Add(btn);

                if (catalogoAvatares != null)
                {
                    var img = btn.GetComponent<Image>() ?? btn.GetComponentInChildren<Image>(true);
                    if (img != null && img.sprite == null)
                    {
                        var sp = catalogoAvatares.ObtenerSprite(avId);
                        if (sp != null) img.sprite = sp;
                    }
                }
            }
        }

        private void OnVolumenChanged(float vol)
        {
            OnCambiarVolumen?.Invoke(vol);
        }

        private void ActualizarVisualSwitch(Toggle toggle, bool activo)
        {
            if (toggle == null) return;

            Transform switchBg = toggle.transform.Find("SwitchBg")
                ?? toggle.transform.Find("Switch/SwitchBg")
                ?? toggle.transform;
            Image bgImg = switchBg.GetComponent<Image>();
            if (bgImg != null)
            {
                UIThemeModel theme = UIThemeController.Theme;
                bgImg.color = theme != null
                    ? activo ? theme.colorPrimario : theme.colorDeshabilitado
                    : activo
                        ? new Color(0.85f, 0.47f, 0.04f, 1f)
                        : new Color(0.85f, 0.86f, 0.88f, 1f);
            }

            Transform knob = toggle.transform.Find("SwitchBg/Knob")
                ?? toggle.transform.Find("Switch/SwitchBg/Knob")
                ?? toggle.transform.Find("Knob");
            if (knob is RectTransform knobRect)
            {
                toggle.graphic = null;
                knob.gameObject.SetActive(true);

                Image knobImage = knob.GetComponent<Image>();
                if (knobImage != null)
                {
                    knobImage.color = Color.white;
                    knobImage.canvasRenderer.SetAlpha(1f);
                }

                float posicionInicial = MargenKnob;
                if (knobRect.parent is RectTransform fondoRect)
                    posicionInicial += knobRect.rect.width - fondoRect.rect.width;

                knobRect.anchoredPosition = new Vector2(
                    activo ? -MargenKnob : posicionInicial,
                    0f);
            }
        }

        private void MostrarDialogoReinicio(bool mostrar)
        {
            if (dialogoConfirmacionReinicio != null)
                dialogoConfirmacionReinicio.SetActive(mostrar);
        }

        private void EjecutarReinicio()
        {
            MostrarDialogoReinicio(false);
            OnConfirmarReinicio?.Invoke();
        }
    }
}
