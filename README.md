# Uchumataqu

**Aplicación educativa para aprender y practicar la lengua Uchumataqu**, hablada por la comunidad Irohito Urus. El proyecto se desarrolló con Unity y cuenta con el respaldo y la colaboración de **FUNPROEIB Andes**.

## Descargar la aplicación

### Android — versión 1.0.11

- [Descargar APK oficial](https://github.com/DevTicona/Uchumataqu/releases/download/v1.0.11/Uchumataqu-1.0.11.apk)
- [Ver el release 1.0.11 y sus notas](https://github.com/DevTicona/Uchumataqu/releases/tag/v1.0.11)
- [Ver todos los releases](https://github.com/DevTicona/Uchumataqu/releases)

La APK está dirigida a dispositivos **Android 9 o posterior, ARM64**. Para instalarla, descarga el archivo, ábrelo desde el teléfono y permite la instalación desde esa fuente si Android lo solicita. La aplicación puede pedir acceso al micrófono para la práctica de pronunciación y a la cámara para Realidad Aumentada. Los escenarios RA requieren compatibilidad con ARCore y Google Play Services for AR.

> **Firma de esta versión:** la APK 1.0.11 publicada está firmada con una clave de depuración. Sirve para instalar y evaluar la aplicación; no es una firma de producción para Google Play ni permite actualizar encima de una instalación firmada con otra clave.

## Qué incluye

- 26 lecciones con actividades de vocabulario, comprensión auditiva, escritura y pronunciación.
- Reconocimiento de voz ejecutado localmente para apoyar la práctica oral.
- Seis experiencias de Realidad Aumentada con vocabulario y elementos de la cultura uru.
- Progreso y perfil guardados localmente en el dispositivo; no hay sincronización entre dispositivos.

El contenido de la app se encuentra en `Assets/Content/Data`; el modelo ASR para Android está en `Assets/StreamingAssets/sherpa`.

## Repositorio de corpus y herramientas lingüísticas

El trabajo de organización y preparación del corpus y las lecciones, junto con las herramientas de **entrenamiento del ASR** y generación de recursos **HFST**, está en el repositorio complementario:

- [Uchumataqu Core — corpus, lecciones y herramientas lingüísticas](https://github.com/DevTicona/uchumataqu-core)

Este repositorio contiene el cliente móvil Unity y sus recursos de ejecución. El repositorio `uchumataqu-core` reúne el trabajo de datos y procesamiento lingüístico relacionado.

## Desarrollo y compilación

### Requisitos

- Unity **6000.5.9f1**.
- Módulo **Android Build Support**, con Android SDK, NDK y OpenJDK.
- Dispositivo ARM64 con Android 9 o posterior para instalar la APK.

### Compilar la APK oficial

1. Clona este repositorio y ábrelo desde Unity Hub.
2. Espera a que Unity restaure los paquetes e importe los recursos.
3. En el menú **Uchumataqu**, selecciona **Build APK Oficial Android**.
4. La compilación genera `Builds/Oficial/Uchumataqu-1.0.11.apk` y conserva la versión `1.0.11`.

Para crear una APK de distribución firmada, configura el keystore y las contraseñas de producción en **Project Settings > Player > Android > Publishing Settings**. No guardes claves ni contraseñas en el repositorio.

Los artefactos de compilación y la carpeta `Builds/` están excluidos de Git. La APK oficial se distribuye como asset del release; las APK de demostración se mantienen fuera de la publicación.

## Estructura del proyecto

- `Assets/Scenes`: `00_Bootstrap`, `01_Onboarding`, `02_Main`, `03_LeccionBase` y `04_RealidadAumentada`.
- `Assets/Content/Data`: contenido compilado que consume la aplicación.
- `Assets/Scripts`: lógica de la app, organizada por capas y áreas funcionales.
- `Assets/StreamingAssets/sherpa`: modelo y recursos ASR para Android.
- `Packages/manifest.json`: paquetes de Unity del proyecto.
- `ProjectSettings/EditorBuildSettings.asset`: escenas habilitadas para compilación.

La aplicación utiliza una arquitectura MVP. Los datos y el progreso se almacenan en `Application.persistentDataPath`; los recursos dinámicos se gestionan con Addressables.

## Documentación

- [Manual de usuario](MANUAL_USUARIO.md): instalación, navegación, lecciones, pronunciación y RA.

## Licencia

El código original del proyecto se distribuye bajo la [licencia MIT](LICENSE). Los paquetes, modelos, tipografías y recursos de terceros conservan sus propias licencias y atribuciones.
