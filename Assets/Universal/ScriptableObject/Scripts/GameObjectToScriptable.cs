using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Viking
{

    public class GameObjectToScriptable : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] private GameObject _gameObject;
        [SerializeField] private ScriptableGameObject _scriptable;
        void Start()
        {
            _scriptable.value = _gameObject;    
        }
    }
}