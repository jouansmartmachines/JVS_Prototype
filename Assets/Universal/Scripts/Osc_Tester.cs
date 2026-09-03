using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OSC
{
    public class Osc_Tester : MonoBehaviour
    {
        [SerializeField] string sceneName;
        [SerializeField, Range(1, 4)] int joyeux;

        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                float x = (float)Input.mousePosition.x / (float)Screen.width;
                float y = (float)Input.mousePosition.y / (float)Screen.height;
                OscMessage message = new OscMessage("/point");
                message.Add(x);
                message.Add(y);
                //Debug.Log("Hit : " + x + " ; " + y);
                OSC_Manager.Instance.onOSCPoint(message);
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                OscMessage msg = new OscMessage("/remote");
                msg.Add("OSC Tester");
                OSC_Manager.Instance.onOSCNameGamer(msg);
            }

            /*
            if (Input.GetKeyDown(KeyCode.L))
            {
                OscMessage msg = new OscMessage("/remote");
                msg.Add(sceneName);
                OSC_Manager.Instance.onOSCLaunch(msg);
            }
            */
            /*
            if (Input.GetKeyDown(KeyCode.J))
            {
                OscMessage msg = new OscMessage("/remote");
                msg.Add(joyeux);
                OSC_Manager.Instance.OnJoyeux(msg);
            }
            */
        }
    }
}

