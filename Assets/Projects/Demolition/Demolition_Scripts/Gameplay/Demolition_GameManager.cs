using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using MenuSelection;
using OSC;

namespace Demolition
{
    public enum LaunchMode { Oiseau, ImpactSimple }

    public class Demolition_GameManager : ReceiveParent
    {
        public static Demolition_GameManager Instance { get; private set; }

        [Header("Modes de lancement")]
        public LaunchMode launchMode = LaunchMode.Oiseau;
        public GameObject oiseauPrefab;
        public GameObject impactEffectPrefab;

        [Header("Structure")]
        public Transform structuresParent;

        [Header("Défilement")]
        public float baseScrollSpeed = 0.3f;
        public float currentScrollSpeed;
        private bool isScrolling = true;

        [Header("Gameplay")]
        public float gameDuration = 60f;
        public float currentTime;
        public int score { get; private set; }
        private bool gameIsRunning = false;

        [Header("Audio")]
        public AudioClip impactSound;
        public AudioClip destructionSound;
        public AudioClip gameOverSound;
        private AudioSource audioSource;

        [Header("UI")]
        private TextMeshProUGUI scoreText;
        private TextMeshProUGUI timerText;
        private TextMeshProUGUI starText;
        public GameObject popupTextPrefab;

        [Header("Étoiles")]
        public int currentStars = 0;

        // Coordonnees d impact
        protected bool gotAPt;
        protected Vector3 newPt;
        protected int w, h;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        float groundY = -2f; // decalage pour poser les structures sur le sol (y=-5 + taille 2, top a y=-4, StructuresParent a y=-2)

        void Start()
        {
            w = Screen.width;
            h = Screen.height;
            audioSource = GetComponent<AudioSource>();

            // Charger les references depuis Resources
            LoadReferences();

            // Mode depuis PlayerPrefs
            launchMode = PlayerPrefs.GetInt(Demolition_GeneralVariables.ModeOiseauKey, 1) == 1
                ? LaunchMode.Oiseau : LaunchMode.ImpactSimple;

            // Difficulté
            SetupDifficulty();

            currentTime = gameDuration;
            score = 0;

            // Spawn du premier tableau SUR le sol
            SpawnTableau(new Vector3(0, groundY, 0));

            // Enregistrement OSC
            OSC_Manager.Instance.receiveP = this;

            gameIsRunning = true;
            StartCoroutine(GameLoop());

            // Trouver les textes UI dans le Canvas
            var canvasGO = GameObject.Find("Canvas");
            if (canvasGO != null)
            {
                scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
                timerText = GameObject.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
                starText = GameObject.Find("StarText")?.GetComponent<TextMeshProUGUI>();
                if (scoreText != null) scoreText.text = "Score: 0";
                if (timerText != null) timerText.text = Mathf.CeilToInt(gameDuration).ToString();
                if (starText != null) starText.text = "★";
            }

            // Assigner popupTextPrefab aux blocs
            foreach (var block in FindObjectsByType<Demolition_Block>(FindObjectsSortMode.None))
            {
                if (block.popupTextPrefab == null)
                    block.popupTextPrefab = popupTextPrefab;
            }
        }

