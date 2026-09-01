using UnityEngine;
using TMPro;

namespace Demolition
{
    public class Demolition_GeneralVariables : Universal_GeneralVariables
    {
        public static Demolition_GeneralVariables Instance { get; private set; }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        [SerializeField] ScoreBoardDisplayer _scoreBoardDisplayer;
        [SerializeField] TMP_FontAsset _font;
        TMP_FontAsset Font() => _font;
        [SerializeField] Color _winnerColor;

        public const string HighScoreKey = "Demolition_HighScore";
        public const string ModeOiseauKey = "Demolition_ModeOiseau";
        public const string GameTimeKey = "Demolition_GameTime";
        public const string ScrollSpeedKey = "Demolition_ScrollSpeed";

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

            _scoreBoardDisplayer.InitScoreBoard(
                ScoreBoardManager.UpdateScoreBoardDescendingOrder(data, GameScoreBoard.Demolition),
                Font, _winnerColor, defaultPlayer);
        }
    }
}