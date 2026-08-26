using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Universal_AnimBehaviour : StateMachineBehaviour
{
    [SerializeField] bool _destroyAtBegin = false;
    [SerializeField] bool _destroyAtEnd = false;

    private bool hasEnded = false;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //Debug.Log("Enter : " + stateInfo + " of " + animator.gameObject.name);

        hasEnded = false;
        if (_destroyAtBegin)
        {
            Destroy(animator.gameObject);
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!hasEnded && stateInfo.normalizedTime >= 1f && !stateInfo.loop)
        {
            hasEnded = true;
            if(_destroyAtEnd) Destroy(animator.gameObject);
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}
