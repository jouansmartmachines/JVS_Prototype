using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using MenuSelection;
using IndieKit;
using System.Collections.Generic;

namespace Freethespirit
{   

    public class Freethespirit_GameManager : MonoBehaviour
    {
        public static Freethespirit_GameManager Instance { get; private set; }

        // Données persistantes de session
        public static int currentLevel = 1;
        public static int sessionScore = 0;

        public static bool isDaytime = false;
        public static float sessionGlobalTimer = -1f;

        [Header("Jour / Nuit Debug")]
        public bool overrideDaytime = false;
        public bool debugIsDaytime = true;

        [Header("Progression de Difficulté")]
        [Tooltip("Nombre de scènes avant de passer à la difficulté suivante")]
        [Min(1)]
        public int scenesPerDifficulty = 2;

        public static int CurrentDifficulty { get; private set; }

        public bool overrideDifficulty = false;

        [Range(1, 3)]
        public int debugDifficulty = 1;

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

        

        [SerializeField] private Freethespirit_EnvironmentSpawner envSpawner;
        [SerializeField] private Freethespirit_ObstacleSpawner obstacleSpawner;
        [SerializeField] private Freethespirit_GuardSpawner guardSpawner;

        private readonly List<Freethespirit_Fantome> activeFantomes = new List<Freethespirit_Fantome>();

        public int GetDifficultyForLevel(int level)
        {
            if (overrideDifficulty)
                return debugDifficulty;

            return ((level - 1) / scenesPerDifficulty) + 1;
        }
        public void ApplyDamageToDestructible(IndieKit.IDamageable target, float damage, Vector3 hitPoint)
        {
            if (target != null)
                target.ApplyDamage(damage, hitPoint);
        }

        public void ApplyDamageToAllInRadius(Vector3 center, float radius, float damage)
        {
            Collider[] hits = Physics.OverlapSphere(center, radius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<IndieKit.IDamageable>(out var damageable))
                {
                    damageable.ApplyDamage(damage, center);
                }
            }
        }

        public bool HasDestructiblesInRadius(Vector3 center, float radius)
        {
            Collider[] hits = Physics.OverlapSphere(center, radius);
            foreach (var hit in hits)
                if (hit.GetComponent<Freethespirit_Destructible>() != null)
                    return true;
            return false;
        }

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

            // Restauration de session
            score = sessionScore;
            sceneScore = 0;

            if (sessionGlobalTimer > 0f)
                globalTimer = sessionGlobalTimer;

            sceneTimer = sceneDuration;
            isRunning = true;
            isGameOver = false;

            StartCoroutine(FadeIn());

            // 1. Calcul du Type de niveau
            // Mise à jour de la variable statique
            CurrentDifficulty = GetDifficultyForLevel(currentLevel);

            // 2. Alternance Jour / Nuit (pense bien à mettre "public static bool isDaytime" en haut de ta classe !)
            if (overrideDaytime)
            {
                isDaytime = debugIsDaytime;
            }
            else
            {
                isDaytime = !isDaytime;
            }

            // 3. Application de l'environnement et Spawns sécurisés
            if (envSpawner != null)
            {
                EnvironmentData currentEnvData = isDaytime ? envSpawner.dayEnvironment : envSpawner.nightEnvironment;
                
                envSpawner.ApplyEnvironment(currentEnvData, CurrentDifficulty);

                if (envSpawner.currentEnvInstance != null)
                {
                    var anchor = envSpawner.currentEnvInstance.GetComponent<Freethespirit_ObstacleAnchor>();
                    obstacleSpawner?.SpawnForDifficulty(currentLevel, anchor);
                    guardSpawner?.SpawnGuards(anchor, currentEnvData.robotsMaterial);
                }
            }
            else
            {
                Debug.LogError("Freethespirit_GameManager : envSpawner n'est pas assigné !");
            }

