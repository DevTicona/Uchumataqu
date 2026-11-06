using System.Collections;

namespace Assets.Scripts.RA
{
    public interface IAccionEscenarioRA
    {
        bool Contiene(string reaccion);
        bool SustituyeColocacion(string objetoId);
        IEnumerator Ejecutar(string reaccion);
        void Reiniciar();
    }
}
