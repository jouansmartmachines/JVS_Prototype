using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Demolition
{
    /// <summary>
    /// Spawn les gardes sur les points de spawn définis dans l'ancre (Demolition_ObstacleAnchor).
    /// Gère la quantité minimale de gardes, leur espacement et leur réapparition automatique.
    /// </summary>
    public class Demolition_GuardSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        public GameObject guardPrefab;

        [Header("Paramètres de Spawn")]
        [Tooltip("Nombre de gardes à spawner en même temps.")]
        public int maxGuardsToSpawn = 2;

        [Tooltip("Écart de distance le long de l'axe rouge (Right) entre chaque garde s'ils partagent le même point.")]
        public float spawnSpacing = 2.5f;

        [Tooltip("Délai en secondes avant de faire respawn un nouveau garde après la mort d'un autre.")]
        public float respawnDelay = 2f;

        private Demolition_ObstacleAnchor currentAnchor;
        private List<Demolition_Guard> activeGuards = new List<Demolition_Guard>();

        public void SpawnGuards(Demolition_ObstacleAnchor anchor)
        {
            currentAnchor = anchor;
            if (currentAnchor == null) return;

            if (guardPrefab == null)
            {
                guardPrefab = Resources.Load<GameObject>("Prefabs/Garde");
                if (guardPrefab == null)
                {
                    Debug.LogWarning("Demolition_GuardSpawner: prefab Garde introuvable.");
                    return;
                }
            }

            Transform[] spawnPoints = GetSpawnPoints(currentAnchor);

            // Nettoie la liste active par sécurité
            activeGuards.Clear();

            // On boucle pour spawner le nombre exact demandé
            for (int i = 0; i < maxGuardsToSpawn; i++)
            {
                Transform targetSpawnPoint = spawnPoints[i % spawnPoints.Length];
                if (targetSpawnPoint == null) continue;

                // CORRECTION : Utilisation de l'axe rouge local (targetSpawnPoint.right) pour l'alignement
                Vector3 spawnPos = targetSpawnPoint.position + (targetSpawnPoint.right * (i - (maxGuardsToSpawn / 2f)) * spawnSpacing);

                SpawnSingleGuard(spawnPos, targetSpawnPoint, i);
            }
        }

        private Transform[] GetSpawnPoints(Demolition_ObstacleAnchor anchor)
        {
            if (anchor.obstaclePrefabs != null)
            {
                int validCount = 0;
                for (int j = 0; j < anchor.obstaclePrefabs.Length; j++)
                {
                    if (anchor.obstaclePrefabs[j] != null)
                        validCount++;
                }

                if (validCount > 0)
                {
                    Transform[] validPoints = new Transform[validCount];
                    int index = 0;
                    for (int j = 0; j < anchor.obstaclePrefabs.Length; j++)
                    {
                        if (anchor.obstaclePrefabs[j] != null)
                        {
                            validPoints[index] = anchor.obstaclePrefabs[j].transform;
                            index++;
                        }
                    }
                    return validPoints;
                }
            }

            return new Transform[] { anchor.transform };
        }

        private void SpawnSingleGuard(Vector3 position, Transform parentPoint, int index)
        {
            var fantome = FindObjectOfType<Demolition_Fantome>();

            GameObject guardGO = Instantiate(guardPrefab, position, Quaternion.identity, parentPoint);
            
            var guard = guardGO.GetComponent<Demolition_Guard>();
            if (guard != null)
            {
                Transform centerTransform = fantome != null ? fantome.transform : currentAnchor.transform;
                guard.Initialize(centerTransform, index * 120f);

                guard.OnGuardDestroyed += HandleGuardDeath;
                activeGuards.Add(guard);
            }

            var btn = guardGO.GetComponent<Universal_Button>();
            if (btn != null && guard != null)
            {
                btn.Event.AddListener(guard.OnTouched);
            }
        }

        private void HandleGuardDeath(Demolition_Guard deadGuard)
        {
            if (deadGuard != null)
            {
                deadGuard.OnGuardDestroyed -= HandleGuardDeath;
                activeGuards.Remove(deadGuard);
            }

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);

            // Vérifie que l'ancre existe toujours et qu'on est en dessous du nombre max de gardes
            if (currentAnchor != null && activeGuards.Count < maxGuardsToSpawn)
            {
                Transform[] spawnPoints = GetSpawnPoints(currentAnchor);
                if (spawnPoints.Length > 0)
                {
                    Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                    
                    // Respecte également l'axe rouge pour le décalage de respawn
                    Vector3 respawnPos = randomSpawnPoint.position + (randomSpawnPoint.right * Random.Range(-2f, 2f));
                    
                    SpawnSingleGuard(respawnPos, randomSpawnPoint, Random.Range(0, 360));
                }
            }
        }
    }
}