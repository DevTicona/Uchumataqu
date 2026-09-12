using System;
using System.Collections.Generic;
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
    /// Especificado en Docs/UI/08-perfil-logros-ajustes-acerca.md §4.
    /// </summary>
    public class AjustesView : MonoBehaviour
    {
        private const float MargenKnob = 5f;

        [Header("Cabecera de Perfil (Vista Principal)")]
        [SerializeField] private Image imgAvatarPrincipal;
        [SerializeField] private CatalogoAvatares catalogoAvatares;
        [SerializeField] private TMP_Text txtNombrePrincipal;
        [SerializeField] private Button btnEditarPerfilBadge;
        [SerializeField] private Button btnEditarPerfilTexto;

        [Header("Modal de Edición de Perfil")]
        [SerializeField] private GameObject modalEditarPerfil;
        [SerializeField] private TMP_InputField inputNombreModal;
        [SerializeField] private Transform contenedorBotonesAvatarModal;
        [SerializeField] private Button btnGuardarModalPerfil;
        [SerializeField] private Button btnCancelarModalPerfil;
        [SerializeField] private Button btnCerrarModalPerfil;
        [SerializeField] private TMP_Text textoErrorModal;

        [Header("Scroll y Contenido")]
        [SerializeField] private GameObject scrollAjustes;

        [Header("Audio")]
        [SerializeField] private Slider sliderVolumenGeneral;
        [SerializeField] private Toggle toggleAudioAutomatico;
        [SerializeField] private Toggle toggleVibracion;

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

        private void Awake()
        {
            InicializarBotonesAvatarModal();

            // Abrir modal al presionar badge o texto de editar
            if (btnEditarPerfilBadge != null)
                btnEditarPerfilBadge.onClick.AddListener(AbrirModalEditarPerfil);

            if (btnEditarPerfilTexto != null)
                btnEditarPerfilTexto.onClick.AddListener(AbrirModalEditarPerfil);

            // Botones del modal de edición
            if (btnGuardarModalPerfil != null)
                btnGuardarModalPerfil.onClick.AddListener(GuardarPerfilDesdeModal);

            if (btnCancelarModalPerfil != null)
                btnCancelarModalPerfil.onClick.AddListener(CerrarModalEditarPerfil);

            if (btnCerrarModalPerfil != null)
                btnCerrarModalPerfil.onClick.AddListener(CerrarModalEditarPerfil);

            // Audio y Háptica
            if (sliderVolumenGeneral != null)
                sliderVolumenGeneral.onValueChanged.AddListener(OnVolumenChanged);

            if (toggleAudioAutomatico != null)
            {
                toggleAudioAutomatico.onValueChanged.AddListener(valor =>
                {
                    ActualizarVisualSwitch(toggleAudioAutomatico, valor);
                    OnCambiarAudioAutomatico?.Invoke(valor);
                });
            }

            if (toggleVibracion != null)
            {
                toggleVibracion.onValueChanged.AddListener(valor =>
                {
                    ActualizarVisualSwitch(toggleVibracion, valor);
                    OnCambiarVibracion?.Invoke(valor);
                });
            }

            // Reinicio de progreso
            if (btnReiniciarProgreso != null)
                btnReiniciarProgreso.onClick.AddListener(() => MostrarDialogoReinicio(true));

            if (btnCancelarReinicio != null)
                btnCancelarReinicio.onClick.AddListener(() => MostrarDialogoReinicio(false));

            if (btnConfirmarReinicio != null)
                btnConfirmarReinicio.onClick.AddListener(EjecutarReinicio);
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
            if (scrollAjustes != null)
                scrollAjustes.SetActive(false);

            if (modalEditarPerfil != null)
                modalEditarPerfil.SetActive(true);

            if (inputNombreModal != null)
                inputNombreModal.text = _nombreActual;

            _avatarModalSeleccionado = _avatarActual;
            ActualizarSeleccionAvatarModal(_avatarModalSeleccionado);

            if (textoErrorModal != null)
                textoErrorModal.gameObject.SetActive(false);
        }

        private void CerrarModalEditarPerfil()
        {
            if (modalEditarPerfil != null)
                modalEditarPerfil.SetActive(false);

            if (scrollAjustes != null)
                scrollAjustes.SetActive(true);

            if (textoErrorModal != null)
                textoErrorModal.gameObject.SetActive(false);
        }

        private void GuardarPerfilDesdeModal()
        {
            string nuevoNombre = inputNombreModal != null ? inputNombreModal.text?.Trim() : string.Empty;

            if (string.IsNullOrEmpty(nuevoNombre) || nuevoNombre.Length < 2)
            {
                MostrarErrorPerfil("El nombre debe tener al menos 2 caracteres.");
                return;
            }

            _nombreActual = nuevoNombre;
            _avatarActual = _avatarModalSeleccionado;

            if (txtNombrePrincipal != null)
                txtNombrePrincipal.text = _nombreActual;

            ActualizarAvatarPrincipal(_avatarActual);

            CerrarModalEditarPerfil();
            OnGuardarPerfil?.Invoke(_nombreActual, _avatarActual);
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
            _botonesAvatarModal.Clear();
            if (contenedorBotonesAvatarModal == null) return;

            // Busca los botones existentes en la grilla
            var botones = contenedorBotonesAvatarModal.GetComponentsInChildren<Button>(true);
            foreach (var btn in botones)
            {
                string avId = btn.name;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SeleccionarAvatarModal(avId));
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
                UITheme theme = UIThemeManager.Theme;
                bgImg.color = theme != null
                    ? activo ? theme.colorPrimario : theme.colorBorde
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
