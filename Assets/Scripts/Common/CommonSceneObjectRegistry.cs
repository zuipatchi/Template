using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Common
{
    public enum ObjectKey
    {
        Sound
    }

    /// <summary>
    /// 共通オブジェクトを保持するシングルトン
    /// </summary>
    public class CommonSceneObjectRegistry : MonoBehaviour
    {
        public static CommonSceneObjectRegistry Instance { get; private set; }
        private Dictionary<ObjectKey, GameObject> _registry = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void Register(ObjectKey key, GameObject go)
        {
            if (!_registry.ContainsKey(key))
            {
                _registry.Add(key, go);
            }
        }

        public T GetObject<T>(ObjectKey key)
        {
            _registry.TryGetValue(key, out GameObject go);
            T obj = go.GetComponent<T>();
            return obj;
        }

        public void Unregister(ObjectKey key)
        {
            if (_registry.ContainsKey(key))
            {
                _registry.Remove(key);
            }
        }
    }
}
