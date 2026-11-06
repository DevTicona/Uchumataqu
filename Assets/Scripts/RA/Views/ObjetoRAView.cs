using System.Collections;
using System.Collections.Generic;
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
        public enum EstadoSeleccion { Neutro, Correcto, Incorrecto }
        [SerializeField] private bool esPersonaje;
        [SerializeField, HideInInspector] private float escalaVisualAplicada = 1f;
        public bool EsPersonaje => esPersonaje;
        public EstadoSeleccion EstadoActual { get; private set; }
        [SerializeField] private string palabraId;
        [SerializeField] private string objetoId;
        [SerializeField] private Transform[] anclasCard = new Transform[0];
        private Transform anclaSeleccionada;
        public IEnumerable<Transform> AnclasCard => anclasCard.Length > 0
            ? anclasCard : new[] { transform.Find("AnclaDialogo") ?? transform };
        public Transform AnclaDialogo
        {
            get
            {
                if (anclaSeleccionada != null) return anclaSeleccionada;
                foreach (var ancla in anclasCard)
                    if (ancla != null) return ancla;
                return transform.Find("AnclaDialogo") ?? transform;
            }
        }
        [SerializeField] private Transform[] piezasApertura = new Transform[0];
        [SerializeField] private Vector3 anguloApertura = new Vector3(0f, -65f, 0f);
        private Quaternion[] girosIniciales;
        private Vector3 escalaInicial;
        private Vector3 posicionInicial;
        private Quaternion giroInicial;
        private bool estadoInicialCapturado;
        private Coroutine pulsoResaltado;
        private Vector3 escalaAntesResaltado;
        private readonly Dictionary<Renderer, MaterialPropertyBlock> materialesAntesResaltado = new Dictionary<Renderer, MaterialPropertyBlock>();
        public bool EstaResaltado => pulsoResaltado != null;
        public string ObjetoId => objetoId;
        public string PalabraId => palabraId;

        private void Awake() => CapturarEstadoInicial();

        private void CapturarEstadoInicial()
        {
            if (estadoInicialCapturado) return;
            estadoInicialCapturado = true;
            escalaInicial = transform.localScale;
            posicionInicial = transform.localPosition;
            giroInicial = transform.localRotation;
            girosIniciales = new Quaternion[piezasApertura.Length];
            for (int i = 0; i < piezasApertura.Length; i++)
                if (piezasApertura[i] != null) girosIniciales[i] = piezasApertura[i].localRotation;
        }

        public void ReiniciarVisual()
        {
            anclaSeleccionada = null;
            CapturarEstadoInicial();
            LimpiarMarca();
            StopAllCoroutines();
            transform.localScale = escalaInicial;
            transform.localPosition = posicionInicial;
            transform.localRotation = giroInicial;
            for (int i = 0; i < piezasApertura.Length; i++)
                if (piezasApertura[i] != null) piezasApertura[i].localRotation = girosIniciales[i];
            GetComponent<PrendaRAView>()?.Reiniciar();
            LimpiarMarca();
        }

        public void Configurar(string id, string palabra)
        {
            objetoId = id;
            palabraId = palabra;
            CapturarEstadoInicial();
        }

        public void SeleccionarAncla(Vector3 punto)
        {
            anclaSeleccionada = null;
            float distancia = float.PositiveInfinity;
            foreach (var ancla in AnclasCard)
            {
                if (ancla == null) continue;
                float candidata = (ancla.position - punto).sqrMagnitude;
                if (candidata >= distancia) continue;
                distancia = candidata;
                anclaSeleccionada = ancla;
            }
        }

        public void Resaltar()
        {
            if (EstaResaltado) return;
            StopAllCoroutines();
            transform.localScale = escalaInicial;
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
            EstadoActual = EstadoSeleccion.Neutro;
            EstablecerResaltado(false);
            foreach (var renderizador in GetComponentsInChildren<Renderer>(true))
                renderizador.SetPropertyBlock(null);
        }

        public void MostrarEstadoSeleccion(EstadoSeleccion estado)
        {
            LimpiarMarca();
            EstadoActual = estado;
            if (estado == EstadoSeleccion.Neutro) return;
            Color color = estado == EstadoSeleccion.Correcto ? new Color(0.18f, 0.72f, 0.36f) : new Color(0.95f, 0.14f, 0.12f);
            foreach (var renderer in GetComponentsInChildren<Renderer>(true))
            {
                var propiedades = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propiedades);
                if (renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_BaseColor"))
                    propiedades.SetColor("_BaseColor", color);
                if (renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_Color"))
                    propiedades.SetColor("_Color", color);
                renderer.SetPropertyBlock(propiedades);
            }
        }

        public void EstablecerResaltado(bool activo)
        {
            if (activo == EstaResaltado) return;
            if (!activo)
            {
                StopCoroutine(pulsoResaltado);
                pulsoResaltado = null;
                transform.localScale = escalaAntesResaltado;
                foreach (var material in materialesAntesResaltado)
                    if (material.Key != null) material.Key.SetPropertyBlock(material.Value);
                materialesAntesResaltado.Clear();
                return;
            }
            escalaAntesResaltado = transform.localScale;
            foreach (var renderizador in GetComponentsInChildren<Renderer>(true))
            {
                var material = renderizador.sharedMaterial;
                if (material == null) continue;
                var anteriores = new MaterialPropertyBlock();
                renderizador.GetPropertyBlock(anteriores);
                materialesAntesResaltado[renderizador] = anteriores;
                var marca = new MaterialPropertyBlock();
                renderizador.GetPropertyBlock(marca);
                Color dorado = new Color(1f, 0.72f, 0.08f, 1f);
                if (material.HasProperty("_BaseColor")) marca.SetColor("_BaseColor", dorado);
                if (material.HasProperty("_Color")) marca.SetColor("_Color", dorado);
                renderizador.SetPropertyBlock(marca);
            }
            pulsoResaltado = StartCoroutine(MantenerResaltado());
        }

        private IEnumerator MantenerResaltado()
        {
            while (true)
            {
                float pulso = 1.12f + 0.06f * Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f);
                transform.localScale = escalaAntesResaltado * pulso;
                yield return null;
            }
        }

        private void OnDisable() => EstablecerResaltado(false);

        public void Reaccionar(string reaccionId)
        {
            if (string.IsNullOrEmpty(reaccionId)) return;
            EstablecerResaltado(false);
            StopAllCoroutines();
            transform.localScale = escalaInicial;
            if (reaccionId == "puerta_abrir" || reaccionId == "ventana_abrir" || reaccionId == "mochila_abrir")
            {
                if (piezasApertura.Length > 0) StartCoroutine(AnimarApertura(true));
                else StartCoroutine(AnimarPulso(1.06f, 0.45f));
            }
            else if (reaccionId == "mochila_lista" && piezasApertura.Length > 0)
                StartCoroutine(AnimarApertura(false));
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

        private IEnumerator AnimarApertura(bool abrir)
        {
            var origenes = new Quaternion[piezasApertura.Length];
            for (int i = 0; i < piezasApertura.Length; i++)
                if (piezasApertura[i] != null) origenes[i] = piezasApertura[i].localRotation;
            float tiempo = 0f;
            while (tiempo < 0.45f)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(tiempo / 0.45f));
                for (int i = 0; i < piezasApertura.Length; i++)
                    if (piezasApertura[i] != null)
                        piezasApertura[i].localRotation = Quaternion.Slerp(origenes[i],
                            girosIniciales[i] * Quaternion.Euler(abrir ? anguloApertura : Vector3.zero), t);
                yield return null;
            }
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
