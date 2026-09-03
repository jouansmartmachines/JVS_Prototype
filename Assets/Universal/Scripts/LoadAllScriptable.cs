using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadAllScriptable : MonoBehaviour
{
    [SerializeField] ScriptableObjectValue[] soFloat;
    [SerializeField] ScriptableBoolean[] soBool;

    // Start is called before the first frame update
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
       if(soFloat.Length == 0) 
        {
            LoadAll();
        }
    }

    public void LoadAll() 
    {
        soFloat = Resources.FindObjectsOfTypeAll<ScriptableObjectValue>();
        foreach (var so in soFloat)
            so.Load();

        soBool = Resources.FindObjectsOfTypeAll<ScriptableBoolean>();
        foreach (var so in soBool)
            so.Load();
    }
}
