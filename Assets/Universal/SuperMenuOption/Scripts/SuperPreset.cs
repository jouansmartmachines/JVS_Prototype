using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Menu/SuperPreset")]
public class SuperPreset : BasePreset
{
    [SerializeField] List<Preset> AllValues;

    public override void SavePreset(ValuePreset.PresetEnum type)
    {
        foreach (var v in AllValues)
        {
            v.SavePreset(type);
        }

    }

    public override void ActivePreset(ValuePreset.PresetEnum type)
    {
        foreach (var v in AllValues)
        {
            v.ActivePreset(type);
        }
        State = type;

        Universal_GeneralVariables.OnPlayerPrefs?.Invoke();
    }

    public override void UpdateAllValues<T>(T value)
    {
        foreach (Preset v in AllValues)
        {
            v.UpdateAllValues(value);
        }
    }
}
