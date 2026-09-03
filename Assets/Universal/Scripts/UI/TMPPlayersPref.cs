using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPPlayersPref : MonoBehaviour
{
    [SerializeField] string _key;
    TextMeshProUGUI _text;

    public void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();

        if(PlayerPrefs.HasKey(_key))
            _text.text = PlayerPrefs.GetString(_key);
    }
}
