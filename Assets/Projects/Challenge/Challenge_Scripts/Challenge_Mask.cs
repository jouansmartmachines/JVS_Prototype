using UnityEngine;

namespace Challenge
{
    public class Challenge_Mask : Challenge_BaseInteractive
    {
        private void Awake()
        {
            type = ObjectType.Mask;
        }

        public override void TakeDamage(int amount)
        {
            TriggerHitEvent();
            Challenge_AudioManager.i.PlayOneShot(SoundType.Goal);
        }

        public override void Move() { /* Géré par les décorateurs */ }

        public override void OnHit() => TakeDamage(0);
    }
}