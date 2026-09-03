using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScorePrinter : MonoBehaviour
{
    [SerializeField] ScriptableObjectValue score;
    [SerializeField] TMP_Text ScoreText;

    private void Start()
    {
        ScoreText.text = ((int)score.value).ToString();
    }
}
