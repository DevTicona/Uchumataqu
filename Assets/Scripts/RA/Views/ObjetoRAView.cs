using System.Collections;
using UnityEngine;

namespace Assets.Scripts.RA
{
    /// <summary>
    /// Componente interactivo asignado a cada modelo 3D en la escena de Realidad Aumentada.
    /// Identifica el objeto del prefab y presenta reacciones visuales simples.
    /// Especificado en Docs/Specifications/03-modulo-ra.md (Sección 6).
    /// </summary>
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "Assets.Scripts.RA", sourceAssembly: "Assembly-CSharp", sourceClassName: "InteractorObjetoRA")]
    public class ObjetoRAView : MonoBehaviour
    {
        [SerializeField] private string palabraId;
        [SerializeField] private string objetoId;
        public string ObjetoId => objetoId;
        public string PalabraId => palabraId;

        public void Configurar(string id, string palabra)
        {
            objetoId = id;
            palabraId = palabra;
        }

        public void Resaltar()
        {
            StartCoroutine(AnimarPulso(1.08f, 0.35f));
        }

        public void MarcarDescubierto()
        {
            foreach (var renderizador in GetComponentsInChildren<Renderer>(true))
            {
                if (!renderizador.enabled || renderizador.sharedMaterial == null ||
                    !renderizador.sharedMaterial.HasProperty("_BaseColor")) continue;

                var propiedades = new MaterialPropertyBlock();
                renderizador.GetPropertyBlock(propiedades);
                Color colorBase = renderizador.sharedMaterial.GetColor("_BaseColor");
                propiedades.SetColor("_BaseColor", Color.Lerp(colorBase, Color.white, 0.28f));
                renderizador.SetPropertyBlock(propiedades);
            }
            Resaltar();
        }

        public void LimpiarMarca()
        {
            foreach (var renderizador in GetComponentsInChildren<Renderer>(true))
                renderizador.SetPropertyBlock(null);
        }

        public void Reaccionar(string reaccionId)
        {
            if (string.IsNullOrEmpty(reaccionId)) return;
            if (reaccionId == "ventana_abrir" || reaccionId == "mochila_abrir")
                StartCoroutine(AnimarGiro(28f));
            else if (reaccionId == "silla_giro_suave")
                StartCoroutine(AnimarGiro(15f));
            else if (reaccionId == "almohada_rebotar")
                StartCoroutine(AnimarRebote());
            else if (reaccionId == "lapiz_escribir_linea")
                DibujarLinea();
            else
                StartCoroutine(AnimarPulso(1.12f, 0.45f));
        }

        private void DibujarLinea()
        {
            var trazo = new GameObject("TrazoLapiz");
            var linea = trazo.AddComponent<LineRenderer>();
            linea.useWorldSpace = true;
            linea.positionCount = 2;
            linea.startWidth = 0.004f;
            linea.endWidth = 0.004f;
            linea.startColor = new Color(0.4f, 0.2f, 0.1f);
            linea.endColor = linea.startColor;
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null)
            {
                var material = new Material(shader);
                material.SetColor("_BaseColor", linea.startColor);
                linea.material = material;
                Destroy(material, 1.6f);
            }
            linea.SetPosition(0, transform.position);
            linea.SetPosition(1, transform.position + transform.right * 0.11f);
            Destroy(trazo, 1.5f);
        }

        private IEnumerator AnimarPulso(float factor, float duracion)
        {
            Vector3 inicial = transform.localScale;
            float tiempo = 0f;
            while (tiempo < duracion)
            {
                tiempo += Time.deltaTime;
                float onda = Mathf.Sin(Mathf.PI * Mathf.Clamp01(tiempo / duracion));
                transform.localScale = inicial * (1f + (factor - 1f) * onda);
                yield return null;
            }
            transform.localScale = inicial;
        }

        private IEnumerator AnimarGiro(float grados)
        {
            Quaternion inicial = transform.localRotation;
            Quaternion final = inicial * Quaternion.Euler(0f, grados, 0f);
            float tiempo = 0f;
            while (tiempo < 0.45f)
            {
                tiempo += Time.deltaTime;
                transform.localRotation = Quaternion.Slerp(inicial, final, Mathf.Clamp01(tiempo / 0.45f));
                yield return null;
            }
        }

        private IEnumerator AnimarRebote()
        {
            Vector3 inicial = transform.localPosition;
            float tiempo = 0f;
            while (tiempo < 0.55f)
            {
                tiempo += Time.deltaTime;
                transform.localPosition = inicial + Vector3.up * (0.08f * Mathf.Sin(Mathf.PI * Mathf.Clamp01(tiempo / 0.55f)));
                yield return null;
            }
            transform.localPosition = inicial;
        }

    }
}
