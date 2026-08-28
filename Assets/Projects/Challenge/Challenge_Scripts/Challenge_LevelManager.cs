using UnityEngine;
using TMPro;
using System.Collections;
using System;
using UnityEngine.UI;

namespace Challenge
{
    public class Challenge_LevelManager : MonoBehaviour
    {
        private Challenge_LevelSettings[] levels;

        [Header("References")]
        public Challenge_ScoreManager scoreManager;

        [Header("UI")]
        public TMP_Text levelText;
        public TMP_Text levelText2;
        public TMP_Text popupTMP;

        public GameObject ball;
        public GameObject ballParent;

        [Header("Level Popup")]
        public GameObject levelPopupPrefab; 
        public Transform popupParent;
        public float popupDuration = 1.5f; // Augmenté pour laisser le temps de lire
        public float fadeDuration = 0.5f;
        public Image imgPopup; 

        [Header("Popup Messages")]
        [SerializeField] private GameObject popupContainer;
        private LevelPopupInfo[] popupMessages;

        public static Challenge_LevelSettings CurrentLevelSettings { get; private set; }
        public int CurrentLevelIndex { get; private set; }
        private int cumulativeScoreToReach = 0;
        public event Action<Challenge_LevelSettings> OnLevelChanged;

        [Header("Audio")]
        private AudioSource levelLoopSource;

        [Header("Levels")]
        public GameObject levelContainerObject;

        private void Start()
        {
            // Initialisation des données
            var containerScript = popupContainer.transform.GetChild(0).GetComponent<Challenge_LevelPopupContainer>();
            popupMessages = containerScript.popupMessages;

            var containerLevels = levelContainerObject.transform.GetChild(0).GetComponent<Challenge_LevelsContainer>();
            levels = containerLevels.levels; 

            // Lancement du son de début de partie
            Challenge_AudioManager.i.PlayOneShot(SoundType.Debut);

            // INITIALISATION DU NIVEAU 0 (Démarrage immédiat)
            SetLevel(0);

            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged += HandleScoreChanged;
                scoreManager.OnTimeOver += HandleTimeOver;
            }
        }

        private void HandleScoreChanged(int newScore)
        {
            if (CurrentLevelIndex >= levels.Length - 1) return;

            if (newScore >= cumulativeScoreToReach)
            {
                SetLevel(CurrentLevelIndex + 1);
            }
        }

        private void SetLevel(int index)
        {
            CurrentLevelIndex = index;
            CurrentLevelSettings = levels[index];

            // Calcul du score cumulé
            cumulativeScoreToReach += CurrentLevelSettings.scoreToReach;

            // Mise à jour de l'UI Text
            if (levelText != null) levelText.text = $"{CurrentLevelSettings.level}";
            if (levelText2 != null) levelText2.text = $"{CurrentLevelSettings.level}";

            // Lancement du popup (incluant le niveau 0)
            ShowLevelPopup(CurrentLevelIndex);
            
            // Événement et Musique
            OnLevelChanged?.Invoke(CurrentLevelSettings);
            PlayLevelLoop(index);
        }

        private void ShowLevelPopup(int levelIndex)
        {
            // Sécurité index
            if (popupMessages == null || levelIndex >= popupMessages.Length) return;

            // 1. Audio
            Challenge_AudioManager.i.PlayOneShot(SoundType.Demarrage);

            // 2. Texte du popup
            string newText = Localizer.Get(popupMessages[levelIndex].title);
            popupTMP.text = newText;

            // 3. Couleur de l'image de fond du popup (Saturée)
            Color baseColor = CurrentLevelSettings.color;
            Color.RGBToHSV(baseColor, out float h, out float s, out float v);
            s = Mathf.Clamp01(s * 1.2f);
            imgPopup.color = Color.HSVToRGB(h, s, v);

            // 4. Instanciation de la balle visuelle
            GameObject ballGO = Instantiate(ball, ballParent.transform); 
            ballGO.GetComponent<Image>().color = CurrentLevelSettings.color;

            // 5. Mise à jour de l'icône de niveau dans le prefab
            Image[] images = levelPopupPrefab.GetComponentsInChildren<Image>(true);
            foreach (Image img in images)
            {
                // On cherche l'image qui n'est pas le fond lui-même
                if (img.gameObject != levelPopupPrefab)
                {
                    img.sprite = CurrentLevelSettings.levelImage;
                    TextMeshProUGUI levelText = img.gameObject.GetComponentInChildren<TextMeshProUGUI>(true);            
                    if (levelText != null)
                    {
                        levelText.text =  $"{Localizer.Get("Level")} {CurrentLevelSettings.level}"; 
                    }
                    break;
                }
            }



            // 6. Création de l'objet de popup et animation
            GameObject popupGO = Instantiate(levelPopupPrefab, popupParent);
            popupGO.SetActive(true);

            StartCoroutine(FadePopupWithoutCanvasGroup(popupGO));
        }

