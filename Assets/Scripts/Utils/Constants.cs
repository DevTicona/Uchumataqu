// Assets/Scripts/Utils/Constants.cs
using UnityEngine;

namespace Assets.Scripts.Utils
{
    /// <summary>
    /// Constantes globales del proyecto UCHUMATAQU.
    /// </summary>
    public static class Constants
    {
        // ============================================================
        // ADDRESSABLES
        // ============================================================
        public const string ADDRESSABLES_GLOBAL_DATA = "Global_Data";
        public const string ADDRESSABLES_LESSONS_PREFIX = "Leccion_";

        // Estos deben coincidir EXACTAMENTE con los nombres
        // configurados como Addressables en Unity.
        public const string FILE_LECCIONES = "lecciones";
        public const string FILE_PALABRAS = "palabras";
        public const string FILE_ACTIVIDADES = "actividades";
        public const string FILE_ACTIVIDADES_LECCION = "actividades_leccion";

        // ============================================================
        // SCENES
        // ============================================================
        public const string SCENE_SPLASH = "Splash";
        public const string SCENE_MAIN_MENU = "MainMenu";
        public const string SCENE_LESSONS = "Lessons";
        public const string SCENE_ACTIVITIES_2D = "Activities2D";
        public const string SCENE_AR_ACTIVITIES = "ARActivities";
        public const string SCENE_RESULTS = "Results";

        // ============================================================
        // TAGS / LAYERS
        // ============================================================
        public const string TAG_AR_PLANE = "ARPlane";
        public const int LAYER_UI = 5;

        // ============================================================
        // GAMIFICACIÓN
        // ============================================================
        public const int XP_PER_CORRECT_ANSWER = 10;
        public const int XP_PER_LESSON_COMPLETE = 50;
        public const int COINS_PER_ACTIVITY = 5;
        public const int MIN_SCORE_TO_PASS = 70;

        // ============================================================
        // AUDIO
        // ============================================================
        public const string AUDIO_PREFIX = "aud_";
        public const float DEFAULT_AUDIO_VOLUME = 0.8f;

        // ============================================================
        // BLOQUES Y LÓGICA
        // ============================================================
        public const int MAX_WORDS_SINGLE_BLOCK = 7;

        // Phase names as strings (MANTENER para data YAML)
        public const string PHASE_APRENDIZAJE = "aprendizaje";
        public const string PHASE_ACTIVIDADES_2D = "actividades_2d";
        public const string PHASE_ACTIVIDADES_RA = "actividades_ra";
        public const string PHASE_QUIZ = "quiz";

        // ============================================================
        // PLN / SPEECH
        // ============================================================
        public const float SPEECH_THRESHOLD_PERFECT = 0.8f;
        public const float SPEECH_THRESHOLD_GOOD = 0.5f;

        // ============================================================
        // UI
        // ============================================================
        public const float TRANSITION_DURATION = 0.3f;
        public const float POPUP_FADE_DURATION = 0.25f;

        // ============================================================
        // PLAYERPREFS
        // ============================================================
        public const string PREF_FIRST_TIME = "FirstTime";
        public const string PREF_USER_PROGRESS = "UserProgress";
        public const string PREF_AUDIO_VOLUME = "AudioVolume";
        public const string PREF_LAST_LESSON = "LastLesson";
    }


    /// <summary>
    /// Nombres de eventos para EventBus.
    /// </summary>
    public static class EventNames
    {
        public const string LESSON_COMPLETED = "LessonCompleted";
        public const string ACTIVITY_COMPLETED = "ActivityCompleted";
        public const string WORD_LEARNED = "WordLearned";
        public const string XP_GAINED = "XPGained";
        public const string ACHIEVEMENT_UNLOCKED = "AchievementUnlocked";
        public const string DATA_LOADED = "DataLoaded";
        public const string SCENE_TRANSITION_START = "SceneTransitionStart";
        public const string SCENE_TRANSITION_END = "SceneTransitionEnd";
    }


    /// <summary>
    /// Tipos de fases dentro de cada bloque de aprendizaje.
    /// </summary>
    public enum PhaseType
    {
        Aprendizaje,
        Actividades2D,
        ActividadesRA,
        Quiz
    }

    /// <summary>
    /// Tipos de actividades 2D disponibles.
    /// </summary>
    public enum Activity2DType
    {
        SeleccionTextual,
        Emparejamiento,
        EliminarIntruso,
        Anagrama
    }

    /// <summary>
    /// Resultado evaluado del módulo de pronunciación (PLN).
    /// </summary>
    public enum SpeechEvaluation
    {
        Excelente,   // 80–100 %
        Casi,        // 50–80 %
        Reintentar   // <50 %
    }
}
