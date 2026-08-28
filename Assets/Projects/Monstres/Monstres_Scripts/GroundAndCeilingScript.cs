using UnityEngine;

namespace Monstres
{
    public class GroundAndCeilingScript : MonoBehaviour
    {
        [SerializeField] private BonusMonster bonusMonster;
        [SerializeField] private bool activate;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.gameObject.GetComponent<Script_TargetBonus>() != null)
            {
                if (!activate)
                {
                    bonusMonster.bonusValues.timerBeforeImpulse = bonusMonster.bonusValues.maxBeforeImpulseTime;
                }
                bonusMonster.alreadyAscended = activate;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.GetComponent<Script_TargetBonus>() != null)
            {
                if (!activate)
                {
                    bonusMonster.bonusValues.timerBeforeImpulse = bonusMonster.bonusValues.maxBeforeImpulseTime;
                }
                bonusMonster.alreadyAscended = activate;
            }
        }
    }
}