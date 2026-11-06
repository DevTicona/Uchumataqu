using UnityEngine;

namespace Assets.Scripts.RA
{
    public sealed class ActorNarrativoRA : MonoBehaviour
    {
        [SerializeField] private string objetoId;
        [SerializeField] private Transform anclaDialogo;
        public string ObjetoId => objetoId;
        public Transform AnclaDialogo => anclaDialogo != null ? anclaDialogo : transform.Find("AnclaDialogo");
        public void Configurar(string id) => objetoId = id;
    }
}
