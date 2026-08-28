using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using MenuSelection;
using System.Collections;

namespace Challenge
{
    public class Challenge_ScoreManager : MonoBehaviour
    {
        public TMP_Text scoreText; // On garde scoreText si tu l'assignes à la main
        private TMP_Text minText, secTensText, secUnitsText; // Passés en privé car récupérés par script
        
        public float startTime = 60f;
        public GameObject popupParent;
        public GameObject timeParent;
        public GameObject popupEndParent;
        public float _time;
        private bool timeOver;
        private readonly HashSet<ITarget> targets = new();

        public int CurrentScore { get; private set; }

        public event Action<int> OnScoreChanged;
        public event Action OnTimeOver;

        public TMP_Text scoreTMP;
        public TMP_Text popupTMP;      
        public Image popupImg; 

        void Start()
        {
            // --- RÉCUPÉRATION DYNAMIQUE ---
            // On cherche dans les enfants les noms spécifiques demandés
            TMP_Text[] childrenTexts = timeParent.GetComponentsInChildren<TMP_Text>(true);
            foreach (var txt in childrenTexts)
            {
                if (txt.gameObject.name == "Minutes") minText = txt;
                else if (txt.gameObject.name == "Dec") secTensText = txt;
                else if (txt.gameObject.name == "Mili") secUnitsText = txt;
            }

            _time = (int)Challenge_GeneralVariables.GetGameDurationFromPrefs();
            CurrentScore = 0;
            UpdateScore();
            UpdateTime();
        }

        void Update()
        {
            if (timeOver) return;
            _time -= Time.deltaTime;
            UpdateTime();

            if (_time <= 0f)
            {
                _time = 0;
                timeOver = true;
                OnTimeOver?.Invoke();
                Debug.Log("TIME OVER");

                StartCoroutine(EndGameRoutine(15f));
            }
        }

        private IEnumerator EndGameRoutine(float delay)
        {
            ShowPopup();
            Challenge_AudioManager.i.PlayOneShot(SoundType.Fin);
            yield return new WaitForSeconds(delay);

            PlayerPrefs.SetFloat(Challenge_GeneralVariables.HighScoreKey, CurrentScore);

            if (BuildState.CurrentState == BuildState.State.normal)
            {
                SceneManager.LoadScene(Challenge_GeneralVariables.i.scoreScene);
                
            }
            else
            {
                if (MenuSelectionButton.Instance != null)
                    MenuSelectionButton.Instance.gameObject.SetActive(true);
            }
        }

        public void Subscribe(ITarget t)
        {
            if (t == null || targets.Contains(t)) return;
            targets.Add(t);
            t.OnDeath += OnDeath;
        }

        private void OnDeath(ITarget t, DeathCause cause)
        {
            // Logique de mort si nécessaire
        }

        public void AddScore(int s)
        {
            CurrentScore += s;
            UpdateScore();
            OnScoreChanged?.Invoke(CurrentScore);
        }

        public void AddTime(float t)
        {
            if (!timeOver)
            {
                _time += t;
                UpdateTime();
            }
        }

        void UpdateScore()
        {
            if (scoreText) scoreText.text = Localizer.Get("Score") + " " + CurrentScore;
        }

        void UpdateTime()
        {
            // Utilise les références récupérées dynamiquement
            if (minText && secTensText && secUnitsText)
            {
                float displayTime = Mathf.Max(0, _time);
                int m = Mathf.FloorToInt(displayTime / 60);
                int s = Mathf.FloorToInt(displayTime % 60);
                minText.text = m.ToString();
                secTensText.text = (s / 10).ToString();
                secUnitsText.text = (s % 10).ToString();
            }
        }

        private void ShowPopup()
        {
            var settings = Challenge_LevelManager.CurrentLevelSettings;
            if (popupEndParent) popupEndParent.SetActive(true);

            if (popupTMP)
            {
                /*
                popupTMP.text = 
                    $"Bravo tu as atteint le niveau <size=140%>{settings.level}</size>\n" +
                    $"avec <size=120%>{CurrentScore}</size> points";
                */

                popupTMP.text = 
                    $"{Localizer.Get("Level_Reached")} <size=140%>{settings.level}</size>\n" +
                    $"{Localizer.Get("With")} <size=120%>{CurrentScore}</size> {Localizer.Get("Points")}";
            }

            if (popupImg) popupImg.color = settings.color;
        }
    }
}