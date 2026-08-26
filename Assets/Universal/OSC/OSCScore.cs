using OSC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OSCScore : MonoBehaviour 
{ 
    // Start is called before the first frame update
    void Start()
    {
     OSC_Manager.Instance.GameEnCours();
    }
}
