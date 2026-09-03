using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Theme
{
    [CreateAssetMenu(fileName = "SwapAnimator", menuName = "Game/Theme/Entity/SwapAnimator")]
    public class SwapAnimator : SwapEntity
    {
        public RuntimeAnimatorController AnimatorController;
    }
}