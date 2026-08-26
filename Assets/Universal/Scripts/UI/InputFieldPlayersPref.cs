using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldPlayersPref : MonoBehaviour
{
    [SerializeField] string _key;
    TMP_InputField _inputField;

    public void Start()
    {
        _inputField = GetComponent<TMP_InputField>();

        _inputField.onEndEdit.AddListener((value) =>
        {
            PlayerPrefs.SetString(_key, value);
        });

        _inputField.onValueChanged.AddListener((value) =>
        {
            PlayerPrefs.SetString(_key, value);
        });

        if(PlayerPrefs.HasKey(_key))
        {
            _inputField.text = PlayerPrefs.GetString(_key);
        }

        Universal_GeneralVariables.OnPlayerPrefs += UpdateData;
    }

    private void UpdateData()
    {
        if (PlayerPrefs.HasKey(_key))
        {
            _inputField.text = PlayerPrefs.GetString(_key);
        }
    }

    public void OnDestroy()
    {
        Universal_GeneralVariables.OnPlayerPrefs -= UpdateData;
    }
}
