using System;
using System.Collections.Generic;
using UnityEngine;

namespace PigeonRooftopRush
{


    public sealed class ObjectPool
    {
        private readonly Stack<GameObject> _free = new Stack<GameObject>();
        private readonly Func<GameObject> _factory;

        public string Key { get; }
        public int CountFree => _free.Count;

        public ObjectPool(string key, Func<GameObject> factory, int prewarm)
        {
            Key = key;
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            for (int i = 0; i < prewarm; i++)
            {
                GameObject go = _factory();
                go.SetActive(false);
                _free.Push(go);
            }
        }

        public GameObject Get(Vector3 localPosition, Transform parent)
        {
            GameObject go = _free.Count > 0 ? _free.Pop() : _factory();
            Transform t = go.transform;
            if (t.parent != parent) t.SetParent(parent, false);
            t.localPosition = localPosition;
            t.localRotation = Quaternion.identity;
            go.SetActive(true);
            return go;
        }

        public void Release(GameObject go)
        {
            if (go == null) return;
            go.SetActive(false);
            _free.Push(go);
        }
    }
}
