using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiMap<T,V>
{
    Dictionary<T, List<V>> _dictionary =
        new Dictionary<T, List<V>>();

    public void Add(T key, V value)
    {
        // Add a key.
        List<V> list;
        if (this._dictionary.TryGetValue(key, out list))
        {
            list.Add(value);
        }
        else
        {
            list = new List<V>();
            list.Add(value);
            this._dictionary[key] = list;
        }
    }

    internal bool ContainsKey(T key)
    {
        return this._dictionary.ContainsKey(key);
    }

    public IEnumerable<T> Keys
    {
        get
        {
            // Get all keys.
            return this._dictionary.Keys;
        }
    }

    public List<V> this[T key]
    {
        get
        {
            // Get list at a key.
            List<V> list;
            if (!this._dictionary.TryGetValue(key, out list))
            {
                list = new List<V>();
                this._dictionary[key] = list;
            }
            return list;
        }
    }

    public IEnumerator GetEnumerator()
    {
        return _dictionary.GetEnumerator();
    }
}