            UpdateUI();
        }

        public void RegisterFantome(Freethespirit_Fantome fantome)
        {
            if (!activeFantomes.Contains(fantome))
                activeFantomes.Add(fantome);
        }

        public void UnregisterFantome(Freethespirit_Fantome fantome)
        {
            if (activeFantomes.Contains(fantome))
                activeFantomes.Remove(fantome);
        }

        private void LoadPreferences()
        {
            sceneDuration = Freethespirit_GeneralVariables.GetSceneDurationFromPrefs();
            if (sessionGlobalTimer <= 0f)
                globalTimer = Freethespirit_GeneralVariables.GetGlobalTimeFromPrefs();
        }

        public void TriggerWinOrReload()
        {
            StartCoroutine(FadeOutAndReload());
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
                sessionGlobalTimer = globalTimer;

                if (globalTimer <= 0)
                {
                    globalTimer = 0;
                    EndGame();
                }
            }

            UpdateUI();
        }



        public void AddScore(int points, Vector3 pos)
        {
            score += points;
            sceneScore += points;
            sessionScore = score;
            UpdateUI();
        }

        public void AddScore(int points, Vector3 pos, Color popupColor, float popupScale, string prefix)
        {
            score += points;
            sceneScore += points;
            sessionScore = score;
            UpdateUI();

            GameObject popupGO = new GameObject("ScorePopup");
            popupGO.transform.position = pos;
            var popup = popupGO.AddComponent<Freethespirit_PopupText>();
            popup.SetText(prefix + points.ToString(), popupColor, popupScale);
        }

        public void PlaySfx(AudioClip clip)
        {
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip);
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

        private void EndScene(string reason)
        {
            if (isGameOver) return;

            isRunning = false;

            Debug.Log($"Freethespirit: Scene terminée - {reason} (Passage au Niveau {currentLevel})");

            PlayerPrefs.SetInt("Freethespirit_SceneScore", sceneScore);

            if (sceneClearSound != null)
                audioSource.PlayOneShot(sceneClearSound);

            // Passe à la scène/niveau suivant
            currentLevel++;

            StartCoroutine(FadeOutAndReload());
        }

        private void EndGame()
        {
            if (isGameOver) return;
            isGameOver = true;
            isRunning = false;

            Debug.Log("Freethespirit: Partie terminée (timer global)");

            int highScore = PlayerPrefs.GetInt(Freethespirit_GeneralVariables.HighScoreKey, 0);
            if (score > highScore)
                PlayerPrefs.SetInt(Freethespirit_GeneralVariables.HighScoreKey, score);

            PlayerPrefs.SetInt("Freethespirit_FinalScore", score);
            PlayerPrefs.Save();

            // Réinitialisation de session pour une prochaine partie
            currentLevel = 1;
            sessionScore = 0;
            sessionGlobalTimer = -1f;

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
            if(guardSpawner.AliveGuardCount == 0)
            {
                for (int i = 0; i < activeFantomes.Count; i++)
                {
                    if (activeFantomes[i] != null)
                        activeFantomes[i].ReleaseAndFlyAway();
                }
                yield return new WaitForSeconds(4f);
            }


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

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private IEnumerator TransitionToScore()
        {
            yield return new WaitForSeconds(2f);

            if (BuildState.CurrentState == BuildState.State.normal)
                SceneManager.LoadScene(Freethespirit_GeneralVariables.Instance?.scoreScene ?? "Score_Freethespirit");
            else if (MenuSelectionButton.Instance != null)
                MenuSelectionButton.Instance.gameObject.SetActive(true);
        }

        public void TriggerImpactFeel(Vector3 pos, int hitCount)
        {
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

        private void UpdateUI()
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";

            if (timerText != null)
                timerText.text = Mathf.CeilToInt(sceneTimer).ToString();

            if (sceneText != null)
                sceneText.text = $"Niveau {currentLevel}";

            if (globalTimerText != null && useGlobalTimer)
            {
                int mins = Mathf.FloorToInt(globalTimer / 60);
                int secs = Mathf.FloorToInt(globalTimer % 60);
                globalTimerText.text = $"{mins}:{secs:D2}";
            }
        }
    }
}
