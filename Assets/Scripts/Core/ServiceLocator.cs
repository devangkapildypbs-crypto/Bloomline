// ServiceLocator.cs — Simple static service locator for Bloomline
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bloomline.Core
{
    /// <summary>
    /// Lightweight static service locator. Services are registered by type
    /// (typically their interface type) and retrieved with Get&lt;T&gt;().
    /// All registrations persist across scenes.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Register a service instance for type T.
        /// Overwrites any previously registered instance of the same type.
        /// </summary>
        public static void Register<T>(T service)
        {
            Type key = typeof(T);
            if (_services.ContainsKey(key))
            {
                Debug.LogWarning($"[ServiceLocator] Overwriting existing service for {key.Name}");
                _services[key] = service;
            }
            else
            {
                _services.Add(key, service);
                Debug.Log($"[ServiceLocator] Registered {key.Name}");
            }
        }

        /// <summary>
        /// Retrieve the registered service for type T.
        /// Returns default(T) and logs an error if not found.
        /// </summary>
        public static T Get<T>()
        {
            Type key = typeof(T);
            if (_services.TryGetValue(key, out object service))
            {
                return (T)service;
            }

            Debug.LogError($"[ServiceLocator] Service not found: {key.Name}");
            return default;
        }

        /// <summary>
        /// Check whether a service of type T is registered.
        /// </summary>
        public static bool Has<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Remove a registered service. Rarely needed.
        /// </summary>
        public static void Unregister<T>()
        {
            Type key = typeof(T);
            if (_services.Remove(key))
            {
                Debug.Log($"[ServiceLocator] Unregistered {key.Name}");
            }
        }

        /// <summary>
        /// Clear all registered services. Used in tests or full reset.
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
            Debug.Log("[ServiceLocator] All services cleared");
        }
    }
}