        void LoadReferences()
        {
            oiseauPrefab = Resources.Load<GameObject>("Prefabs/Oiseau");
            impactEffectPrefab = Resources.Load<GameObject>("Prefabs/ImpactExplosion");
            impactSound = Resources.Load<AudioClip>("Sounds/impact");
            destructionSound = Resources.Load<AudioClip>("Sounds/destruction");
            gameOverSound = Resources.Load<AudioClip>("Sounds/gameover");

            popupTextPrefab = Resources.Load<GameObject>("Prefabs/PopupText");

            // Chercher/creer le sol (pleine largeur + defilement)
            if (GameObject.Find("Ground") == null)
            {
                var groundGO = new GameObject("Ground", typeof(BoxCollider2D), typeof(SpriteRenderer));
                var col = groundGO.GetComponent<BoxCollider2D>();
                col.size = new Vector2(200, 2);
                col.offset = new Vector2(0, 0);
                groundGO.transform.position = new Vector3(0, -5f, 0);

                // Texture sol en tiling
                var sr = groundGO.GetComponent<SpriteRenderer>();
                var solTex = Resources.Load<Sprite>("Textures/sol");
                if (solTex != null) sr.sprite = solTex;
                else
                {
                    var tex = new Texture2D(128, 32);
                    for (int x = 0; x < 128; x++)
                        for (int y = 0; y < 32; y++)
                            tex.SetPixel(x, y, new Color(0.4f, 0.3f, 0.2f));
                    tex.Apply();
                    sr.sprite = Sprite.Create(tex, new Rect(0, 0, 128, 32), new Vector2(0.5f, 0.5f));
                }
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(200, 2);
                sr.sortingOrder = 1;

                // Ajouter un script pour faire defiler le sol avec le scroll
                var groundScroll = groundGO.AddComponent<Demolition_GroundScroll>();
                groundScroll.scrollSpeedRef = () => currentScrollSpeed;
            }
        }

        void Update()
        {
            if (!gameIsRunning) return;

            // Défilement
            if (isScrolling)
            {
                Vector3 pos = structuresParent.position;
                pos.x -= currentScrollSpeed * Time.deltaTime;
                structuresParent.position = pos;
            }

            // Timer
            currentTime -= Time.deltaTime;
            if (timerText != null)
                timerText.text = Mathf.CeilToInt(currentTime).ToString();
            if (currentTime <= 0)
            {
                EndGame();
            }
        }

        public override void ReceivePoint(float xPoint, float yPoint)
        {
            if (!gameIsRunning) return;

            newPt.x = xPoint * w;
            newPt.y = yPoint * h;
            gotAPt = true;

            // Lancer l'action au point d'impact
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(
                new Vector3(newPt.x, newPt.y, -Camera.main.transform.position.z));
            worldPos.z = 0;

            LaunchAt(worldPos);
        }

        void LaunchAt(Vector3 worldPos)
        {
            // Hitstop (appelé avant)
            StartCoroutine(HitstopCoroutine());

            // Screen shake
            StartCoroutine(ScreenShake());

            // Son d'impact
            if (impactSound != null)
                audioSource.PlayOneShot(impactSound);

            switch (launchMode)
            {
                case LaunchMode.Oiseau:
                    // L'oiseau apparaît au point d'impact, de dos, et rétrécit en s'éloignant
                    LaunchOiseau(worldPos);
                    break;
                case LaunchMode.ImpactSimple:
                    // Simple explosion visuelle au point d'impact
                    LaunchImpact(worldPos);
                    break;
            }
        }

        void LaunchOiseau(Vector3 worldPos)
        {
            if (oiseauPrefab == null) return;

            GameObject oiseau = Instantiate(oiseauPrefab, worldPos, Quaternion.identity);
            Demolition_Projectile proj = oiseau.GetComponent<Demolition_Projectile>();
            if (proj != null)
                proj.Launch(worldPos);
        }

        void LaunchImpact(Vector3 worldPos)
        {
            if (impactEffectPrefab != null)
            {
                GameObject effect = Instantiate(impactEffectPrefab, worldPos, Quaternion.identity);
                // Charger le sprite impact depuis Resources (Texture2D -> Sprite)
                var sr = effect.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite == null)
                {
                    Texture2D tex = Resources.Load<Texture2D>("Textures/impact");
                    if (tex != null)
                        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
                Destroy(effect, 2f);
            }

            // Appliquer une force à la structure la plus proche
            ApplyForceToNearestStructure(worldPos, 500f);
        }

