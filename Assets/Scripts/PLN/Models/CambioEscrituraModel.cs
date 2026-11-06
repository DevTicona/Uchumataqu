namespace Assets.Scripts.PLN
{
    public enum TipoCambioEscritura { Igual, Sustitucion, Faltante, Sobrante }

    public readonly struct CambioEscrituraModel
    {
        public TipoCambioEscritura Tipo { get; }
        public string Recibido { get; }
        public string Esperado { get; }

        public CambioEscrituraModel(TipoCambioEscritura tipo, string recibido, string esperado)
        {
            Tipo = tipo;
            Recibido = recibido;
            Esperado = esperado;
        }
    }
}
