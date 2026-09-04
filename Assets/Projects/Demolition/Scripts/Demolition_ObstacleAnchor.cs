using UnityEngine;

namespace Demolition
{
    /// <summary>
    /// Simple marqueur de position. Aucun prefab référencé ici.
    /// L'ObstacleSpawner lit ces transforms pour savoir où spawner les obstacles.
    /// </summary>
    public class Demolition_ObstacleAnchor : MonoBehaviour
    {
        public float spawnRadius = 2f;
        public int minCount = 1;
        public int maxCount = 3;
        public bool isFantomeAnchor = false;
    }
}