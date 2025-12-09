using System.Collections.Generic;
using UnityEngine;

namespace ReferenceSystem
{
    /// <summary>
    /// Manages references to MonoBehaviours by string keys for easy cross-scene access.
    /// Provides a centralized registry to find game objects and components without direct references.
    /// </summary>
    public class ReferenceManager : SingletonObject<ReferenceManager>
    {
        private readonly Dictionary<string, int> keyToIndex = new Dictionary<string, int>();
        [SerializeField]
        private List<MonoBehaviour> indexToComponents = new List<MonoBehaviour>();

        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Registers or updates a MonoBehaviour reference with the specified key.
        /// </summary>
        /// <param name="key">Unique identifier for the component</param>
        /// <param name="component">The MonoBehaviour to register</param>
        /// <param name="isDontDestory">Reserved for future use (currently unused)</param>
        public void SetReferableObject(string key, MonoBehaviour component, bool isDontDestory)
        {
            if (keyToIndex.ContainsKey(key))
            {
                indexToComponents[keyToIndex[key]] = component;
            } else
            {
                int index = indexToComponents.Count;
                indexToComponents.Add(component);
                keyToIndex.Add(key, index);
            }
        }

        /// <summary>
        /// Finds and returns a component by its registered key.
        /// </summary>
        /// <typeparam name="T">The type to cast the component to</typeparam>
        /// <param name="key">The key used to register the component</param>
        /// <returns>The component cast to type T, or null if not found</returns>
        public T FindComponentByName<T>(string key) where T : class
        {
            try
            {
                return indexToComponents[keyToIndex[key]] as T;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Finds and returns a GameObject by its registered key.
        /// </summary>
        /// <param name="key">The key used to register the component</param>
        /// <returns>The GameObject containing the registered component, or null if not found</returns>
        public GameObject FindGameObjectByName(string key)
        {
            if (!keyToIndex.ContainsKey(key))
            {
                Debug.LogWarning($"Key '{key}' not found in ReferenceManager");
                return null;
            }
        
            MonoBehaviour component = indexToComponents[keyToIndex[key]];
            if (component == null)
            {
                Debug.LogWarning($"Component for key '{key}' is null");
                return null;
            }
        
            return component.gameObject;
        }
    }
}
