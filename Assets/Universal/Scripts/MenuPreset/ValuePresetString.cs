using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Menu/ValueString")]
public class ValuePresetString : ValuePreset<string>
{
    public override string GetValue(PresetEnum level)
    {
        switch (level)
        {
            case PresetEnum.Easy:
                ValueEasy = PlayerPrefs.GetString(Key);
                return ValueEasy;

            case PresetEnum.Medium:
                ValueMedium = PlayerPrefs.GetString(Key);
                return ValueMedium;

            case PresetEnum.Hard:
                ValueHard = PlayerPrefs.GetString(Key);
                return ValueHard;

            default:
                break;
        }
        return string.Empty;
    }

    public override void SaveValue(PresetEnum level)
    {
        switch (level)
        {
            case PresetEnum.Easy:
                PlayerPrefs.SetString(Key, ValueEasy);
                break;

            case PresetEnum.Medium:
                PlayerPrefs.SetString(Key, ValueMedium);
                break;

            case PresetEnum.Hard:
                PlayerPrefs.SetString(Key, ValueHard);
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
                ValueEasy = PlayerPrefs.GetString(Key);
                break;

            case PresetEnum.Medium:
                ValueMedium = PlayerPrefs.GetString(Key);
                break;

            case PresetEnum.Hard:
                ValueHard = PlayerPrefs.GetString(Key);
                break;

            default:
                break;
        }
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    public override void SetValue(PresetEnum level, string value, bool save = false)
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
