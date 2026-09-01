using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using OSC;
using MenuSelection;

namespace Sparks
{
    /// <summary>
    /// Gestionnaire principal de Sparks.
    /// Spawn des primitives depuis le volcan, boucle de jeu, score, timer.
    /// N'est PAS un ReceiveParent — les primitives gèrent leur propre clic via Universal_Button.
    /// </summary>
    public class Sparks_GameManager : MonoBehaviour
    {
        public static Sparks_GameManager Instance { get; private set; }

        [Header("Volcan")]
        public Transform volcanoOrigin;
        public float spawnInterval = 1.2f;
        public float spawnIntervalRapide = 0.6f;
        public float forceMin = 6f;
        public float forceMax = 12f;

        [Header("Préfabs des Primitives")]
        public GameObject spherePrefab;
        public GameObject cubePrefab;
        public GameObject capsulePrefab;

        [Header("Gameplay & Durée")]
        public float gameDuration = 60f;
        public float currentTime;
        public int score { get; private set; }
        private bool gameIsRunning = false;
        private bool modeRapide = false;

        [Header("Audio")]
        public AudioClip clickSound;
        public AudioClip gameOverSound;
        private AudioSource audioSource;

        [Header("UI")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI timerText;

        [Header("Particules")]
        public GameObject clickEffectPrefab;

        // Anti-flood
        private float lastClickTime = -1f;
        public float minTimeBetweenClicks = 0.08f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        void Start()
        {
            // Auto-découverte des éléments
            if (volcanoOrigin == null)
            {
                var go = GameObject.Find("VolcanoOrigin");
                if (go != null) volcanoOrigin = go.transform;
            }

            LoadResourcesReferences();
            BindSceneElements();
            SetupPreferences();

            currentTime = gameDuration;
            score = 0;

            // Enregistrer dans OSC_Manager si besoin
            if (OSC_Manager.Instance != null)
            {
                // Pas besoin de receiveP — les primitives ont leur propre Universal_Button
            }

            gameIsRunning = true;
            StartCoroutine(SpawnLoop());

            UpdateUI();
        }

        private void LoadResourcesReferences()
        {
            if (spherePrefab == null)
                spherePrefab = Resources.Load<GameObject>("Prefabs/Primitive_Sphere");
            if (cubePrefab == null)
                cubePrefab = Resources.Load<GameObject>("Prefabs/Primitive_Cube");
            if (capsulePrefab == null)
                capsulePrefab = Resources.Load<GameObject>("Prefabs/Primitive_Capsule");

            if (clickSound == null)
                clickSound = Resources.Load<AudioClip>("Sounds/click");
            if (gameOverSound == null)
                gameOverSound = Resources.Load<AudioClip>("Sounds/gameover");

            if (clickEffectPrefab == null)
                clickEffectPrefab = Resources.Load<GameObject>("Prefabs/ClickEffect");
        }

        private void BindSceneElements()
        {
            if (scoreText == null)
                scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            if (timerText == null)
                timerText = GameObject.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        }

        private void SetupPreferences()
        {
            modeRapide = PlayerPrefs.GetInt(Sparks_GeneralVariables.ModeRapideKey, 0) == 1;

            string timeSetting = PlayerPrefs.GetString(Sparks_GeneralVariables.GameTimeKey, "Normal");
            switch (timeSetting)
            {
                case "Easy":   gameDuration = 90f; break;
                case "Normal": gameDuration = 60f; break;
                case "Hard":   gameDuration = 45f; break;
                default:       gameDuration = 60f; break;
            }
        }

        void Update()
        {
            if (!gameIsRunning) return;

            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0;
                EndGame();
            }

            UpdateUI();
        }

        private IEnumerator SpawnLoop()
        {
            while (gameIsRunning)
            {
                float interval = modeRapide ? spawnIntervalRapide : spawnInterval;
                yield return new WaitForSeconds(interval);

                if (!gameIsRunning) yield break;
                SpawnPrimitive();
            }
        }

        private void SpawnPrimitive()
        {
            if (volcanoOrigin == null) return;

            // Choisir un type aléatoire
            int typeIndex = Random.Range(0, 3);
            GameObject prefab = null;
            int pts = 10;

            switch (typeIndex)
            {
                case 0: prefab = spherePrefab;  pts = 10; break;
                case 1: prefab = cubePrefab;    pts = 20; break;
                case 2: prefab = capsulePrefab; pts = 30; break;
            }

            if (prefab == null) return;

            Vector3 origin = volcanoOrigin.position + Random.insideUnitSphere * 0.3f;
            origin.y = volcanoOrigin.position.y;

            GameObject go = Instantiate(prefab, origin, Random.rotation);
            var prim = go.GetComponent<Sparks_Primitive>();
            if (prim != null)
            {
                prim.points = pts;
                prim.Launch(origin, forceMin, forceMax);
            }
        }

        public void AddScore(int points, Vector3 worldPos)
        {
            if (!gameIsRunning) return;

            // Anti-flood
            if (Time.unscaledTime - lastClickTime < minTimeBetweenClicks)
                return;
            lastClickTime = Time.unscaledTime;

            score += points;

            if (clickSound != null)
                audioSource.PlayOneShot(clickSound, 0.6f);
        }

        public void PlayClickEffect(Vector3 worldPos, Sparks_Primitive.PrimitiveType type)
        {
            if (clickEffectPrefab != null)
            {
                GameObject effect = Instantiate(clickEffectPrefab, worldPos, Quaternion.identity);
                Destroy(effect, 0.5f);
            }
        }

        private void UpdateUI()
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score}";

            if (timerText != null)
                timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }

        private void EndGame()
        {
            gameIsRunning = false;
            StopAllCoroutines();

            if (gameOverSound != null)
                audioSource.PlayOneShot(gameOverSound, 1f);

            // Sauvegarder le score (pattern Dobble : SetFloat direct)
            int finalScore = score;
            int highScore = PlayerPrefs.GetInt(Sparks_GeneralVariables.HighScoreKey, 0);
            if (finalScore > highScore)
            {
                PlayerPrefs.SetInt(Sparks_GeneralVariables.HighScoreKey, finalScore);
            }
            PlayerPrefs.SetInt("Sparks_FinalScore", finalScore);
            PlayerPrefs.Save();

            StartCoroutine(TransitionToScore());
        }

        private IEnumerator TransitionToScore()
        {
            yield return new WaitForSeconds(2.0f);

            if (Sparks_GeneralVariables.Instance != null && !string.IsNullOrEmpty(Sparks_GeneralVariables.Instance.scoreScene))
            {
                if (BuildState.CurrentState == BuildState.State.normal)
                {
                    SceneManager.LoadScene(Sparks_GeneralVariables.Instance.scoreScene);
                }
                else if (MenuSelectionButton.Instance != null)
                {
                    MenuSelectionButton.Instance.gameObject.SetActive(true);
                }
            }
            else
            {
                SceneManager.LoadScene("Score_Sparks");
            }
        }
    }
}