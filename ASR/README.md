# Preparación ASR

Los archivos `lexicon.txt`, `tokens.txt` y `fonemas.tsv` de `Generated/` se producen de forma determinística desde
`Assets/Content/Data/palabras.json`:

```powershell
python scripts/preparar_asr.py
python -m unittest scripts/test_preparar_asr.py
```

El lexicon contiene únicamente raíces y formas pedagógicas pronunciables. Los
nombres propios contextuales de las oraciones no se reincorporan como familias ni
como objetivos de audio léxico.

El modelo acústico ONNX y sus grabaciones de entrenamiento se mantienen fuera de
`Assets/Content/Data/` hasta definir el binding final de Sherpa-onnx.
