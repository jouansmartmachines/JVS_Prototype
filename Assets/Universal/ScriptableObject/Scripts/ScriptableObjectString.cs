using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ScriptableObject", menuName = "ScriptableObjects/String", order = 1)]
public class ScriptableObjectString : ScritptableValueGeneric<string>
{
    public override string Load()
    {
        value = PlayerPrefs.GetString(name);
        return value;
    }
}
