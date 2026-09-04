using UnityEngine;
using System.Collections.Generic;

namespace Demolition
{
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

        [Header("Parent dans la hiérarchie")]
        public Transform obstaclesParent;

        private int currentDifficulty = 0;
        public int CurrentDifficulty
        {
            get => currentDifficulty;
            set => currentDifficulty = Mathf.Clamp(value, 0, difficultyLevels.Length - 1);
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

            foreach (var anchor in anchors)
            {
                if (anchor.isFantomeAnchor)
                {
                    if (config.includeFantome && fantomePrefab != null)
                        SpawnFantome(anchor);
                    continue;
                }

                SpawnObstaclesInZone(anchor, config);
            }
        }

        private void SpawnObstaclesInZone(Demolition_ObstacleAnchor anchor, DifficultyLevel config)
        {
            GameObject[] prefabsToSpawn = config.availablePrefabs;

            if (prefabsToSpawn == null || prefabsToSpawn.Length == 0)
            {
                FillDefaultPrefabs(config);
                prefabsToSpawn = config.availablePrefabs;
                if (prefabsToSpawn.Length == 0) return;
            }

            int count = Random.Range(config.minCount, config.maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                GameObject prefab = prefabsToSpawn[Random.Range(0, prefabsToSpawn.Length)];
                if (prefab == null) continue;

                Transform targetParent = obstaclesParent != null ? obstaclesParent : anchor.transform;
                Vector3 spawnPosition = anchor.transform.position;

                if (anchor.obstaclePrefabs != null && anchor.obstaclePrefabs.Length > 0)
                {
                    GameObject parentObj = anchor.obstaclePrefabs[Random.Range(0, anchor.obstaclePrefabs.Length)];
                    if (parentObj != null && parentObj.scene.IsValid())
                    {
                        targetParent = parentObj.transform;
                        spawnPosition = targetParent.position;
                    }
                }

                GameObject obj = Instantiate(prefab, spawnPosition, Random.rotation, targetParent);

                SetupInteractable(obj);
            }
        }

        private void SpawnFantome(Demolition_ObstacleAnchor anchor)
        {
            Vector3 spawnPos = obstaclesParent != null ? obstaclesParent.position : anchor.transform.position;
            Transform parentT = obstaclesParent != null ? obstaclesParent : anchor.transform;

            GameObject fantome = Instantiate(fantomePrefab, spawnPos, Quaternion.identity, parentT);

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