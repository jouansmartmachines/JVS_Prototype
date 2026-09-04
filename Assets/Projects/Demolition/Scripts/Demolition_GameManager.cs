using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using MenuSelection;

namespace Demolition
{
    public class Demolition_GameManager : MonoBehaviour
    {
        public static Demolition_GameManager Instance { get; private set; }

        [Header("Timers")]
        public float sceneDuration = 60f;
        public float sceneTimer;
        public float globalTimer = 300f;
        public bool useGlobalTimer = true;

        [Header("Score")]
        public int score { get; private set; }
        public int sceneScore;

        [Header("UI")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI sceneText;
        public TextMeshProUGUI globalTimerText;

        [Header("Fondu")]
        public CanvasGroup fadeCanvasGroup;
        public float fadeInDuration = 0.5f;
        public float fadeOutDuration = 1f;

        [Header("Audio")]
        public AudioClip sceneClearSound;
        public AudioClip gameOverSound;
        private AudioSource audioSource;

        [Header("Scrolling")]
        public float currentScrollSpeed = 2f;
        public Transform structuresParent;

        private bool isRunning = false;
        private bool isGameOver = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        void Start()
        {
            EnsureSceneElements();
            LoadPreferences();

            sceneTimer = sceneDuration;
            sceneScore = 0;
            isRunning = true;
            isGameOver = false;

            StartCoroutine(FadeIn());
            UpdateUI();
        }

        private void LoadPreferences()
        {
            sceneDuration = Demolition_GeneralVariables.GetSceneDurationFromPrefs();
            globalTimer = Demolition_GeneralVariables.GetGlobalTimeFromPrefs();
        }

        private void EnsureSceneElements()
        {
            if (scoreText == null)
                scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            if (timerText == null)
                timerText = GameObject.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
            if (sceneText == null)
                sceneText = GameObject.Find("SceneNumberText")?.GetComponent<TextMeshProUGUI>();
            if (globalTimerText == null)
                globalTimerText = GameObject.Find("GlobalTimerText")?.GetComponent<TextMeshProUGUI>();

            if (fadeCanvasGroup == null)
            {
                var fadeGo = GameObject.Find("FadeCanvas");
                if (fadeGo != null)
                    fadeCanvasGroup = fadeGo.GetComponent<CanvasGroup>();
            }
        }

        void Update()
        {
            if (!isRunning || isGameOver) return;

            sceneTimer -= Time.deltaTime;
            if (sceneTimer <= 0)
            {
                sceneTimer = 0;
                EndScene("Temps écoulé !");
            }

            if (useGlobalTimer)
            {
                globalTimer -= Time.deltaTime;
                if (globalTimer <= 0)
                {
                    globalTimer = 0;
                    EndGame();
                }
            }

            UpdateUI();
        }

        public void OnFantomeKilled()
        {
            if (!isRunning || isGameOver) return;
            EndScene("Fantôme vaincu !");
        }

        public void AddScore(int points, Vector3 pos)
        {
            score += points;
            sceneScore += points;
            UpdateUI();
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip);
        }

        private void EndScene(string reason)
        {
            if (isGameOver) return;
            isRunning = false;

            Debug.Log($"Demolition: Scene terminée - {reason}");

            PlayerPrefs.SetInt("Demolition_SceneScore", sceneScore);

            if (sceneClearSound != null)
                audioSource.PlayOneShot(sceneClearSound);

            StartCoroutine(FadeOutAndReload());
        }

        private void EndGame()
        {
            if (isGameOver) return;
            isGameOver = true;
            isRunning = false;

            Debug.Log("Demolition: Partie terminée (timer global)");

            int highScore = PlayerPrefs.GetInt(Demolition_GeneralVariables.HighScoreKey, 0);
            if (score > highScore)
                PlayerPrefs.SetInt(Demolition_GeneralVariables.HighScoreKey, score);

            PlayerPrefs.SetInt("Demolition_FinalScore", score);
            PlayerPrefs.Save();

            if (gameOverSound != null)
                audioSource.PlayOneShot(gameOverSound);

            StartCoroutine(TransitionToScore());
        }

        private IEnumerator FadeIn()
        {
            if (fadeCanvasGroup == null) yield break;
            float t = 0;
            fadeCanvasGroup.alpha = 1f;
            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeInDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
        }

        private IEnumerator FadeOutAndReload()
        {
            yield return new WaitForSeconds(1f);

            if (fadeCanvasGroup != null)
            {
                float t = 0;
                while (t < fadeOutDuration)
                {
                    t += Time.deltaTime;
                    fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeOutDuration);
                    yield return null;
                }
                fadeCanvasGroup.alpha = 1f;
            }

            // Recharger la même scène (reset)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private IEnumerator TransitionToScore()
        {
            yield return new WaitForSeconds(2f);

            if (BuildState.CurrentState == BuildState.State.normal)
                SceneManager.LoadScene(Demolition_GeneralVariables.Instance?.scoreScene ?? "Score_Demolition");
            else if (MenuSelectionButton.Instance != null)
                MenuSelectionButton.Instance.gameObject.SetActive(true);
        }

        public void PlaySfx(AudioClip clip, float pitch, float volume)
        {
            if (clip != null && audioSource != null)
            {
                float origPitch = audioSource.pitch;
                float origVolume = audioSource.volume;
                audioSource.pitch = pitch;
                audioSource.volume = volume;
                audioSource.PlayOneShot(clip);
                audioSource.pitch = origPitch;
                audioSource.volume = origVolume;
            }
        }

        public void AddScore(int points, Vector3 pos, Color popupColor, float popupScale, string prefix)
        {
            score += points;
            sceneScore += points;
            UpdateUI();

            // Popup flottant
            GameObject popupGO = new GameObject("ScorePopup");
            popupGO.transform.position = pos;
            var popup = popupGO.AddComponent<Demolition_PopupText>();
            popup.SetText(prefix + points.ToString(), popupColor, popupScale);
        }

        public void TriggerImpactFeel(Vector3 pos, int hitCount)
        {
            // Petit effet de punch au ralenti sur les impacts
            StartCoroutine(ImpactSlowMo());
        }

        private IEnumerator ImpactSlowMo()
        {
            Time.timeScale = 0.3f;
            yield return new WaitForSecondsRealtime(0.06f);
            Time.timeScale = 1f;
        }

        public IEnumerator CollapseSlowMo()
        {
            Time.timeScale = 0.15f;
            yield return new WaitForSecondsRealtime(0.3f);
            Time.timeScale = 1f;
        }

        public void TriggerPigDestroyed(int starValue)
        {
            Demolition_DebrisSpawner.SpawnStarBurst(Vector3.zero, 5 + starValue * 3);
        }

        private void UpdateUI()
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";

            if (timerText != null)
                timerText.text = Mathf.CeilToInt(sceneTimer).ToString();

            if (sceneText != null)
                sceneText.text = $"Niveau en cours";

            if (globalTimerText != null && useGlobalTimer)
            {
                int mins = Mathf.FloorToInt(globalTimer / 60);
                int secs = Mathf.FloorToInt(globalTimer % 60);
                globalTimerText.text = $"{mins}:{secs:D2}";
            }
        }
    }
}