using Basket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Basket
{
    public class Basket_GeneralVariable : Universal_GeneralVariables
    {
        public static Basket_GeneralVariable i;
        private void Awake()
        {
            if(i != null)
            {
                Destroy(gameObject);
                return;
            }

            i = this;
        }

        public const string TimerKey = "Basket_Timer";
        public const string DifficultyKey = "Basket_Difficulty";
        public const string HighScoreKey = "Basket_HighScore";

        public override void ReceiveName(string name)
        {
            if(Basket_ScoreBoardManager.i != null)
            {
                Basket_ScoreBoardManager.i.OnReceiveName(name);
            }
        }

    }
}