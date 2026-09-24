# Reporte de composición: Pages de actividades

Este documento describe la composición actual de `Assets/Scenes/03_LeccionBase.unity` para rediseñar los bloques visuales sin romper la creación de actividades en tiempo de ejecución.

## Arquitectura compartida

- Las siete páginas están dentro de `Canvas/SafeArea/ActividadHost`.
- Cada página es un objeto de escena, **no un prefab independiente**. Su estructura común es:

  ```text
  PageX
  └─ ScrollRect
     └─ Viewport (RectMask2D)
        └─ Content (VerticalLayoutGroup + ContentSizeFitter vertical: Preferred Size)
  ```

- `Content` es quien determina la altura total desplazable. Los bloques internos deben participar en el `VerticalLayoutGroup` mediante su `LayoutElement`; no se debe depender de alturas manuales de `RectTransform` cuando un layout controle ese objeto.
- Todas las páginas usan `ActividadLayoutController` para solicitar una reconstrucción después de cambiar datos, fichas, feedback o tamaños.
- El encabezado de lección, el footer (`FooterLeccion/BtnPrincipal`), el feedback y los modales están fuera de las Pages. No los conviertas en hijos de `Content`.
- En ejecución el presentador activa una sola Page con `LeccionBaseView.Instanciar...()` y desactiva las demás. Los hijos dinámicos se generan al llamar `Configurar(...)`.

## Mapa rápido de pages, tipos y prefabs

| Page de escena | Vista | Tipos de actividad del JSON | Prefab dinámico |
| --- | --- | --- | --- |
| `PagePresentacionFamilia` | `PresentacionFamiliaView` | `presentacion_familia` | `fila_forma_familia.prefab` |
| `PageSeleccionMultiple` | `SeleccionMultipleView` | `uch_a_es`, `es_a_uch`, `audio_a_uch`, `audio_a_es`, `forma_a_raiz`, `forma_a_es`, `es_a_forma`, `audio_a_forma`, `completar_oracion`, `audio_a_es_oracion` | `opcion_respuesta.prefab` |
| `PageEmparejamiento` | `EmparejamientoView` | `emparejar_uch_es`, `emparejar_audio_uch`, `familia_completa` | `item_emparejamiento.prefab` |
| `PageMemoria` | `MemoriaView` | `memoria_uch_es` | `tarjeta_memoria.prefab` |
| `PageWordBank` | `WordBankView` | `ordenar_oracion`, `dictado_word_bank`, `traduccion_constructiva` | `ficha_wordbank.prefab` |
| `PageEscritura` | `EscrituraView` | `escribir_palabra`, `escribir_forma`, `escribir_oracion` | No usa prefab de respuesta; el panel de corrección se crea por código cuando corresponde. |
| `PageASR` | `ASRView` | `escucha_y_repite_asr`, `escucha_y_repite_oracion_asr` | No genera prefabs. |

Los prefabs reutilizables se encuentran en `Assets/Content/Prefabs/Actividades/`.

## 1. PagePresentacionFamilia

**Propósito:** mostrar una palabra raíz, escucharla, practicar pronunciación y explorar sus formas documentadas.

```text
PagePresentacionFamilia
└─ ScrollRect / Viewport / Content
   ├─ EncabezadoTxtIndicacion
   │  ├─ IconoActividad
   │  └─ TxtIndicacion
   ├─ CardRaiz
   │  ├─ Uch
   │  ├─ Esp
   │  └─ AccionesPresentacion
   │     ├─ BtnAudioRaiz
   │     └─ BtnPracticarRaiz
   └─ FormasContainer
      ├─ ContentTitulo / TxtTitulo
      └─ Items
         └─ instancias de fila_forma_familia
```

- **Prefab usado:** `fila_forma_familia.prefab`.
- **Estructura contractual del prefab:** `ColTexto/TxtUchForma`, `ColTexto/TxtEsForma`, `BtnEscuchar` y `BtnPracticar`. Estos nombres los busca `PresentacionFamiliaView`; no deben renombrarse sin actualizar el código.
- **Bloque dinámico:** `FormasContainer/Items`. Al iniciar la actividad se eliminan las filas de muestra y se crean tantas filas como formas tenga la palabra del corpus.
- **Altura:** `FormasContainer` y `Items` usan `VerticalLayoutGroup` y `LayoutElement`. `PresentacionFamiliaView` calcula la altura preferida total de las filas; por eso conviene modificar padding, spacing y altura mínima en esos objetos, no fijar el alto final de `FormasContainer`.
- **Acciones:** audio y micrófono de raíz; cada fila tiene audio y micrófono propios.
- **Para rediseñar:** mantener `Items` como contenedor vertical y reservar un bloque de título separado. Se puede cambiar por completo el card raíz, el orden visual y los sprites, siempre que se preserven las referencias serializadas y los nombres internos contractuales de la fila.

