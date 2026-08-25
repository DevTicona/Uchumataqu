using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    public sealed class MemoriaView : MonoBehaviour
    {
        public event Action<bool> OnMemoriaCompletada;

        private readonly List<Carta> _cartas = new List<Carta>();
        private Carta _primera;
        private bool _comparando;
        private int _paresResueltos;

        public void InicializarRuntime()
        {
            RuntimeActividadUIFactory.CrearTexto(transform, "Instruccion", 25f).text =
                "Encuentra cada palabra y su significado:";
        }

        public void Configurar(IReadOnlyDictionary<string, string> pares)
        {
            foreach (Carta carta in _cartas)
                if (carta.Boton != null) Destroy(carta.Boton.gameObject);
            _cartas.Clear();
            _primera = null;
            _comparando = false;
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
                Button boton = RuntimeActividadUIFactory.CrearBoton(transform, "CartaMemoria", "?");
                var carta = new Carta(boton, dato.texto, dato.pareja);
                boton.onClick.AddListener(() => Seleccionar(carta));
                _cartas.Add(carta);
            }
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
            bool coinciden = string.Equals(_primera.Texto, carta.Pareja, StringComparison.OrdinalIgnoreCase);
            if (coinciden)
            {
                _primera.Resolver();
                carta.Resolver();
                _primera = null;
                _comparando = false;
                _paresResueltos++;
                if (_paresResueltos * 2 >= _cartas.Count)
                    OnMemoriaCompletada?.Invoke(true);
            }
            else
            {
                StartCoroutine(OcultarIncorrectas(_primera, carta));
            }
        }

        private IEnumerator OcultarIncorrectas(Carta primera, Carta segunda)
        {
            yield return new WaitForSecondsRealtime(0.65f);
            primera.Ocultar();
            segunda.Ocultar();
            _primera = null;
            _comparando = false;
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
            }

            public void Mostrar() => _etiqueta.text = Texto;
            public void Ocultar() => _etiqueta.text = "?";

            public void Resolver()
            {
                Resuelta = true;
                Boton.interactable = false;
                Boton.GetComponent<Image>().color = new Color(0.4f, 0.73f, 0.42f);
            }
        }
    }
}
