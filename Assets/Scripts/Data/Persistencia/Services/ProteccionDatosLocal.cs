using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Persistencia
{
    /// <summary>
    /// Autentica la persistencia con el servicio nativo de Android o DPAPI en Windows.
    /// El nombre de archivo forma parte del contexto para impedir intercambios entre documentos.
    /// </summary>
    public static class ProteccionDatosLocal
    {
        private static readonly byte[] Cabecera = Encoding.ASCII.GetBytes("UCHUMATAQU:SEC1\n");
        private const string ClaseAndroid = "com.devticona.uchumataqu.SeguridadLocal";

        public static bool EstaProtegido(byte[] contenido)
        {
            if (contenido == null || contenido.Length <= Cabecera.Length) return false;
            for (int i = 0; i < Cabecera.Length; i++)
                if (contenido[i] != Cabecera[i]) return false;
            return true;
        }

        public static byte[] Cifrar(byte[] datos, string nombre)
        {
            byte[] protegido = Transformar(datos, nombre, true);
            var resultado = new byte[Cabecera.Length + protegido.Length];
            Buffer.BlockCopy(Cabecera, 0, resultado, 0, Cabecera.Length);
            Buffer.BlockCopy(protegido, 0, resultado, Cabecera.Length, protegido.Length);
            return resultado;
        }

        public static byte[] Descifrar(byte[] datos, string nombre)
        {
            if (!EstaProtegido(datos)) throw new CryptographicException("Formato de persistencia no autenticado.");
            var contenido = new byte[datos.Length - Cabecera.Length];
            Buffer.BlockCopy(datos, Cabecera.Length, contenido, 0, contenido.Length);
            return Transformar(contenido, nombre, false);
        }

        private static byte[] Transformar(byte[] datos, string nombre, bool cifrar)
        {
            byte[] contexto = Encoding.UTF8.GetBytes("Uchumataqu/SEC1/" + nombre);
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using var api = new AndroidJavaClass(ClaseAndroid);
                return api.CallStatic<byte[]>(cifrar ? "cifrar" : "descifrar", datos, contexto);
            }
            catch (AndroidJavaException ex) { throw new CryptographicException("No se pudo autenticar la persistencia local.", ex); }
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return TransformarWindows(datos, contexto, cifrar);
#else
            throw new PlatformNotSupportedException("La protección local requiere Android o Windows.");
#endif
        }

        public static bool MigracionFinalizadaEnSistema()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var api = new AndroidJavaClass(ClaseAndroid);
            return api.CallStatic<bool>("migracionFinalizada");
#else
            return false;
#endif
        }

        public static void FinalizarMigracionEnSistema()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var api = new AndroidJavaClass(ClaseAndroid);
            api.CallStatic("finalizarMigracion");
#endif
        }

        public static void ReemplazarAtomico(string temporal, string destino)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            using var api = new AndroidJavaClass(ClaseAndroid);
            api.CallStatic("reemplazarAtomico", temporal, destino);
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            for (int intento = 0; ; intento++)
            {
                if (MoveFileEx(temporal, destino, 1 | 8)) return;
                int error = Marshal.GetLastWin32Error();
                if (intento >= 3 || (error != 32 && error != 33 && error != 5))
                    throw new IOException("No se pudo confirmar el guardado atómico.", new System.ComponentModel.Win32Exception(error));
                System.Threading.Thread.Sleep(5 << intento);
            }
#else
            if (File.Exists(destino)) File.Replace(temporal, destino, null);
            else File.Move(temporal, destino);
#endif
        }

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        [DllImport("Kernel32.dll", EntryPoint = "MoveFileExW", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool MoveFileEx(string origen, string destino, int flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct Blob { public int Longitud; public IntPtr Datos; }

        [DllImport("Crypt32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptProtectData(ref Blob entrada, string descripcion, ref Blob entropia,
            IntPtr reservado, IntPtr aviso, int flags, out Blob salida);
        [DllImport("Crypt32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptUnprotectData(ref Blob entrada, IntPtr descripcion, ref Blob entropia,
            IntPtr reservado, IntPtr aviso, int flags, out Blob salida);
        [DllImport("Kernel32.dll")] private static extern IntPtr LocalFree(IntPtr memoria);

        private static byte[] TransformarWindows(byte[] datos, byte[] contexto, bool cifrar)
        {
            var entrada = new Blob { Longitud = datos.Length, Datos = Marshal.AllocHGlobal(datos.Length) };
            var entropia = new Blob { Longitud = contexto.Length, Datos = Marshal.AllocHGlobal(contexto.Length) };
            Blob salida = default;
            try
            {
                Marshal.Copy(datos, 0, entrada.Datos, datos.Length);
                Marshal.Copy(contexto, 0, entropia.Datos, contexto.Length);
                bool ok = cifrar
                    ? CryptProtectData(ref entrada, null, ref entropia, IntPtr.Zero, IntPtr.Zero, 1, out salida)
                    : CryptUnprotectData(ref entrada, IntPtr.Zero, ref entropia, IntPtr.Zero, IntPtr.Zero, 1, out salida);
                if (!ok) throw new CryptographicException(Marshal.GetLastWin32Error());
                var resultado = new byte[salida.Longitud];
                Marshal.Copy(salida.Datos, resultado, 0, resultado.Length);
                return resultado;
            }
            finally
            {
                if (salida.Datos != IntPtr.Zero) LocalFree(salida.Datos);
                Marshal.FreeHGlobal(entrada.Datos);
                Marshal.FreeHGlobal(entropia.Datos);
            }
        }
#endif
    }
}
