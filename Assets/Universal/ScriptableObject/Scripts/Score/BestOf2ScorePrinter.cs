using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BestOf2ScorePrinter : MonoBehaviour
{
    [SerializeField] ScriptableObjectValue scoreP1, scoreP2;
    [SerializeField] TMP_Text textScore;
    void Start()
    {
        if (scoreP1.value >= scoreP2.value)
            textScore.text = scoreP1.value.ToString();
        else
            textScore.text = scoreP2.value.ToString();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
