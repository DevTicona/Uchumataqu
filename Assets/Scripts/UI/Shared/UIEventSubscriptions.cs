using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Core.UI
{
    /// <summary>Conserva y retira exclusivamente las suscripciones propias de una Vista.</summary>
    public sealed class UIEventSubscriptions : IDisposable
    {
        private readonly List<Action> _unsubscribe = new List<Action>();

        public void Add(Button button, UnityAction action)
        {
            if (button == null) return;
            button.onClick.AddListener(action);
            _unsubscribe.Add(() => { if (button != null) button.onClick.RemoveListener(action); });
        }

        public void Add<T>(UnityEvent<T> unityEvent, UnityAction<T> action)
        {
            unityEvent.AddListener(action);
            _unsubscribe.Add(() => unityEvent.RemoveListener(action));
        }

        public void Dispose()
        {
            foreach (Action unsubscribe in _unsubscribe) unsubscribe();
            _unsubscribe.Clear();
        }
    }
}
