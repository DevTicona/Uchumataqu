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

        private void Start()
        {
            if (view == null || backHandler == null)
            {
                Debug.LogError("[MainPresenter] Faltan referencias de composición de la escena.");
                enabled = false;
                return;
            }

            SuscribirEventos();
            backHandler.Configurar(OnBackPresionado);
            CambiarPagina(ContextoNavegacion.PaginaDestino);
        }

        private void OnDestroy() => DesuscribirEventos();

        private void SuscribirEventos()
        {
            if (view.BottomNav != null) view.BottomNav.OnTabSelected += CambiarPagina;
            if (view.Inicio != null)
            {
                view.Inicio.OnSolicitarActualizacion += RefrescarInicio;
                view.Inicio.OnContinuarLeccion += IrALeccion;
            }
            if (view.MapaLecciones != null)
            {
                view.MapaLecciones.OnSolicitarActualizacion += RefrescarMapa;
                view.MapaLecciones.OnLeccionSeleccionada += IrALeccion;
                view.MapaLecciones.OnHitoRaSeleccionado += IrAEscenarioRa;
            }
            if (view.Logros != null) view.Logros.OnSolicitarActualizacion += RefrescarLogros;
            if (view.Ajustes != null)
            {
                view.Ajustes.OnSolicitarActualizacion += RefrescarAjustes;
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
                view.Inicio.OnSolicitarActualizacion -= RefrescarInicio;
                view.Inicio.OnContinuarLeccion -= IrALeccion;
            }
            if (view.MapaLecciones != null)
            {
                view.MapaLecciones.OnSolicitarActualizacion -= RefrescarMapa;
                view.MapaLecciones.OnLeccionSeleccionada -= IrALeccion;
                view.MapaLecciones.OnHitoRaSeleccionado -= IrAEscenarioRa;
            }
            if (view.Logros != null) view.Logros.OnSolicitarActualizacion -= RefrescarLogros;
            if (view.Ajustes != null)
            {
                view.Ajustes.OnSolicitarActualizacion -= RefrescarAjustes;
                view.Ajustes.OnGuardarPerfil -= GuardarPerfil;
                view.Ajustes.OnCambiarVolumen -= CambiarVolumen;
                view.Ajustes.OnCambiarAudioAutomatico -= CambiarAudioAutomatico;
                view.Ajustes.OnCambiarVibracion -= CambiarVibracion;
                view.Ajustes.OnConfirmarReinicio -= ReiniciarProgreso;
            }
        }

        public void CambiarPagina(PaginaMain pagina)
        {
            _paginaActual = pagina;
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
                perfil?.rango_edad,
                perfil?.avatar_id,
                configuracion.volumen_general,
                configuracion.audio_automatico,
                configuracion.vibracion);
        }

        private void GuardarPerfil(string nombre, string rangoEdad, string avatarId)
        {
            PerfilUsuario perfil = PerfilRepository.CargarPerfil() ?? new PerfilUsuario();
            ResultadoValidacionPerfil validacion = PerfilOnboardingService.AplicarEdicion(
                perfil,
                nombre,
                rangoEdad,
                avatarId);
            if (!validacion.Valido)
            {
                view.Ajustes?.MostrarErrorPerfil(validacion.Mensaje);
                return;
            }

            PerfilRepository.GuardarPerfil(perfil);
            view.Ajustes?.MostrarPerfilGuardado();
            RefrescarInicio();
            AudioService.Instance.ReproducirEfecto("click_boton");
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

        private static void ReiniciarProgreso()
        {
            ProgresoRepository.ReiniciarProgreso();
            NavigationService.IrABootstrap();
        }

        private static void IrALeccion(string id)
        {
            AudioService.Instance.ReproducirEfecto("click_boton");
            NavigationService.IrALeccion(id);
        }

        private static void IrAEscenarioRa(string id) => NavigationService.IrARealidadAumentada(id);

        private void OnBackPresionado()
        {
            if (_paginaActual != PaginaMain.Inicio) CambiarPagina(PaginaMain.Inicio);
            else view.MostrarDialogoSalida(true);
        }
    }
}
