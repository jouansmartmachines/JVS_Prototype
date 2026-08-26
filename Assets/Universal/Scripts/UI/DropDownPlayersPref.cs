using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Dropdown))]
public class DropDownPlayersPref : MonoBehaviour
{
    [SerializeField] string _key;
    TMP_Dropdown _dropdown;

    public void Start()
    {
        _dropdown = GetComponent<TMP_Dropdown>();

        if (PlayerPrefs.HasKey(_key))
            _dropdown.value = PlayerPrefs.GetInt(_key);

        Universal_GeneralVariables.OnPlayerPrefs += UpdateData;

        //_dropdown.onValueChanged.AddListener((value) => PlayerPrefs.SetInt(_key, value));
        _dropdown.onValueChanged.AddListener((value) => 
        {
            PlayerPrefs.SetInt(_key, value);
            Debug.Log($"[Dropdown] {_key} mis à jour : {value}");
        });

    }

    private void UpdateData()
    {
        if (PlayerPrefs.HasKey(_key))
            _dropdown.value = PlayerPrefs.GetInt(_key);
        Debug.Log("DropDownPlayersPref get" +  _dropdown.value);
    }

    public void OnDestroy()
    {
        Universal_GeneralVariables.OnPlayerPrefs -= UpdateData;
    }
}
