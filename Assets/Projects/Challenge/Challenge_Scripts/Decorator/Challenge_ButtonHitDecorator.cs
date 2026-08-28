using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Challenge
{
    public class ButtonHitDecorator : Challenge_TargetDecorator
    {
        private Universal_Button instanceButton;

        private void Start()
        {
            instanceButton = gameObject.GetComponent<Universal_Button>();
            //Debug.Log(instanceButton);
            instanceButton.Event.AddListener(() =>
            {
                target.OnHit();
            });
        }
    }
}
