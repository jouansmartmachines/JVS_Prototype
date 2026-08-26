using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UnityEngine.UI.Toggle))]
public class PresetToggle : MonoBehaviour
{
    [SerializeField] BasePreset preset;
    [SerializeField] ValuePreset<bool> value;

    public void Start()
    {
        var toggle = GetComponent<UnityEngine.UI.Toggle>();

        toggle.onValueChanged.AddListener(OnToggleChanged);
        toggle.isOn = value.GetValue(ValuePreset.PresetEnum.Easy);

    }

    public void OnToggleChanged(bool isOn)
    {
        Debug.Log($"All Value Update to : {isOn}");
        preset.UpdateAllValues<bool>(isOn);
        preset.ActivePreset(ValuePreset.PresetEnum.Easy);

        //preset.SavePreset(ValuePreset.PresetEnum.Medium);
        //preset.ActivePreset(ValuePreset.PresetEnum.Medium);
        
        //preset.SavePreset(ValuePreset.PresetEnum.Hard);
        //preset.ActivePreset(ValuePreset.PresetEnum.Hard);
    }
}
