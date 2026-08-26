//using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Menu/ValueInt")]
public class ValuePresetInt : ValuePreset<int>
{
    public override int GetValue(PresetEnum level)
    {
        switch (level)
        {
            case PresetEnum.Easy:
                ValueEasy = PlayerPrefs.GetInt(Key);
                return ValueEasy;
                
            case PresetEnum.Medium:
                ValueMedium = PlayerPrefs.GetInt(Key);
                return ValueMedium;

            case PresetEnum.Hard:
                ValueHard = PlayerPrefs.GetInt(Key);
                return ValueHard;

            default:
                break;
        }
        return -1;
    }

    public override void SaveValue(PresetEnum level)
    {
        switch (level)
        {
            case PresetEnum.Easy:
                PlayerPrefs.SetInt(Key, ValueEasy);
                break;

            case PresetEnum.Medium:
                PlayerPrefs.SetInt(Key, ValueMedium);
                break;

            case PresetEnum.Hard:
                PlayerPrefs.SetInt(Key, ValueHard);
                break;

            default:
                break;
        }
    }

    public override void RetrieveValue(PresetEnum level)
    {
        switch (level)
        {
            case PresetEnum.Easy:
                ValueEasy = PlayerPrefs.GetInt(Key);
                break;

            case PresetEnum.Medium:
                ValueMedium = PlayerPrefs.GetInt(Key);
                break;

            case PresetEnum.Hard:
                ValueHard = PlayerPrefs.GetInt(Key);
                break;

            default:
                break;
        }



#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    public override void SetValue(PresetEnum level, int value, bool save = false)
    {
        switch (level)
        {
            case PresetEnum.Easy:
                ValueEasy = value;
                break;

            case PresetEnum.Medium:
                ValueMedium = value;
                break;

            case PresetEnum.Hard:
                ValueHard = value;
                break;

            default:
                break;
        }
        if(save) SaveValue(level);
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
}
