using UnityEngine;
using System.Collections.Generic;

namespace Demolition
{
    public class Demolition_ObstacleSpawner : MonoBehaviour
    {
        [System.Serializable]
        public struct BlockConfig
        {
            public Vector2Int size;
            public GameObject[] prefab;
        }

        [Header("Configuration de la Grille")]
        public int gridWidth = 6;
        public int gridHeight = 7;
        public float cellSize = 1f;

        [Header("Subdivision Interne (Précision x2)")]
        [Range(1, 4)] public int gridSubdivision = 2;

        [Header("Ressources")]
        public List<BlockConfig> availableBlocks = new List<BlockConfig>();
        public GameObject fantomePrefab;

        private int currentDifficulty = 1;
        public int CurrentDifficulty
        {
            get => currentDifficulty;
            set => currentDifficulty = Mathf.Max(1, value);
        }



        public void SpawnForDifficulty(int level,Demolition_ObstacleAnchor anchor )
        {
            CurrentDifficulty = level;
            SpawnWithGrid(CurrentDifficulty, anchor);
        }

        private void SpawnWithGrid(int level,Demolition_ObstacleAnchor anchor )
        {

            Debug.Log($"[ObstacleSpawner] Spawning obstacles for level {level} at anchor {anchor.name}");


            // Toujours utiliser la liste complète des formes disponibles (poutres, piliers, caisses...)
            var blocksToUse = (availableBlocks != null && availableBlocks.Count > 0)
                ? availableBlocks
                : GetDefaultBlockConfigs();

            // Nettoyage préalable sous toutes les ancres et leurs points de spawn (obstaclePrefabs)

            CleanOldObstacles(anchor.transform);
            if (anchor.obstaclePrefabs != null)
            {
                foreach (var spawnPoint in anchor.obstaclePrefabs)
                {
                    if (spawnPoint != null)
                        CleanOldObstacles(spawnPoint.transform);
                }
            }
            

            // Récupération des points de spawn cibles (priorité aux transforms référencés dans obstaclePrefabs)
            var spawnTargets = new List<Transform>();

            if (anchor.obstaclePrefabs != null && anchor.obstaclePrefabs.Length > 0)
            {
                foreach (var p in anchor.obstaclePrefabs)
                {
                    if (p != null)
                    {
                        spawnTargets.Add(p.transform);
                    }
                }
            }
            else
            {
                spawnTargets.Add(anchor.transform);
            }
            

            if (spawnTargets.Count == 0)
            {
                Debug.LogWarning("[ObstacleSpawner] Aucun point de spawn valide trouvé.");
                return;
            }

            // Nombre de points de spawn à utiliser selon le niveau
            int targetsToUse = 1;
            if (level >= 5) targetsToUse = spawnTargets.Count;
            else if (level >= 3) targetsToUse = Mathf.Min(2, spawnTargets.Count);

            for (int i = 0; i < targetsToUse; i++)
            {
                Transform targetParent = spawnTargets[i];
                Debug.Log("[ObstacleSpawner] Spawning obstacles for level " + level + " at " + targetParent.name);

                Demolition_GridGenerator.GenerateProceduralStructure(
                    targetParent,
                    blocksToUse,
                    fantomePrefab,
                    gridWidth,
                    gridHeight,
                    cellSize,
                    gridSubdivision,
                    level + i
                );
            }
        }

        private void CleanOldObstacles(Transform parent)
        {
            if (parent == null) return;

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                var child = parent.GetChild(i);
                if (child.GetComponent<Demolition_Pushable>() != null || 
                    child.GetComponent<Demolition_Fantome>() != null || 
                    child.name.Contains("(Clone)"))
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private List<BlockConfig> GetDefaultBlockConfigs()
        {
            return new List<BlockConfig>();
        }
    }
}
