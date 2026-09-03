using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextScriptable : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI _text;
    [SerializeField] private ScriptableObjectValue _soValue;


    void Start()
    {
        _text.text = Mathf.RoundToInt(_soValue.TrueValue).ToString();
    }

    void Update()
    {
        _text.text = Mathf.RoundToInt(_soValue.TrueValue).ToString();
    }
}
