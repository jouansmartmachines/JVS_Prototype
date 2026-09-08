using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demolition
{
    public class Demolition_EnvironmentSpawner : MonoBehaviour
    {
        private static bool _toggleNight = false;

        [Header("Skybox")]
        public Material daySkybox;
        public Material nightSkybox;

        [Header("Settings Prefabs (reflection probes, lighting tweaks)")]
        public GameObject[] daySettingsPrefabs;
        public GameObject[] nightSettingsPrefabs;

        [Header("Env Prefabs (décor, terrain, obstacles)")]
        public GameObject[] dayEnvPrefabs;
        public GameObject[] nightEnvPrefabs;

        [Header("Parents dans la hiérarchie")]
        public Transform settingsParent;
        public Transform envParent;

        void Start()
        {
            bool isNight = IsNightScene();
            if (_toggleNight)
                isNight = !isNight;

            SetSkybox(isNight);
            SpawnSettings(isNight);
            SpawnEnv(isNight);

            _toggleNight = true;

            var spawner = FindObjectOfType<Demolition_ObstacleSpawner>();
            if (spawner)
                spawner.SpawnForDifficulty(Demolition_GameManager.currentLevel);
        }

        private static bool IsNightScene()
        {
            string name = SceneManager.GetActiveScene().name;
            return name.Contains("Night") || name.Contains("Nuit");
        }

        private void SetSkybox(bool isNight)
        {
            Material targetSkybox = isNight ? nightSkybox : daySkybox;
            if (targetSkybox)
                RenderSettings.skybox = targetSkybox;
        }

        private void SpawnSettings(bool isNight)
        {
            GameObject[] pool = isNight ? nightSettingsPrefabs : daySettingsPrefabs;
            SpawnRandomPrefab(pool, settingsParent, "Settings");
        }

        private void SpawnEnv(bool isNight)
        {
            GameObject[] pool = isNight ? nightEnvPrefabs : dayEnvPrefabs;
            SpawnRandomPrefab(pool, envParent, "Env");
        }

        private void SpawnRandomPrefab(GameObject[] pool, Transform parentTransform, string categoryName)
        {
            if (pool == null || pool.Length == 0) return;

            GameObject prefab = pool[Random.Range(0, pool.Length)];
            if (!prefab) return;

            Transform targetParent = parentTransform ? parentTransform : transform;
            Instantiate(prefab, Vector3.zero, Quaternion.identity, targetParent);
        }
    }
}
