using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.UI.Main;

namespace Assets.Scripts.UI.Main.Paginas
{
    /// <summary>
    /// Vista de la pestaña de Inicio en la escena 02_Main.
    /// Especificado en Docs/UI/03-escena-main.md §4.
    /// </summary>
    public class InicioView : MonoBehaviour
    {
        [Header("Encabezado de Perfil")]
        [SerializeField] private TMP_Text txtSaludoUsuario;
        [SerializeField] private Image imgAvatarUsuario;

        [Header("Tarjeta Continuar")]
        [SerializeField] private TMP_Text txtTituloContinuar;
        [SerializeField] private TMP_Text txtSubtituloContinuar;
        [SerializeField] private Button btnContinuarLeccion;

        [Header("Resumen Rápido")]
        [SerializeField] private TMP_Text txtContadorLecciones;
        [SerializeField] private TMP_Text txtContadorRA;

        private string _leccionSiguienteId = "L01";
        private string _avatarSolicitado;

        public event Action<string> OnContinuarLeccion;
        public event Action OnSolicitarActualizacion;

        private void Awake()
        {
            if (btnContinuarLeccion != null)
            {
                btnContinuarLeccion.onClick.AddListener(OnContinuarLeccionClicked);
            }
        }

        private void OnEnable()
        {
            OnSolicitarActualizacion?.Invoke();
        }

        public void Mostrar(InicioPresentacionData datos)
        {
            if (datos == null) return;
            string nombre = string.IsNullOrEmpty(datos.Nombre) ? "Estudiante" : datos.Nombre;
            _ = MostrarAvatarAsync(datos.AvatarId);

            if (txtSaludoUsuario != null)
            {
                txtSaludoUsuario.text = $"¡Hola, {nombre}!";
            }

            if (txtContadorLecciones != null)
                txtContadorLecciones.text = $"{datos.LeccionesCompletadas} / 27";

            if (txtContadorRA != null)
            {
                string proximoRa = string.IsNullOrWhiteSpace(datos.ProximoRaTitulo)
                    ? "Todos los escenarios visitados"
                    : $"Próximo: {datos.ProximoRaTitulo} ({datos.ProximoRaCompletadas}/{datos.ProximoRaTotal})";
                txtContadorRA.text = $"{datos.EscenariosVisitados} / 6\n{proximoRa}";
            }

            _leccionSiguienteId = datos.LeccionId;

            if (txtTituloContinuar != null)
            {
                string accion = datos.CurriculoCompletado
                    ? "Repasar"
                    : datos.EsContinuacion ? "Continuar" : "Comenzar";
                txtTituloContinuar.text = $"{accion}: {datos.LeccionId}";
            }

            if (txtSubtituloContinuar != null)
                txtSubtituloContinuar.text = datos.TituloLeccion;

            if (btnContinuarLeccion != null)
            {
                btnContinuarLeccion.interactable = !string.IsNullOrWhiteSpace(datos.LeccionId);
                TMP_Text etiquetaBoton = btnContinuarLeccion.GetComponentInChildren<TMP_Text>(true);
                if (etiquetaBoton != null)
                {
                    etiquetaBoton.text = datos.CurriculoCompletado
                        ? "REPASAR LECCION 27"
                        : datos.EsContinuacion ? "CONTINUAR" : "COMENZAR";
                }
            }
        }

        private void OnContinuarLeccionClicked()
        {
            OnContinuarLeccion?.Invoke(_leccionSiguienteId);
        }

        private async Task MostrarAvatarAsync(string avatarId)
        {
            if (imgAvatarUsuario == null || string.IsNullOrWhiteSpace(avatarId)) return;
            _avatarSolicitado = avatarId;

            try
            {
                Sprite sprite = await AddressablesService.CargarAssetAsync<Sprite>(ContentAddress.Avatar(avatarId));
                if (imgAvatarUsuario == null || _avatarSolicitado != avatarId) return;
                imgAvatarUsuario.sprite = sprite;
                imgAvatarUsuario.color = Color.white;
                imgAvatarUsuario.preserveAspect = true;
                imgAvatarUsuario.enabled = true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[InicioView] No se pudo cargar {avatarId}: {ex.Message}");
            }
        }
    }
}
