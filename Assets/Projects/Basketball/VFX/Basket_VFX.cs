using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Basket_VFX : MonoBehaviour
{
    Transform _target;

    public void Setup(Transform target)
    {
        _target = target;
    }

    public void Update()
    {
        if (_target == null) return;
        //this.transform.LookAt(_target.transform, Vector3.up);
        //this.transform.localEulerAngles = new Vector3(-this.transform.localEulerAngles.x, 180, -this.transform.localEulerAngles.z);
        this.transform.forward = _target.forward;
    }
}
