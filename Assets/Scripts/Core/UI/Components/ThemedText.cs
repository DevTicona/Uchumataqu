using TMPro;
using UnityEngine;
using Assets.Scripts.Core.UI.Theme;

namespace Assets.Scripts.Core.UI.Components
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ThemedText : MonoBehaviour
    {
        public enum TipoTexto
        {
            TituloGrande,
            Titulo,
            Subtitulo,
            Normal,
            Pequeño
        }

        [Header("Configuración")]
        public TipoTexto tipoTexto = TipoTexto.Normal;

        [Header("Opcional")]
        public bool usarColorPersonalizado = false;
        public Color colorPersonalizado;

        private TextMeshProUGUI texto;
        private bool estaSuscrito = false;

        private void Awake()
        {
            texto = GetComponent<TextMeshProUGUI>();

            if (texto == null)
            {
                Debug.LogError($"[ThemedText] No se encontró TextMeshProUGUI en {gameObject.name}");
            }
        }

        private void OnEnable()
        {
            SuscribirseEvento();

            // Aplicar tema si ya existe
            if (UIThemeManager.Theme != null)
            {
                Debug.Log($"[ThemedText] Aplicando tema inicial en {gameObject.name}");
                AplicarTema();
            }
            else
            {
                Debug.LogWarning($"[ThemedText] Theme aún no cargado en {gameObject.name}");
            }
        }

        private void OnDisable()
        {
            DesuscribirseEvento();
        }

        private void SuscribirseEvento()
        {
            if (estaSuscrito) return;

            UIThemeManager.OnThemeChanged += AplicarTema;
            estaSuscrito = true;
        }

        private void DesuscribirseEvento()
        {
            if (!estaSuscrito) return;

            UIThemeManager.OnThemeChanged -= AplicarTema;
            estaSuscrito = false;
        }

        private void AplicarTema()
        {
            if (texto == null)
            {
                Debug.LogError($"[ThemedText] Texto es null en {gameObject.name}");
                return;
            }

            var theme = UIThemeManager.Theme;

            if (theme == null)
            {
                Debug.LogWarning($"[ThemedText] Theme es null en {gameObject.name}");
                return;
            }

            // =========================
            // FUENTE (reactivada segura)
            // =========================
            if (theme.fuentePrincipal != null)
            {
                texto.font = theme.fuentePrincipal;
                Debug.Log($"[ThemedText] Fuente aplicada en {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"[ThemedText] Fuente del tema es null en {gameObject.name}");
            }

            // =========================
            // COLOR
            // =========================
            texto.color = usarColorPersonalizado
                ? colorPersonalizado
                : theme.colorTexto;

            // =========================
            // TAMAÑO
            // =========================
            switch (tipoTexto)
            {
                case TipoTexto.TituloGrande:
                    texto.fontSize = theme.tituloGrande;
                    break;

                case TipoTexto.Titulo:
                    texto.fontSize = theme.titulo;
                    break;

                case TipoTexto.Subtitulo:
                    texto.fontSize = theme.subtitulo;
                    break;

                case TipoTexto.Normal:
                    texto.fontSize = theme.textoNormal;
                    break;

                case TipoTexto.Pequeño:
                    texto.fontSize = theme.textoPequeno;
                    break;

                default:
                    Debug.LogWarning($"[ThemedText] TipoTexto no reconocido en {gameObject.name}");
                    break;
            }

            // =========================
            // FORZAR ACTUALIZACIÓN
            // =========================
            texto.ForceMeshUpdate();

            Debug.Log($"[ThemedText] Tema aplicado correctamente en {gameObject.name}");
        }
    }
}