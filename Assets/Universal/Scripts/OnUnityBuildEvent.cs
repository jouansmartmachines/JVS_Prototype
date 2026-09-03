using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnUnityBuildEvent : MonoBehaviour
{
    public UnityEvent _event;

#if !UNITY_EDITOR
    
    public void Start()
    {
        _event?.Invoke();
    }

#endif
}
