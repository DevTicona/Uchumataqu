#!/usr/bin/env python3
"""Genera el lexicon y los tokens ASR desde el corpus pedagógico vigente."""

from __future__ import annotations

import argparse
import json
import re
import unicodedata
from pathlib import Path
from typing import Iterable


FONEMAS_CANONICOS = frozenset(
    {
        "A", "AA", "E", "EE", "I", "II", "O", "OO", "U", "UU",
        "P", "T", "K", "Q", "M", "N", "R", "S", "X", "J", "L", "LL", "Y", "W",
        "TS", "CH", "SH", "NY", "PH", "TH", "KH", "QH", "CHH",
        "P_EJ", "T_EJ", "K_EJ", "Q_EJ", "CH_EJ",
    }
)

SIMBOLOS_ESPECIALES = ("<blk>", "<sos/eos>", "<unk>", "<sil>")

MAPEO_IPA = {
    "A": "a", "AA": "aː", "E": "e", "EE": "eː", "I": "i", "II": "iː",
    "O": "o", "OO": "oː", "U": "u", "UU": "uː",
    "P": "p", "T": "t", "K": "k", "Q": "q", "M": "m", "N": "n", "R": "r", "S": "s",
    "J": "x", "X": "χ", "L": "l", "LL": "ʎ", "Y": "j", "W": "w",
    "TS": "t͡s", "CH": "t͡ʃ", "SH": "ʃ", "NY": "ɲ",
    "PH": "pʰ", "TH": "tʰ", "KH": "kʰ", "QH": "qʰ", "CHH": "t͡ʃʰ",
    "P_EJ": "pʼ", "T_EJ": "tʼ", "K_EJ": "kʼ", "Q_EJ": "qʼ", "CH_EJ": "t͡ʃʼ",
}

# Cada patrón se evalúa en la posición actual. Los grafemas más largos preceden
# estrictamente a sus prefijos para que, por ejemplo, tsh no se segmente como th+s.
REGLAS_G2P = (
    (re.compile(r"ch['’ʼ]"), ("CH_EJ",)),
    (re.compile(r"chh"), ("CHH",)),
    (re.compile(r"tsh"), ("T", "SH")),
    (re.compile(r"ph"), ("PH",)),
    (re.compile(r"th"), ("TH",)),
    (re.compile(r"kh"), ("KH",)),
    (re.compile(r"qh"), ("QH",)),
    (re.compile(r"p['’ʼ]"), ("P_EJ",)),
    (re.compile(r"t['’ʼ]"), ("T_EJ",)),
    (re.compile(r"k['’ʼ]"), ("K_EJ",)),
    (re.compile(r"q['’ʼ]"), ("Q_EJ",)),
    (re.compile(r"ch"), ("CH",)),
    (re.compile(r"tk"), ("T", "K")),
    (re.compile(r"ts"), ("TS",)),
    (re.compile(r"ll|ly|l['’ʼ]"), ("LL",)),
    (re.compile(r"ñ"), ("NY",)),
    (re.compile(r"sh"), ("SH",)),
    (re.compile(r"ä|aa|ā|â|a:"), ("AA",)),
    (re.compile(r"ë|ee|ē|ê|e:"), ("EE",)),
    (re.compile(r"ï|ii|ī|î|i:"), ("II",)),
    (re.compile(r"ö|oo|ō|ô|o:"), ("OO",)),
    (re.compile(r"ü|uu|ū|û|u:"), ("UU",)),
    (re.compile(r"c"), ("K",)),
    (re.compile(r"d"), ("T",)),
    (re.compile(r"h"), ()),
    (re.compile(r"á"), ("A",)),
    (re.compile(r"é"), ("E",)),
    (re.compile(r"í"), ("I",)),
    (re.compile(r"ó"), ("O",)),
    (re.compile(r"ú"), ("U",)),
    (re.compile(r"p"), ("P",)),
    (re.compile(r"t"), ("T",)),
    (re.compile(r"k"), ("K",)),
    (re.compile(r"q"), ("Q",)),
    (re.compile(r"s|z"), ("S",)),
    (re.compile(r"x"), ("X",)),
    (re.compile(r"j"), ("J",)),
    (re.compile(r"m"), ("M",)),
    (re.compile(r"n"), ("N",)),
    (re.compile(r"r"), ("R",)),
    (re.compile(r"l"), ("L",)),
    (re.compile(r"y"), ("Y",)),
    (re.compile(r"w"), ("W",)),
    (re.compile(r"a"), ("A",)),
    (re.compile(r"e"), ("E",)),
    (re.compile(r"i"), ("I",)),
    (re.compile(r"o"), ("O",)),
    (re.compile(r"u"), ("U",)),
)


