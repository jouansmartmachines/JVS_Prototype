using UnityEngine;
using System.Collections.Generic;

namespace Demolition
{
    /// <summary>
    /// Configure les obstacles à spawner + les 3 niveaux de difficulté.
    /// Tous les prefabs sont référencés ICI, pas dans les ObstacleAnchors.
    /// Les ObstacleAnchors ne sont que des positions (Transforms) dans la scène.
    /// </summary>
    public class Demolition_ObstacleSpawner : MonoBehaviour
    {
        [Header("Prefabs obstacles")]
        public GameObject caissePrefab;
        public GameObject barilPrefab;
        public GameObject fantomePrefab;

        [System.Serializable]
        public class DifficultyLevel
        {
            public string name = "Niveau 1";
            public GameObject[] availablePrefabs;
            public int minCount = 1;
            public int maxCount = 3;
            public bool includeFantome = false;
        }

        [Header("3 niveaux de difficulté")]
        public DifficultyLevel[] difficultyLevels = new DifficultyLevel[3];

        [Header("Anti-overlap")]
        public float minDistanceBetweenObstacles = 0.8f;

        [Header("Parent dans la hiérarchie")]
        public Transform obstaclesParent;

        private int currentDifficulty = 0;
        public int CurrentDifficulty
        {
            get => currentDifficulty;
            set => currentDifficulty = Mathf.Clamp(value, 0, difficultyLevels.Length - 1);
        }

        void Start()
        {
            SpawnForDifficulty(currentDifficulty);
        }

        public void SpawnForDifficulty(int level)
        {
            CurrentDifficulty = level;
            DifficultyLevel config = difficultyLevels[currentDifficulty];
            if (config == null) return;

            Demolition_ObstacleAnchor[] anchors = FindObjectsOfType<Demolition_ObstacleAnchor>();
            if (anchors.Length == 0)
            {
                Debug.LogWarning("Demolition_ObstacleSpawner: aucun ObstacleAnchor trouvé dans la scène.");
                return;
            }

            List<Vector3> usedPositions = new List<Vector3>();

            foreach (var anchor in anchors)
            {
                if (anchor.isFantomeAnchor)
                {
                    if (config.includeFantome && fantomePrefab != null)
                        SpawnFantome(anchor);
                    continue;
                }

                SpawnObstaclesInZone(anchor, config, usedPositions);
            }
        }

        private void SpawnObstaclesInZone(Demolition_ObstacleAnchor anchor, DifficultyLevel config, List<Vector3> usedPositions)
        {
            if (config.availablePrefabs == null || config.availablePrefabs.Length == 0)
            {
                FillDefaultPrefabs(config);
                if (config.availablePrefabs.Length == 0) return;
            }

            int count = Random.Range(config.minCount, config.maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = GetRandomPositionInRadius(anchor.transform.position, anchor.spawnRadius, usedPositions);
                if (pos == Vector3.zero) continue;

                GameObject prefab = config.availablePrefabs[Random.Range(0, config.availablePrefabs.Length)];
                if (prefab == null) continue;

                GameObject obj = Instantiate(prefab, pos, Random.rotation, obstaclesParent);

                SetupInteractable(obj);
                usedPositions.Add(pos);
            }
        }

        private void SpawnFantome(Demolition_ObstacleAnchor anchor)
        {
            GameObject fantome = Instantiate(fantomePrefab, anchor.transform.position, Quaternion.identity, obstaclesParent);

            if (fantome.GetComponent<Demolition_Fantome>() == null)
                fantome.AddComponent<Demolition_Fantome>();
        }

        private void FillDefaultPrefabs(DifficultyLevel config)
        {
            var defaults = new List<GameObject>();
            if (caissePrefab != null) defaults.Add(caissePrefab);
            if (barilPrefab != null) defaults.Add(barilPrefab);
            if (defaults.Count == 0) return;
            config.availablePrefabs = defaults.ToArray();
        }

        private Vector3 GetRandomPositionInRadius(Vector3 center, float radius, List<Vector3> used)
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                Vector2 circle = Random.insideUnitCircle * radius;
                Vector3 candidate = new Vector3(center.x + circle.x, center.y + circle.y, center.z);

                bool overlap = false;
                foreach (var usedPos in used)
                {
                    if (Vector3.Distance(candidate, usedPos) < minDistanceBetweenObstacles)
                    {
                        overlap = true;
                        break;
                    }
                }

                if (!overlap && Physics.CheckSphere(candidate, minDistanceBetweenObstacles * 0.4f) == false)
                    return candidate;
            }

            return Vector3.zero;
        }

        private void SetupInteractable(GameObject obj)
        {
            var btn = obj.GetComponent<Universal_Button>();
            if (btn == null) return;

            var pushable = obj.GetComponent<Demolition_Pushable>();
            if (pushable == null)
            {
                Debug.LogWarning($"Demolition_ObstacleSpawner: {obj.name} a Universal_Button mais pas Demolition_Pushable.");
                return;
            }

            btn.Event.AddListener(pushable.OnPushed);
        }
    }
}