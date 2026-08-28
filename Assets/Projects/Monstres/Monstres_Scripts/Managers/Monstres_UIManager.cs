using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Theme;

namespace Monstres
{
    public class Monstres_UIManager : MonoBehaviour
    {
        public static Monstres_UIManager Instance { get; private set; }
        public TextMeshProUGUI scoreText;
        public Image timerImage;
        public GameObject endUI;
        public Transform timerBarPivot;
        public Image timer;

        public SwapObject rockSprites;

        private float rockTimeInterval;
        private float lastTimeChange = 0f;
        private int rockSpriteIdx = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            rockTimeInterval = Monstres_GameManager.Instance.gameDuration / rockSprites.GetSwapEntity<SwapSprite>().Sprites.Count;
            lastTimeChange = Monstres_GameManager.Instance.gameDuration;
            timerImage.sprite = rockSprites.GetSwapEntity<SwapSprite>().Sprites[0];
        }

        public void UpdateScore(int newScore)
        {
            if (newScore > 0)
            {
                //scoreText.text = newScore.ToString("") + " "points";
                scoreText.text = newScore.ToString("") + " "+  Localizer.Get("Points");
            }
            else
            {
                //scoreText.text = newScore.ToString("") + " point";
                scoreText.text = newScore.ToString("") + " "+  Localizer.Get("Point");
            }
        }

        public void UpdateTimer(float currentTimer)
        {
            if (currentTimer <= lastTimeChange - rockTimeInterval)
            {
                lastTimeChange = currentTimer;

                rockSpriteIdx++;
                timerImage.sprite = rockSprites.GetSwapEntity<SwapSprite>().Sprites[rockSpriteIdx];
            }
        }

        public void ShowEnd()
        {
            endUI.SetActive(true);
        }
    }
}