using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Models;
using Assets.Scripts.Services;
using Assets.Scripts.Views.UI;

namespace Assets.Scripts.Presenters
{
    public class PracticaPresenter : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private LeccionBaseView view;
        [SerializeField] private CarouselView carousel;

        private List<PalabraData> palabras;
        private int indicePalabra;

        public event Action AlFinalizarPractica;

        // ===============================
        // Colores de UI (definidos por el Presenter)
        // ===============================
        private static readonly Color COLOR_NAV_FONDO =
            new Color32(0x3B, 0x6E, 0xF5, 0xFF); // #3B6EF5

        private static readonly Color COLOR_NAV_TEXTO = Color.white;

        private static readonly Color COLOR_SECUNDARIO_FONDO =
            new Color32(0xB0, 0xB0, 0xB0, 0xFF); // gris claro

        private static readonly Color COLOR_SECUNDARIO_TEXTO = Color.black;

        // ===============================
        // Inicio de práctica
        // ===============================
        public void IniciarPractica(string idLeccion)
        {
            PalabrasService.ObtenerPalabrasPorLeccion(idLeccion, lista =>
            {
                if (lista == null || lista.Count == 0)
                {
                    AlFinalizarPractica?.Invoke();
                    return;
                }

                palabras = lista;
                indicePalabra = 0;

                IniciarModoPractica();
            });
        }

        private void IniciarModoPractica()
        {
            carousel.gameObject.SetActive(true);
            carousel.Mostrar(palabras[indicePalabra], true);

            view.SetTextoIndicacion("Escucha la palabra y luego repítela");

            ActualizarBotonesPractica();
        }

        // ===============================
        // Navegación normal
        // ===============================
        private void Siguiente()
        {
            if (indicePalabra < palabras.Count - 1)
            {
                indicePalabra++;
                carousel.Mostrar(palabras[indicePalabra], true);
                ActualizarBotonesPractica();
            }
            else
            {
                FinalizarPractica();
            }
        }

        private void Anterior()
        {
            if (indicePalabra <= 0)
                return;

            indicePalabra--;
            carousel.Mostrar(palabras[indicePalabra], false);
            ActualizarBotonesPractica();
        }

        // ===============================
        // Finalización
        // ===============================
        private void FinalizarPractica()
        {
            carousel.gameObject.SetActive(false);

            view.SetTextoIndicacion(
                "¡Felicitaciones!\nTerminaste la práctica de pronunciación."
            );

            view.LimpiarBotones();

            view.CrearBoton(
                "Volver a repasar",
                COLOR_SECUNDARIO_FONDO,
                COLOR_SECUNDARIO_TEXTO,
                ReiniciarPractica
            );

            view.CrearBoton(
                "Continuar con actividades",
                COLOR_NAV_FONDO,
                COLOR_NAV_TEXTO,
                () => AlFinalizarPractica?.Invoke()
            );
        }

        // ===============================
        // Reinicio de práctica
        // ===============================
        private void ReiniciarPractica()
        {
            indicePalabra = 0;
            IniciarModoPractica();
        }

        // ===============================
        // UI dinámica
        // ===============================
        private void ActualizarBotonesPractica()
        {
            view.LimpiarBotones();

            if (indicePalabra > 0)
            {
                view.CrearBoton(
                    "Anterior",
                    COLOR_NAV_FONDO,
                    COLOR_NAV_TEXTO,
                    Anterior
                );
            }

            view.CrearBoton(
                "Siguiente",
                COLOR_NAV_FONDO,
                COLOR_NAV_TEXTO,
                Siguiente
            );
        }

        // ===============================
        // Datos
        // ===============================
        public List<PalabraData> ObtenerPalabras()
        {
            return palabras;
        }
    }
}
