using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SciptableValueShower : MonoBehaviour
{
    [SerializeField] private ScriptableObjectValue _so;

    [SerializeField] private TextMeshProUGUI _valueText;

    [SerializeField] private bool _allwaysRefresh;
    [SerializeField] private int _numberOfDecimal;
    [SerializeField] private float _multipliedBy;
    [SerializeField] private string _beforeValue;
    [SerializeField] private string _afterValue;
    // Start is called before the first frame update
    void Start()
    {
        ChangeValue();
    }
    private void Update()
    {
        if (_allwaysRefresh)
            ChangeValue();
    }

    public void ChangeMode( bool mode) 
    {
        _allwaysRefresh = mode;
    }


    public void ChangeValue()
    {
        if(_beforeValue == "{}")
            _beforeValue = Localizer.Get("Score");
        if(_beforeValue == "{*}")
            _beforeValue = Localizer.Get("Score");
        if(_beforeValue == "{-}")
            _beforeValue = Localizer.Get("Bravo") + " !\n";
        if(_afterValue == "{}")
            _afterValue = Localizer.Get("Points");
        _valueText.text = (_beforeValue + (_so.value * _multipliedBy).ToString("F" + _numberOfDecimal) +" " + _afterValue).Replace("\\n", "\n");
    }
}
