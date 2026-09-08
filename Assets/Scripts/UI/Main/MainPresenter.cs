using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Core;
using Assets.Scripts.Core.Services;
using Assets.Scripts.Core.Sistema;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.Data.Persistencia;
using Assets.Scripts.Persistencia;
using Assets.Scripts.UI.Main.Paginas;

namespace Assets.Scripts.UI.Main
{
    public class MainPresenter : MonoBehaviour
    {
        [SerializeField] private MainView view;
        [SerializeField] private NavigationBackHandler backHandler;

        private PaginaMain _paginaActual = PaginaMain.Inicio;

        private async void Start()
        {
            if (view == null || backHandler == null)
            {
                Debug.LogError("[MainPresenter] Faltan referencias de composición de la escena.");
                enabled = false;
                return;
            }

            if (!CorpusRepository.EstaInicializado && !await CorpusRepository.InicializarAsync())
            {
                Debug.LogError("[MainPresenter] No se pudo inicializar el corpus para mostrar la escena Main.");
                enabled = false;
                return;
            }

            if (this == null) return;
            SuscribirEventos();
            backHandler.Configurar(OnBackPresionado);
            PaginaMain paginaDestino = ContextoNavegacion.PaginaDestino;
            ContextoNavegacion.PaginaDestino = PaginaMain.Inicio;
            CambiarPagina(paginaDestino);
        }

        private void OnDestroy() => DesuscribirEventos();

        private void SuscribirEventos()
        {
            if (view.BottomNav != null) view.BottomNav.OnTabSelected += CambiarPagina;
            if (view.Inicio != null)
            {
                view.Inicio.OnContinuarLeccion += IrALeccion;
            }
            if (view.MapaLecciones != null)
            {
                view.MapaLecciones.OnLeccionSeleccionada += IrALeccion;
                view.MapaLecciones.OnHitoRaSeleccionado += IrAEscenarioRa;
            }
            if (view.Ajustes != null)
            {
                view.Ajustes.OnGuardarPerfil += GuardarPerfil;
                view.Ajustes.OnCambiarVolumen += CambiarVolumen;
                view.Ajustes.OnCambiarAudioAutomatico += CambiarAudioAutomatico;
                view.Ajustes.OnCambiarVibracion += CambiarVibracion;
                view.Ajustes.OnConfirmarReinicio += ReiniciarProgreso;
            }
        }

        private void DesuscribirEventos()
        {
            if (view == null) return;
            if (view.BottomNav != null) view.BottomNav.OnTabSelected -= CambiarPagina;
            if (view.Inicio != null)
            {
                view.Inicio.OnContinuarLeccion -= IrALeccion;
            }
            if (view.MapaLecciones != null)
            {
                view.MapaLecciones.OnLeccionSeleccionada -= IrALeccion;
                view.MapaLecciones.OnHitoRaSeleccionado -= IrAEscenarioRa;
            }
            if (view.Ajustes != null)
            {
                view.Ajustes.OnGuardarPerfil -= GuardarPerfil;
                view.Ajustes.OnCambiarVolumen -= CambiarVolumen;
                view.Ajustes.OnCambiarAudioAutomatico -= CambiarAudioAutomatico;
                view.Ajustes.OnCambiarVibracion -= CambiarVibracion;
                view.Ajustes.OnConfirmarReinicio -= ReiniciarProgreso;
            }
        }

        public void CambiarPagina(PaginaMain pagina)
        {
            if (!System.Enum.IsDefined(typeof(PaginaMain), pagina)) pagina = PaginaMain.Inicio;
            _paginaActual = pagina;
            view.MostrarDialogoSalida(false);
            view.MostrarPagina(pagina);
            switch (pagina)
            {
                case PaginaMain.Inicio: RefrescarInicio(); break;
                case PaginaMain.Lecciones: RefrescarMapa(); break;
                case PaginaMain.Logros: RefrescarLogros(); break;
                case PaginaMain.Ajustes: RefrescarAjustes(); break;
            }
        }

        private void RefrescarInicio()
        {
            if (view.Inicio == null) return;
            PerfilUsuario perfil = PerfilRepository.CargarPerfil();
            ProgresoUsuario progreso = ProgresoRepository.ObtenerProgreso();
            view.Inicio.Mostrar(MainPresentacionService.CrearInicio(
                perfil,
                progreso,
                CorpusRepository.ObtenerTodasLecciones(),
                CorpusRepository.ObtenerTodosEscenariosRa()));
        }

        private void RefrescarMapa()
        {
            if (view.MapaLecciones == null) return;
            List<LeccionCompiladaData> lecciones = CorpusRepository.ObtenerTodasLecciones();
            List<EscenarioRAData> escenarios = CorpusRepository.ObtenerTodosEscenariosRa();
            ProgresoUsuario progreso = ProgresoRepository.ObtenerProgreso();

            view.MapaLecciones.Mostrar(
                MapaLeccionesPresentacionService.Crear(lecciones, escenarios, progreso));
        }

        private void RefrescarLogros()
        {
            if (view.Logros == null) return;
            view.Logros.Mostrar(MainPresentacionService.CrearLogros(ProgresoRepository.ObtenerProgreso()));
        }

        private void RefrescarAjustes()
        {
            PerfilUsuario perfil = PerfilRepository.CargarPerfil();
            var configuracion = ConfiguracionRepository.Obtener();
            view.Ajustes?.Mostrar(
                perfil?.nombre,
                perfil?.avatar_id,
                configuracion.volumen_general,
                configuracion.audio_automatico,
                configuracion.vibracion);
        }

        private void GuardarPerfil(string nombre, string avatarId)
        {
            PerfilUsuario perfil = PerfilRepository.CargarPerfil() ?? new PerfilUsuario();
            ResultadoValidacionPerfil validacion = PerfilOnboardingService.AplicarEdicion(
                perfil,
                nombre,
                avatarId);
            if (!validacion.Valido)
            {
                view.Ajustes?.MostrarErrorPerfil(validacion.Mensaje);
                return;
            }

            PerfilRepository.GuardarPerfil(perfil);
            view.Ajustes?.MostrarPerfilGuardado();
            RefrescarInicio();
        }

        private static void CambiarVolumen(float volumen)
        {
            float valor = Mathf.Clamp01(volumen);
            AudioListener.volume = valor;
            ConfiguracionRepository.CambiarVolumen(valor);
        }

        private static void CambiarAudioAutomatico(bool activo)
        {
            var configuracion = ConfiguracionRepository.Obtener();
            configuracion.audio_automatico = activo;
            ConfiguracionRepository.Guardar(configuracion);
        }

        private static void CambiarVibracion(bool activa)
        {
            var configuracion = ConfiguracionRepository.Obtener();
            configuracion.vibracion = activa;
            ConfiguracionRepository.Guardar(configuracion);
        }

        private void ReiniciarProgreso()
        {
            ProgresoRepository.ReiniciarProgreso();
            RefrescarMapa();
            RefrescarLogros();
            CambiarPagina(PaginaMain.Inicio);
        }

        private static void IrALeccion(string id)
        {
            NavigationService.IrALeccion(id);
        }

        private static void IrAEscenarioRa(string id) => NavigationService.IrARealidadAumentada(id);

        private void OnBackPresionado()
        {
            if (view.DialogoSalidaVisible) view.MostrarDialogoSalida(false);
            else if (_paginaActual != PaginaMain.Inicio) CambiarPagina(PaginaMain.Inicio);
            else view.MostrarDialogoSalida(true);
        }
    }
}
