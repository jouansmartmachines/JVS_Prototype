using DG.Tweening.Core.Easing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SliderSavePlayersPref : MonoBehaviour
{
    Slider _slider;
    [SerializeField]
    string _playerPrefKey;

    private void Start()
    {
        _slider = GetComponent<Slider>();
        Debug.Log(PlayerPrefs.GetFloat(_playerPrefKey));
        if (PlayerPrefs.HasKey(_playerPrefKey))
            _slider.value = PlayerPrefs.GetFloat(_playerPrefKey);
            
        Universal_GeneralVariables.OnPlayerPrefs += UpdateData;
        _slider.onValueChanged.AddListener((value) => PlayerPrefs.SetFloat(_playerPrefKey, value));

        
    }

    private void UpdateData()
    {
        if (PlayerPrefs.HasKey(_playerPrefKey))
            _slider.value = PlayerPrefs.GetFloat(_playerPrefKey);
        Debug.Log(_playerPrefKey + " " +  PlayerPrefs.GetFloat(_playerPrefKey));
    }

    public void OnDestroy()
    {
        Universal_GeneralVariables.OnPlayerPrefs -= UpdateData;
    }
}
