using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Theme
{
    public class SwapAnimatorBehaviour : SwapObjectBehaviour
    {
        [SerializeField] Animator animator;

        protected override void Swap(GameTheme theme)
        {
            var entity = _swapObject.GetSwapEntity(theme) as SwapAnimator;
            Debug.Log(animator.runtimeAnimatorController.name + " => " + entity.AnimatorController.name);
            animator.runtimeAnimatorController = entity.AnimatorController;
            Debug.Log(animator.runtimeAnimatorController.name);
        }
    }
}