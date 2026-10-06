using UnityEngine;
using System.Collections.Generic;

namespace Freethespirit
{
    public class Freethespirit_GuardCombat : MonoBehaviour
    {
        public static readonly List<Freethespirit_Guard> ActiveGuards = new List<Freethespirit_Guard>();

        public int MaxHits { get; private set; }
        public int CurrentHits { get; private set; }
        public bool IsDead => CurrentHits >= MaxHits;

        public static bool GroupCombatActive { get; private set; }
        public static bool IsGroupGuardFinished => GroupCombatActive && Time.time >= groupGuardEndTime;

        private static float groupGuardEndTime;

        public static void RegisterGuard(Freethespirit_Guard guard)
        {
            if (guard != null && !ActiveGuards.Contains(guard)) ActiveGuards.Add(guard);
        }

        public static void UnregisterGuard(Freethespirit_Guard guard)
        {
            if (guard != null) ActiveGuards.Remove(guard);
        }

        public void InitializeHealth(int configuredHits)
        {
            MaxHits = configuredHits > 0 ? configuredHits : CalculateHitsByLevel();
            CurrentHits = 0;
        }

        public bool TakeDamage()
        {
            CurrentHits++;
            return IsDead;
        }

        public static int CalculateHitsByLevel()
        {
            float roll = Random.value;

            switch (Freethespirit_GameManager.CurrentDifficulty)
            {
                case 1:
                    return roll < 0.80f ? 2 : 3;

                case 2:
                    if (roll < 0.50f) return 3;
                    if (roll < 0.80f) return 4;
                    return 2;

                case 3:
                    if (roll < 0.40f) return 5;
                    if (roll < 0.80f) return 4;
                    return 3;

                default:
                    return 2;
            }
        }

        public static void BeginGroupCombat(Freethespirit_Guard attacker, float defensiveDuration)
        {
            GroupCombatActive = true;
            groupGuardEndTime = Time.time + defensiveDuration;

            for (int i = 0; i < ActiveGuards.Count; i++)
            {
                Freethespirit_Guard guard = ActiveGuards[i];
                if (guard == null || guard.currentState == GuardState.Dead || guard.guardMode == GuardMode.Static) continue;
                guard.EnterDefensiveState();
            }
        }

        public static void EndGroupCombat()
        {
            if (!GroupCombatActive) return;

            GroupCombatActive = false;
            groupGuardEndTime = 0f;

        }

        void Update()
        {
            if (!GroupCombatActive || Time.time < groupGuardEndTime) return;

            for (int i = 0; i < ActiveGuards.Count; i++)
            {
                Freethespirit_Guard guard = ActiveGuards[i];
                if (guard == null) continue;

                if (guard.gameObject == gameObject) EndGroupCombat();
                break;
            }
        }

        public void AlertAllGuards(Freethespirit_Guard source)
        {
            for(int i=0;i<ActiveGuards.Count;i++)
            {
                Freethespirit_Guard guard=ActiveGuards[i];

                if(guard==null||guard==source||guard.currentState==GuardState.Dead)
                    continue;

                guard.ReceiveAlert(source.transform.position,0f);
            }
        }
    }
}