class GrafemaNoReconocidoError(ValueError):
    """Indica que una forma no puede representarse con el inventario cerrado."""


def normalizar_forma(texto: str) -> str:
    normalizado = unicodedata.normalize("NFC", texto).lower().strip()
    return re.sub(r"[-.¿?¡!,;]", "", normalizado)


def grafema_a_fonemas(palabra: str) -> tuple[str, ...]:
    texto = normalizar_forma(palabra)
    if not texto or any(caracter.isspace() for caracter in texto):
        raise ValueError(f"Se esperaba una sola forma pronunciable: {palabra!r}")

    fonemas: list[str] = []
    posicion = 0
    while posicion < len(texto):
        for patron, reemplazo in REGLAS_G2P:
            coincidencia = patron.match(texto, posicion)
            if coincidencia is None:
                continue
            fonemas.extend(reemplazo)
            posicion = coincidencia.end()
            break
        else:
            raise GrafemaNoReconocidoError(
                f"Grafema no reconocido en {palabra!r}, posición {posicion}: {texto[posicion]!r}"
            )

    desconocidos = set(fonemas) - FONEMAS_CANONICOS
    if desconocidos:
        raise GrafemaNoReconocidoError(
            f"Fonemas fuera del inventario en {palabra!r}: {sorted(desconocidos)}"
        )
    if not fonemas:
        raise GrafemaNoReconocidoError(f"La forma {palabra!r} no produjo fonemas")
    return tuple(fonemas)


def extraer_entradas_lexicon(palabras: Iterable[dict]) -> list[str]:
    entradas: set[str] = set()
    for palabra in palabras:
        formas = [palabra, *palabra.get("formas", [])]
        for forma in formas:
            for componente in forma["uch"].split():
                entrada = normalizar_forma(componente)
                if entrada:
                    entradas.add(entrada)
    return sorted(entradas)


def construir_lexicon(palabras: Iterable[dict]) -> list[tuple[str, tuple[str, ...]]]:
    return [(entrada, grafema_a_fonemas(entrada)) for entrada in extraer_entradas_lexicon(palabras)]


def escribir_lexicon(entradas: Iterable[tuple[str, tuple[str, ...]]], ruta: Path) -> None:
    lineas = [f"{palabra}\t{' '.join(fonemas)}" for palabra, fonemas in entradas]
    ruta.write_text("\n".join(lineas) + "\n", encoding="utf-8", newline="\n")


def escribir_tokens(ruta: Path) -> None:
    tokens = [*SIMBOLOS_ESPECIALES, *sorted(FONEMAS_CANONICOS)]
    lineas = [f"{indice}\t{token}" for indice, token in enumerate(tokens)]
    ruta.write_text("\n".join(lineas) + "\n", encoding="utf-8", newline="\n")


def escribir_mapeo_ipa(ruta: Path) -> None:
    if set(MAPEO_IPA) != FONEMAS_CANONICOS:
        faltantes = FONEMAS_CANONICOS - set(MAPEO_IPA)
        extras = set(MAPEO_IPA) - FONEMAS_CANONICOS
        raise ValueError(f"Mapeo IPA inconsistente. Faltantes={sorted(faltantes)}, extras={sorted(extras)}")
    lineas = ["token_ascii\tfonema_ipa"]
    lineas.extend(f"{token}\t{MAPEO_IPA[token]}" for token in sorted(FONEMAS_CANONICOS))
    ruta.write_text("\n".join(lineas) + "\n", encoding="utf-8", newline="\n")


def generar(palabras_json: Path, directorio_salida: Path) -> tuple[int, int]:
    documento = json.loads(palabras_json.read_text(encoding="utf-8"))
    entradas = construir_lexicon(documento["palabras"])
    directorio_salida.mkdir(parents=True, exist_ok=True)
    escribir_lexicon(entradas, directorio_salida / "lexicon.txt")
    escribir_tokens(directorio_salida / "tokens.txt")
    escribir_mapeo_ipa(directorio_salida / "fonemas.tsv")
    return len(entradas), len(SIMBOLOS_ESPECIALES) + len(FONEMAS_CANONICOS)


def main() -> int:
    raiz = Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument(
        "--palabras",
        type=Path,
        default=raiz / "Assets/Content/Data/palabras.json",
        help="Ruta al palabras.json vigente",
    )
    parser.add_argument(
        "--salida",
        type=Path,
        default=raiz / "ASR/Generated",
        help="Directorio para lexicon.txt y tokens.txt",
    )
    argumentos = parser.parse_args()
    entradas, tokens = generar(argumentos.palabras, argumentos.salida)
    print(f"ASR preparado: {entradas} entradas léxicas, {tokens} tokens.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
