using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Basket
{
    [RequireComponent(typeof(Collider))]
    public class NetSensor : MonoBehaviour
    {
        public UnityEvent _event;
        public bool IsActive = true;
        private void OnTriggerEnter(Collider collider)
        {
            //Debug.Log(collider.CompareTag("Ball"));
            if (collider.CompareTag("Ball") && IsActive)
            {
                //Debug.Log($"{this.gameObject.name} Point");
                _event?.Invoke();
            }
        }
    }
}