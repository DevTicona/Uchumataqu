using System.Collections.Generic;
using System.Linq;
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
                view.Ajustes.OnGuardarNombre += GuardarNombre;
                view.Ajustes.OnCambiarVolumen += CambiarVolumen;
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
                view.Ajustes.OnGuardarNombre -= GuardarNombre;
                view.Ajustes.OnCambiarVolumen -= CambiarVolumen;
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
            int completadas = progreso?.TotalLeccionesCompletadas ?? 0;
            int siguiente = Mathf.Clamp(completadas + 1, 1, 27);
            string id = $"L{siguiente:D2}";
            LeccionCompiladaData leccion = CorpusRepository.ObtenerLeccion(id);

            view.Inicio.Mostrar(new InicioPresentacionData
            {
                Nombre = perfil?.nombre,
                LeccionId = id,
                TituloLeccion = leccion?.titulo ?? $"Lección {siguiente}",
                LeccionesCompletadas = completadas,
                EscenariosVisitados = progreso?.TotalEscenariosVisitados ?? 0
            });
        }

        private void RefrescarMapa()
        {
            if (view.MapaLecciones == null) return;
            List<LeccionCompiladaData> lecciones = CorpusRepository.ObtenerTodasLecciones();
            List<EscenarioRAData> escenarios = CorpusRepository.ObtenerTodosEscenariosRa();
            ProgresoUsuario progreso = ProgresoRepository.ObtenerProgreso();

            var hitoPorUltimaLeccion = escenarios
                .Where(e => e.lecciones_asociadas != null && e.lecciones_asociadas.Count > 0)
                .ToDictionary(e => e.lecciones_asociadas[e.lecciones_asociadas.Count - 1], e => e);

            var islas = new List<IslaMapaPresentacionData>();
            for (int i = 0; i < lecciones.Count; i++)
            {
                LeccionCompiladaData leccion = lecciones[i];
                bool completada = progreso != null && progreso.LeccionCompletada(leccion.id);
                bool disponible = i == 0 || progreso != null && progreso.LeccionCompletada(lecciones[i - 1].id);
                islas.Add(new IslaMapaPresentacionData
                {
                    LeccionId = leccion.id,
                    Numero = i + 1,
                    Titulo = leccion.titulo,
                    Estado = completada ? EstadoIsla.Completada : disponible ? EstadoIsla.Disponible : EstadoIsla.Bloqueada
                });

                if (!hitoPorUltimaLeccion.TryGetValue(leccion.id, out EscenarioRAData escenario)) continue;
                bool desbloqueado = ProgresoRepository.EstaEscenarioDesbloqueado(escenario.id);
                bool visitado = progreso != null && progreso.EscenarioVisitado(escenario.id);
                islas.Add(new IslaMapaPresentacionData
                {
                    EscenarioRaId = escenario.id,
                    Titulo = escenario.titulo,
                    Estado = visitado ? EstadoIsla.Completada : desbloqueado ? EstadoIsla.Disponible : EstadoIsla.Bloqueada
                });
            }

            view.MapaLecciones.Mostrar(islas);
        }

        private void RefrescarLogros()
        {
            if (view.Logros == null) return;
            ProgresoUsuario progreso = ProgresoRepository.ObtenerProgreso();
            int completadas = progreso?.TotalLeccionesCompletadas ?? 0;
            int visitados = progreso?.TotalEscenariosVisitados ?? 0;
            view.Logros.Mostrar(new List<LogroPresentacionData>
            {
                new LogroPresentacionData { Id = "l1", Titulo = "Primer paso", Descripcion = "Completa tu primera lección.", Desbloqueado = completadas >= 1 },
                new LogroPresentacionData { Id = "l5", Titulo = "Caminante del lago", Descripcion = "Completa 5 lecciones.", Desbloqueado = completadas >= 5 },
                new LogroPresentacionData { Id = "l10", Titulo = "Guardián de la palabra", Descripcion = "Completa 10 lecciones.", Desbloqueado = completadas >= 10 },
                new LogroPresentacionData { Id = "ra1", Titulo = "Visión aumentada", Descripcion = "Visita tu primer escenario RA.", Desbloqueado = visitados >= 1 },
                new LogroPresentacionData { Id = "ra3", Titulo = "Explorador Uru", Descripcion = "Visita 3 escenarios RA.", Desbloqueado = visitados >= 3 },
                new LogroPresentacionData { Id = "l27", Titulo = "Heredero de la lengua", Descripcion = "Completa las 27 lecciones.", Desbloqueado = completadas >= 27 }
            });
        }

        private void RefrescarAjustes()
        {
            PerfilUsuario perfil = PerfilRepository.CargarPerfil();
            view.Ajustes?.Mostrar(perfil?.nombre, AudioListener.volume);
        }

        private void GuardarNombre(string nombre)
        {
            PerfilUsuario perfil = PerfilRepository.CargarPerfil() ?? new PerfilUsuario();
            perfil.nombre = nombre;
            PerfilRepository.GuardarPerfil(perfil);
            AudioService.Instance.ReproducirEfecto("click_boton");
        }

        private static void CambiarVolumen(float volumen) => AudioListener.volume = Mathf.Clamp01(volumen);

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
