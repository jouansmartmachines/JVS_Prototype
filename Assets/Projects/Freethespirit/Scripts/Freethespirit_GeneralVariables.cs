using UnityEngine;
using TMPro;

namespace Freethespirit
{
    public class Freethespirit_GeneralVariables : Universal_GeneralVariables
    {
        public static Freethespirit_GeneralVariables Instance { get; private set; }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        [SerializeField] ScoreBoardDisplayer _scoreBoardDisplayer;
        [SerializeField] TMP_FontAsset _font;
        TMP_FontAsset Font() => _font;
        [SerializeField] Color _winnerColor;

        public const string HighScoreKey = "Freethespirit_HighScore";

        public const string GameTimeKey = "Freethespirit_GameTime";

        public const string SceneTimeKey = "Freethespirit_SceneTime";
        public const string GlobalTimeKey = "Freethespirit_GlobalTime";

        public static float GetSceneDurationFromPrefs()
        {
            int index = PlayerPrefs.GetInt(SceneTimeKey, 1);
            return index switch
            {
                0 => 60f,
                1 => 60f,
                2 => 90f,
                _ => 300f
            };
        }

        public static float GetGlobalTimeFromPrefs()
        {
            return PlayerPrefs.GetFloat(GlobalTimeKey, 15000f);
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
                ScoreBoardManager.UpdateScoreBoardDescendingOrder(data, GameScoreBoard.Freethespirit),
                Font, _winnerColor, defaultPlayer);
        }
    }
}