        void ApplyForceToNearestStructure(Vector3 point, float radius)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(point, radius);
            foreach (var hit in hits)
            {
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 direction = (hit.transform.position - (Vector3)point).normalized;
                    rb.AddForceAtPosition(direction * 200f, point, ForceMode2D.Impulse);
                }
            }
        }

        IEnumerator HitstopCoroutine()
        {
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(0.05f);
            Time.timeScale = 1f;
        }

        IEnumerator ScreenShake()
        {
            Vector3 originalPos = Camera.main.transform.position;
            for (int i = 0; i < 5; i++)
            {
                Camera.main.transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.3f;
                yield return new WaitForSeconds(0.02f);
            }
            Camera.main.transform.position = originalPos;
        }

        public IEnumerator BigShake()
        {
            Vector3 originalPos = Camera.main.transform.position;
            for (int i = 0; i < 10; i++)
            {
                Camera.main.transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.6f;
                yield return new WaitForSeconds(0.03f);
            }
            Camera.main.transform.position = originalPos;
        }

        public IEnumerator CollapseSlowMo()
        {
            Time.timeScale = 0.3f;
            yield return new WaitForSecondsRealtime(0.4f);
            Time.timeScale = 1f;
        }

        void SpawnTableau(Vector3 origin)
        {
            Demolition_StructureBuilder.BuildRandomStructure(structuresParent, origin);
        }

        void SetupDifficulty()
        {
            string diff = PlayerPrefs.GetString(Demolition_GeneralVariables.GameTimeKey, "Normal");

            switch (diff)
            {
                case "Easy":
                    gameDuration = 90f;
                    baseScrollSpeed = 0.2f;
                    break;
                case "Normal":
                    gameDuration = 60f;
                    baseScrollSpeed = 0.3f;
                    break;
                case "Hard":
                    gameDuration = 45f;
                    baseScrollSpeed = 0.6f;
                    break;
            }

            currentScrollSpeed = baseScrollSpeed;
        }

        IEnumerator GameLoop()
        {
            // Timer avant chargement d'un nouveau tableau
            float nextTableauTime = 15f;
            float elapsed = 0f;

            while (gameIsRunning)
            {
                elapsed += Time.deltaTime;
                if (elapsed >= nextTableauTime)
                {
                    float rightEdge = Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x;
                    SpawnTableau(new Vector3(rightEdge + 5f, groundY, 0));
                    elapsed = 0f;
                    nextTableauTime = Random.Range(12f, 20f);
                }
                yield return null;
            }
        }

        public void AddScore(int points, Vector3 pos)
        {
            score += points;
            if (scoreText != null)
                scoreText.text = "Score: " + score;
        }

        void EndGame()
        {
            gameIsRunning = false;
            StopAllCoroutines();
            Time.timeScale = 1f;

            // Calcul des étoiles (1-3)
            currentStars = 1;
            int highScore = PlayerPrefs.GetInt(Demolition_GeneralVariables.HighScoreKey, 0);
            if (score >= 150 && score < 500)
                currentStars = 2;
            else if (score >= 500)
                currentStars = 3;

            // Afficher les étoiles
            if (starText != null)
            {
                string stars = "";
                for (int i = 0; i < currentStars; i++) stars += "★";
                for (int i = currentStars; i < 3; i++) stars += "☆";
                starText.text = stars;
            }

            // Ne pas écraser un meilleur score
            if (score > highScore)
            {
                PlayerPrefs.SetInt(Demolition_GeneralVariables.HighScoreKey, score);
                PlayerPrefs.SetInt("Demolition_Stars", currentStars);
            }

            if (gameOverSound != null)
                audioSource.PlayOneShot(gameOverSound);

            PlayerPrefs.SetInt("Demolition_FinalScore", score);

            StartCoroutine(TransitionToScore());
        }

        IEnumerator TransitionToScore()
        {
            yield return new WaitForSeconds(2f);

            if (BuildState.CurrentState == BuildState.State.normal)
                SceneManager.LoadScene(Demolition_GeneralVariables.Instance.scoreScene);
            else
                MenuSelectionButton.Instance.gameObject.SetActive(true);
        }
    }
}