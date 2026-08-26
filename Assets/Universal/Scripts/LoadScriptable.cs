using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScriptable : MonoBehaviour
{
    [SerializeField] private List<ScriptableObjectValue> _listFloat;
    [SerializeField] private List<ScriptableBoolean> _listBool;
    [SerializeField] private bool _unloadLoad;
    // Start is called before the first frame update
    void Start()
    {
        if (_unloadLoad)
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        else
            Load();
    }

    
    private void Load()
    {
        foreach (var soFloat in _listFloat)
            soFloat.Load();
        foreach (var soBool in _listBool)
            soBool.Load();
    }

    private void OnSceneUnloaded(Scene arg0)
    {
        Load();
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }
}
