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
        /// <summary>
        /// Alterne jour/nuit à chaque reload de scène.
        /// La scène name sert pour la première détection, ensuite toggle.
        /// </summary>
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
            // Au premier lancement on utilise le nom de la scène, ensuite on alterne
            bool isNight = IsNightScene();
            if (_toggleNight)
                isNight = !isNight;

            SetSkybox(isNight);
            SpawnSettings(isNight);
            SpawnEnv(isNight);

            // Alternance pour le prochain reload
            _toggleNight = true;

            // Déclencher le spawn des obstacles après que les env soient dans la scène
            var spawner = FindObjectOfType<Demolition_ObstacleSpawner>();
            if (spawner != null)
                spawner.SpawnForDifficulty(spawner.CurrentDifficulty);
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
                Instantiate(prefab, Vector3.zero, Quaternion.identity, settingsParent);
        }

        private void SpawnEnv(bool isNight)
        {
            GameObject[] pool = isNight ? nightEnvPrefabs : dayEnvPrefabs;
            if (pool == null || pool.Length == 0) return;
            GameObject prefab = pool[Random.Range(0, pool.Length)];
            if (prefab != null)
                Instantiate(prefab, Vector3.zero, Quaternion.identity, envParent);
        }
    }
}