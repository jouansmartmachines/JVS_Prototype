using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using MenuSelection;
using OSC;

namespace Demolition
{
    public enum LaunchMode { Oiseau, ImpactSimple }

    /// <summary>
    /// Gestionnaire principal du jeu Démolition : boucle de jeu, réceptions OSC et souris avec protection anti-flood,
    /// gestion de la caméra (Shake doux, Hitstop léger), audio punchy, UI et score.
    /// Les éléments de scène (Background, Ground, UI) sont placés hors Play via l'Editor Tool.
    /// </summary>
    public class Demolition_GameManager : ReceiveParent
    {
        public static Demolition_GameManager Instance { get; private set; }

        [Header("Modes de Tir")]
        public LaunchMode launchMode = LaunchMode.Oiseau;
        public GameObject oiseauPrefab;
        public GameObject impactEffectPrefab;
        public GameObject popupTextPrefab;

        [Header("Structures & Sol")]
        public Transform structuresParent;
        public float groundY = -4.0f;

        [Header("Défilement")]
        public float baseScrollSpeed = 0.5f;
        public float currentScrollSpeed;
        private bool isScrolling = true;

        [Header("Gameplay & Durée")]
        public float gameDuration = 60f;
        public float currentTime;
        public int score { get; private set; }
        private bool gameIsRunning = false;

        [Header("Cadence de Tir & Anti-Flood")]
        public float minTimeBetweenShots = 0.12f;
        private float lastShotTime = -1f;

        [Header("Audio Clips")]
        public AudioClip impactSound;
        public AudioClip destructionSound;
        public AudioClip gameOverSound;
        public AudioClip pigHitSound;
        private AudioSource audioSource;

        [Header("Interface Utilisateur (UI)")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI timerText;
        public TextMeshProUGUI starText;

        [Header("Étoiles")]
        public int currentStars = 1;

        // Effets de caméra & Juice doux
        private Vector3 originalCameraPos;
        private float shakeIntensity = 0f;
        private float shakeDecay = 5.0f;
        private Coroutine hitstopCoroutine;

        // Combo system
        private float comboTimer = 0f;
        private int comboMultiplier = 1;

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
            audioSource.spatialBlend = 0f;
        }

        void Start()
        {
            if (Camera.main != null)
            {
                originalCameraPos = Camera.main.transform.position;
            }

            LoadResourcesReferences();
            BindSceneElements();
            SetupPreferencesAndDifficulty();

            currentTime = gameDuration;
            score = 0;
            comboMultiplier = 1;
            comboTimer = 0f;

            if (OSC_Manager.Instance != null)
            {
                OSC_Manager.Instance.receiveP = this;
            }

            SpawnStructure(new Vector3(2f, groundY, 0));

            gameIsRunning = true;
            StartCoroutine(StructureSpawnLoop());

            UpdateUI();
        }

        private void LoadResourcesReferences()
        {
            if (oiseauPrefab == null)
                oiseauPrefab = Resources.Load<GameObject>("Prefabs/Oiseau");
            if (impactEffectPrefab == null)
                impactEffectPrefab = Resources.Load<GameObject>("Prefabs/ImpactExplosion");
            if (popupTextPrefab == null)
                popupTextPrefab = Resources.Load<GameObject>("Prefabs/PopupText");

            if (impactSound == null)
                impactSound = Resources.Load<AudioClip>("Sounds/impact");
            if (destructionSound == null)
                destructionSound = Resources.Load<AudioClip>("Sounds/destruction");
            if (gameOverSound == null)
                gameOverSound = Resources.Load<AudioClip>("Sounds/gameover");
            if (pigHitSound == null)
                pigHitSound = Resources.Load<AudioClip>("Sounds/pig_hit");
        }

        private void BindSceneElements()
        {
            if (structuresParent == null)
            {
                var existingParent = GameObject.Find("StructuresParent");
                if (existingParent != null)
                {
                    structuresParent = existingParent.transform;
                }
            }

            if (scoreText == null)
                scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            if (timerText == null)
                timerText = GameObject.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
            if (starText == null)
                starText = GameObject.Find("StarText")?.GetComponent<TextMeshProUGUI>();
        }

