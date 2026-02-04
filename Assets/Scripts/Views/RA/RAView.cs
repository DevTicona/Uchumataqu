using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Assets.Scripts.Presenters.RA;

namespace Assets.Scripts.Views.RA
{
    /// <summary>
    /// Vista de Realidad Aumentada.
    /// Detecta el piso y delega la creación al Presenter.
    /// </summary>
    public class RAView : MonoBehaviour
    {
        [Header("Referencias AR")]
        [SerializeField] private ARRaycastManager raycastManager;

        private RAPresenter presenter;
        private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

        private void Awake()
        {
            presenter = new RAPresenter();
        }

        private void Update()
        {
            if (Input.touchCount == 0)
                return;

            Touch toque = Input.GetTouch(0);

            if (toque.phase != TouchPhase.Began)
                return;

            IntentarColocarPersonaje(toque.position);
        }

        private void IntentarColocarPersonaje(Vector2 posicionPantalla)
        {
            if (!raycastManager.Raycast(
                posicionPantalla,
                hits,
                TrackableType.PlaneWithinPolygon))
                return;

            Pose pose = hits[0].pose;

            presenter.ColocarPersonaje(
                pose.position,
                pose.rotation
            );
        }
    }
}
