using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Challenge
{
    public enum ObjectType
    {
        Target,
        Mask,
        Other
    }
    public interface ITarget
    {
        void TakeDamage(int amount);
        void Move();
        void OnHit();

        Vector2 pos { get; set; }

        event Action<ITarget, DeathCause> OnDeath;
        event Action<ITarget> OnHitEvent;


        int Stage { get; }

        ObjectType Type { get; }
    }
    public abstract class Challenge_BaseInteractive : MonoBehaviour, ITarget
    {
        [SerializeField] protected int stage = 1;
        [SerializeField] protected ObjectType type;
        [SerializeField] protected Vector2 _pos;
        public int Stage => stage;
        public void SetStage(int newLevel)
        {
            stage = newLevel;
        }

        public ObjectType Type => type;
        public event Action<ITarget, DeathCause> OnDeath;
        public event Action<ITarget> OnHitEvent;

        protected void TriggerHitEvent()
        {
            OnHitEvent?.Invoke(this);
        }

        protected void TriggerDeathEvent(DeathCause cause)
        {
            
            OnDeath?.Invoke(this,cause);
        }

        public Vector2 pos
        {
            get => _pos;
            set => _pos = value;
        }
        public abstract void TakeDamage(int amount);
        public abstract void Move();
        public abstract void OnHit();
    }

}
