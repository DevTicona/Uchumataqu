using System.Linq;
using Assets.Scripts.Data.Corpus;
using Assets.Scripts.RA;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

namespace Assets.Scripts.Editor.Tests.RA
{
    public class RA3CompositionTests
    {
        private const string DataPath = "Assets/Content/Data/escenarios_ra.json";
        private const string PrefabPath = "Assets/Content/Prefabs/RA/Escenarios/ra_casa_objetos.prefab";

        private static EscenarioRAModel CargarEscenario()
        {
            var text = AssetDatabase.LoadAssetAtPath<TextAsset>(DataPath);
            Assert.IsNotNull(text);
            var corpus = JsonUtility.FromJson<EscenariosRACorpusModel>(text.text);
            return corpus.escenarios.Single(item => item.id == "ra_casa_objetos");
        }

        [Test]
        public void PrefabCoincideConContratoYAddressable()
        {
            var config = CargarEscenario();
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.IsNotNull(prefab);
            Assert.AreEqual(10, config.objetos.Count);
            Assert.AreEqual(3, config.mision.fases.Count);
            var objetos = prefab.GetComponentsInChildren<ObjetoRAView>(true);
            CollectionAssert.AreEquivalent(config.objetos.Select(item => item.id),
                objetos.Select(item => item.ObjetoId));
            Assert.IsTrue(objetos.All(item => item.GetComponent<Collider>() != null));
            Assert.AreEqual(3, objetos.Single(item => item.ObjetoId == "obj_ventana").transform.childCount);
            Assert.AreEqual(3, objetos.Single(item => item.ObjetoId == "obj_almohada").transform.childCount);
            Assert.AreEqual(6, objetos.Single(item => item.ObjetoId == "obj_lapiz").transform.childCount);
            Assert.AreEqual(5, objetos.Single(item => item.ObjetoId == "obj_cuaderno").transform.childCount);
            var mochila = objetos.Single(item => item.ObjetoId == "obj_mochila");
            var rendererMochila = mochila.GetComponentsInChildren<Renderer>(true)
                .First(item => item.enabled && item.sharedMaterial != null);
            Assert.AreEqual("ra3_azul_mochila", rendererMochila.sharedMaterial.name);
            var destino = prefab.GetComponentInChildren<DestinoRA>(true);
            Assert.AreEqual("dst_mochila", destino.DestinoId);
            var settings = AddressableAssetSettingsDefaultObject.GetSettings(false);
            var entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(PrefabPath));
            Assert.AreEqual(config.prefab_key, entry.address);
        }

        [Test]
        public void MisionRA3AvanzaSoloConAciertosYTerminaAlColocar()
        {
            var config = CargarEscenario();
            var mision = new GestorMisionRA(config.mision);
            foreach (var id in config.mision.fases[0].objetivos_objeto_ids)
                Assert.IsTrue(mision.RegistrarObjeto(id));
            Assert.AreEqual("encontrar", mision.FaseActual.id);

            foreach (var paso in config.mision.fases[1].pasos)
            {
                Assert.IsFalse(mision.RegistrarObjeto("obj_puerta"));
                Assert.AreSame(paso, mision.PasoActual);
                Assert.IsTrue(mision.RegistrarObjeto(paso.objetivo_objeto_id));
            }

            Assert.AreEqual("preparar_mochila", mision.FaseActual.id);
            Assert.IsTrue(mision.RegistrarObjeto("obj_mochila"));
            Assert.IsTrue(mision.RegistrarObjeto("obj_lapiz"));
            Assert.IsTrue(mision.EsperandoDestino);
            Assert.IsFalse(mision.RegistrarDestino("destino_incorrecto"));
            Assert.IsTrue(mision.RegistrarDestino("dst_mochila"));
            Assert.IsTrue(mision.RegistrarObjeto("obj_cuaderno"));
            Assert.IsTrue(mision.RegistrarDestino("dst_mochila"));
            Assert.IsTrue(mision.Completada);
        }
    }
}
