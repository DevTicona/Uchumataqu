using System;
using System.IO;

namespace Assets.Scripts.Persistencia
{
    /// <summary>Confirma un archivo temporal vaciado a disco y conserva el anterior como respaldo.</summary>
    public static class EscrituraAtomica
    {
#if UNITY_EDITOR
        public static Action<string> AntesDeConfirmar;
#endif
        public static void Guardar(string ruta, byte[] contenido, bool conservarRespaldo = true)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ruta));
            string temporal = ruta + ".tmp";
            EscribirYVaciar(temporal, contenido);
#if UNITY_EDITOR
            AntesDeConfirmar?.Invoke(ruta);
#endif
            if (conservarRespaldo && File.Exists(ruta))
            {
                string respaldo = ruta + ".bak";
                EscribirYVaciar(respaldo + ".tmp", File.ReadAllBytes(ruta));
                ProteccionDatosLocal.ReemplazarAtomico(respaldo + ".tmp", respaldo);
            }
            ProteccionDatosLocal.ReemplazarAtomico(temporal, ruta);
        }

        private static void EscribirYVaciar(string ruta, byte[] contenido)
        {
            using var stream = new FileStream(ruta, FileMode.Create, FileAccess.Write, FileShare.None);
            stream.Write(contenido, 0, contenido.Length);
            stream.Flush(true);
        }
    }
}
