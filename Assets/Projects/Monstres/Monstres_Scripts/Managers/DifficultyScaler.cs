using Monstres;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyScaler : MonoBehaviour
{
    [SerializeField] private List<Monstres_PathManager> monstres_PathManagers = new List<Monstres_PathManager>();
    public float _startCounting = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _startCounting -= Time.deltaTime;
        if (_startCounting > 0)
            return;
        int targets = 0;
        foreach (var path in monstres_PathManagers)
            targets += path.CheckTargets();
        if(targets <= 3) 
        {
            foreach (var path in monstres_PathManagers) 
            {
                path.SpeedUpNoSmoothSlower();
                _startCounting = 10;
            }
        }
    }
}
