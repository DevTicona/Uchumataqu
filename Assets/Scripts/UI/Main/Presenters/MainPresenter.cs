using Assets.Scripts.Data.Persistencia.Validation;
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
        [SerializeField] private NavegacionAtrasView backHandler;

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
            PaginaMain paginaDestino = ContextoNavegacionModel.PaginaDestino;
            ContextoNavegacionModel.PaginaDestino = PaginaMain.Inicio;
            CambiarPagina(paginaDestino);
        }

        private void OnDestroy()
        {
            if (backHandler != null) backHandler.Configurar(null);
            DesuscribirEventos();
        }

        private void SuscribirEventos()
        {
            view.OnConfirmarSalida += Salir;
            if (view.BottomNav != null) view.BottomNav.OnTabSelected += CambiarPagina;
            if (view.Inicio != null)
            {
                view.Inicio.OnContinuarLeccion += IrALeccion;
            }
            if (view.MapaLecciones != null)
            {
                view.MapaLecciones.OnLeccionSeleccionada += IrALeccion;
                view.MapaLecciones.OnHitoRaSeleccionado += IrAEscenarioRa;
                view.MapaLecciones.OnSpriteSolicitado += CargarSpriteIsla;
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
            view.OnConfirmarSalida -= Salir;
            if (view.BottomNav != null) view.BottomNav.OnTabSelected -= CambiarPagina;
            if (view.Inicio != null)
            {
                view.Inicio.OnContinuarLeccion -= IrALeccion;
            }
            if (view.MapaLecciones != null)
            {
                view.MapaLecciones.OnLeccionSeleccionada -= IrALeccion;
                view.MapaLecciones.OnHitoRaSeleccionado -= IrAEscenarioRa;
                view.MapaLecciones.OnSpriteSolicitado -= CargarSpriteIsla;
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
                case PaginaMain.Acerca: RefrescarAcerca(); break;
            }
        }

        private void RefrescarAcerca()
        {
            view.Acerca?.SetVersion(Application.version);
        }

        private void RefrescarInicio()
        {
            if (view.Inicio == null) return;
            PerfilUsuarioModel perfil = PerfilRepository.CargarPerfil();
            ProgresoUsuarioModel progreso = ProgresoRepository.ObtenerProgreso();
            view.Inicio.Mostrar(MainModelFactory.CrearInicio(
                perfil,
                progreso,
                CorpusRepository.ObtenerTodasLecciones(),
                CorpusRepository.ObtenerTodosEscenariosRa()));
        }

        private void RefrescarMapa()
        {
            if (view.MapaLecciones == null) return;
            List<LeccionCompiladaModel> lecciones = CorpusRepository.ObtenerTodasLecciones();
            List<EscenarioRAModel> escenarios = CorpusRepository.ObtenerTodosEscenariosRa();
            ProgresoUsuarioModel progreso = ProgresoRepository.ObtenerProgreso();

            List<IslaMapaModel> mapa = MapaLeccionesModelFactory.Crear(lecciones, escenarios, progreso);
            view.MapaLecciones.MostrarResumen(
                mapa.FindAll(isla => !isla.EsHitoRa && isla.Estado == EstadoIsla.Completada).Count,
                mapa.FindAll(isla => !isla.EsHitoRa).Count);
            view.MapaLecciones.Mostrar(mapa);
        }

        private void RefrescarLogros()
        {
            if (view.Logros == null) return;
            view.Logros.Mostrar(LogrosPresenter.CrearModel(MainModelFactory.CrearLogros(ProgresoRepository.ObtenerProgreso())));
        }

        private void RefrescarAjustes()
        {
            PerfilUsuarioModel perfil = PerfilRepository.CargarPerfil();
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
            PerfilUsuarioModel actual = PerfilRepository.CargarPerfil();
            var perfil = new PerfilUsuarioModel
            {
                version = actual.version,
                onboarding_completado = actual.onboarding_completado
            };
            ResultadoValidacionPerfilModel validacion = PerfilValidator.AplicarEdicion(
                perfil,
                nombre,
                avatarId);
            if (!validacion.Valido)
            {
                view.Ajustes?.MostrarErrorPerfil(validacion.Mensaje);
                return;
            }

            if (!PerfilRepository.GuardarPerfil(perfil)) return;
            view.Ajustes?.MostrarPerfilGuardado();
            RefrescarAjustes();
            RefrescarInicio();
        }

        private void CambiarVolumen(float volumen)
        {
            float valor = Mathf.Clamp01(volumen);
            var configuracion = ConfiguracionRepository.Obtener();
            configuracion.volumen_general = valor;
            if (ConfiguracionRepository.Guardar(configuracion)) AudioListener.volume = valor;
            else RefrescarAjustes();
        }

        private void CambiarAudioAutomatico(bool activo)
        {
            var configuracion = ConfiguracionRepository.Obtener();
            configuracion.audio_automatico = activo;
            if (!ConfiguracionRepository.Guardar(configuracion)) RefrescarAjustes();
        }

        private void CambiarVibracion(bool activa)
        {
            var configuracion = ConfiguracionRepository.Obtener();
            configuracion.vibracion = activa;
            if (!ConfiguracionRepository.Guardar(configuracion)) RefrescarAjustes();
        }

        private void ReiniciarProgreso()
        {
            if (!ProgresoRepository.ReiniciarProgreso()) return;
            RefrescarMapa();
            RefrescarLogros();
            CambiarPagina(PaginaMain.Inicio);
        }

        private static void IrALeccion(string id)
        {
            NavigationService.IrALeccion(id);
        }

        private static void IrAEscenarioRa(string id) => NavigationService.IrARealidadAumentada(id);

        private async void CargarSpriteIsla(IslaItemView isla, string id)
        {
            try
            {
                Sprite sprite = await IslaSpriteRepository.ObtenerAsync(id);
                if (this != null && isla != null) isla.MostrarSprite(sprite, id);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[MainPresenter] No se pudo cargar la isla '{id}': {ex.Message}");
            }
        }

        private static void Salir() => Application.Quit();

        private void OnBackPresionado()
        {
            if (view.DialogoSalidaVisible) view.MostrarDialogoSalida(false);
            else if (_paginaActual != PaginaMain.Inicio) CambiarPagina(PaginaMain.Inicio);
            else view.MostrarDialogoSalida(true);
        }
    }
}
