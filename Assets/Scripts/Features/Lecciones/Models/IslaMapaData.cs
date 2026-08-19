namespace Assets.Scripts.Features.Lecciones.Models
{
    public enum TipoIsla { Leccion, RA }

    /// <summary>
    /// Representa una isla en el mapa, ya sea una lección normal o una isla RA de tema.
    /// </summary>
    public class IslaMapaData
    {
        public string       Id     { get; }
        public string       Nombre { get; }
        public TipoIsla     Tipo   { get; }
        public string       IdTema { get; }
        public int          Orden  { get; }
        public EstadoLeccion Estado { get; }

        public IslaMapaData(string id, string nombre, TipoIsla tipo, string idTema, int orden, EstadoLeccion estado)
        {
            Id     = id;
            Nombre = nombre;
            Tipo   = tipo;
            IdTema = idTema;
            Orden  = orden;
            Estado = estado;
        }
    }
}