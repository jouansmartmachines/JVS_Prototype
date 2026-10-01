using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Demolition
{
    /// <summary>
    /// Spawn les gardes sur les points de spawn définis dans l'ancre (Demolition_ObstacleAnchor).
    /// Lit le nombre de soldats (guardCount), la formation, la rotation et l'échelle (spawnScale) 
    /// directement depuis l'ObstacleAnchor de la scène/zone.
    /// </summary>
    public class Demolition_GuardSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        public GameObject guardPrefab;

        private Demolition_ObstacleAnchor currentAnchor;
        private List<Demolition_Guard> activeGuards = new List<Demolition_Guard>();
        public int AliveGuardCount => activeGuards.Count;
        private Material currentMaterial; // Sauvegarde du matériau

        private Coroutine patrolLoopCoroutine;

        private void Start()
        {
            StartPatrolRotation();
        }

        public void StartPatrolRotation()
        {
            if (patrolLoopCoroutine != null)
                StopCoroutine(patrolLoopCoroutine);

            patrolLoopCoroutine = StartCoroutine(PatrolLoopRoutine());
        }

        private IEnumerator PatrolLoopRoutine()
        {
            while (true)
            {
                // 1. Attente initiale (10 à 20s)
                float initialWait = Random.Range(10f, 20f);
                yield return new WaitForSeconds(initialWait);

                // 2. Recherche des gardes éligibles
                List<Demolition_Guard> availableGuards = activeGuards.FindAll(g => 
                    g != null && 
                    g.currentState != GuardState.Dead 

                );

                if (availableGuards.Count == 0) 
                    continue;

                // 3. Sélection et lancement de la patrouille
                Demolition_Guard selectedGuard = availableGuards[Random.Range(0, availableGuards.Count)];
                selectedGuard.guardMode = GuardMode.Patrol;
                selectedGuard.startPatrol = true;

                // 4. Durée de la patrouille (10 à 20s)
                float patrolDuration = Random.Range(10f, 20f);
                yield return new WaitForSeconds(patrolDuration);

                // 5. Remise en statique
                if (selectedGuard != null && 
                    selectedGuard.currentState != GuardState.Dead )
                {
                    selectedGuard.startPatrol = false;
                    selectedGuard.guardMode = GuardMode.Static;

                }

                // 6. Pause courte de 0 à 15s avant de relancer le cycle complet
                float cooldown = Random.Range(0f, 15f);
                yield return new WaitForSeconds(cooldown);
            }
        }

        /// <summary>
        /// Instancie les gardes en leur appliquant un matériau spécifique.
        /// </summary>
        /// <param name="anchor">L'ancre de spawn.</param>
        /// <param name="guardMaterial">Le matériau à appliquer sur le MeshRenderer du garde.</param>
        public void SpawnGuards(Demolition_ObstacleAnchor anchor, Material guardMaterial)
        {
            currentAnchor = anchor;
            currentMaterial = guardMaterial;

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

            // Nettoie la liste active
            activeGuards.Clear();

            int countToSpawn = Mathf.Max(0, currentAnchor.guardCount);

            // On boucle pour spawner le nombre exact de soldats configuré dans l'ancre
            for (int i = 0; i < countToSpawn; i++)
            {
                Transform targetSpawnPoint = spawnPoints[i % spawnPoints.Length];
                if (targetSpawnPoint == null) continue;

                Vector2 offset = currentAnchor.GetFormationOffset(i, countToSpawn);
                Vector3 spawnPos = targetSpawnPoint.position 
                    + (targetSpawnPoint.right * offset.x) 
                    + (targetSpawnPoint.forward * offset.y);

                SpawnSingleGuard(spawnPos, targetSpawnPoint, i, currentMaterial);
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

        private void HandleGuardDeath(Demolition_Guard deadGuard)
        {
            if (deadGuard != null)
            {
                deadGuard.OnGuardDestroyed -= HandleGuardDeath;
                activeGuards.Remove(deadGuard);
            }

            // Si tous les gardes sont morts, on prévient le GameManager
            if (activeGuards.Count == 0)
            {
                Demolition_GameManager.Instance?.TriggerWinOrReload(); // Appelle ta méthode de rechargement
            }
        }

        private void SpawnSingleGuard(Vector3 position, Transform parentPoint, int index, Material mat)
        {
            var fantome = Object.FindFirstObjectByType<Demolition_Fantome>();

            // Instanciation du garde sans parent immédiat pour préserver le scale
            GameObject guardGO = Instantiate(guardPrefab, position, parentPoint.rotation);
            guardGO.transform.SetParent(parentPoint, true);

            // Application du matériau sur le MeshRenderer (ou SkinnedMeshRenderer)
            Transform bodyTransform = guardGO.transform.Find("Body");

            if (bodyTransform != null)
            {
                var renderer = bodyTransform.GetComponent<Renderer>();
                if (renderer != null && mat != null)
                    renderer.material = mat;
            }

            // Application du facteur d'échelle (Scale)
            if (currentAnchor != null)
            {
                Vector3 defaultScale = guardPrefab.transform.localScale;
                guardGO.transform.localScale = Vector3.Scale(defaultScale, currentAnchor.spawnScale);
            }

            var guard = guardGO.GetComponent<Demolition_Guard>();
            if (guard != null)
            {
                // Force le démarrage en statique/Idle
                guard.startPatrol = false;

                Transform centerTransform = fantome != null ? fantome.transform : currentAnchor.transform;
                guard.Initialize(centerTransform, index * 120f, currentAnchor.patrolWaypoints);

                guard.OnGuardDestroyed += HandleGuardDeath;
                activeGuards.Add(guard);
            }

            var btn = guardGO.GetComponent<Universal_Button>();
            if (btn != null && guard != null)
            {
                btn.Event.AddListener(guard.OnTouched);
            }
        }
    }
}