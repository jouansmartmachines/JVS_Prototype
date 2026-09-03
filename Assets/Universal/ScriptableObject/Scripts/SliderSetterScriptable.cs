using System.Collections;
using System.Collections.Generic;
using CovidKiller;
using UnityEngine;
using UnityEngine.UI;

public class SliderSetterScriptable : MonoBehaviour
{
    [SerializeField] Slider _slider;
    [SerializeField] private ScriptableObjectValue _soValue;
    //[SerializeField] private ScriptableObjectValue _soInit;
    // Start is called before the first frame update
    
    void Start() 
    {

    }
    // Update is called once per frame
    void Update()
    {
        _slider.value = (_soValue.value/CrudiCrush_GeneralVariables.GetTime());
        //Debug.Log(_slider.value);   
    }
}
