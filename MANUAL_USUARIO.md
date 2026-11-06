# ANEXO F
# MANUAL DE USUARIO DE LA APLICACIÓN UCHUMATAQU

**Versión de la aplicación:** 1.0.11 (código Android 12)

## 1. PRESENTACIÓN

Uchumataqu es una aplicación móvil educativa que permite practicar el idioma de la comunidad Irohito Urus mediante 26 lecciones con actividades de vocabulario, escritura, comprensión auditiva y pronunciación. También incorpora seis escenarios de Realidad Aumentada con objetos y personajes relacionados con la cultura uru. Los contenidos y el progreso se almacenan localmente, permitiendo utilizar las actividades sin conexión a internet.

## 2. REQUISITOS E INSTALACIÓN

### 2.1. Requisitos del dispositivo

| Elemento | Requisito |
|---|---|
| Sistema operativo | Android 9 o superior |
| Arquitectura | ARM64 de 64 bits |
| Cámara y micrófono | Disponibles y con permisos habilitados |
| Realidad Aumentada | Compatibilidad con ARCore |
| Almacenamiento | Espacio suficiente para instalar la aplicación |
| Internet | No necesario para las actividades educativas |

**Nota.** El uso de Realidad Aumentada requiere Google Play Services para RA, cuya instalación o actualización puede necesitar conexión a internet.

### 2.2. Instalación de la aplicación

1. Copiar el archivo APK al teléfono.
2. Abrir el archivo desde el administrador de archivos.
3. Autorizar la instalación si Android lo solicita.
4. Pulsar **Instalar** y posteriormente **Abrir**.

La aplicación solicitará acceso a la cámara para Realidad Aumentada y al micrófono para las actividades de pronunciación. Si algún permiso fue rechazado, puede habilitarse desde los ajustes de Android.

**Figura F1.** Instalación de Uchumataqu en Android.

## 3. PRIMER INGRESO Y NAVEGACIÓN

### 3.1. Registro del usuario

Al abrir la aplicación por primera vez, se muestra una pantalla de bienvenida. Después de pulsar **CONTINUAR**, el usuario debe escribir un nombre de entre 2 y 30 caracteres, seleccionar uno de los seis avatares disponibles y pulsar **COMENZAR**. El perfil se guarda automáticamente.

**Figura F2.** Pantallas de bienvenida y registro del usuario.

### 3.2. Pantalla principal

La pantalla principal muestra el perfil, el progreso y la lección que corresponde continuar. Desde la barra inferior se puede acceder a **Inicio, Lecciones, Logros, Ajustes y Acerca**. Algunas pantallas también incluyen un icono de ayuda (?) con instrucciones sobre su funcionamiento.

**Figura F3.** Pantalla principal y opciones de navegación.

## 4. LECCIONES Y ACTIVIDADES

### 4.1. Acceso a las lecciones

Las 26 lecciones se presentan en un recorrido progresivo. Para comenzar, se ingresa a **Lecciones** y se selecciona una actividad disponible. Al completar una lección se habilita la siguiente, mientras que las pendientes permanecen bloqueadas.

**Figura F4.** Mapa de lecciones y estados de disponibilidad.

### 4.2. Actividades de aprendizaje

Durante las lecciones se presentan distintas actividades:

- **Presentación de vocabulario:** permite escuchar palabras, conocer sus significados y revisar formas documentadas.
- **Selección:** consiste en elegir la respuesta correcta según una pregunta o audio.
- **Emparejamiento:** relaciona palabras en Uchumataqu con sus significados.
- **Ordenamiento:** permite formar oraciones seleccionando palabras en el orden correcto.
- **Escritura:** solicita escribir palabras u oraciones para comprobarlas.
- **Pronunciación:** permite repetir palabras u oraciones y recibir una evaluación mediante reconocimiento de voz.

En las actividades que lo requieren, se utiliza **COMPROBAR** para evaluar la respuesta y **SIGUIENTE** para continuar.

**Figura F5.** Actividades de vocabulario, selección y emparejamiento.

**Figura F6.** Actividades de ordenamiento y escritura.

### 4.3. Evaluación y retroalimentación

Después de cada respuesta, la aplicación presenta un resultado visual:

| Resultado | Significado |
|---|---|
| Correcto (verde) | Respuesta aceptada |
| Casi (amarillo) | Respuesta con diferencias que deben revisarse |
| Incorrecto (rojo) | Respuesta no aceptada |

En escritura, también se muestran los caracteres incorrectos o faltantes y la respuesta esperada. Cuando existe información morfológica documentada, se presenta una explicación adicional.

**Figura F7.** Evaluación y retroalimentación de las respuestas.

### 4.4. Práctica de pronunciación

Para practicar la pronunciación, se debe escuchar el audio de referencia, pulsar **PRACTICAR**, decir la palabra u oración y seleccionar **DETENER** para obtener el resultado.

