using UnityEngine;
using TMPro;

namespace Sparks
{
    public class Sparks_GeneralVariables : Universal_GeneralVariables
    {
        public static Sparks_GeneralVariables Instance { get; private set; }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        [SerializeField] ScoreBoardDisplayer _scoreBoardDisplayer;
        [SerializeField] TMP_FontAsset _font;
        TMP_FontAsset Font() => _font;
        [SerializeField] Color _winnerColor;

        public const string HighScoreKey = "Sparks_HighScore";
        public const string ModeRapideKey = "Sparks_ModeRapide";
        public const string GameTimeKey = "Sparks_GameTime";

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
                ScoreBoardManager.UpdateScoreBoardDescendingOrder(data, GameScoreBoard.Sparks),
                Font, _winnerColor, defaultPlayer);
        }
    }
}