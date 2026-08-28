using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Theme;

namespace Challenge
{
    public class Challenge_GeneralVariables : Universal_GeneralVariables
    {
        public static Challenge_GeneralVariables i;
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



        

        public const string ScreenRatio = "Challenge_ScreenRatio";
        public const string HighScoreKey = "Challenge_HighScore";
        public const string GameTime = "Challenge_GameTime";
        public const string Ephemere = "Challenge_EphemereTime";


        public static float GetGameDurationFromPrefs()
        {
            float value = PlayerPrefsHelper.GetValue<float>(GameTime, 150, 1, 300);
            return value; 
        }

        public static float GetScreenRatioFromPrefs()
        {
            float screenRatio = PlayerPrefs.GetFloat(ScreenRatio, 100f);
            return 1f + (100f - screenRatio) / 50f;
        }

        public static float GetEphemereTimeFromPrefs()
        {
            int index = PlayerPrefs.GetInt(Ephemere, 0);
            return index switch
            {
                0 => 10f,
                1 => 15f,
                2 => 20f,
                _ => 10f
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

            _scoreBoardDisplayer.InitScoreBoard(ScoreBoardManager.UpdateScoreBoardDescendingOrder(data, GameScoreBoard.Challenges), Font, _winnerColor, defaultPlayer);
        }
    }
}