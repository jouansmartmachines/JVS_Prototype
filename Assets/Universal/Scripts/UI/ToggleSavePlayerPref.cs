using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleSavePlayerPref : MonoBehaviour
{
    private Toggle _toggle;
    [SerializeField] string _key;
    private enum Type
    {
        String,
        Float,
        Int
    }
    [SerializeField] Type _type;

    private void Awake()
    {
        _toggle = GetComponent<Toggle>();
        switch (_type)
        {
            case Type.Int:
                _toggle.onValueChanged.AddListener((value) => PlayerPrefs.SetInt(_key, value ? 1 : 0));
                break;
            case Type.Float:
                _toggle.onValueChanged.AddListener((value) => PlayerPrefs.SetFloat(_key, value ? 1f : 0f));
                break;
            case Type.String:
                _toggle.onValueChanged.AddListener((value) => PlayerPrefs.SetString(_key, value.ToString()));
                break;
            default:
                _toggle.onValueChanged.AddListener((value) => PlayerPrefs.SetString(_key, value.ToString()));
                break;
        }

        if (PlayerPrefs.HasKey(_key))
        {
            switch (_type)
            {
                case Type.Int:
                    _toggle.isOn = PlayerPrefs.GetInt(_key) != 0;
                    break;
                case Type.Float:
                    _toggle.isOn = PlayerPrefs.GetFloat(_key) != 0;
                    break;
                case Type.String:
                    _toggle.isOn = PlayerPrefs.GetString(_key).ToLower() == "true";
                    break;
                default:
                    _toggle.isOn = PlayerPrefs.GetString(_key).ToLower() == "true";
                    break;
            }
        }
        else
            _toggle.isOn = PlayerPrefs.HasKey(_key);
    }
}
