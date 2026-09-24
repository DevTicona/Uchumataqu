using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Lecciones;
using UnityEngine.Events;
using Assets.Scripts.Core.UI.Components;

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

        [SerializeField] private Transform columnaIzquierda;
        [SerializeField] private Transform columnaDerecha;
        [SerializeField] private GameObject prefabItem;
        public event Action<string, string> OnParPropuesto;
        public event Action<string> OnAudioClicked;
        private bool _mostrarAudioComoEstimulo;
        private bool _reproduceAudioAlSeleccionar;
        private List<string> _idsIzquierda = new List<string>();
        private IReadOnlyDictionary<string, string> _audiosPorClaveIzquierda;

        private readonly List<Button> botonesColumnaIzq = new List<Button>();
        private readonly List<TMP_Text> textosColumnaIzq = new List<TMP_Text>();

        private readonly List<Button> botonesColumnaDer = new List<Button>();
        private readonly List<TMP_Text> textosColumnaDer = new List<TMP_Text>();

        public event Action<bool> OnEmparejamientoCompletado;

        private int _seleccionIzq = -1;
        private int _seleccionDer = -1;
        private Dictionary<string, string> _paresCorrectos = new Dictionary<string, string>();
        private HashSet<int> _indicesIzqResueltos = new HashSet<int>();
        private HashSet<int> _indicesDerResueltos = new HashSet<int>();
        private int _totalPares = 0;
        private readonly List<UnityAction> _accionesIzquierda = new List<UnityAction>();
        private readonly List<UnityAction> _accionesDerecha = new List<UnityAction>();

        private void OnDisable()
        {
            StopAllCoroutines();
            CancelInvoke();
        }

        private void Awake()
        {
            RegistrarListeners();
        }

        private void RegistrarListeners()
        {
            DesregistrarListeners();
            for (int i = 0; i < botonesColumnaIzq.Count; i++)
            {
                int index = i;
                UnityAction accion = () => SeleccionarIzq(index);
                _accionesIzquierda.Add(accion);
                if (botonesColumnaIzq[i] != null)
                    botonesColumnaIzq[i].onClick.AddListener(accion);
            }

            for (int i = 0; i < botonesColumnaDer.Count; i++)
            {
                int index = i;
                UnityAction accion = () => SeleccionarDer(index);
                _accionesDerecha.Add(accion);
                if (botonesColumnaDer[i] != null)
                    botonesColumnaDer[i].onClick.AddListener(accion);
            }
        }

        private void OnDestroy() => DesregistrarListeners();

        private void DesregistrarListeners()
        {
            for (int i = 0; i < _accionesIzquierda.Count && i < botonesColumnaIzq.Count; i++)
                if (botonesColumnaIzq[i] != null)
                    botonesColumnaIzq[i].onClick.RemoveListener(_accionesIzquierda[i]);
            for (int i = 0; i < _accionesDerecha.Count && i < botonesColumnaDer.Count; i++)
                if (botonesColumnaDer[i] != null)
                    botonesColumnaDer[i].onClick.RemoveListener(_accionesDerecha[i]);
            _accionesIzquierda.Clear();
            _accionesDerecha.Clear();
        }

        public void Configurar(Dictionary<string, string> pares, string tipo,
            IReadOnlyDictionary<string, string> audiosPorClaveIzquierda = null)
        {
            CancelInvoke();
            _mostrarAudioComoEstimulo = tipo == TipoPasoLeccion.EmparejarAudioUch;
            _reproduceAudioAlSeleccionar = tipo == TipoPasoLeccion.EmparejarUchEspanol ||
                tipo == TipoPasoLeccion.FamiliaCompleta || _mostrarAudioComoEstimulo;
            _audiosPorClaveIzquierda = audiosPorClaveIzquierda;
            _paresCorrectos = pares ?? new Dictionary<string, string>();
            LimpiarItemsRegenerables();
            if (prefabItem != null)
            {
                for (int indice = 0; indice < _paresCorrectos.Count; indice++)
                {
                    Button izq = Instantiate(prefabItem, columnaIzquierda, false).GetComponent<Button>();
                    Button der = Instantiate(prefabItem, columnaDerecha, false).GetComponent<Button>();
                    botonesColumnaIzq.Add(izq); textosColumnaIzq.Add(izq.GetComponentInChildren<TMP_Text>());
                    botonesColumnaDer.Add(der); textosColumnaDer.Add(der.GetComponentInChildren<TMP_Text>());
                    ConfigurarEstiloColumnaDerecha(der);
                }
                RegistrarListeners();
            }
            _indicesIzqResueltos.Clear();
            _indicesDerResueltos.Clear();
            _seleccionIzq = -1;
            _seleccionDer = -1;
            _totalPares = _paresCorrectos.Count;

            if (txtInstruccion != null)
                txtInstruccion.text = _mostrarAudioComoEstimulo
                    ? "Une cada audio con su palabra:"
                    : "Empareja cada palabra con su significado:";

            List<string> listaIzq = new List<string>(_paresCorrectos.Keys);
            List<string> listaDer = new List<string>(_paresCorrectos.Values);

            // Barajar independientemente
            Barajar(listaIzq);
            Barajar(listaDer);
            _idsIzquierda = listaIzq;

            for (int i = 0; i < botonesColumnaIzq.Count; i++)
            {
                if (i < listaIzq.Count)
                {
                    botonesColumnaIzq[i].gameObject.SetActive(true);
                    botonesColumnaIzq[i].interactable = true;
                    textosColumnaIzq[i].text = _mostrarAudioComoEstimulo ? "Escuchar" : listaIzq[i];
                    Transform icono = botonesColumnaIzq[i].transform.Find("IconoAudio");
                    if (icono != null) icono.gameObject.SetActive(_reproduceAudioAlSeleccionar);
                    MostrarEstado(botonesColumnaIzq[i], ThemedButtonView.EstadoBoton.Normal);
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
                    MostrarEstado(botonesColumnaDer[i], ThemedButtonView.EstadoBoton.Normal);
                }
                else
                {
                    botonesColumnaDer[i].gameObject.SetActive(false);
                }
            }

            GetComponent<ActividadLayoutController>()?.SolicitarReconstruccion();
        }

        public void Configurar(Dictionary<string, string> pares, bool audio = false)
        {
            Configurar(pares, audio ? TipoPasoLeccion.EmparejarAudioUch : TipoPasoLeccion.EmparejarUchEspanol);
        }

        private void LimpiarItemsRegenerables()
        {
            DesregistrarListeners();
            LimpiarColumna(columnaIzquierda);
            LimpiarColumna(columnaDerecha);
            botonesColumnaIzq.Clear();
            textosColumnaIzq.Clear();
            botonesColumnaDer.Clear();
            textosColumnaDer.Clear();
        }

        private static void LimpiarColumna(Transform columna)
        {
            if (columna == null) return;

            for (int indice = columna.childCount - 1; indice >= 0; indice--)
            {
                GameObject item = columna.GetChild(indice).gameObject;
                if (item.GetComponent<Button>() != null)
                    RuntimeActividadUIFactory.Retirar(item);
            }
        }

        private static void ConfigurarEstiloColumnaDerecha(Button item)
        {
            if (item == null) return;

            ThemedTextView texto = item.GetComponentInChildren<ThemedTextView>(true);
            if (texto != null)
            {
                texto.tipoColor = ThemedTextView.ColorTexto.Secundario;
                texto.AplicarTema();
            }

            Transform iconoAudio = item.transform.Find("IconoAudio");
            ThemedGraphicView icono = iconoAudio != null
                ? iconoAudio.GetComponent<ThemedGraphicView>()
                : null;
            if (icono == null) return;

            icono.Rol = ThemedGraphicView.RolColor.TextoSecundario;
            icono.AplicarTema();
        }

        private void SeleccionarIzq(int index)
        {
            if (index < 0 || index >= _totalPares || _indicesIzqResueltos.Contains(index)) return;

            if (_reproduceAudioAlSeleccionar && _audiosPorClaveIzquierda != null &&
                _audiosPorClaveIzquierda.TryGetValue(_idsIzquierda[index], out string audioId))
                OnAudioClicked?.Invoke(audioId);
            _seleccionIzq = index;
            ActualizarColoresColumna(botonesColumnaIzq, _indicesIzqResueltos, _seleccionIzq);

            ComprobarPar();
        }

        private void SeleccionarDer(int index)
        {
            if (index < 0 || index >= _totalPares || _indicesDerResueltos.Contains(index)) return;

            _seleccionDer = index;
            ActualizarColoresColumna(botonesColumnaDer, _indicesDerResueltos, _seleccionDer);

            ComprobarPar();
        }

        private void ComprobarPar()
        {
            if (_seleccionIzq == -1 || _seleccionDer == -1) return;

            string uch = _idsIzquierda[_seleccionIzq];
            string es = textosColumnaDer[_seleccionDer].text;

            OnParPropuesto?.Invoke(uch, es);
        }

        public void MostrarResultadoPar(bool correcto)
        {
            if (_seleccionIzq < 0 || _seleccionDer < 0) return;
            if (correcto)
            {
                // Par correcto
                _indicesIzqResueltos.Add(_seleccionIzq);
                _indicesDerResueltos.Add(_seleccionDer);

                botonesColumnaIzq[_seleccionIzq].interactable = false;
                botonesColumnaDer[_seleccionDer].interactable = false;

                MostrarEstado(botonesColumnaIzq[_seleccionIzq], ThemedButtonView.EstadoBoton.Correcto);
                MostrarEstado(botonesColumnaDer[_seleccionDer], ThemedButtonView.EstadoBoton.Correcto);

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
                MostrarEstado(botonesColumnaIzq[_seleccionIzq], ThemedButtonView.EstadoBoton.Incorrecto);
                MostrarEstado(botonesColumnaDer[_seleccionDer], ThemedButtonView.EstadoBoton.Incorrecto);

                _seleccionIzq = -1;
                _seleccionDer = -1;

                Invoke(nameof(RestaurarColoresNoResueltos), 0.5f);
            }
        }

        private void RestaurarColoresNoResueltos()
        {
            ActualizarColoresColumna(botonesColumnaIzq, _indicesIzqResueltos, _seleccionIzq);
            ActualizarColoresColumna(botonesColumnaDer, _indicesDerResueltos, _seleccionDer);
        }

        private void ActualizarColoresColumna(List<Button> botones, HashSet<int> resueltos, int seleccionado)
        {
            for (int i = 0; i < botones.Count; i++)
            {
                if (!botones[i].gameObject.activeSelf) continue;

                if (resueltos.Contains(i))
                {
                    MostrarEstado(botones[i], ThemedButtonView.EstadoBoton.Correcto);
                }
                else if (i == seleccionado)
                {
                    MostrarEstado(botones[i], ThemedButtonView.EstadoBoton.Seleccionado);
                }
                else
                {
                    MostrarEstado(botones[i], ThemedButtonView.EstadoBoton.Normal);
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

        private static void MostrarEstado(Button boton, ThemedButtonView.EstadoBoton estado)
        {
            boton.GetComponent<ThemedButtonView>()?.MostrarEstado(estado);
        }
    }
}
