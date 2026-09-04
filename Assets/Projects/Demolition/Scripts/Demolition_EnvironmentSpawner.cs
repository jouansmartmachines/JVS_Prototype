using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demolition
{
    /// <summary>
    /// Spawn le terrain + décor aléatoires au Start() selon jour/nuit.
    /// Place sur un empty EnvSpawner dans la GameScene.
    /// </summary>
    public class Demolition_EnvironmentSpawner : MonoBehaviour
    {
        [Header("Environnements (contiennent décor + ObstacleAnchors)")]
            public GameObject[] dayEnvPrefabs;
        public GameObject[] nightEnvPrefabs;

        [Header("Paramètres")]
        public string[] nightSceneKeywords = new string[] { "Night", "Nuit" };

        void Start()
        {
            bool isNight = IsNightScene();
            SpawnEnv(isNight);
        }

        private bool IsNightScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            foreach (string kw in nightSceneKeywords)
            {
                if (sceneName.Contains(kw))
                    return true;
            }
            return false;
        }

        private void SpawnEnv(bool isNight)
        {
            GameObject[] pool = isNight ? nightEnvPrefabs : dayEnvPrefabs;
            if (pool == null || pool.Length == 0) return;

            GameObject prefab = pool[Random.Range(0, pool.Length)];
            if (prefab != null)
                Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }
    }
}