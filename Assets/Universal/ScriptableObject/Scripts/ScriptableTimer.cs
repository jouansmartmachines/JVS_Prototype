using System.Collections;
using System.Collections.Generic;
using CovidKiller;
using UnityEngine;

public class ScriptableTimer : MonoBehaviour
{
    [SerializeField] private ScriptableObjectValue _time;
    //[SerializeField] private ScriptableObjectValue _timeInit;
    [SerializeField] private GameEvent _timeFinished;
    // Start is called before the first frame update
    [SerializeField] private bool end;
    void Start()
    {
        if(!end)
        {
            _time.value = CrudiCrush_GeneralVariables.GetTime(); 
        } 
        else
        {
            _time.value = 3;
        }

    }

    // Update is called once per frame
    void Update()
    {
        _time.value -= Time.deltaTime;
        if(_time.value <= 0)
        {
            _timeFinished.Raise();
            Destroy(this);
        }
    }
}
