using System;
using UnityEngine;

namespace Monstres
{
    [Serializable]
    public class BonusValues
    {
        public BonusEnum monsterEnum;
        [Space(10)]
        [Tooltip("Speed of the bonus monster")] public float speed;
        [Tooltip("At what strength the bonus monster is going upside")] public float flightImpulseAmount;
        [Tooltip("Irregular impulse to break monotonous behaviour")] public float irregularImpulse;
        [Space(10)]
        [Tooltip("Range before it starts to go up")] public float minBeforeImpulseTime;
        [Tooltip("Range before it starts to go up")] public float maxBeforeImpulseTime;
        [Space(10)]
        [Tooltip("Range it goes up")] public float minTimeOfImpulse;
        [Tooltip("Range it goes up")] public float maxTimeOfImpulse;
        [Space(10)]
        [Tooltip("Range for time to ascend again")] public float minTimeToAscendAgain;
        [Tooltip("Range for time to ascend again")] public float maxTimeToAscendAgain;
        [Space(10)]
        [Tooltip("Gravity strength on the bonus monster")] public float gravityStrength;

        #region Hidden
        [HideInInspector] public float timerBeforeImpulse;
        [HideInInspector] public float defaultTimerBeforeImpulse;
        [HideInInspector] public float timeOfImpulse;
        [HideInInspector] public float defaultTimeOfImpulse;
        [HideInInspector] public float timeToAscendAgain;
        [HideInInspector] public float defaultTimeToAscendAgain;
        #endregion
    }
}
