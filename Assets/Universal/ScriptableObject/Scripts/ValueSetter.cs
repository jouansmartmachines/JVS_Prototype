using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueSetter : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public List<ScriptableTable> tables;

    void Start()
    {
        foreach(ScriptableTable t in tables) // don't want to reset after game menu, only when the game is launch for the first time
        {
            t.so.Load();
            if (t.so.value != 0)
                continue;
            t.so.value = t.baseValue;
        }
    }
}

[System.Serializable]
public class ScriptableTable
{
    public ScriptableObjectValue so;
    public float baseValue;
}