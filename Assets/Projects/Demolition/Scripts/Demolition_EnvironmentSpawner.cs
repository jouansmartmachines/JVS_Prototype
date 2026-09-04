using UnityEngine;
using UnityEngine.SceneManagement;

namespace Demolition
{
    /// <summary>
    /// Gère l'environnement visuel : skybox + settings prefabs (reflection probes, éclairage).
    /// Ne spawn PAS de terrain ni d'obstacles — juste le décor ambiant.
    /// </summary>
    public class Demolition_EnvironmentSpawner : MonoBehaviour
    {
        [Header("Skybox")]
        public Material daySkybox;
        public Material nightSkybox;

        [Header("Settings Prefabs (reflection probes, lighting tweaks, etc.)")]
        public GameObject[] daySettingsPrefabs;
        public GameObject[] nightSettingsPrefabs;

        [Header("Paramètres")]
        public string[] nightSceneKeywords = new string[] { "Night", "Nuit" };

        void Start()
        {
            bool isNight = IsNightScene();
            SetSkybox(isNight);
            SpawnSettings(isNight);
        }

        private bool IsNightScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            foreach (string kw in nightSceneKeywords)
                if (sceneName.Contains(kw)) return true;
            return false;
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
    }
}