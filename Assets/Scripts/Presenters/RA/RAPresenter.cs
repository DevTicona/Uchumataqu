using UnityEngine;
using Assets.Scripts.Services.RA;

namespace Assets.Scripts.Presenters.RA
{
    /// <summary>
    /// Presenter de la actividad RA.
    /// </summary>
    public class RAPresenter
    {
        private readonly RAService raService;

        public RAPresenter()
        {
            raService = new RAService();
        }

        public void ColocarPersonaje(Vector3 posicion, Quaternion rotacion)
        {
            raService.InstanciarPersonajeBase(posicion, rotacion);
        }
    }
}
