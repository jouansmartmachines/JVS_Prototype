using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "Boolean", menuName = "ScriptableObjects/Value/Boolean", order = 1)]
public class ScriptableBoolean : ScritptableValueGeneric<bool>
{
    public override bool Load()
    {
        bool.TryParse(PlayerPrefs.GetString(name), out value);
        return value;
    }

    public void SetValue(bool mode) { value = mode; }
}
