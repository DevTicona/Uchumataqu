using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.RA
{
    /// <summary>Conserva la composición importada, incluidas piezas que no son objetivos pedagógicos.</summary>
    public sealed class EstadoInicialEscenarioRA : MonoBehaviour
    {
        private struct Pose
        {
            public Transform nodo, padre;
            public Vector3 posicion, escala;
            public Quaternion giro;
            public bool activo;
        }
        private readonly List<Pose> poses = new List<Pose>();
        private readonly Dictionary<Collider, bool> colliders = new Dictionary<Collider, bool>();
        private readonly Dictionary<SkinnedMeshRenderer, float[]> blendshapes = new Dictionary<SkinnedMeshRenderer, float[]>();
        private bool capturado;
        private void Awake() => Capturar();

        public void Capturar()
        {
            if (capturado) return;
            capturado = true;
            foreach (var nodo in GetComponentsInChildren<Transform>(true))
            {
                if (nodo == transform) continue;
                poses.Add(new Pose { nodo = nodo, padre = nodo.parent, posicion = nodo.localPosition,
                    giro = nodo.localRotation, escala = nodo.localScale, activo = nodo.gameObject.activeSelf });
            }
            foreach (var collider in GetComponentsInChildren<Collider>(true)) colliders.Add(collider, collider.enabled);
            foreach (var malla in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (malla.sharedMesh == null) continue;
                var pesos = new float[malla.sharedMesh.blendShapeCount];
                for (int i = 0; i < pesos.Length; i++) pesos[i] = malla.GetBlendShapeWeight(i);
                blendshapes.Add(malla, pesos);
            }
        }

        public void Reiniciar()
        {
            Capturar();
            foreach (var pose in poses)
            {
                if (pose.nodo == null) continue;
                pose.nodo.SetParent(pose.padre, false);
                pose.nodo.SetLocalPositionAndRotation(pose.posicion, pose.giro);
                pose.nodo.localScale = pose.escala;
                pose.nodo.gameObject.SetActive(pose.activo);
            }
            foreach (var estado in colliders) if (estado.Key != null) estado.Key.enabled = estado.Value;
            foreach (var estado in blendshapes)
                if (estado.Key != null)
                    for (int i = 0; i < estado.Value.Length; i++) estado.Key.SetBlendShapeWeight(i, estado.Value[i]);
        }
    }
}