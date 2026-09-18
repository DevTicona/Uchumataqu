using Assets.Scripts.Core.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Editor.Tests
{
    public class UIEventSubscriptionsTests
    {
        [Test]
        public void Dispose_RetiraSoloListenersPropiosYPermiteReconfigurar()
        {
            var objeto = new GameObject("BotonPrueba", typeof(RectTransform), typeof(Button));
            var subscriptions = new UIEventSubscriptions();
            try
            {
                Button button = objeto.GetComponent<Button>();
                int externos = 0, propios = 0;
                button.onClick.AddListener(() => externos++);
                subscriptions.Add(button, () => propios++);
                button.onClick.Invoke();
                subscriptions.Dispose();
                subscriptions.Dispose();
                button.onClick.Invoke();
                Assert.AreEqual(2, externos);
                Assert.AreEqual(1, propios);

                subscriptions.Add(button, () => propios++);
                button.onClick.Invoke();
                Assert.AreEqual(3, externos);
                Assert.AreEqual(2, propios);
            }
            finally
            {
                subscriptions.Dispose();
                Object.DestroyImmediate(objeto);
            }
        }
    }
}