        private void SetupPreferencesAndDifficulty()
        {
            launchMode = PlayerPrefs.GetInt(Demolition_GeneralVariables.ModeOiseauKey, 1) == 1
                ? LaunchMode.Oiseau : LaunchMode.ImpactSimple;

            string diff = PlayerPrefs.GetString(Demolition_GeneralVariables.GameTimeKey, "Normal");
            switch (diff)
            {
                case "Easy":
                    gameDuration = 90f;
                    baseScrollSpeed = 0.35f;
                    break;
                case "Normal":
                case "Medium":
                    gameDuration = 60f;
                    baseScrollSpeed = 0.5f;
                    break;
                case "Hard":
                    gameDuration = 45f;
                    baseScrollSpeed = 0.75f;
                    break;
                default:
                    gameDuration = 60f;
                    baseScrollSpeed = 0.5f;
                    break;
            }

            currentScrollSpeed = baseScrollSpeed;
        }

        void Update()
        {
            if (!gameIsRunning) return;

            if (isScrolling && structuresParent != null)
            {
                Vector3 pos = structuresParent.position;
                pos.x -= currentScrollSpeed * Time.deltaTime;
                structuresParent.position = pos;
            }

            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0;
                EndGame();
            }

            if (comboMultiplier > 1)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0)
                {
                    comboMultiplier = 1;
                }
            }

            UpdateCameraShake();
            HandlePlayerInput();
            UpdateUI();
        }

        private void HandlePlayerInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 mouseScreen = Input.mousePosition;
                if (Camera.main != null)
                {
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, -Camera.main.transform.position.z));
                    worldPos.z = 0;
                    FireAt(worldPos);
                }
            }
        }

        public override void ReceivePoint(float xPoint, float yPoint)
        {
            if (!gameIsRunning) return;

            if (Camera.main != null)
            {
                float screenX = xPoint * Screen.width;
                float screenY = yPoint * Screen.height;
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenX, screenY, -Camera.main.transform.position.z));
                worldPos.z = 0;
                FireAt(worldPos);
            }
        }

        public void FireAt(Vector3 worldPos)
        {
            if (!gameIsRunning) return;

            if (Time.unscaledTime - lastShotTime < minTimeBetweenShots)
            {
                return;
            }
            lastShotTime = Time.unscaledTime;

            if (launchMode == LaunchMode.Oiseau)
            {
                if (oiseauPrefab != null)
                {
                    GameObject oiseau = Instantiate(oiseauPrefab);
                    var proj = oiseau.GetComponent<Demolition_Projectile>();
                    if (proj != null)
                    {
                        proj.Launch(worldPos);
                    }
                }
            }
            else
            {
                TriggerDirectImpact(worldPos);
            }
        }

        private void TriggerDirectImpact(Vector3 worldPos)
        {
            TriggerImpactFeel(worldPos, 1);

            if (impactEffectPrefab != null)
            {
                GameObject effect = Instantiate(impactEffectPrefab, worldPos, Quaternion.identity);
                Destroy(effect, 0.25f);
            }

            Collider2D directHit = Physics2D.OverlapPoint(worldPos);
            if (directHit == null)
            {
                directHit = Physics2D.OverlapCircle(worldPos, 0.4f);
            }

            if (directHit != null)
            {
                var rb = directHit.GetComponent<Rigidbody2D>();
                if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
                {
                    Vector2 dir = ((Vector2)directHit.transform.position - (Vector2)worldPos);
                    if (dir.sqrMagnitude < 0.01f) dir = Vector2.up;
                    else dir.Normalize();

                    rb.AddForceAtPosition(dir * 2.5f, worldPos, ForceMode2D.Impulse);
                }

                var blk = directHit.GetComponent<Demolition_Block>();
                if (blk != null)
                {
                    blk.TakeDamage(1, Vector2.up);
                }
            }
        }

        public void TriggerImpactFeel(Vector3 worldPos, int hitCount)
        {
            if (hitstopCoroutine != null) StopCoroutine(hitstopCoroutine);
            hitstopCoroutine = StartCoroutine(HitstopCoroutine(0.025f));

            AddCameraShake(Mathf.Clamp(0.08f + hitCount * 0.03f, 0.08f, 0.25f));

            if (impactSound != null)
            {
                PlaySfx(impactSound, Random.Range(0.95f, 1.1f), 0.65f);
            }
        }

        public void TriggerPigDestroyed(int starVal)
        {
            AddCameraShake(0.35f);
            StartCoroutine(CollapseSlowMo());
            currentStars = Mathf.Clamp(Mathf.Max(currentStars, starVal), 1, 3);
        }

        public void AddScore(int points, Vector3 pos, Color? customColor = null, float scaleMultiplier = 1f, string prefix = "")
        {
            comboTimer = 1.2f;
            comboMultiplier = Mathf.Min(comboMultiplier + 1, 5);

            int finalPoints = points * comboMultiplier;
            score += finalPoints;

            if (popupTextPrefab != null)
            {
                GameObject popup = Instantiate(popupTextPrefab, pos + Vector3.up * 0.3f, Quaternion.identity);
                var popupScript = popup.GetComponent<Demolition_PopupText>();
                if (popupScript != null)
                {
                    string label = comboMultiplier > 1 ? $"{prefix}+{finalPoints} (x{comboMultiplier})!" : $"{prefix}+{finalPoints}";
                    popupScript.SetText(label, customColor, scaleMultiplier);
                }
            }
        }

        public void PlaySfx(AudioClip clip, float pitch = 1f, float volume = 1f)
        {
            if (clip == null || audioSource == null) return;

            audioSource.pitch = pitch;
            audioSource.PlayOneShot(clip, volume);
        }

        public void AddCameraShake(float intensity)
        {
            shakeIntensity = Mathf.Max(shakeIntensity, intensity);
        }

        private void UpdateCameraShake()
        {
            if (Camera.main == null) return;

            if (shakeIntensity > 0.005f)
            {
                Vector2 offset = Random.insideUnitCircle * shakeIntensity;
                Camera.main.transform.position = originalCameraPos + new Vector3(offset.x, offset.y, 0);
                shakeIntensity = Mathf.MoveTowards(shakeIntensity, 0f, shakeDecay * Time.unscaledDeltaTime);
            }
            else
            {
                Camera.main.transform.position = originalCameraPos;
                shakeIntensity = 0f;
            }
        }

        private IEnumerator HitstopCoroutine(float duration)
        {
            Time.timeScale = 0.1f;
            yield return new WaitForSecondsRealtime(duration);
            Time.timeScale = 1f;
        }

        public IEnumerator CollapseSlowMo()
        {
            Time.timeScale = 0.5f;
            yield return new WaitForSecondsRealtime(0.2f);
            Time.timeScale = 1f;
        }

        public IEnumerator BigShake()
        {
            AddCameraShake(0.35f);
            yield return null;
        }

        private void SpawnStructure(Vector3 position)
        {
            if (structuresParent != null)
            {
                Demolition_StructureBuilder.BuildRandomStructure(structuresParent, position);
            }
        }

        private IEnumerator StructureSpawnLoop()
        {
            float nextSpawnInterval = 7.0f;
            float elapsed = 0f;

            while (gameIsRunning)
            {
                elapsed += Time.deltaTime;
                if (elapsed >= nextSpawnInterval)
                {
                    elapsed = 0f;
                    if (Camera.main != null && structuresParent != null)
                    {
                        float rightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, -Camera.main.transform.position.z)).x;
                        SpawnStructure(new Vector3(rightEdge + 4f, groundY, 0));
                    }
                    nextSpawnInterval = Random.Range(7.0f, 10.0f);
                }
                yield return null;
            }
        }

        private void UpdateUI()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }

            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(currentTime).ToString();
            }

            if (starText != null)
            {
                string stars = "";
                for (int i = 0; i < currentStars; i++) stars += "★";
                for (int i = currentStars; i < 3; i++) stars += "☆";
                starText.text = stars;
            }
        }

        private void EndGame()
        {
            gameIsRunning = false;
            StopAllCoroutines();
            Time.timeScale = 1f;

            if (gameOverSound != null)
            {
                PlaySfx(gameOverSound, 1f, 1f);
            }

            int highScore = PlayerPrefs.GetInt(Demolition_GeneralVariables.HighScoreKey, 0);
            if (score > highScore)
            {
                PlayerPrefs.SetInt(Demolition_GeneralVariables.HighScoreKey, score);
            }
            PlayerPrefs.SetInt("Demolition_FinalScore", score);
            PlayerPrefs.SetInt("Demolition_Stars", currentStars);

            StartCoroutine(TransitionToScore());
        }

        private IEnumerator TransitionToScore()
        {
            yield return new WaitForSeconds(2.0f);

            if (Demolition_GeneralVariables.Instance != null && !string.IsNullOrEmpty(Demolition_GeneralVariables.Instance.scoreScene))
            {
                if (BuildState.CurrentState == BuildState.State.normal)
                {
                    SceneManager.LoadScene(Demolition_GeneralVariables.Instance.scoreScene);
                }
                else if (MenuSelectionButton.Instance != null)
                {
                    MenuSelectionButton.Instance.gameObject.SetActive(true);
                }
            }
            else
            {
                SceneManager.LoadScene("Score_Demolition");
            }
        }
    }
}
