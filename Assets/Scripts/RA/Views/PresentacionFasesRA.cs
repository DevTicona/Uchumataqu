using System;
using UnityEngine;

namespace Assets.Scripts.RA
{
    [Serializable]
    public class GrupoFaseRA
    {
        public GameObject contenido;
        public string[] fasesVisibles;
    }

    public sealed class PresentacionFasesRA : MonoBehaviour
    {
        [SerializeField] private string faseInicial;
        [SerializeField] private GrupoFaseRA[] grupos = Array.Empty<GrupoFaseRA>();

        private void Awake() => MostrarFase(faseInicial);

        public void Configurar(string inicial, GrupoFaseRA[] configuracion)
        {
            faseInicial = inicial;
            grupos = configuracion;
            MostrarFase(inicial);
        }

        public void MostrarFase(string fase)
        {
            foreach (var grupo in grupos)
                if (grupo.contenido != null)
                    grupo.contenido.SetActive(Array.IndexOf(grupo.fasesVisibles, fase) >= 0);
        }
    }
}
