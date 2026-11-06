package com.devticona.uchumataqu;

import android.content.res.AssetManager;
import android.security.keystore.KeyGenParameterSpec;
import android.security.keystore.KeyProperties;
import android.system.Os;
import android.system.OsConstants;
import android.system.ErrnoException;
import java.io.File;
import java.io.FileDescriptor;
import java.io.InputStream;
import java.security.KeyStore;
import java.security.MessageDigest;
import java.util.Arrays;
import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.GCMParameterSpec;

/** Primitivas del sistema operativo; ninguna clave se exporta a archivos o a C#. */
public final class SeguridadLocal {
    private static final String CLAVE = "uchumataqu.persistencia.aes.v1";
    private static final String MIGRACION = "uchumataqu.persistencia.migrada.v1";
    private SeguridadLocal() { }

    private static KeyStore almacen() throws Exception {
        KeyStore store = KeyStore.getInstance("AndroidKeyStore");
        store.load(null);
        return store;
    }

    private static SecretKey generar(String alias) throws Exception {
        KeyGenerator generator = KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, "AndroidKeyStore");
        generator.init(new KeyGenParameterSpec.Builder(alias,
                KeyProperties.PURPOSE_ENCRYPT | KeyProperties.PURPOSE_DECRYPT)
                .setKeySize(256).setBlockModes(KeyProperties.BLOCK_MODE_GCM)
                .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                .setRandomizedEncryptionRequired(true).build());
        return generator.generateKey();
    }

    public static synchronized byte[] cifrar(byte[] datos, byte[] contexto) throws Exception {
        KeyStore store = almacen();
        SecretKey key = store.containsAlias(CLAVE) ? (SecretKey) store.getKey(CLAVE, null) : generar(CLAVE);
        Cipher cipher = Cipher.getInstance("AES/GCM/NoPadding");
        cipher.init(Cipher.ENCRYPT_MODE, key); // El proveedor genera un IV aleatorio nuevo.
        cipher.updateAAD(contexto);
        byte[] contenido = cipher.doFinal(datos);
        byte[] iv = cipher.getIV();
        if (iv.length != 12) throw new SecurityException("IV no compatible");
        byte[] resultado = new byte[iv.length + contenido.length];
        System.arraycopy(iv, 0, resultado, 0, iv.length);
        System.arraycopy(contenido, 0, resultado, iv.length, contenido.length);
        return resultado;
    }

    public static synchronized byte[] descifrar(byte[] datos, byte[] contexto) throws Exception {
        if (datos.length < 28) throw new SecurityException("Archivo protegido incompleto");
        KeyStore store = almacen();
        if (!store.containsAlias(CLAVE)) throw new SecurityException("Clave local no disponible");
        Cipher cipher = Cipher.getInstance("AES/GCM/NoPadding");
        cipher.init(Cipher.DECRYPT_MODE, (SecretKey) store.getKey(CLAVE, null),
                new GCMParameterSpec(128, Arrays.copyOfRange(datos, 0, 12)));
        cipher.updateAAD(contexto);
        return cipher.doFinal(datos, 12, datos.length - 12);
    }

    public static boolean migracionFinalizada() throws Exception {
        return almacen().containsAlias(MIGRACION);
    }

    public static synchronized void finalizarMigracion() throws Exception {
        if (!migracionFinalizada()) generar(MIGRACION);
    }

    public static void reemplazarAtomico(String temporal, String destino) throws Exception {
        Os.rename(temporal, destino);
        FileDescriptor directorio = Os.open(new File(destino).getParent(), OsConstants.O_RDONLY, 0);
        try {
            try { Os.fsync(directorio); }
            catch (ErrnoException ex) { if (ex.errno != OsConstants.EINVAL) throw ex; }
        }
        finally { Os.close(directorio); }
    }

    public static boolean verificarAsset(AssetManager assets, String ruta, String esperado) throws Exception {
        MessageDigest hash = MessageDigest.getInstance("SHA-256");
        try (InputStream stream = assets.open(ruta)) {
            byte[] buffer = new byte[32768];
            int n;
            while ((n = stream.read(buffer)) != -1) hash.update(buffer, 0, n);
        }
        StringBuilder valor = new StringBuilder(64);
        for (byte b : hash.digest()) valor.append(String.format(java.util.Locale.ROOT, "%02x", b & 255));
        return MessageDigest.isEqual(valor.toString().getBytes(java.nio.charset.StandardCharsets.US_ASCII),
                esperado.getBytes(java.nio.charset.StandardCharsets.US_ASCII));
    }
}
