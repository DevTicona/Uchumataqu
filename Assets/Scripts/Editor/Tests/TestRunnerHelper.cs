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
            EjecutarPrueba("Addressables: datos y todos los audios vigentes", () => estabilizacion.Addressables_ContieneDatosYTodosLosAudiosDisponibles(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Main: cinco pestanas y dialogos operativos", () => estabilizacion.Main_ContieneLasCincoPestanasYDialogosOperativos(), ref pruebasPasadas, ref totalPruebas);

            // 5. Semana 3
            var semana3 = new Semana3Tests();
            semana3.PrepararCorpus();
            EjecutarPrueba("Onboarding: validación de perfil y avatar", () => semana3.Onboarding_ValidaNombreUnicodeYAvatarExplicito(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Mapa: 27 lecciones, 6 hitos y 6 regiones", () => semana3.Mapa_Crea27LeccionesSeisHitosYSeisRegiones(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Mapa: desbloqueo de RA1 y L07", () => semana3.Mapa_DesbloqueaRa1YL07TrasCompletarL01AL06(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("L01-L05: construcción de actividades", () => semana3.L01AL05_ConstruyenTodosSusPasosEvaluables(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Nombres propios: solo contexto, sin familia ni audio léxico", () => semana3.NombresPropios_NoSeEnsenanComoFamiliasNiTienenAudioLexico(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Semana 3: composición de escenas", () => semana3.EscenasSemana3_TienenComposicionSerializada(), ref pruebasPasadas, ref totalPruebas);

            // 6. Semana 4
            var semana4 = new Semana4Tests();
            semana4.PrepararCorpus();
            EjecutarPrueba("L01-L27: construcción de actividades", () => semana4.L01AL27_ConstruyenTodosLosPasosEvaluables(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Inicio: reanudación por estado persistido", () => semana4.Inicio_PriorizaLeccionEnProgresoAunqueHayaHuecosCompletados(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Logros: umbrales canónicos", () => semana4.Logros_UsanUmbralesCanonicosYProgresoAcotado(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("Perfil: edición validada sin alterar progreso", () => semana4.EdicionPerfil_ValidaTodosLosCamposSinAlterarProgreso(), ref pruebasPasadas, ref totalPruebas);

            // 7. Lógica ASR no bloqueante
            EjecutarPrueba("ASR: ausencia produce resultado neutral", () => semana4.AsrNoDisponible_DevuelveResultadoNeutralNoBloqueante(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("ASR: transcripción delegada al motor PLN", () => semana4.AsrDisponible_DelegaLaEvaluacionAlMotorDeTexto(), ref pruebasPasadas, ref totalPruebas);
            EjecutarPrueba("ASR: fallo nativo no bloqueante", () => semana4.FalloDelBindingAsr_NoPropagaLaExcepcionNiBloquea(), ref pruebasPasadas, ref totalPruebas);

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
