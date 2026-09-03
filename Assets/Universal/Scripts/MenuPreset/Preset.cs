using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Game/Menu/Preset")]
public class Preset : BasePreset
{
    [SerializeField] List<ValuePreset> AllValues;

    public override void SavePreset(ValuePreset.PresetEnum type)
    {
        foreach (var v in AllValues)
        {
            v.RetrieveValue(type);
        }
    }

    public override void ActivePreset(ValuePreset.PresetEnum type)
    {
        foreach (var v in AllValues)
        {
            v.SaveValue(type);
        }
        State = type;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
        Universal_GeneralVariables.OnPlayerPrefs?.Invoke();
    }

    public override void UpdateAllValues<T>(T value)
    {
        for(int i = 0; i < AllValues.Count; i++)
        {
            var v = AllValues[i];
            if (v is ValuePreset<T>)
            {
                (v as ValuePreset<T>).SetValue(ValuePreset.PresetEnum.Easy, value, true);
                (v as ValuePreset<T>).SetValue(ValuePreset.PresetEnum.Medium, value, true);
                (v as ValuePreset<T>).SetValue(ValuePreset.PresetEnum.Hard, value, true);

#if UNITY_EDITOR
                EditorUtility.SetDirty((v as ValuePreset<T>));
#endif
            }
        }
    }
}
