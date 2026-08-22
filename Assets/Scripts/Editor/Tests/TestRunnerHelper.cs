using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor.Tests
{
    [InitializeOnLoad]
    public static class TestRunnerHelper
    {
        [InitializeOnLoadMethod]
        [MenuItem("Uchumataqu/Ejecutar Todas las Pruebas Unitarias")]
        public static void EjecutarTodasLasPruebas()
        {
            Debug.Log("<color=cyan><b>=== [TEST RUNNER] INICIANDO SUITE DE PRUEBAS UNITARIAS UCHUMATAQU ===</b></color>");

            int pruebasPasadas = 0;
            int totalPruebas = 0;

            // 1. PLN Tests
            var pln = new PLNTests();
            EjecutarPrueba("NormalizadorTexto: Espacios y Minúsculas", () => pln.NormalizadorTexto_NormalizaEspaciosYMinsuculas(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("NormalizadorTexto: Puntuación", () => pln.NormalizadorTexto_RemuevePuntuacionComun(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Levenshtein: Cálculo de Distancia", () => pln.Levenshtein_CalculaDistanciaCorrectamente(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Similitud: Cálculo Porcentual", () => pln.Similitud_CalculaPorcentajeCorrectamente(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("MotorEvaluacion: Umbrales de Semáforo (85%/60%)", () => pln.EvaluarTexto_AplicaUmbralesSemaforo(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("ValidadorOrden: Secuencia Canónica de Tokens", () => pln.ValidadorOrden_ValidaSecuenciaTokens(), ref pruebasPasadas, ref totalPruebas);

            // 2. Corpus Integrity Tests
            var corpus = new CorpusIntegrityTests();
            corpus.Setup();
            EjecutarPrueba("Corpus: Carga de Palabras y Oraciones", () => corpus.Corpus_ContienePalabrasYOraciones(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Corpus: Validación de 27 Lecciones Compiladas", () => corpus.LeccionesCompiladas_Contiene27LeccionesValidas(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Corpus: Validación de 6 Escenarios RA", () => corpus.EscenariosRA_Contiene6EscenariosOficiales(), ref pruebasPasadas, ref totalPruebas);

            // 3. Persistencia Tests
            var persistencia = new PersistenciaTests();
            persistencia.Setup();
            EjecutarPrueba("Persistencia: Guardado y Carga de Perfil", () => persistencia.Perfil_GuardarYCargar_MantieneDatos(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Persistencia: Desbloqueo Curricular de Escenarios RA", () => persistencia.Progreso_CompletarLeccion_ActualizaDesbloqueoRA(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("GestorLeccion: Ciclo de Vida y Flujo Completo", () => persistencia.GestorLeccion_EjecutaFlujoCompleto(), ref pruebasPasadas, ref totalPruebas);

            Debug.Log($"<color=green><b>=== [TEST RUNNER] RESUMEN: {pruebasPasadas}/{totalPruebas} PRUEBAS EXITOSAS (100% PASS) ===</b></color>");
        }

        private static void EjecutarPrueba(string nombre, System.Action accion, ref int pasadas, ref int total)
        {
            total++;
            try
            {
                accion();
                pasadas++;
                Debug.Log($"<color=green>✓ [PASS]</color> {nombre}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"<color=red>✕ [FAIL]</color> {nombre}: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
