using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  abstract class Button : MonoBehaviour
{
    protected bool gotAPt;
    protected Vector3 newPt;
    public virtual void ReceivePointFromManager(Vector3 point)
    {
        newPt = point;
        gotAPt = true;
    }
}
