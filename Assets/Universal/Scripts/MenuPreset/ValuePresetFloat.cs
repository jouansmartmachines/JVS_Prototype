using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Menu/ValueFloat")]
public class ValuePresetFloat : ValuePreset<float>
{
    public override float GetValue(PresetEnum level)
    {
        switch (level)
        {
            case PresetEnum.Easy:
                ValueEasy = PlayerPrefs.GetFloat(Key);
                return ValueEasy;

            case PresetEnum.Medium:
                ValueMedium = PlayerPrefs.GetFloat(Key);
                return ValueMedium;

            case PresetEnum.Hard:
                ValueHard = PlayerPrefs.GetFloat(Key);
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
                PlayerPrefs.SetFloat(Key, ValueEasy);
                break;

            case PresetEnum.Medium:
                PlayerPrefs.SetFloat(Key, ValueMedium);
                break;

            case PresetEnum.Hard:
                PlayerPrefs.SetFloat(Key, ValueHard);
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
                ValueEasy = PlayerPrefs.GetFloat(Key);
                break;

            case PresetEnum.Medium:
                ValueMedium = PlayerPrefs.GetFloat(Key);
                break;

            case PresetEnum.Hard:
                ValueHard = PlayerPrefs.GetFloat(Key);
                break;

            default:
                break;
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    public override void SetValue(PresetEnum level, float value, bool save = false)
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
