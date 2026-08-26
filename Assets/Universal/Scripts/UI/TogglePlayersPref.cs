using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class TogglePlayersPref : MonoBehaviour
{
    [SerializeField] string _key;
    Toggle _toogle;

    public void Start()
    {
        _toogle = GetComponent<Toggle>();


        if (!PlayerPrefs.HasKey(_key)) PlayerPrefs.SetInt(_key, 0);

        _toogle.isOn = PlayerPrefs.GetInt(_key) == 1;

        //Debug.Log("TogglePlayersPref Start 1 : " + _key + " " + _toogle.isOn);

        _toogle.onValueChanged.AddListener(OnToggleValueChanged);
        Universal_GeneralVariables.OnPlayerPrefs += UpdateData;

    }

    private void OnToggleValueChanged(bool value)
    {
        //Debug.Log($"Toggle value changed for key: {_key}, new value : {value}");

        PlayerPrefs.SetInt(_key, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void UpdateData()
    {
        if (PlayerPrefs.HasKey(_key))
        {
            _toogle.isOn = PlayerPrefs.GetInt(_key) == 1;
            //Debug.Log("TogglePlayersPref UpdateData : " + _key + " " + _toogle.isOn);
        }
            
    }

    public void OnDestroy()
    {
        Universal_GeneralVariables.OnPlayerPrefs -= UpdateData;
    }
}
