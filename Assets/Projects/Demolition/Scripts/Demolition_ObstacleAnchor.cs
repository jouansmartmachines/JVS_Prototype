using UnityEngine;

namespace Demolition
{
    /// <summary>
    /// Marqueur de position avec ses propres prefabs et sa caméra dédiée.
    /// L'ObstacleSpawner lit ces transforms et ces prefabs pour spawner les obstacles.
    /// </summary>
    public class Demolition_ObstacleAnchor : MonoBehaviour
    {
        [Header("Zone de spawn")]
        public float spawnRadius = 2f;
        public int minCount = 1;
        public int maxCount = 3;
        public bool isFantomeAnchor = false;

        [Header("Prefabs de cet anchor")]
        public GameObject[] obstaclePrefabs;

        [Header("Caméra dédiée")]
        public Camera anchorCamera;
    }
}