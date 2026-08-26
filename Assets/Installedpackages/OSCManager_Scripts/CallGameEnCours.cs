using OSC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallGameEnCours : MonoBehaviour
{
    private void Awake()
    {
        if (OSC_Manager.Instance != null)
        {
            OSC_Manager.Instance.GameEnCours();
        }
    }
}
