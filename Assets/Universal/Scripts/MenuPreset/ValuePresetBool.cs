using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Menu/ValueBool")]
public class ValuePresetBool : ValuePreset<bool>
{
    public override bool GetValue(PresetEnum level)
    {
        switch (level)
        {
            case PresetEnum.Easy:
                ValueEasy = PlayerPrefs.GetInt(Key, 0) == 1;
                return ValueEasy;

            case PresetEnum.Medium:
                ValueMedium = PlayerPrefs.GetInt(Key, 0) == 1;
                return ValueMedium;

            case PresetEnum.Hard:
                ValueHard = PlayerPrefs.GetInt(Key, 0) == 1;
                return ValueHard;

            default:
                break;
        }
        return false;
    }

    public override void SetValue(PresetEnum level, bool value, bool save = false)
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

    public override void SaveValue(PresetEnum level)
    {
        switch (level)
        {
            case PresetEnum.Easy:
                PlayerPrefs.SetInt(Key, ValueEasy ? 1 : 0);
                break;

            case PresetEnum.Medium:
                PlayerPrefs.SetInt(Key, ValueMedium ? 1 : 0);
                break;

            case PresetEnum.Hard:
                PlayerPrefs.SetInt(Key, ValueHard ? 1 : 0);
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
                ValueEasy = PlayerPrefs.GetInt(Key) == 1;
                break;

            case PresetEnum.Medium:
                ValueMedium = PlayerPrefs.GetInt(Key) == 1;
                break;

            case PresetEnum.Hard:
                ValueHard = PlayerPrefs.GetInt(Key) == 1;
                break;

            default:
                break;
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }
}