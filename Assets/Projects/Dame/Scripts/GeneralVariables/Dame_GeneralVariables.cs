using UnityEngine;
using TMPro;

namespace Dame
{
    public class Dame_GeneralVariables : Universal_GeneralVariables
    {
        public static Dame_GeneralVariables Instance { get; private set; }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        [SerializeField] ScoreBoardDisplayer _scoreBoardDisplayer;
        [SerializeField] TMP_FontAsset _font;
        TMP_FontAsset Font() => _font;
        [SerializeField] Color _winnerColor;

        public const string HighScoreKey = "Dame_HighScore";
        public const string TimePerMoveKey = "Dame_GameTime";
        public const string Player1NameKey = "Dame_Player1";
        public const string Player2NameKey = "Dame_Player2";
        public const string ThemeKey = "Dame_Theme";

        public override void ReceiveName(string name)
        {
            float score = PlayerPrefs.GetFloat(HighScoreKey);
            PlayerData data = new PlayerData() { Name = name, Score = score };
            PlayerData defaultPlayer = new PlayerData() { Name = Localizer.Get("Unknown"), Score = 0 };

            _scoreBoardDisplayer.InitScoreBoard(
                ScoreBoardManager.UpdateScoreBoardDescendingOrder(data, GameScoreBoard.Dame),
                Font, _winnerColor, defaultPlayer);
        }
    }
}