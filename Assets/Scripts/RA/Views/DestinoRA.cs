using UnityEngine;

namespace Assets.Scripts.RA
{
    public sealed class DestinoRA : MonoBehaviour
    {
        [SerializeField] private string destinoId;
        public string DestinoId => destinoId;
        public void Configurar(string id) => destinoId = id;
    }
}