## 2. PageSeleccionMultiple

**Propósito:** resolver preguntas de traducción, reconocimiento por audio, formas gramaticales o completar un hueco.

```text
PageSeleccionMultiple
└─ ScrollRect / Viewport / Content
   ├─ EncabezadoTxtInstruccion
   ├─ EstimuloHost
   │  ├─ PanelTexto / TxtEstimulo
   │  ├─ PanelOracionHueco / TxtOracion
   │  └─ BtnAudioPregunta
   └─ OpcionesContainer
      ├─ TxtAyudaOpciones
      └─ Items / instancias de opcion_respuesta
```

- **Prefab usado:** `opcion_respuesta.prefab`.
- **Bloque dinámico:** `OpcionesContainer`. Se crean opciones hasta alcanzar la cantidad indicada por el paso; las instancias se reutilizan y las sobrantes se ocultan.
- **Variantes visuales que la misma Page debe soportar:**
  - pregunta de texto: muestra `PanelTexto`;
  - completar oración: muestra `PanelOracionHueco`;
  - pregunta por audio: oculta el estímulo de texto y muestra el botón de audio.
- **Acción principal:** el footer queda deshabilitado hasta elegir una opción; luego muestra `COMPROBAR`.
- **Para rediseñar:** no eliminar `PanelTexto`, `PanelOracionHueco`, `BtnAudioPregunta` ni el contenedor de opciones. Es recomendable que el estímulo tenga altura flexible según el número de líneas y que las opciones tengan altura preferida basada en texto, especialmente para traducciones largas.

## 3. PageEmparejamiento

**Propósito:** asociar palabras con significados, audio con palabra o completar una familia.

```text
PageEmparejamiento
└─ ScrollRect / Viewport / Content
   ├─ Encabezado / TxtInstruccion
   └─ PairArea (HorizontalLayoutGroup)
      ├─ ColumnaIzquierda
      │  └─ instancias de item_emparejamiento
      └─ ColumnaDerecha
         └─ instancias de item_emparejamiento
```

- **Prefab usado en ambas columnas:** `item_emparejamiento.prefab`.
- **Bloques dinámicos:** `ColumnaIzquierda` y `ColumnaDerecha`. El mismo número de botones se crea en ambas columnas; el contenido se baraja de forma independiente.
- **Variante de audio:** en `emparejar_audio_uch`, los ítems de la izquierda muestran el estado/botón de audio y el texto se reemplaza por “Escuchar n”.
- **Para rediseñar:** conservar las dos columnas como contenedores distintos y mantener un ancho flexible equivalente. Si en móvil las palabras son extensas, aumentar alto por ítem o permitir varias líneas; no convertir el área a una lista única sin adaptar la interacción de dos columnas.

## 4. PageMemoria

**Propósito:** encontrar pares de palabra y significado.

```text
PageMemoria
└─ ScrollRect / Viewport / Content
   ├─ EncabezadoTxtInstruccion
   ├─ Ayuda
   └─ GridTarjetas (GridLayoutGroup)
      └─ instancias de tarjeta_memoria
```

- **Prefab usado:** `tarjeta_memoria.prefab`.
- **Bloque dinámico:** `GridTarjetas`; por cada par se generan dos tarjetas y se barajan.
- **Layout actual:** grilla de dos columnas. El tamaño de celda del `GridLayoutGroup` es la autoridad del tamaño de las tarjetas.
- **Para rediseñar:** conservar la cuadrícula para que la mecánica sea reconocible. Ajustar `cellSize`, spacing y el aspecto del prefab conjuntamente; si cambias cantidad de columnas, revisar teléfonos angostos y el número máximo de pares del corpus.

## 5. PageWordBank

**Propósito:** ordenar una oración, construir una respuesta desde dictado o traducir construyendo la oración.

```text
PageWordBank
└─ ScrollRect / Viewport / Content
   ├─ EncabezadoTxtInstruccion
   ├─ EstimuloHost
   │  ├─ TxtEstimulo
   │  └─ BtnAudio
   ├─ TxtRespuesta
   ├─ RespuestaRows (contenedorDestino)
   ├─ TxtBanco
   └─ BancoRows (contenedorOrigen)
      └─ filas temporales / instancias de ficha_wordbank
```

- **Prefab usado:** `ficha_wordbank.prefab`.
- **Bloques dinámicos:** `RespuestaRows` y `BancoRows`.
- **Distribución especial:** `WordBankView` crea filas horizontales temporales llamadas `Row` y reparte las fichas según el ancho disponible. El ancho del contenedor determina cuándo una ficha pasa a otra fila; se reconstruye si cambia el ancho del dispositivo.
- **Variantes:** `dictado_word_bank` muestra el botón de audio y puede ocultar el estímulo textual; las otras variantes muestran la consigna correspondiente.
- **Para rediseñar:** no añadir `HorizontalLayoutGroup` fijo directamente a `BancoRows` o `RespuestaRows`, porque el código ya crea las filas internas. Mantener esos dos como contenedores verticales. El ancho y padding de ambos impactan directamente en el salto de línea de las fichas.

