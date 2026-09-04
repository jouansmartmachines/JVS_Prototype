using UnityEngine;
using System.Collections.Generic;

namespace Demolition
{
    public class Demolition_ObstacleSpawner : MonoBehaviour
    {
        [Header("Prefabs de structures (Angry Birds)")]
        [Tooltip("Glisse ici tes prefabs de structures complètes.")]
        public GameObject[] structurePrefabs;

        [Header("Prefab Fantôme")]
        [Tooltip("Glisse ici ton prefab de fantôme (cochon) à spawner.")]
        public GameObject fantomePrefab;

        [System.Serializable]
        public class DifficultyLevel
        {
            public string name = "Niveau 1";
            [Tooltip("Nombre de structures à spawner pour ce niveau.")]
            public int numberOfStructures = 2;
            [Tooltip("Nombre de fantômes à spawner pour ce niveau.")]
            public int numberOfFantomes = 1;
        }

        [Header("Niveaux de difficulté")]
        public DifficultyLevel[] difficultyLevels = new DifficultyLevel[3];

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

            Demolition_ObstacleAnchor[] anchors = FindObjectsOfType<Demolition_ObstacleAnchor>();
            if (anchors.Length == 0) return;

            List<Demolition_ObstacleAnchor> availableAnchors = new List<Demolition_ObstacleAnchor>(anchors);
            ShuffleList(availableAnchors);

            int anchorIndex = 0;

            // 1. SPAWN DES STRUCTURES
            anchorIndex = SpawnBatch(structurePrefabs, config.numberOfStructures, availableAnchors, anchorIndex, true);

            // 2. SPAWN DES FANTÔMES
            SpawnBatch(new GameObject[] { fantomePrefab }, config.numberOfFantomes, availableAnchors, anchorIndex, false);
        }

        private int SpawnBatch(GameObject[] prefabs, int count, List<Demolition_ObstacleAnchor> anchors, int startIndex, bool isStructure)
        {
            if (prefabs.Length == 0) return startIndex;

            int toSpawn = Mathf.Min(count, anchors.Count - startIndex);
            for (int i = 0; i < toSpawn; i++)
            {
                var anchor = anchors[startIndex++];
                SpawnObjectOnAnchor(anchor, prefabs[Random.Range(0, prefabs.Length)], isStructure);
            }
            return startIndex;
        }

        private void SpawnObjectOnAnchor(Demolition_ObstacleAnchor anchor, GameObject prefabToSpawn, bool isStructure)
        {
            Transform targetParent = anchor.transform;
            Vector3 spawnPosition = anchor.transform.position;

            if (anchor.obstaclePrefabs.Length > 0)
            {
                GameObject parentObj = anchor.obstaclePrefabs[Random.Range(0, anchor.obstaclePrefabs.Length)];
                if (parentObj.scene.IsValid())
                {
                    targetParent = parentObj.transform;
                    spawnPosition = targetParent.position;
                }
            }

            GameObject spawnedObj = Instantiate(prefabToSpawn, spawnPosition, anchor.transform.rotation, targetParent);

            if (isStructure)
            {
                foreach (var pushable in spawnedObj.GetComponentsInChildren<Demolition_Pushable>())
                {
                    var btn = pushable.GetComponent<Universal_Button>();
                    if (btn) btn.Event.AddListener(pushable.OnPushed);
                }
            }
            else
            {
                if (!spawnedObj.GetComponent<Demolition_Fantome>())
                    spawnedObj.AddComponent<Demolition_Fantome>();
            }
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = Random.Range(i, list.Count);
                T temp = list[i];
                list[i] = list[randomIndex];
                list[randomIndex] = temp;
            }
        }
    }
}