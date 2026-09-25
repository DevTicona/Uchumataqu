using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Lecciones;
using Assets.Scripts.Persistencia;
using Assets.Scripts.UI.LeccionBase;
using Assets.Scripts.UI.LeccionBase.Actividades;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Assets.Scripts.Editor
{
    /// <summary>
    /// Renderiza pasos reales del corpus en 03_LeccionBase sin iniciar el juego.
    /// Los cambios son exclusivamente una vista previa de escena y no modifican el JSON.
    /// </summary>
    public sealed class LeccionBasePreviewWindow : EditorWindow
    {
        private int _indiceLeccion;
        private int _indicePaso;

        [MenuItem("Uchumataqu/UI/Vista previa de LeccionBase")]
        private static void Abrir()
        {
            GetWindow<LeccionBasePreviewWindow>("Vista previa lección");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Datos reales de lecciones_compiladas.json", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Elige un paso y pulsa Renderizar. La escena queda lista para diseñar ese caso real sin entrar en Play.",
                MessageType.Info);

            if (!CorpusRepository.Inicializar())
            {
                EditorGUILayout.HelpBox("No se pudo cargar el corpus local.", MessageType.Error);
                return;
            }

            List<LeccionCompiladaModel> lecciones = CorpusRepository.ObtenerTodasLasLecciones();
            if (lecciones.Count == 0)
            {
                EditorGUILayout.HelpBox("No hay lecciones compiladas disponibles.", MessageType.Warning);
                return;
            }

            _indiceLeccion = Mathf.Clamp(_indiceLeccion, 0, lecciones.Count - 1);
            _indiceLeccion = EditorGUILayout.Popup("Lección", _indiceLeccion,
                lecciones.Select(leccion => $"{leccion.id} · {leccion.titulo}").ToArray());

            LeccionCompiladaModel leccion = lecciones[_indiceLeccion];
            if (leccion.pasos == null || leccion.pasos.Count == 0)
            {
                EditorGUILayout.HelpBox("La lección no contiene pasos.", MessageType.Warning);
                return;
            }

            _indicePaso = Mathf.Clamp(_indicePaso, 0, leccion.pasos.Count - 1);
            _indicePaso = EditorGUILayout.Popup("Paso", _indicePaso,
                leccion.pasos.Select((paso, indice) => $"{indice + 1:00} · {paso.tipo}").ToArray());

            PasoLeccionModel pasoSeleccionado = leccion.pasos[_indicePaso];
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Caso seleccionado", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Tipo", pasoSeleccionado.tipo ?? "—");
            EditorGUILayout.LabelField("Objetivo", ObtenerObjetivo(pasoSeleccionado));

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Renderizar en 03_LeccionBase", GUILayout.Height(30f)))
                    Renderizar(leccion, _indicePaso);

                if (GUILayout.Button("Restablecer", GUILayout.Height(30f)))
                    Restablecer();
            }
        }

        private static void Renderizar(LeccionCompiladaModel leccion, int indicePaso)
        {
            LeccionBaseView vista = BuscarVista();
            if (vista == null) return;

            PasoLeccionModel paso = leccion.pasos[indicePaso];
            Undo.RegisterFullObjectHierarchyUndo(vista.gameObject, "Vista previa de lección");
            vista.ConfigurarHeader(leccion.titulo, (float)indicePaso / leccion.pasos.Count);
            vista.OcultarFeedback();

            try
            {
                RenderizarActividad(vista, paso);
                ConfigurarAccion(vista, paso);
                FinalizarCambio(vista);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[LeccionBasePreview] No se pudo renderizar '{paso.tipo}': {exception.Message}", vista);
            }
        }

        private static void RenderizarActividad(LeccionBaseView vista, PasoLeccionModel paso)
        {
            if (TipoPasoLeccion.EsPresentacion(paso.tipo))
            {
                vista.InstanciarPresentacion().Configurar(paso, CorpusRepository.ObtenerPalabra(paso.objetivo_id));
                return;
            }

            if (TipoPasoLeccion.EsSeleccion(paso.tipo))
            {
                ActividadSeleccionModel datos = GestorActividadLeccion.CrearSeleccion(paso);
                vista.InstanciarSeleccionMultiple().Configurar(
                    datos.Tipo, datos.Prompt, new List<string>(datos.Opciones),
                    datos.AudioPalabraId ?? datos.AudioOracionId, datos.PistaEs);
                return;
            }

            if (paso.tipo == TipoPasoLeccion.MemoriaUchEspanol)
            {
                vista.InstanciarMemoria().Configurar(GestorActividadLeccion.CrearPares(paso));
                return;
            }

            if (TipoPasoLeccion.EsEmparejamiento(paso.tipo))
            {
                vista.InstanciarEmparejamiento().Configurar(GestorActividadLeccion.CrearPares(paso), paso.tipo);
                return;
            }

            if (TipoPasoLeccion.EsWordBank(paso.tipo))
            {
                ActividadWordBankModel datos = GestorActividadLeccion.CrearWordBank(paso);
                vista.InstanciarWordBank().Configurar(
                    datos.Tipo, datos.Prompt, new List<string>(datos.Tokens), datos.AudioOracionId);
                return;
            }

            if (TipoPasoLeccion.EsEscritura(paso.tipo))
            {
                ActividadEscrituraModel datos = GestorActividadLeccion.CrearEscritura(paso);
                vista.InstanciarEscritura().Configurar(datos.Tipo, datos.Estimulo);
                return;
            }

            if (TipoPasoLeccion.EsPracticaVoz(paso.tipo))
            {
                bool esOracion = paso.tipo == TipoPasoLeccion.EscuchaYRepiteOracionAsr;
                string objetivo = esOracion
                    ? CorpusRepository.ObtenerOracion(paso.oracion_id)?.uch
                    : CorpusRepository.ObtenerPalabra(paso.objetivo_id)?.uch;
                vista.InstanciarASR().Configurar(objetivo, disponible: true, esOracion: esOracion);
                return;
            }

            throw new InvalidOperationException($"Tipo de paso no soportado: '{paso.tipo}'.");
        }

        private static void ConfigurarAccion(LeccionBaseView vista, PasoLeccionModel paso)
        {
            bool requiereComprobacion = TipoPasoLeccion.EsSeleccion(paso.tipo)
                || TipoPasoLeccion.EsWordBank(paso.tipo)
                || TipoPasoLeccion.EsEscritura(paso.tipo);
            vista.PresentarAccionPrincipal(
                requiereComprobacion ? LeccionBaseView.AccionPrincipal.Comprobar : LeccionBaseView.AccionPrincipal.Continuar,
                interactuable: !requiereComprobacion);
        }

        private static void Restablecer()
        {
            LeccionBaseView vista = BuscarVista();
            if (vista == null) return;

            Undo.RegisterFullObjectHierarchyUndo(vista.gameObject, "Restablecer vista previa de lección");
            vista.LimpiarActividadActual();
            vista.ConfigurarHeader("Vista previa de lección", 0f);
            vista.PresentarAccionPrincipal(LeccionBaseView.AccionPrincipal.Continuar, false, false);
            vista.OcultarFeedback();
            FinalizarCambio(vista);
        }

        private static LeccionBaseView BuscarVista()
        {
            LeccionBaseView vista = UnityEngine.Object.FindAnyObjectByType<LeccionBaseView>(FindObjectsInactive.Include);
            if (vista != null) return vista;

            EditorUtility.DisplayDialog("Vista previa de lección", "Abre la escena 03_LeccionBase antes de renderizar.", "Entendido");
            return null;
        }

        private static void FinalizarCambio(LeccionBaseView vista)
        {
            Canvas.ForceUpdateCanvases();
            EditorUtility.SetDirty(vista.gameObject);
            EditorSceneManager.MarkSceneDirty(vista.gameObject.scene);
            SceneView.RepaintAll();
        }

        private static string ObtenerObjetivo(PasoLeccionModel paso)
        {
            return paso.objetivo_id ?? paso.forma_id ?? paso.oracion_id ?? paso.raiz_id ?? "—";
        }
    }
}
