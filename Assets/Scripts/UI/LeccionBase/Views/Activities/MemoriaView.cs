using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Core.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Core.UI.Components;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    public sealed class MemoriaView : MonoBehaviour
    {
        [SerializeField] private Transform contenedorCartas;
        [SerializeField] private GameObject prefabTarjeta;
        public event Action<string, string> OnParPropuesto;
        private Carta _segunda;

        public event Action<bool> OnMemoriaCompletada;

        private readonly List<Carta> _cartas = new List<Carta>();
        private Carta _primera;
        private bool _comparando;
        private bool _ocultandoIncorrectas;
        private readonly UIEventSubscriptions _subscriptions = new UIEventSubscriptions();

        private void OnDestroy() => _subscriptions.Dispose();
        private int _paresResueltos;

        private void OnDisable()
        {
            StopAllCoroutines();
            CancelInvoke();
            if (_primera != null && !_primera.Resuelta) _primera.Ocultar();
            if (_segunda != null && !_segunda.Resuelta) _segunda.Ocultar();
            _primera = null;
            _segunda = null;
            _comparando = false;
            _ocultandoIncorrectas = false;
        }

        public void Configurar(IReadOnlyDictionary<string, string> pares)
        {
            _subscriptions.Dispose();
            StopAllCoroutines();
            foreach (Carta carta in _cartas)
                if (carta.Boton != null)
                {
                    carta.Boton.gameObject.SetActive(false);
                    RuntimeActividadUIFactory.Retirar(carta.Boton.gameObject);
                }
            _cartas.Clear();
            _primera = null;
            _segunda = null;
            _comparando = false;
            _ocultandoIncorrectas = false;
            _paresResueltos = 0;

            var datos = new List<(string texto, string pareja)>();
            foreach (var par in pares)
            {
                datos.Add((par.Key, par.Value));
                datos.Add((par.Value, par.Key));
            }
            Barajar(datos);

            foreach (var dato in datos)
            {
                Transform padre = contenedorCartas != null ? contenedorCartas : transform;
                Button boton = Instantiate(prefabTarjeta, padre, false).GetComponent<Button>();
                var carta = new Carta(boton, dato.texto, dato.pareja);
                _subscriptions.Add(boton, () => Seleccionar(carta));
                _cartas.Add(carta);
            }

            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        private void Seleccionar(Carta carta)
        {
            if (_comparando || carta.Resuelta || carta == _primera) return;
            carta.Mostrar();
            if (_primera == null)
            {
                _primera = carta;
                return;
            }

            _comparando = true;
            _segunda = carta;
            OnParPropuesto?.Invoke(_primera.Texto, carta.Pareja);
        }

        public void MostrarResultadoPar(bool coinciden)
        {
            Carta carta = _segunda;
            if (!_comparando || _ocultandoIncorrectas || _primera == null || carta == null) return;
            if (coinciden)
            {
                _primera.Resolver();
                carta.Resolver();
                _primera = null;
                _segunda = null;
                _comparando = false;
                _paresResueltos++;
                if (_paresResueltos * 2 >= _cartas.Count)
                    OnMemoriaCompletada?.Invoke(true);
            }
            else
            {
                _ocultandoIncorrectas = true;
                StartCoroutine(OcultarIncorrectas(_primera, carta));
            }
        }

        private IEnumerator OcultarIncorrectas(Carta primera, Carta segunda)
        {
            yield return new WaitForSecondsRealtime(0.65f);
            primera.Ocultar();
            segunda.Ocultar();
            _primera = null;
            _segunda = null;
            _comparando = false;
            _ocultandoIncorrectas = false;
        }

        private static void Barajar<T>(IList<T> elementos)
        {
            var random = new System.Random();
            for (int i = elementos.Count - 1; i > 0; i--)
            {
                int destino = random.Next(i + 1);
                (elementos[i], elementos[destino]) = (elementos[destino], elementos[i]);
            }
        }

        private sealed class Carta
        {
            public Button Boton { get; }
            public string Texto { get; }
            public string Pareja { get; }
            public bool Resuelta { get; private set; }
            private readonly TMP_Text _etiqueta;

            public Carta(Button boton, string texto, string pareja)
            {
                Boton = boton;
                Texto = texto;
                Pareja = pareja;
                _etiqueta = boton.GetComponentInChildren<TMP_Text>();
                Ocultar();
            }

            public void Mostrar()
            {
                _etiqueta.text = Texto;
                Boton.GetComponent<ThemedButtonView>()?.MostrarEstado(ThemedButtonView.EstadoBoton.Seleccionado);
            }
            public void Ocultar()
            {
                _etiqueta.text = "?";
                Boton.GetComponent<ThemedButtonView>()?.MostrarEstado(ThemedButtonView.EstadoBoton.Normal);
            }

            public void Resolver()
            {
                Resuelta = true;
                Boton.interactable = false;
                Boton.GetComponent<ThemedButtonView>()?.MostrarEstado(ThemedButtonView.EstadoBoton.Correcto);
            }
        }
    }
}
