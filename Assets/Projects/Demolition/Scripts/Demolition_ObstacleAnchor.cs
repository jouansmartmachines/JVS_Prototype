using UnityEngine;

namespace Demolition
{
    /// <summary>
    /// Zone de spawn pour obstacles. Place un empty dans la scène 3D,
    /// règle le radius, la pool de prefabs, et combien en spawner.
    /// Si fantomeAnchor = true, le fantôme spawn ici (position exacte).
    /// </summary>
    public class Demolition_ObstacleAnchor : MonoBehaviour
    {
        [Header("Pool d'obstacles")]
        public GameObject[] obstaclePrefabs;

        [Header("Zone de spawn")]
        public float spawnRadius = 2f;
        public int minCount = 1;
        public int maxCount = 3;

        [Header("Fantôme")]
        public bool isFantomeAnchor = false;
        public GameObject fantomePrefab;
    }
}