using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Basket
{
    public class Basket_ScoreBoardManager : MonoBehaviour
    {
        public static Basket_ScoreBoardManager i;
        private void Awake()
        {
            if (i != null)
            {
                Destroy(gameObject);
                return;
            }

            i = this;
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
            float score = PlayerPrefs.GetFloat(Basket_GeneralVariable.HighScoreKey);

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

            _scoreBoardDisplayer.InitScoreBoard(ScoreBoardManager.UpdateScoreBoardDescendingOrder(data, GameScoreBoard.BasketballBoard), Font, _winnerColor, defaultPlayer);
        }
    }
}