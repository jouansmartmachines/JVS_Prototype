using System.Collections.Generic;
using UnityEngine;

namespace Monstres
{
    public class BonusValuesScript : MonoBehaviour
    {
        public static BonusValuesScript Instance;
        public List<BonusValues> bonuses = new List<BonusValues>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }
    }
}
