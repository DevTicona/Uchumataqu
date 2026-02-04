using System;

[Serializable]
public class TipoActividad
{
    public string id;  // ej: "SeleccionSimple", "EmparejamientoPalabras", "CompletarTexto"

    public TipoActividad() { }

    public TipoActividad(string id)
    {
        this.id = id;
    }

    public override string ToString()
    {
        return id ?? "";
    }

    public override bool Equals(object obj)
    {
        return obj is TipoActividad other && other.id == this.id;
    }

    public override int GetHashCode()
    {
        return id?.GetHashCode() ?? 0;
    }

    // Métodos helper para comparación
    public bool Es(string tipoId) => id == tipoId;
}
