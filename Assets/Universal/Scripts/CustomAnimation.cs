using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CustomAnimation : MonoBehaviour
{
    [SerializeField] protected CustomAnimation _next;
    [SerializeField] protected bool _notAtStart;
    [SerializeField] protected Color _color;
    [SerializeField] protected GameEvent _SOEvent;
    public virtual void launchAnim() 
    {
        StartCoroutine(Animate());
    }
    public abstract IEnumerator Animate();
    public virtual void SetColor(Color color)
    { 
        _color = color;

    }

    public virtual void Start() 
    {
        if(!_notAtStart)
            launchAnim();
    }
}
