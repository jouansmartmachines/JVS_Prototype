using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScriptableInputFieldFloat : MonoBehaviour
{

    [SerializeField] private ScriptableObjectValue _so;
    [SerializeField] private TMP_InputField _valueText;
    [SerializeField] private bool _allwaysRefresh;
    [SerializeField] private int _numberOfDecimal;
    [SerializeField] private string _afterValue;
    // Start is called before the first frame update
    void Start()
    {
        _so.Load();
        ChangeValue();
    }
    private void Update()
    {
        if (_allwaysRefresh)
            ChangeValue();
    }


    public void ChangeValue()
    {
        _valueText.text = _so.value.ToString("F" + _numberOfDecimal) + _afterValue;
    }
}
