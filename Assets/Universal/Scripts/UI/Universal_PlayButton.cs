using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Universal
{
    public class Universal_PlayButton : Universal_Button
    {
        public void Awake()
        {
            _event.AddListener(Play);
        }

        private void Play()
        {
            GameObject.FindObjectOfType<Universal_GeneralVariables>().StartGame();
        }
    }
}