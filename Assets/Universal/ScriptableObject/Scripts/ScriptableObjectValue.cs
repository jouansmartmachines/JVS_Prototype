using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Float", menuName = "ScriptableObjects/Value/Float", order = 1)]
public class ScriptableObjectValue : ScritptableValueGeneric<float>
{
    public override float Load()
    {
        //float.TryParse(PlayerPrefs.GetString(name), out value);
        float value = PlayerPrefs.GetFloat(name);
    
        return value;
    }

    public void ReadStringInput(string s)
    {
        value = float.Parse(s);
        Save();
    }
}
