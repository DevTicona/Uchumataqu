# Uchumataqu

Actualizado: 5 de noviembre de 2026 (America/La_Paz).

Aplicación educativa Android en Unity 6000.5.9f1, con contenido local, práctica de escritura, reconocimiento de voz y seis experiencias de realidad aumentada.

Está diseñada para el aprendizaje de la lengua indígena **Uchumataqu**. El proyecto cuenta con el respaldo y la colaboración de **FUNPROEIB Andes**.

El corpus ejecutable contiene **26 lecciones, 879 pasos de lección, 11 tipos activos y 88 pasos RA**. Las cantidades se obtienen de los JSON actuales; no representan una nueva ejecución de toda la suite de pruebas.

## Instalación en Android

1. Descarga el [APK de producción](https://github.com/DevTicona/Uchumataqu/releases/download/v1.0.0/Uchumataqu-1.0.11-produccion.apk) desde el [release v1.0.0](https://github.com/DevTicona/Uchumataqu/releases/tag/v1.0.0).
2. En un dispositivo Android 9 o superior, con arquitectura ARM64, abre el APK y autoriza la instalación desde la aplicación que utilizaste para descargarlo.
3. Concede acceso al micrófono para practicar con voz y a la cámara para las actividades de realidad aumentada. Estas actividades requieren un dispositivo compatible con ARCore y Google Play Services for AR.

El tag de distribución es `v1.0.0`; el APK precompilado conserva su versión interna `1.0.11` (código 12). El repositorio es privado: los enlaces requieren acceso autorizado a GitHub.

## Compilación

1. Clona el repositorio y abre su carpeta raíz desde Unity Hub con **Unity 6000.5.9f1**. Instala el módulo **Android Build Support**, incluidos Android SDK, NDK y OpenJDK.
2. Espera a que Unity restaure los paquetes de `Packages/manifest.json` e importe los recursos.
3. En **File > Build Profiles**, selecciona Android y activa ese perfil. Conserva las cinco escenas habilitadas de `ProjectSettings/EditorBuildSettings.asset`, empezando por `00_Bootstrap`.
4. Compila los Addressables desde **Window > Asset Management > Addressables > Groups > Build > New Build > Default Build Script**.
5. En el perfil Android, desactiva **Build App Bundle** y usa **Build** para generar un APK en `Builds/Android/`. Para distribuir actualizaciones instalables sobre la versión publicada, configura la misma clave de firma en **Player Settings > Publishing Settings**; las claves y sus contraseñas se gestionan fuera del repositorio.

Los APK son artefactos de distribución: `*.apk` está excluido mediante `.gitignore` y los binarios se publican como assets de Releases.

### Distribución con conexión inestable

El workflow manual **Ensamblar APK de Release** permite reunir en GitHub partes de un APK ya compilado. Las partes se adjuntan a un release en borrador con nombres consecutivos como `Uchumataqu-1.0.11-produccion.apk.part-001`, `.part-002`, etc. Los binarios y sus partes se mantienen fuera del código versionado.

En **Actions**, ejecuta el workflow indicando el identificador del borrador, el nombre del APK, su tamaño original en bytes y su SHA-256. El script `scripts/ensamblar_apk_release.py` comprueba la secuencia, el tamaño y las huellas antes de adjuntar el APK completo. Retira las partes temporales después de que GitHub confirme el digest del asset final. El release permanece en borrador para revisar el tag y publicar la distribución.

El script también admite `--local-parts` y `--output` para comprobar el ensamblado localmente; `--help` muestra sus parámetros. El workflow usa el token del repositorio y minutos de GitHub Actions.

## Documentación vigente

- Manual de usuario: [MANUAL_USUARIO.md](MANUAL_USUARIO.md).

El material académico ampliado está en `Docs` dentro de este entorno de trabajo.
Esa carpeta está excluida de Git y no forma parte de una copia nueva del repositorio.

## Proyecto

Las escenas son `00_Bootstrap`, `01_Onboarding`, `02_Main`, `03_LeccionBase` y `04_RealidadAumentada`, bajo `Assets/Scenes`. Las lecciones comparten una escena y los escenarios RA se instancian como prefabs Addressables.

`Assets/Content/Data` contiene los datos compilados; `Assets/Scripts` contiene la implementación. Las referencias fijas de interfaz están serializadas y los recursos dinámicos se cargan mediante Addressables. El modelo ASR local está en `Assets/StreamingAssets/sherpa`.

La presentación utiliza MVP; los gestores mantienen reglas de sesión, los repositorios acceden a datos y los motores implementan la evaluación. El progreso del usuario se guarda en `Application.persistentDataPath`.

`Docs` y `Logs` son carpetas locales ignoradas por Git según la configuración existente. Los Word y el expediente académico se conservan en `Docs`; esta limpieza no modifica esos archivos.

## Licencia

El código original del proyecto se distribuye bajo la [licencia MIT](LICENSE). Los paquetes, modelos, tipografías y demás recursos de terceros conservan sus propias licencias y atribuciones.
