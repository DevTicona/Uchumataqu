import importlib.util
import json
import tempfile
import unittest
from pathlib import Path


RAIZ = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location("preparar_asr", Path(__file__).with_name("preparar_asr.py"))
ASR = importlib.util.module_from_spec(SPEC)
assert SPEC.loader is not None
SPEC.loader.exec_module(ASR)


class PrepararAsrTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        ruta = RAIZ / "Assets/Content/Data/palabras.json"
        cls.palabras = json.loads(ruta.read_text(encoding="utf-8"))["palabras"]

    def test_g2p_respeta_coincidencia_mas_larga(self):
        self.assertEqual(("CHH", "A"), ASR.grafema_a_fonemas("chha"))
        self.assertEqual(("T", "SH", "A"), ASR.grafema_a_fonemas("tsha"))
        self.assertEqual(("CH_EJ", "A"), ASR.grafema_a_fonemas("ch'a"))
        self.assertEqual(("II",), ASR.grafema_a_fonemas("ï"))

    def test_corpus_completo_usa_solo_fonemas_canonicos(self):
        lexicon = ASR.construir_lexicon(self.palabras)
        self.assertTrue(lexicon)
        for palabra, fonemas in lexicon:
            self.assertTrue(fonemas, palabra)
            self.assertFalse(set(fonemas) - ASR.FONEMAS_CANONICOS, palabra)

    def test_nombres_propios_no_regresan_al_lexicon_pedagogico(self):
        entradas = set(ASR.extraer_entradas_lexicon(self.palabras))
        nombres = {"irohito", "urus", "juana", "lorenzo", "maria", "maría", "pedro", "waki"}
        self.assertFalse(entradas & nombres)

    def test_generacion_es_deterministica_y_contiene_42_tokens(self):
        ruta_palabras = RAIZ / "Assets/Content/Data/palabras.json"
        with tempfile.TemporaryDirectory() as temporal:
            salida = Path(temporal)
            entradas, tokens = ASR.generar(ruta_palabras, salida)
            primera = (salida / "lexicon.txt").read_bytes()
            ASR.generar(ruta_palabras, salida)
            self.assertEqual(primera, (salida / "lexicon.txt").read_bytes())
            self.assertEqual(42, tokens)
            self.assertEqual(entradas, len(primera.decode("utf-8").splitlines()))
            mapeo = (salida / "fonemas.tsv").read_text(encoding="utf-8").splitlines()
            self.assertEqual(39, len(mapeo))
            self.assertEqual(set(ASR.FONEMAS_CANONICOS), set(ASR.MAPEO_IPA))


if __name__ == "__main__":
    unittest.main()
