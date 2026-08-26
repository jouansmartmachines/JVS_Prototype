using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScorePrefabScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _rank;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _score;
    [SerializeField] private int _monoSpaceValue;
    // Start is called before the first frame update
    public void SetText(string rank,string name, string score, Color color) 
    {
        if(color != null) 
        {
            _rank.color = color;
            _name.color = color;
            _score.color = color;
        }
        _rank.text = rank;
        _name.text = "<mspace="+_monoSpaceValue+">" +name+"</mspace>";
        _score.text = score;
    } 
}
