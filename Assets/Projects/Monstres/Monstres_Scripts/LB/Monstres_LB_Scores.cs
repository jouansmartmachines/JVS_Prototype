using UnityEngine;
using TMPro;

namespace Monstres
{
    public class Monstres_LB_Scores : MonoBehaviour
    {
        public TextMeshProUGUI rank;
        public TextMeshProUGUI player_name;
        public TextMeshProUGUI score;
        [SerializeField] private int _monoSpaceValue;

        public void SetupScore(string s_rank, string s_name, string s_score, Color newScoreColor)
        {
            if (newScoreColor != null)
            {
                rank.color = newScoreColor;
                player_name.color = newScoreColor;
                score.color = newScoreColor;
            }

            rank.text = s_rank;
            player_name.text = "<mspace=" + _monoSpaceValue + ">" + s_name + "</mspace>";
            score.text = s_score;
        }

        public void SetupScore(string s_rank, string s_name, string s_score)
        {
            rank.text = s_rank;
            player_name.text = "<mspace=" + _monoSpaceValue + ">" + s_name + "</mspace>";
            score.text = s_score;
        }
    }
}