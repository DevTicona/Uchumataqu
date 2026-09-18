using System.Collections.Generic;

namespace Assets.Scripts.Lecciones
{
    public static class TipoPasoLeccion
    {
        public const string PresentacionFamilia = "presentacion_familia";
        public const string UchAEspanol = "uch_a_es";
        public const string EspanolAUch = "es_a_uch";
        public const string AudioAUch = "audio_a_uch";
        public const string AudioAEspanol = "audio_a_es";
        public const string FormaARaiz = "forma_a_raiz";
        public const string FormaAEspanol = "forma_a_es";
        public const string EspanolAForma = "es_a_forma";
        public const string AudioAForma = "audio_a_forma";
        public const string EmparejarUchEspanol = "emparejar_uch_es";
        public const string EmparejarAudioUch = "emparejar_audio_uch";
        public const string FamiliaCompleta = "familia_completa";
        public const string MemoriaUchEspanol = "memoria_uch_es";
        public const string OrdenarOracion = "ordenar_oracion";
        public const string DictadoWordBank = "dictado_word_bank";
        public const string TraduccionConstructiva = "traduccion_constructiva";
        public const string CompletarOracion = "completar_oracion";
        public const string AudioAEspanolOracion = "audio_a_es_oracion";
        public const string EscuchaYRepiteAsr = "escucha_y_repite_asr";

        private static readonly HashSet<string> Tipos = new HashSet<string>
        {
            PresentacionFamilia,
            UchAEspanol,
            EspanolAUch,
            AudioAUch,
            AudioAEspanol,
            FormaARaiz,
            FormaAEspanol,
            EspanolAForma,
            AudioAForma,
            EmparejarUchEspanol,
            EmparejarAudioUch,
            FamiliaCompleta,
            MemoriaUchEspanol,
            OrdenarOracion,
            DictadoWordBank,
            TraduccionConstructiva,
            CompletarOracion,
            AudioAEspanolOracion,
            EscuchaYRepiteAsr
        };

        public static IReadOnlyCollection<string> Todos => Tipos;

        public static bool EsSoportado(string tipo) => !string.IsNullOrEmpty(tipo) && Tipos.Contains(tipo);

        public static bool EsPresentacion(string tipo) => tipo == PresentacionFamilia;

        public static bool EsSeleccion(string tipo) =>
            tipo == UchAEspanol || tipo == EspanolAUch || tipo == AudioAUch ||
            tipo == AudioAEspanol || tipo == FormaARaiz || tipo == FormaAEspanol ||
            tipo == EspanolAForma || tipo == AudioAForma || tipo == CompletarOracion ||
            tipo == AudioAEspanolOracion;

        public static bool EsEmparejamiento(string tipo) =>
            tipo == EmparejarUchEspanol || tipo == EmparejarAudioUch ||
            tipo == FamiliaCompleta || tipo == MemoriaUchEspanol;

        public static bool EsWordBank(string tipo) =>
            tipo == OrdenarOracion || tipo == DictadoWordBank || tipo == TraduccionConstructiva;
    }
}
