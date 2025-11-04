using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WheelOfFortune.Infrastructure.Pooling
{
    public class PoolManager<T> where T : Component
    {
        private readonly Queue<T> _pool = new();
        private readonly T _prefab;
        private readonly Transform _parent;

        public PoolManager(T prefab, int initialCount, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;

            for (int i = 0; i < initialCount; ++i)
            {
                var obj = GameObject.Instantiate(_prefab, _parent);
                obj.gameObject.SetActive(false);
                _pool.Enqueue(obj);
            }
        }

        public T Get()
        {
            if (_pool.Count == 0)
            {
                _pool.Enqueue(GameObject.Instantiate(_prefab, _parent));
            }

            var obj = _pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
}