Si la respuesta no es aceptada, se puede repetir el ejercicio. Después de tres intentos no aprobados o ante un error técnico, la aplicación permite continuar sin registrar la respuesta como correcta.

**Figura F8.** Práctica de pronunciación y resultados del reconocimiento de voz.

### 4.5. Finalización y progreso

Al finalizar una lección, se muestran los aciertos, la precisión alcanzada y una calificación de una a tres estrellas. No se exige una puntuación mínima para completarla. El progreso se guarda localmente y puede consultarse desde Inicio, Lecciones y Logros.

**Figura F9.** Resumen de la lección y seguimiento del progreso.

## 5. REALIDAD AUMENTADA

### 5.1. Acceso y colocación

Para utilizar un escenario de Realidad Aumentada:

1. Ingresar a **Lecciones** y seleccionar un escenario desbloqueado.
2. Permitir el acceso a la cámara cuando sea necesario.
3. Apuntar hacia una superficie horizontal e iluminada.
4. Mover lentamente el teléfono hasta detectar la superficie.
5. Tocar la pantalla para colocar el escenario y pulsar **COMENZAR**.

**Figura F10.** Detección de superficies y colocación de escenarios.

### 5.2. Escenarios disponibles

| Escenario | Contenido | Lecciones necesarias |
|---|---|---|
| RA1. Encuentro y comunicación | Saludos y conversaciones | L01–L05 |
| RA2. Familia y vestimenta | Familia y prendas tradicionales | L06–L10 |
| RA3. Casa y objetos cotidianos | Objetos del hogar | L11–L14 |
| RA4. Cocina y alimentos | Alimentos y utensilios | L15–L18 |
| RA5. Animales y ecosistema | Animales terrestres y acuáticos | L19–L23 |
| RA6. Naturaleza y orientación | Naturaleza, direcciones y embarcaciones | L24–L26 |

Los escenarios incluyen actividades de exploración, selección, pronunciación, escritura y colocación de objetos virtuales. El usuario debe seguir las instrucciones mostradas en pantalla para completar cada recorrido.

**Figura F11.** Ejemplos de interacción en los escenarios de Realidad Aumentada.

### 5.3. Recolocación y salida

El botón **RECOLOCAR** permite cambiar la ubicación del escenario y reiniciar su recorrido. Si se pierde el seguimiento espacial, se recomienda orientar nuevamente la cámara hacia la superficie original.

Para abandonar la experiencia se utiliza la flecha de retorno y se confirma la salida. Al finalizar las actividades, el botón **FINALIZAR** permite regresar al mapa de lecciones.

## 6. CONFIGURACIÓN Y AJUSTES

Desde **Ajustes**, el usuario puede modificar su nombre y avatar, controlar el volumen, activar o desactivar la reproducción automática de audio y configurar la vibración.

También se dispone de la opción **Reiniciar Todo el Progreso**, que elimina el avance de las lecciones y las visitas a los escenarios RA, manteniendo el perfil y las preferencias.

**Advertencia.** El reinicio del progreso no puede deshacerse desde la aplicación.

**Figura F12.** Configuración del perfil, preferencias y reinicio del progreso.

## 7. ALMACENAMIENTO LOCAL

El perfil, las preferencias y el progreso se guardan directamente en el teléfono. Las lecciones, grabaciones y modelos tridimensionales están incluidos en la instalación, por lo que las actividades educativas no requieren conexión a internet.

**Nota.** Desinstalar la aplicación o borrar sus datos desde Android puede eliminar el progreso, ya que no existe sincronización automática entre dispositivos.

## 8. SOLUCIÓN DE PROBLEMAS FRECUENTES

| Problema | Solución recomendada |
|---|---|
| La aplicación no se instala | Verificar la compatibilidad de Android y el espacio disponible. |
| No se escucha el audio | Revisar el volumen del teléfono y de la aplicación. |
| No funciona el micrófono | Comprobar el permiso de micrófono en Android. |
| La pronunciación no se reconoce | Reducir el ruido y repetir la grabación. |
| Una lección está bloqueada | Completar las lecciones anteriores. |
| No se puede utilizar RA | Comprobar la compatibilidad con ARCore y los permisos de cámara. |
| No se detecta una superficie | Mejorar la iluminación y mover lentamente el teléfono. |
| El escenario pierde su posición | Apuntar hacia la superficie original o utilizar RECOLOCAR. |
| La aplicación responde lentamente | Cerrar otras aplicaciones y evitar el sobrecalentamiento del dispositivo. |

## 9. RECOMENDACIONES DE USO

Se recomienda avanzar por las lecciones en orden, escuchar los audios y revisar las correcciones antes de continuar. Para practicar pronunciación conviene utilizar un ambiente silencioso, mientras que las experiencias de Realidad Aumentada requieren espacios iluminados y despejados. La aplicación sirve como apoyo para el aprendizaje del Uchumataqu y puede complementarse con la orientación de docentes y personas conocedoras del idioma.
