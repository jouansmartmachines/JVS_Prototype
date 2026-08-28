using UnityEngine;
using System;

namespace Challenge
{
    public class Challenge_TargetDecorator : MonoBehaviour, ITarget
    {
        protected ITarget target;

        public virtual void SetTarget(ITarget t) => target = t;

        public Vector2 pos
        {
            get => target != null ? target.pos : Vector2.zero;
            set { if (target != null) target.pos = value; }
        }

        public ObjectType Type => target != null ? target.Type : ObjectType.Other;
        public int Stage => target != null ? target.Stage : 0;

        public virtual void TakeDamage(int amount) => target?.TakeDamage(amount);
        public virtual void Move() => target?.Move();
        public virtual void OnHit() => target?.OnHit();

        public event Action<ITarget, DeathCause> OnDeath
        {
            add { if (target != null) target.OnDeath += value; }
            remove { if (target != null) target.OnDeath -= value; }
        }

        public event Action<ITarget> OnHitEvent
        {
            add { if (target != null) target.OnHitEvent += value; }
            remove { if (target != null) target.OnHitEvent -= value; }
        }
    }
}
