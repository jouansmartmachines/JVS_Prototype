using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Theme
{
    public class TestManager : MonoBehaviour
    {
        [SerializeField] Transform _holder;
        [SerializeField] GameObject _prefab;

        public void Start()
        {
            Instantiate(_prefab, _holder);
        }
    }
}