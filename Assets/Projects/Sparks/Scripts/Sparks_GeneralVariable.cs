using UnityEngine;
using TMPro;

namespace Sparks
{
    public class Sparks_GeneralVariable : Universal_GeneralVariables
    {
        public static Sparks_GeneralVariable i;

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
        [SerializeField] TMP_FontAsset _font;
        TMP_FontAsset Font() => _font;
        [SerializeField] Color _winnerColor;

        public const string HighScoreKey = "Sparks_HighScore";
        public const string GameTimeKey = "Sparks_GameTime";
        public const string ModeRapideKey = "Sparks_ModeRapide";

        public static float GetGameDurationFromPrefs()
        {
            int index = PlayerPrefs.GetInt(GameTimeKey, 0);
            return index switch
            {
                0 => 60f,
                1 => 90f,
                2 => 120f,
                _ => 60f
            };
        }

        public static bool GetModeRapideFromPrefs()
        {
            return PlayerPrefs.GetInt(ModeRapideKey, 0) == 1;
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

            _scoreBoardDisplayer.InitScoreBoard(
                ScoreBoardManager.UpdateScoreBoardDescendingOrder(data, GameScoreBoard.Sparks),
                Font, _winnerColor, defaultPlayer);
        }
    }
}