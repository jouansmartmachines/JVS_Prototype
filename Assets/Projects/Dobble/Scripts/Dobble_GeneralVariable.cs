using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Dobble
{
    public class Dobble_GeneralVariable : Universal_GeneralVariables
    {
        public static Dobble_GeneralVariable i;
        private void Awake()
        {
            if (i != null)
            {
                Destroy(gameObject);
                return;
            }

            i = this;
        }


        [SerializeField] ScoreBoardDisplayer _scoreBoardDisplayer;

        [SerializeField]
        TMP_FontAsset _font;
        TMP_FontAsset Font() => _font;

    
        [SerializeField]
        Color _winnerColor;

        public const string NbrSymbols = "Dobble_NbrSymbols";
        public const string HighScoreKey = "Dobble_HighScore";
        public const string GameTime = "Dobble_GameTime";

        public const string PlayerNames1 = "Dobble_Name1";
        public const string PlayerNames2 = "Dobble_Name2";

      
        public static string GetPlayerNamesFromPrefs(int playerIndex)
        {
            string key = playerIndex == 0 ? PlayerNames1 : PlayerNames2;
            
            if (PlayerPrefs.HasKey(key))
            {
                return PlayerPrefs.GetString(key);
            }
            return key;
        }
        public void UpdateDifficulty(int value)
        {
            PlayerPrefs.SetInt("Différences_Difficulty", value);
        }

        public static int GetNbrOfSymbolsFromPrefs()
        {
            int index = PlayerPrefs.GetInt(NbrSymbols, 0);
            return index switch
            {
                0 => 4,
                1 => 5,
                2 => 6,
                _ => 4
            };
        }

        public static float GetSceneDurationFromPrefs()
        {
            int index = PlayerPrefs.GetInt(GameTime, 0);
            return index switch
            {
                0 => 60f,
                1 => 90f,
                2 => 120f,
                _ => 60f
            };
        }



        public override void ReceiveName(string name)
        {
            float score = PlayerPrefs.GetFloat(HighScoreKey);
         

            PlayerData data = new PlayerData()
            {
                Name = name,
                Score = score,
            };

            PlayerData defaultPlayer = new PlayerData()
            {
                Name = Localizer.Get("Unknown"),
                Score = 0,
            };

            _scoreBoardDisplayer.InitScoreBoard(ScoreBoardManager.UpdateScoreBoardDescendingOrder(data, GameScoreBoard.Pareil), Font, _winnerColor, defaultPlayer);
        }
    
    }
}