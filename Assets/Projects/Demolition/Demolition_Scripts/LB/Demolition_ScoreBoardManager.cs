using TMPro;
using UnityEngine;

namespace Demolition
{
    public class Demolition_ScoreBoardManager : MonoBehaviour
    {
        public static Demolition_ScoreBoardManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        [SerializeField]
        ScoreBoardDisplayer _scoreBoardDisplayer;

        [SerializeField]
        TMP_FontAsset _font;
        TMP_FontAsset Font() => _font;

        [SerializeField]
        Color _winnerColor;

        public void OnReceiveName(string name)
        {
            float score = PlayerPrefs.GetFloat(Demolition_GeneralVariables.HighScoreKey);

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