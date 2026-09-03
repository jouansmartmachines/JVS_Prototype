using System.Collections;
using UnityEngine;
public class InstructionDisplayer : MonoBehaviour
{
    [SerializeField] ValuePresetBool presetBool;        
    [SerializeField] ValuePreset.PresetEnum presetEnum;    
    bool invertDisplay = false;    

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool value = presetBool.GetValue(presetEnum);
        UpdateDisplay(value);
    
    }

    private void UpdateDisplay(bool value)
    {
        GameObject child = transform.GetChild(0).gameObject;
        bool active = invertDisplay ? !value : value;
        child.SetActive(active);
    }
}