        private static readonly int OutlineAlphaID = Shader.PropertyToID("_OutlineAlpha");

        private IEnumerator FadePopupWithoutCanvasGroup(GameObject popupGO)
        {
            float visibleDuration = popupDuration - (fadeDuration * 2f);
            if (visibleDuration < 0) visibleDuration = 0.1f;

            Graphic[] graphics = popupGO.GetComponentsInChildren<Graphic>(true);
            Renderer[] renderers = popupGO.GetComponentsInChildren<Renderer>(true);

            int graphicCount = graphics.Length;
            Color[] originalColors = new Color[graphicCount];

            for (int i = 0; i < graphicCount; i++)
                originalColors[i] = graphics[i].color;

            // Gestion des matériaux avec Outline
            Material[] outlineMats = new Material[renderers.Length];
            float[] outlineOriginal = new float[renderers.Length];
            int outlineCount = 0;

            for (int i = 0; i < renderers.Length; i++)
            {
                Material mat = renderers[i].material;
                if (mat.HasProperty(OutlineAlphaID))
                {
                    outlineMats[outlineCount] = mat;
                    outlineOriginal[outlineCount] = mat.GetFloat(OutlineAlphaID);
                    outlineCount++;
                }
            }

            // ---------- FADE IN ----------
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, t / fadeDuration);
                ApplyAlpha(graphics, originalColors, outlineMats, outlineOriginal, outlineCount, p);
                yield return null;
            }

            yield return new WaitForSeconds(visibleDuration);

            // ---------- FADE OUT ----------
            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float p = Mathf.SmoothStep(1f, 0f, t / fadeDuration);
                ApplyAlpha(graphics, originalColors, outlineMats, outlineOriginal, outlineCount, p);
                yield return null;
            }

            Destroy(popupGO);
        }

        private void ApplyAlpha(Graphic[] graphics, Color[] originals, Material[] mats, float[] matOriginals, int matCount, float alpha)
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                Color c = originals[i];
                c.a *= alpha;
                graphics[i].color = c;
            }

            for (int i = 0; i < matCount; i++)
                mats[i].SetFloat(OutlineAlphaID, matOriginals[i] * alpha);
        }

        private void PlayLevelLoop(int levelIndex)
        {
            if (levelLoopSource != null)
            {
                levelLoopSource.Stop();
                Destroy(levelLoopSource);
            }
            
            // Map Niveau 0 -> Son 1, Niveau 1 -> Son 2, etc.
            int soundIndex = Mathf.Clamp(levelIndex + 1, 1, 4);

            levelLoopSource = Challenge_AudioManager.i.CreateSource(
                gameObject,
                SoundType.Level,
                soundIndex
            );

            levelLoopSource.Play();
        }

        private void HandleTimeOver()
        {
            if (levelLoopSource != null) levelLoopSource.Stop();
        }

        private void OnDestroy()
        {
            if (levelLoopSource != null) levelLoopSource.Stop();

            if (scoreManager != null)
            {
                scoreManager.OnScoreChanged -= HandleScoreChanged;
                scoreManager.OnTimeOver -= HandleTimeOver;
            }
        }
    }
}