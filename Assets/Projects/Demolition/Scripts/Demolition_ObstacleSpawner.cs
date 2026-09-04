using UnityEngine;
using System.Collections.Generic;

namespace Demolition
{
    /// <summary>
    /// Au démarrage de la scène, lit tous les ObstacleAnchor, spawn les obstacles
    /// aléatoirement dans leur radius, ajoute Universal_Button + Demolition_Pushable
    /// et bind l'Event automatiquement.
    /// </summary>
    public class Demolition_ObstacleSpawner : MonoBehaviour
    {
        [Header("Anti-overlap")]
        public float minDistanceBetweenObstacles = 0.8f;

        void Start()
        {
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
                    SpawnFantome(anchor);
                    continue;
                }

                SpawnObstaclesInZone(anchor);
            }
        }

        private void SpawnObstaclesInZone(Demolition_ObstacleAnchor anchor)
        {
            if (anchor.obstaclePrefabs == null || anchor.obstaclePrefabs.Length == 0) return;

            int count = Random.Range(anchor.minCount, anchor.maxCount + 1);
            List<Vector3> usedPositions = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = GetRandomPositionInRadius(anchor.transform.position, anchor.spawnRadius, usedPositions);
                if (pos == Vector3.zero) continue;

                GameObject prefab = anchor.obstaclePrefabs[Random.Range(0, anchor.obstaclePrefabs.Length)];
                if (prefab == null) continue;

                GameObject obj = Instantiate(prefab, pos, Random.rotation);
                obj.transform.SetParent(anchor.transform);

                SetupInteractable(obj);
                usedPositions.Add(pos);
            }
        }

        private void SpawnFantome(Demolition_ObstacleAnchor anchor)
        {
            if (anchor.fantomePrefab == null)
            {
                Debug.LogError("Demolition_ObstacleSpawner: anchor fantome sans fantomePrefab !");
                return;
            }

            GameObject fantome = Instantiate(anchor.fantomePrefab, anchor.transform.position, Quaternion.identity);
            fantome.transform.SetParent(anchor.transform);

            if (fantome.GetComponent<Demolition_Fantome>() == null)
                fantome.AddComponent<Demolition_Fantome>();
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
                Debug.LogWarning($"Demolition_ObstacleSpawner: {obj.name} a Universal_Button mais pas Demolition_Pushable — ajoute-le manuellement sur le prefab.");
                return;
            }

            btn.Event.AddListener(pushable.OnPushed);
        }
    }
}