using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demolition
{
    /// <summary>
    /// Gère l'environnement visuel : settings prefabs (reflection, éclairage)
    /// + env prefabs (décor/terrain). Pas de keywords — la détection jour/nuit
    /// est hardcodée sur le nom de la scène.
    /// </summary>
    public class Demolition_EnvironmentSpawner : MonoBehaviour
    {
        [Header("Skybox")]
        public Material daySkybox;
        public Material nightSkybox;

        [Header("Settings Prefabs (reflection probes, lighting tweaks)")]
        public GameObject[] daySettingsPrefabs;
        public GameObject[] nightSettingsPrefabs;

        [Header("Env Prefabs (décor, terrain, obstacles)")]
        public GameObject[] dayEnvPrefabs;
        public GameObject[] nightEnvPrefabs;

        void Start()
        {
            bool isNight = IsNightScene();
            SetSkybox(isNight);
            SpawnSettings(isNight);
            SpawnEnv(isNight);
        }

        private static bool IsNightScene()
        {
            string name = SceneManager.GetActiveScene().name;
            return name.Contains("Night") || name.Contains("Nuit");
        }

        private void SetSkybox(bool isNight)
        {
            RenderSettings.skybox = isNight ? nightSkybox : daySkybox;
        }

        private void SpawnSettings(bool isNight)
        {
            GameObject[] pool = isNight ? nightSettingsPrefabs : daySettingsPrefabs;
            if (pool == null || pool.Length == 0) return;
            GameObject prefab = pool[Random.Range(0, pool.Length)];
            if (prefab != null)
                Instantiate(prefab, Vector3.zero, Quaternion.identity);
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