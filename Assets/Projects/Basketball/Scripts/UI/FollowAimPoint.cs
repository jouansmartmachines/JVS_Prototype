using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basket
{
    public class FollowAimPoint : MonoBehaviour
    {
        [SerializeField] Camera _cam;
        [SerializeField] Transform _aimPoint;

        public void Update()
        {
            var pos = _cam.WorldToScreenPoint(_aimPoint.position);
            transform.position = pos;
        }
    }
}