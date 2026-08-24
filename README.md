# Uchumataqu

Aplicación móvil educativa offline desarrollada en Unity para el aprendizaje progresivo del idioma Uchumataqu mediante 27 lecciones, práctica oral opcional y seis experiencias de Realidad Aumentada.

## Fuentes de verdad

Cuando dos documentos difieran, se aplica este orden:

1. Los JSON compilados de `Assets/Content/Data/` definen IDs, contenido y tipos de pasos.
2. `Docs/Specifications/04-flujo-leccion-app.md` define la secuencia pedagógica continua, sin bloques A/B.
3. `Docs/Specifications/03-modulo-ra.md`, `03-1-diseno-escenarios-ra.md` y `escenarios_ra.json` definen los hitos RA.
4. `Docs/Specifications/08-patron-arquitectonico-mvp.md` define dependencias y responsabilidades de código.
5. `Docs/Specifications/07-cronograma-implementacion.md` organiza el trabajo, pero no redefine contratos técnicos.

## Arquitectura

El proyecto utiliza MVP (Model–View–Presenter) con Services/Gestores, Repositories y Motores puros.

```text
View -> Presenter -> Service/Gestor -> Repository
                     |
                     +-> Motor
```

- View: presentación e interacción; recibe datos y emite eventos.
- Presenter: coordina la View y los casos de uso.
- Service/Gestor: concentra lógica de aplicación reutilizable.
- Repository: centraliza lectura y escritura de datos.
- Motor: implementa algoritmos determinísticos reutilizables.

Las reglas completas están en [08-patron-arquitectonico-mvp.md](Docs/Specifications/08-patron-arquitectonico-mvp.md).

## Escenas oficiales

1. `00_Bootstrap`
2. `01_Onboarding`
3. `02_Main`
4. `03_LeccionBase`
5. `04_RealidadAumentada`

No se crea una escena por lección ni por escenario RA.

## Flujo pedagógico

Cada lección consume su secuencia de `pasos` desde `lecciones_compiladas.json`. No existen bloques A/B ni agrupaciones rígidas de cuatro palabras. Presentaciones, actividades léxicas, actividades de oración y práctica oral opcional forman una secuencia continua.

Una lección se completa al recorrer todos sus pasos obligatorios. La precisión se registra como desempeño, pero no bloquea la finalización ni el desbloqueo de RA. La práctica ASR es opcional y no bloqueante.

## Hitos de Realidad Aumentada

| Escenario | Lecciones requeridas | Posición |
|---|---|---|
| Encuentro y comunicación | L01–L06 | después de L06 |
| Familia y vestimenta | L07–L11 | después de L11 |
| Casa y objetos cotidianos | L12–L15 | después de L15 |
| Cocina y alimentos | L16–L19 | después de L19 |
| Animales y ecosistema | L20–L24 | después de L24 |
| Naturaleza y orientación | L25–L27 | después de L27 |

La posición se deriva de la última entrada de `lecciones_asociadas`; no se hardcodea en UI.

## Datos y recursos offline

- Corpus: `Assets/Content/Data/`
- Audio de palabras y formas: `Assets/Content/Audio/Palabras/`
- Audio de oraciones: `Assets/Content/Audio/Oraciones/`
- Claves canónicas: `audio_palabra/{id}` y `audio_oracion/{id}`
- Todos los recursos de runtime se resuelven mediante Addressables.
- La persistencia del usuario se guarda bajo `Application.persistentDataPath`.

## Validación mínima

1. Compilar sin errores.
2. Ejecutar Edit Mode Tests.
3. Validar integridad del corpus y contrato de tipos de pasos.
4. Revisar referencias críticas y duplicados de las cinco escenas.
5. Probar en Play Mode al menos L01 y una lección afectada.
6. Para cambios móviles, validar en Android real y modo avión.

La guía detallada está en `Docs/Specifications/guia-pruebas-validacion.md`.
