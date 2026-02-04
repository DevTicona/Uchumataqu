// Assets/Scripts/Utils/EventBus.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    /// <summary>
    /// EventBus global muy ligero.
    /// Permite publicar y escuchar eventos sin instancias ni singletons.
    /// Usa diccionario estático de listas de callbacks.
    /// </summary>
    public static class EventBus
    {
        // Diccionario: nombreEvento → lista de callbacks
        private static readonly Dictionary<string, List<Action>> _eventTable =
            new Dictionary<string, List<Action>>();

        /// <summary>
        /// Suscribirse a un evento por nombre.
        /// </summary>
        public static void Subscribe(string eventName, Action callback)
        {
            if (string.IsNullOrEmpty(eventName) || callback == null)
                return;

            if (!_eventTable.ContainsKey(eventName))
                _eventTable[eventName] = new List<Action>();

            _eventTable[eventName].Add(callback);
        }

        /// <summary>
        /// Desuscribirse de un evento.
        /// </summary>
        public static void Unsubscribe(string eventName, Action callback)
        {
            if (string.IsNullOrEmpty(eventName) || callback == null)
                return;

            if (_eventTable.ContainsKey(eventName))
                _eventTable[eventName].Remove(callback);
        }

        /// <summary>
        /// Publicar un evento global.
        /// </summary>
        public static void Publish(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            if (_eventTable.ContainsKey(eventName))
            {
                // Copiamos la lista para evitar problemas si un callback se desuscribe mientras se ejecutan
                var listeners = new List<Action>(_eventTable[eventName]);
                foreach (var action in listeners)
                {
                    try
                    {
                        action?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[EventBus] Error en callback de evento '{eventName}': {e}");
                    }
                }
            }
        }
    }
}
