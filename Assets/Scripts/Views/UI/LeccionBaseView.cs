using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Services;

namespace Assets.Scripts.Views.UI
{
    public class LeccionBaseView : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI txtTitulo;
        [SerializeField] private Image iconoLeccion;
        [SerializeField] private Button btnAtras;

        [Header("Indicaciones")]
        [SerializeField] private TextMeshProUGUI txtIndicacion;

        [Header("Contenedor de botones")]
        [SerializeField] private Transform contenedorBotones;

        private readonly List<BotonView> botonesActivos = new();

        // ======================
        // Visual
        // ======================
        public void SetTitulo(string titulo)
        {
            if (txtTitulo != null)
                txtTitulo.text = titulo;
        }

        public void SetIcono(Sprite icono)
        {
            if (iconoLeccion != null)
                iconoLeccion.sprite = icono;
        }

        public void SetTextoIndicacion(string texto)
        {
            if (txtIndicacion != null)
                txtIndicacion.text = texto;
        }

        // ======================
        // Botones dinámicos
        // ======================
        public void CrearBoton(
            string texto,
            Color colorFondo,
            Color colorTexto,
            Action onClick
        )
        {
            if (contenedorBotones == null)
            {
                Debug.LogError("[LeccionBaseView] Contenedor de botones no asignado.");
                return;
            }

            StartCoroutine(CrearBotonAsync(texto, colorFondo, colorTexto, onClick));
        }

        private IEnumerator CrearBotonAsync(
            string texto,
            Color colorFondo,
            Color colorTexto,
            Action onClick
        )
        {
            bool instanciaCompletada = false;
            BotonView boton = null;

            LeccionBaseService.InstanciarBoton(
                contenedorBotones,
                gameObject =>
                {
                    boton = gameObject.GetComponent<BotonView>();
                    instanciaCompletada = true;
                },
                () =>
                {
                    Debug.LogError("[LeccionBaseView] No se pudo instanciar botón.");
                    instanciaCompletada = true;
                }
            );

            yield return new WaitUntil(() => instanciaCompletada);

            if (boton == null)
                yield break;

            boton.Configurar(texto, colorFondo, colorTexto, onClick);
            botonesActivos.Add(boton);
        }

        public void LimpiarBotones()
        {
            foreach (var boton in botonesActivos)
            {
                if (boton != null)
                    Destroy(boton.gameObject);
            }

            botonesActivos.Clear();
        }

        // ======================
        // Header
        // ======================
        public void BindBotonAtras(Action onAtras)
        {
            if (btnAtras == null)
                return;

            btnAtras.onClick.RemoveAllListeners();
            btnAtras.onClick.AddListener(() => onAtras?.Invoke());
        }
    }}