## 6. PageEscritura

**Propósito:** escribir una palabra, una forma o una oración.

```text
PageEscritura
└─ ScrollRect / Viewport / Content
   ├─ EncabezadoTxtEscritura
   ├─ EstimuloEscritura / TxtInstruccion
   ├─ BtnAudioReferencia (opcional según el paso)
   ├─ InputRespuesta
   │  └─ Text Area / Placeholder / Text
   └─ RespuestaCorrecta (se crea al primer error o resultado "casi")
      ├─ Etiqueta
      └─ Texto
```

- **Prefabs usados:** ninguno para la entrada o la corrección.
- **Bloque condicional:** `BtnAudioReferencia` se activa solo cuando la actividad tiene audio.
- **Bloque generado por código:** `RespuestaCorrecta` se inserta debajo de `InputRespuesta`, con `LayoutElement` de alto preferido. No conviene crear manualmente otro objeto con ese mismo nombre.
- **Para rediseñar:** `InputRespuesta` debe conservar `TMP_InputField`, `Text Area`, `Text` y `Placeholder`, además de `RectMask2D` para el recorte del texto. Mantener el panel de respuesta correcta dentro del flujo vertical, justo después del input.

## 7. PageASR

**Propósito:** escuchar una referencia y practicar pronunciación de palabra u oración.

```text
PageASR
└─ ScrollRect / Viewport / Content
   ├─ EncabezadoTxtInstruccion
   ├─ Referencia
   │  ├─ EtiquetaUch
   │  └─ TxtObjetivo
   └─ AccionesPronunciacion
      ├─ BtnAudioReferencia
      └─ BtnPracticar
```

- **Prefabs usados:** ninguno; todos los controles están en la Page.
- **Variantes:** para oraciones el botón dice “ESCUCHAR ORACIÓN”; para palabras, “ESCUCHAR PALABRA”.
- **Estados visuales:** `Listo`, `Grabando`, `Procesando`, `Resultado`, `NoDisponible` y `Error`. La vista cambia texto, icono e interactividad de los botones, por lo que conviene conservar ambos botones y su hijo `Icono`.
- **Para rediseñar:** el bloque `AccionesPronunciacion` usa dos botones de ancho flexible. Puede convertirse a pila vertical en pantallas estrechas, pero hay que preservar ambas referencias y permitir que el footer común cambie temporalmente a `DETENER` durante una grabación.

## Prefabs reutilizables y responsabilidades

| Archivo | Usado por | Debe conservar |
| --- | --- | --- |
| `Assets/Content/Prefabs/Actividades/fila_forma_familia.prefab` | Presentación | `ColTexto`, textos `TxtUchForma` y `TxtEsForma`, botones `BtnEscuchar` y `BtnPracticar`. |
| `Assets/Content/Prefabs/Actividades/opcion_respuesta.prefab` | Selección múltiple | Componente `Button`, `Image`, texto TMP y, si se mantiene el estilo actual, `TarjetaSeleccionView`. |
| `Assets/Content/Prefabs/Actividades/item_emparejamiento.prefab` | Emparejamiento | Componente `Button`, texto TMP e `IconoAudio` para la variante de audio. |
| `Assets/Content/Prefabs/Actividades/tarjeta_memoria.prefab` | Memoria | Componente `Button`, texto TMP y la presentación de estado oculta/mostrada por `MemoriaView`. |
| `Assets/Content/Prefabs/Actividades/ficha_wordbank.prefab` | Word bank | Componente `Button`, texto TMP de una línea y `LayoutElement`, cuyo ancho se recalcula por código. |

## Reglas de rediseño seguro

1. Cambiar sprites, márgenes, tipografía, colores, orden de bloques y tamaños mínimos es seguro si se conservan las referencias serializadas de cada View.
2. No renombrar rutas internas que el código busca con `Transform.Find`, especialmente las de `fila_forma_familia`.
3. Mantener los contenedores dinámicos como hijos de `Content` y conservar sus `LayoutGroup`/`LayoutElement` cuando la actividad los usa para calcular tamaño.
4. Probar cada Page con contenido corto y largo. Las más sensibles a ancho son selección, emparejamiento y word bank; las más sensibles a alto son presentación con formas y escritura con respuesta correcta.
5. Los datos de prueba se pueden mostrar desde `Uchumataqu > UI > Vista previa de LeccionBase`; esa herramienta usa pasos reales de `lecciones_compiladas.json` y no reemplaza el flujo de Play.
