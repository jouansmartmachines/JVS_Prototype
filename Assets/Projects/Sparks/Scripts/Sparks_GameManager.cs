using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using MenuSelection;

namespace Sparks
{
    /// <summary>
    /// Gestionnaire principal de Sparks — boucle de jeu, spawn des primitives, score, timer.
    /// ReceiveParent pour recevoir les touches OSC (cas 3D : tir libre → spawn projectile/impact).
    /// </summary>
    public class Sparks_GameManager : ReceiveParent
    {
        public static Sparks_GameManager i { get; private set; }

        [Header("Volcan")]
        public Transform volcanoOrigin;
        public float spawnInterval = 1.2f;
        public float spawnIntervalRapide = 0.6f;
        public float forceMin = 6f;
        public float forceMax = 12f;

        [Header("Primitives")]
        public GameObject spherePrefab;
        public GameObject cubePrefab;
        public GameObject capsulePrefab;

        [Header("Gameplay")]
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

        // Anti-flood
        private float lastShotTime = -1f;
        public float minTimeBetweenShots = 0.12f;

        private void Awake()
        {
            if (i == null)
                i = this;
            else
                { Destroy(gameObject); return; }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        void Start()
        {
            // Auto-découverte
            if (volcanoOrigin == null)
            {
                var go = GameObject.Find("VolcanoOrigin");
                if (go != null) volcanoOrigin = go.transform;
            }

            LoadResources();
            BindSceneElements();
            SetupPreferences();

            currentTime = gameDuration;
            score = 0;

            gameIsRunning = true;
            StartCoroutine(SpawnLoop());
            UpdateUI();
        }

        private void LoadResources()
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
            gameDuration = Sparks_GeneralVariable.GetGameDurationFromPrefs();
            modeRapide = Sparks_GeneralVariable.GetModeRapideFromPrefs();
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

        // ── OSC / Toucher mur (ReceiveParent) ──
        public override void ReceivePoint(float xPoint, float yPoint)
        {
            if (!gameIsRunning) return;

            // Anti-flood
            if (Time.unscaledTime - lastShotTime < minTimeBetweenShots)
                return;
            lastShotTime = Time.unscaledTime;

            // Convertir le point normalisé en position monde
            if (Camera.main != null)
            {
                float screenX = xPoint * Screen.width;
                float screenY = yPoint * Screen.height;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenX, screenY, -Camera.main.transform.position.z));

                // Vérifier si on a cliqué sur une primitive (Physics.Raycast 3D)
                Ray ray = Camera.main.ScreenPointToRay(new Vector3(screenX, screenY));
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    var prim = hit.collider.GetComponent<Sparks_Primitive>();
                    if (prim != null && prim.IsAlive)
                    {
                        prim.OnClicked();
                        return;
                    }
                }

                // Si pas de clic sur primitive = tir dans le vide (effet visuel léger)
                // Pour l'instant, rien — les primitives se gèrent via leur Universal_Button
            }
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

            int typeIndex = Random.Range(0, 3);
            GameObject prefab = typeIndex switch
            {
                0 => spherePrefab,
                1 => cubePrefab,
                2 => capsulePrefab,
                _ => spherePrefab,
            };

            int pts = typeIndex switch
            {
                0 => 10,
                1 => 20,
                2 => 30,
                _ => 10,
            };

            if (prefab == null) return;

            Vector3 origin = volcanoOrigin.position + Random.insideUnitSphere * 0.3f;
            origin.y = volcanoOrigin.position.y;

            GameObject go = Instantiate(prefab, origin, Random.rotation);
            var prim = go.GetComponent<Sparks_Primitive>();
            if (prim != null)
            {
                prim.Init(pts, forceMin, forceMax);
            }
        }

        public void AddScore(int points)
        {
            if (!gameIsRunning) return;
            score += points;
            if (clickSound != null)
                audioSource.PlayOneShot(clickSound, 0.6f);
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

            // Pattern Dobble : SetFloat direct
            PlayerPrefs.SetFloat(Sparks_GeneralVariable.HighScoreKey, score);
            PlayerPrefs.Save();

            StartCoroutine(TransitionToScore());
        }

        private IEnumerator TransitionToScore()
        {
            yield return new WaitForSeconds(2.0f);

            if (Sparks_GeneralVariable.i != null && !string.IsNullOrEmpty(Sparks_GeneralVariable.i.scoreScene))
            {
                if (BuildState.CurrentState == BuildState.State.normal)
                    SceneManager.LoadScene(Sparks_GeneralVariable.i.scoreScene);
                else if (MenuSelectionButton.Instance != null)
                    MenuSelectionButton.Instance.gameObject.SetActive(true);
            }
            else
            {
                SceneManager.LoadScene("Score_Sparks");
            }
        }
    }
}