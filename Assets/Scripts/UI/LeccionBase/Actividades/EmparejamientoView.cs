using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Core.Services;

namespace Assets.Scripts.UI.LeccionBase.Actividades
{
    /// <summary>
    /// Vista para actividades de emparejamiento de pares (UCH <-> ES).
    /// Especificación: Docs/Specifications/04-flujo-leccion-app.md §4.3
    /// </summary>
    public class EmparejamientoView : MonoBehaviour
    {
        [Header("Instrucción")]
        [SerializeField] private TMP_Text txtInstruccion;

        [Header("Columnas de Botones")]
        [SerializeField] private List<Button> botonesColumnaIzq = new List<Button>();
        [SerializeField] private List<TMP_Text> textosColumnaIzq = new List<TMP_Text>();
        [SerializeField] private List<Image> fondosColumnaIzq = new List<Image>();

        [SerializeField] private List<Button> botonesColumnaDer = new List<Button>();
        [SerializeField] private List<TMP_Text> textosColumnaDer = new List<TMP_Text>();
        [SerializeField] private List<Image> fondosColumnaDer = new List<Image>();

        [Header("Colores")]
        [SerializeField] private Color colorNormal = new Color(0.95f, 0.95f, 0.95f, 1f);
        [SerializeField] private Color colorSeleccionado = new Color(0.92f, 0.76f, 0.35f, 1f);
        [SerializeField] private Color colorEmparejado = new Color(0.70f, 0.88f, 0.70f, 0.6f);
        [SerializeField] private Color colorError = new Color(0.95f, 0.50f, 0.50f, 1f);

        public event Action<bool> OnEmparejamientoCompletado;

        private int _seleccionIzq = -1;
        private int _seleccionDer = -1;
        private Dictionary<string, string> _paresCorrectos = new Dictionary<string, string>();
        private HashSet<int> _indicesIzqResueltos = new HashSet<int>();
        private HashSet<int> _indicesDerResueltos = new HashSet<int>();
        private int _totalPares = 0;

        private void Awake()
        {
            for (int i = 0; i < botonesColumnaIzq.Count; i++)
            {
                int index = i;
                botonesColumnaIzq[i].onClick.AddListener(() => SeleccionarIzq(index));
            }

            for (int i = 0; i < botonesColumnaDer.Count; i++)
            {
                int index = i;
                botonesColumnaDer[i].onClick.AddListener(() => SeleccionarDer(index));
            }
        }

        public void Configurar(Dictionary<string, string> pares)
        {
            _paresCorrectos = pares ?? new Dictionary<string, string>();
            _indicesIzqResueltos.Clear();
            _indicesDerResueltos.Clear();
            _seleccionIzq = -1;
            _seleccionDer = -1;
            _totalPares = _paresCorrectos.Count;

            if (txtInstruccion != null)
                txtInstruccion.text = "Empareja cada palabra con su significado:";

            List<string> listaIzq = new List<string>(_paresCorrectos.Keys);
            List<string> listaDer = new List<string>(_paresCorrectos.Values);

            // Barajar independientemente
            Barajar(listaIzq);
            Barajar(listaDer);

            for (int i = 0; i < botonesColumnaIzq.Count; i++)
            {
                if (i < listaIzq.Count)
                {
                    botonesColumnaIzq[i].gameObject.SetActive(true);
                    botonesColumnaIzq[i].interactable = true;
                    textosColumnaIzq[i].text = listaIzq[i];
                    fondosColumnaIzq[i].color = colorNormal;
                }
                else
                {
                    botonesColumnaIzq[i].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < botonesColumnaDer.Count; i++)
            {
                if (i < listaDer.Count)
                {
                    botonesColumnaDer[i].gameObject.SetActive(true);
                    botonesColumnaDer[i].interactable = true;
                    textosColumnaDer[i].text = listaDer[i];
                    fondosColumnaDer[i].color = colorNormal;
                }
                else
                {
                    botonesColumnaDer[i].gameObject.SetActive(false);
                }
            }
        }

        private void SeleccionarIzq(int index)
        {
            if (_indicesIzqResueltos.Contains(index)) return;

            _seleccionIzq = index;
            ActualizarColoresColumna(botonesColumnaIzq, fondosColumnaIzq, _indicesIzqResueltos, _seleccionIzq);

            ComprobarPar();
        }

        private void SeleccionarDer(int index)
        {
            if (_indicesDerResueltos.Contains(index)) return;

            _seleccionDer = index;
            ActualizarColoresColumna(botonesColumnaDer, fondosColumnaDer, _indicesDerResueltos, _seleccionDer);

            ComprobarPar();
        }

        private void ComprobarPar()
        {
            if (_seleccionIzq == -1 || _seleccionDer == -1) return;

            string uch = textosColumnaIzq[_seleccionIzq].text;
            string es = textosColumnaDer[_seleccionDer].text;

            if (_paresCorrectos.TryGetValue(uch, out string esEsperado) && esEsperado.Equals(es, StringComparison.OrdinalIgnoreCase))
            {
                // Par correcto
                _indicesIzqResueltos.Add(_seleccionIzq);
                _indicesDerResueltos.Add(_seleccionDer);

                botonesColumnaIzq[_seleccionIzq].interactable = false;
                botonesColumnaDer[_seleccionDer].interactable = false;

                fondosColumnaIzq[_seleccionIzq].color = colorEmparejado;
                fondosColumnaDer[_seleccionDer].color = colorEmparejado;

                AudioService.Instance?.ReproducirEfecto("correcto");

                _seleccionIzq = -1;
                _seleccionDer = -1;

                if (_indicesIzqResueltos.Count >= _totalPares)
                {
                    OnEmparejamientoCompletado?.Invoke(true);
                }
            }
            else
            {
                // Par incorrecto
                fondosColumnaIzq[_seleccionIzq].color = colorError;
                fondosColumnaDer[_seleccionDer].color = colorError;

                AudioService.Instance?.ReproducirEfecto("incorrecto");

                _seleccionIzq = -1;
                _seleccionDer = -1;

                Invoke(nameof(RestaurarColoresNoResueltos), 0.5f);
            }
        }

        private void RestaurarColoresNoResueltos()
        {
            ActualizarColoresColumna(botonesColumnaIzq, fondosColumnaIzq, _indicesIzqResueltos, -1);
            ActualizarColoresColumna(botonesColumnaDer, fondosColumnaDer, _indicesDerResueltos, -1);
        }

        private void ActualizarColoresColumna(List<Button> botones, List<Image> fondos, HashSet<int> resueltos, int seleccionado)
        {
            for (int i = 0; i < botones.Count; i++)
            {
                if (!botones[i].gameObject.activeSelf) continue;

                if (resueltos.Contains(i))
                {
                    fondos[i].color = colorEmparejado;
                }
                else if (i == seleccionado)
                {
                    fondos[i].color = colorSeleccionado;
                }
                else
                {
                    fondos[i].color = colorNormal;
                }
            }
        }

        private void Barajar<T>(List<T> lista)
        {
            System.Random rng = new System.Random();
            int n = lista.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = lista[k];
                lista[k] = lista[n];
                lista[n] = value;
            }
        }
    }
}
