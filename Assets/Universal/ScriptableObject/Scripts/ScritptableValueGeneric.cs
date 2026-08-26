using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ScritptableValueGeneric<T> : ScriptableObject
{
    public T value;
    public T TrueValue
    {
        get
        {
            return Load();
        }

        set
        {
            this.value = value;
            Save();
        }
    }

    public void Save() 
    {
        PlayerPrefs.SetString(name, value.ToString());
    }

    public abstract T Load();
}
