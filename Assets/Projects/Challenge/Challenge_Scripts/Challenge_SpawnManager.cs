using System.Collections.Generic;
using UnityEngine;

namespace Challenge
{
    public class Challenge_SpawnManager : MonoBehaviour
    {
        [Header("References")]
        public GameObject targetPrefab;
        public RectTransform spawnZone;
        public Challenge_GridManager gridManager;
        public Challenge_ScoreManager scoreManager;
        public Challenge_LevelManager levelManager;
        public Challenge_MaterialManager materialManager;

        [Header("Parents")]
        public GameObject popupParent;
        public GameObject particleParent;

        [Header("Spawn Settings")]
        public int maxActiveTargets = 10;
        private int activeTargets = 0;

        [Header("Recipes System")]
        [SerializeField] GameObject challenge_RecipeContainer;
        private RecipeCategory[] globalRecipes;


        // ✅ FIX : levelSettings n'a plus de setter complexe
        // On rafraîchit uniquement quand le niveau change réellement
        private Challenge_LevelSettings _levelSettings;
        public Challenge_LevelSettings LevelSettings => _levelSettings;
        private Vector2 lastpos;

        private void Start()
        {
            var containerScript = challenge_RecipeContainer.GetComponentInChildren<Challenge_RecipeContainer>();
            globalRecipes = containerScript.globalRecipes;

            GameObject tempInstance = Instantiate(targetPrefab, spawnZone);
            Transform child = tempInstance.transform.GetChild(0);
            child.SetParent(spawnZone);
            targetPrefab = child.gameObject;
            Destroy(tempInstance);


            RefreshLevelSettings();
            gridManager.InitializeGrid((targetPrefab.transform as RectTransform), _levelSettings.targetScale);

            for (int i = 0; i < 4; i++)
                TrySpawnTarget();
        }

        // ✅ FIX : La mise à jour de levelSettings est explicite et centralisée
        private void RefreshLevelSettings()
        {
            var newSettings = Challenge_LevelManager.CurrentLevelSettings;
            if (_levelSettings == newSettings) return;

            _levelSettings = newSettings;
            gridManager.ClearLastMode();
        }

        private void TrySpawnTarget()
        {
            if (activeTargets >= maxActiveTargets) return;

            // ✅ On rafraîchit seulement si nécessaire, de façon explicite
            RefreshLevelSettings();

            Vector2 spawnPos = gridManager.GetPositionForLevel(Vector2.zero, _levelSettings,lastpos);
            lastpos = spawnPos;
            SpawnTarget(spawnPos);

        }

        // ✅ FIX : Plus de paramètre qui shadow le champ — on utilise directement _levelSettings
        private void SpawnTarget(Vector2 pos)
        {
            GameObject go = Instantiate(targetPrefab, spawnZone);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            Vector3 newPos = rt.position;
            newPos.z = 0f;
            rt.position = newPos;
            rt.localScale = Vector3.one * _levelSettings.targetScale;

            var baseTarget = go.GetComponent<Challenge_Target>();
            baseTarget.SetStage(1);
            baseTarget.Pos = pos;

            // 1. Déterminer la catégorie
            TargetCategoryValue categoryValue = GetRandomTargetCategory(_levelSettings);
            baseTarget.category = categoryValue;



            // 4. Gestion du Grid Release
            activeTargets++;
            var gridDecorator = go.AddComponent<Challenge_GridReleaseDecorator>();
            gridDecorator.Initialize(gridManager, rt);

            // ✅ FIX : on stocke le handler pour pouvoir se désinscrire
            void OnReleased(Vector2 _)
            {
                activeTargets--;
                gridDecorator.OnCellReleased -= OnReleased;
                TrySpawnTarget();
            }
            gridDecorator.OnCellReleased += OnReleased;

            foreach (var category in globalRecipes)
            {
                if (category == null || category.recipes == null) continue;

                foreach (var recipe in category.recipes)
                {
                    if (recipe != null)
                    {
                        recipe.Execute(go, baseTarget, baseTarget.category.category, this, rt);
                    }
                }
            }
        
        }

        private TargetCategoryValue GetRandomTargetCategory(Challenge_LevelSettings settings)
        {
            var categories = settings.targetCategories;
            if (categories == null || categories.Length == 0) return default;

            float totalChance = 0f;
            foreach (var cat in categories)
                totalChance += Mathf.Max(0f, cat.spawnChance);

            if (totalChance <= 0f)
                return categories[Random.Range(0, categories.Length)];

            float roll = Random.Range(0f, totalChance);
            float cumulative = 0f;
            foreach (var cat in categories)
            {
                cumulative += Mathf.Max(0f, cat.spawnChance);
                if (roll <= cumulative) return cat;
            }

            return categories[categories.Length - 1];
        }
    }
}

