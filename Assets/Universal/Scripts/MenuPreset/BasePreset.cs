using UnityEngine;

public abstract class BasePreset : ScriptableObject
{
    public abstract void SavePreset(ValuePreset.PresetEnum type);
    public abstract void ActivePreset(ValuePreset.PresetEnum type);
    public abstract void UpdateAllValues<T>(T value);
    public ValuePreset.PresetEnum State
    {
        get
        {
            int value = PlayerPrefs.GetInt("CurrentState_" + name, 0); // Ne pas rennomer des playerPrefs de ma même manière
            return (ValuePreset.PresetEnum)value;
        }
        set
        {
            PlayerPrefs.SetInt("CurrentState_" + name, (int)value);
        }
    }
    
}
