using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private ScriptableObjectValue _timer;
    [SerializeField] private ScriptableObjectValue _timerInit;
    [SerializeField] private GameEvent _endGame;
    [SerializeField] private List<MonoBehaviour> _objectToDestroy;
    [SerializeField] private bool _isStop;
    void Start()
    {
        _timer.value = _timerInit.value;
    }

    // Update is called once per frame
    void Update()
    {
        if(_timer.value > _timerInit.value)
            _timer.value = _timerInit.value;

        if (_isStop)
            return;

        if(_timer.value <= 0)
        {
            _endGame.Raise();
            foreach (MonoBehaviour mono in _objectToDestroy)
                Destroy(mono);
            Destroy(this);
        }

        _timer.value -= Time.deltaTime;
    }

    public void StopTime(bool mode) 
    {
        _isStop = mode;
    }
}
