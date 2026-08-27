using TMPro;
using UnityEngine;

namespace Demolition
{
    public class Demolition_LB_Scores : MonoBehaviour
    {
        public TextMeshProUGUI rankText;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI scoreText;

        public void SetupScore(string rank, string name, string score, Color color)
        {
            if (rankText != null) rankText.text = rank;
            if (nameText != null) nameText.text = name;
            if (scoreText != null) scoreText.text = score;

            // Couleur pour le nouveau score
            if (rankText != null) rankText.color = color;
            if (nameText != null) nameText.color = color;
            if (scoreText != null) scoreText.color = color;
        }
    }
}