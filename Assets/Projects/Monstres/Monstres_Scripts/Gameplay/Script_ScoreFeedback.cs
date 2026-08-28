using UnityEngine;
using TMPro;

namespace Monstres
{
    public class Script_ScoreFeedback : MonoBehaviour
    {
        public TextMeshProUGUI textComp;

        public void DestroyGameObject()
        {
            Destroy(gameObject);
        }

        public void SetupText(int scoreAddValue, Color newTextColor)
        {
            if (scoreAddValue >= 0)
            {
                textComp.text = "+" + scoreAddValue.ToString("");
                textComp.color = newTextColor;
            }
            else
            {
                textComp.text = scoreAddValue.ToString("");
                textComp.color = Color.red;
            }

        }
    }
}