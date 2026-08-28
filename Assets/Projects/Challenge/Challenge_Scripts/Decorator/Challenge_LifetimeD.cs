using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Challenge
{
    public class Challenge_LifetimeD : Challenge_TargetDecorator
    {
        public float lifetime = 5f;
        public float fadeDuration = 1.5f;

        void Start()
        {
            Invoke(nameof(StartFadeAndDestroy), lifetime);
        }

        void StartFadeAndDestroy()
        {
            //StartCoroutine(FadeAndDestroy());
    
            DestroyTarget();
    
        }


        void DestroyTarget()
        {
            if (target is Challenge_Target t)
            {
                t.Die(DeathCause.Lifetime,0f);
            }
        }
    }
}
