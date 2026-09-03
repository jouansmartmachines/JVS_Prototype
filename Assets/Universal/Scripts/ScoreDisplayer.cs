using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplayer : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI[] _texts;

    public void Init(string[] texts, Color textColor, TMP_FontAsset textFont = null)
    {
        for (int i = 0; i < texts.Length; i++)
        {
            if (i >= _texts.Length) break;

            _texts[i].text = texts[i];
            _texts[i].color = textColor;
            if (textFont != null) _texts[i].font = textFont;
        }
    }
}
