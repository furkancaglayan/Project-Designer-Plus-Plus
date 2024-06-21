using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectDesigner.Core
{
    /// <summary>
    /// A dictionary class wrapper that can be serialized. Unity won't serialize the usual Dictionary. <br></br>
    /// Reference: <a href="https://docs.unity3d.com/ScriptReference/SerializeField.html"/>
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeReference]
        private List<TKey> _keys = new List<TKey>();

        [SerializeReference]
        private List<TValue> _values = new List<TValue>();

        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
        public int Count => _dictionary.Count;

        public TValue this[TKey index]
        {
            get
            {
                return _dictionary[index];
            }
            set
            {
                _dictionary[index] = value;
            }
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys => _dictionary.Keys;
        public Dictionary<TKey, TValue>.ValueCollection Values => _dictionary.Values;

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            foreach (KeyValuePair<TKey, TValue> pair in _dictionary)
            {
                _keys.Add(pair.Key);
                _values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            _dictionary.Clear();

            if (_keys.Count != _values.Count)
            {
                throw new System.Exception(string.Format("there are {0} _keys and {1} _values after deserialization. Make sure that both key and value types are serializable."));
            }

            for (int i = 0; i < _keys.Count; i++)
            {
                _dictionary.Add(_keys[i], _values[i]);
            }
        }

        public void Add(string name, (TKey, TValue) pair)
        {
            _dictionary.Add(pair.Item1, pair.Item2);
        }

        public bool Remove(TKey key)
        {
            return _dictionary.Remove(key);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue val)
        {
            return _dictionary.TryGetValue(key, out val);
        }

        public void Add(TKey key, TValue val)
        {
            _dictionary.Add(key, val);
        }

        public void Add((TKey, TValue) pair)
        {
            _dictionary.Add(pair.Item1, pair.Item2);
        }
    }

    /// <summary>
    /// Serialized NodeBase, Connection dictionary.
    /// </summary>
    [Serializable]
    public class SerializableConnectionDictionary : SerializableDictionary<NodeBase, ConnectionBase>
    {
      
    }
}