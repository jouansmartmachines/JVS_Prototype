using UnityEngine;

namespace Monstres
{
    [CreateAssetMenu(fileName = "SpawnableTarget", menuName = "new Spawnable")]
    public class ScriptableObject_SpawnableTarget : ScriptableObject
    {
        public GameObject targetPrefab;
        public int pointValue = 100;
        public Color spriteColor;
    }
}
