using UnityEngine;
using UnityEngine.UI;

public class SliderScriptable : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private ScriptableObjectValue _soValue;

    void Start()
    {
        
        _slider.onValueChanged.AddListener(SetValue);
        Universal_GeneralVariables.OnPlayerPrefs += UpdateData;

        SetValue(_slider.value);
        UpdateData();
        
        
    }

    void OnDestroy()
    {
        Universal_GeneralVariables.OnPlayerPrefs -= UpdateData;
    }

    void SetValue(float value)
    {
        _soValue.TrueValue = value;
        
    }

    void UpdateData()
    {
        _slider.value = _soValue.TrueValue;
    }
}
