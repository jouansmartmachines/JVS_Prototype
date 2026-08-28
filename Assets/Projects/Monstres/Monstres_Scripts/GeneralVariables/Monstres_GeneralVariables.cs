using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Monstres
{
    public class Monstres_GeneralVariables : Universal_GeneralVariables
    {
        public static Monstres_GeneralVariables Instance { get; private set; }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            //CheckSavedData();
        }

        public const string UseDefaultPictureKEY = "Monstres_UseDefaultPicture";

        [SerializeField]
        ScoreBoardDisplayer _scoreBoardDisplayer;

        [SerializeField]
        TMP_FontAsset _font;
        TMP_FontAsset Font() => _font;

        [SerializeField]
        Color _winnerColor;

        public const string HighScoreKey = "Challenge_HighScore";

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

            _scoreBoardDisplayer.InitScoreBoard(ScoreBoardManager.UpdateScoreBoardDescendingOrder(data, GameScoreBoard.Monstres), Font, _winnerColor, defaultPlayer);
        }
    }
}