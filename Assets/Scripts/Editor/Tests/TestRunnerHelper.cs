using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor.Tests
{
    [InitializeOnLoad]
    public static class TestRunnerHelper
    {
        [MenuItem("Uchumataqu/Ejecutar Todas las Pruebas Unitarias")]
        public static void EjecutarTodasLasPruebas()
        {
            Debug.Log("<color=cyan><b>=== [TEST RUNNER] INICIANDO SUITE DE PRUEBAS UNITARIAS UCHUMATAQU ===</b></color>");

            int pruebasPasadas = 0;
            int totalPruebas = 0;

            // 1. PLN Tests (Sección 7)
            var pln = new PLNTests();
            EjecutarPrueba("NormalizadorTexto: Espacios y Minúsculas", () => pln.NormalizadorTexto_NormalizaEspaciosYMinsuculas(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("NormalizadorTexto: Puntuación", () => pln.NormalizadorTexto_RemuevePuntuacionComun(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("NormalizadorTexto: Conservación de Grafías Uchumataqu", () => pln.NormalizadorTexto_ConservaGrafiasPropiasYVocalesLargas(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Levenshtein: Cálculo de Distancia y Simetría", () => pln.Levenshtein_CalculaDistanciaCorrectamente(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Similitud: Cálculo Porcentual", () => pln.Similitud_CalculaPorcentajeCorrectamente(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("MotorEvaluacion: Umbrales de Semáforo (85%/60%)", () => pln.EvaluarTexto_AplicaUmbralesSemaforo(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("ValidadorOrden: Secuencia Canónica de Tokens", () => pln.ValidadorOrden_ValidaSecuenciaTokens(), ref pruebasPasadas, ref totalPruebas);

            // 2. Corpus Integrity Tests (Sección 8 & 20)
            var corpus = new CorpusIntegrityTests();
            corpus.Setup();
            EjecutarPrueba("Corpus: Carga de Palabras y Oraciones", () => corpus.Corpus_ContienePalabrasYOraciones(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Corpus: Validación de 27 Lecciones Compiladas", () => corpus.LeccionesCompiladas_Contiene27LeccionesValidas(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Corpus: Validación de 6 Escenarios RA", () => corpus.EscenariosRA_Contiene6EscenariosOficiales(), ref pruebasPasadas, ref totalPruebas);

            // 3. Persistencia & Progreso Tests (Sección 11, 12, 13 & 23)
            var persistencia = new PersistenciaTests();
            persistencia.Setup();
            EjecutarPrueba("Persistencia: Guardado y Carga de Perfil", () => persistencia.Perfil_GuardarYCargar_MantieneDatos(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Persistencia: Desbloqueo Curricular de Escenarios RA", () => persistencia.Progreso_CompletarLeccion_ActualizaDesbloqueoRA(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("GestorLeccion: Finalización por Recorrido sin Precisión Mínima ni ASR", () => persistencia.GestorLeccion_FinalizaSinPrecisionMinimaNiASR(), ref pruebasPasadas, ref totalPruebas);

            // 4. Puerta de estabilización previa a Semana 3
            var estabilizacion = new EstabilizacionTests();
            estabilizacion.PrepararCorpus();
            EjecutarPrueba("Contrato: 19 tipos compilados soportados", () => estabilizacion.Contrato_CubreLos19TiposCompilados(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Actividades: 27 lecciones construibles", () => estabilizacion.TodasLasLecciones_PuedenConstruirSusActividades(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("L01: contrato completo válido", () => estabilizacion.L01_TodosSusPasosTienenContratoValido(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("RA: hitos curriculares canónicos", () => estabilizacion.EscenariosRa_RespetanLosHitosCanonicos(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Escenas: una View principal por escena", () => estabilizacion.Escenas_TienenUnaUnicaViewPrincipal(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Addressables: datos y 382 audios disponibles", () => estabilizacion.Addressables_ContieneDatosYTodosLosAudiosDisponibles(), ref pruebasPasadas, ref totalPruebas);

            string color = pruebasPasadas == totalPruebas ? "green" : "red";
            string estado = pruebasPasadas == totalPruebas ? "PASS" : "CON FALLOS";
            Debug.Log($"<color={color}><b>=== [TEST RUNNER] RESUMEN: {pruebasPasadas}/{totalPruebas} PRUEBAS EXITOSAS ({estado}) ===</b></color>");
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
