using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using MenuSelection;

namespace Demolition
{
    public enum LaunchMode { Oiseau, ImpactSimple }

    public class Demolition_GameManager : ReceiveParent
    {
        public static Demolition_GameManager Instance { get; private set; }

        [Header("Modes de lancement")]
        public LaunchMode launchMode = LaunchMode.Oiseau;
        public GameObject oiseauPrefab { get; private set; }
        public GameObject impactEffectPrefab { get; private set; }

        [Header("Structure")]
        public Transform structuresParent;
        public GameObject[] tableauPrefabs { get; private set; }
        public string[] tableauNames = { "Tableau_1", "Tableau_2", "Tableau_3" };

        [Header("Défilement")]
        public float baseScrollSpeed = 2f;
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

        // Coordonnees d impact
        protected bool gotAPt;
        protected Vector3 newPt;
        protected int w, h;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

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

            // Spawn du premier tableau
            SpawnTableau(Vector3.zero);

            // Enregistrement OSC
            OSC_Manager.Instance.receiveP = this;

            gameIsRunning = true;
            StartCoroutine(GameLoop());
        }

        void LoadReferences()
        {
            oiseauPrefab = Resources.Load<GameObject>("Prefabs/Oiseau");
            impactEffectPrefab = Resources.Load<GameObject>("Prefabs/ImpactExplosion");
            impactSound = Resources.Load<AudioClip>("Sounds/impact");
            destructionSound = Resources.Load<AudioClip>("Sounds/destruction");
            gameOverSound = Resources.Load<AudioClip>("Sounds/gameover");

            tableauPrefabs = new GameObject[tableauNames.Length];
            for (int i = 0; i < tableauNames.Length; i++)
                tableauPrefabs[i] = Resources.Load<GameObject>("Prefabs/" + tableauNames[i]);
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
                proj.Launch(structuresParent, currentScrollSpeed);
        }

        void LaunchImpact(Vector3 worldPos)
        {
            if (impactEffectPrefab != null)
            {
                GameObject effect = Instantiate(impactEffectPrefab, worldPos, Quaternion.identity);
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

        void SpawnTableau(Vector3 origin)
        {
            if (tableauPrefabs.Length == 0) return;

            GameObject prefab = tableauPrefabs[Random.Range(0, tableauPrefabs.Length)];
            GameObject tableau = Instantiate(prefab, structuresParent);
            tableau.transform.localPosition = origin;
        }

        void SetupDifficulty()
        {
            string diff = PlayerPrefs.GetString(Demolition_GeneralVariables.DifficultyKey, "Normal");

            switch (diff)
            {
                case "Easy":
                    gameDuration = 90f;
                    baseScrollSpeed = 0.008f;
                    break;
                case "Normal":
                    gameDuration = 60f;
                    baseScrollSpeed = 0.01f;
                    break;
                case "Hard":
                    gameDuration = 45f;
                    baseScrollSpeed = 0.02f;
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
                    SpawnTableau(new Vector3(rightEdge + 5f, 0, 0));
                    elapsed = 0f;
                    nextTableauTime = Random.Range(12f, 20f);
                }
                yield return null;
            }
        }

        public void AddScore(int points, Vector3 pos)
        {
            score += points;
        }

        void EndGame()
        {
            gameIsRunning = false;
            StopAllCoroutines();
            Time.timeScale = 1f;

            if (gameOverSound != null)
                audioSource.PlayOneShot(gameOverSound);

            PlayerPrefs.SetInt("Demolition_FinalScore", score);
            PlayerPrefs.SetFloat(Demolition_GeneralVariables.HighScoreKey, score);

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