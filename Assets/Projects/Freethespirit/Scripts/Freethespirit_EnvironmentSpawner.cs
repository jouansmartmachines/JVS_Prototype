using UnityEngine;
using UnityEngine.Rendering;

namespace Freethespirit
{
    [System.Serializable]
    public struct LevelScenes
    {
        public int level;
        public GameObject[] scenes;
    }

    [System.Serializable]
    public struct EnvironmentData
    {
        public string environmentName;
        public Material skyboxMaterial;

        [Header("Lighting & Shadows")]
        [ColorUsage(true, true)]
        public Color realtimeShadowColor;

        [Header("Environment Lighting (Gradient)")]
        [ColorUsage(true, true)]
        public Color ambientSkyColor;

        [ColorUsage(true, true)]
        public Color ambientEquatorColor;

        [ColorUsage(true, true)]
        public Color ambientGroundColor;

        [Header("Environment Reflections")]
        [Range(1, 5)]
        public int reflectionBounces;

        public float reflectionIntensityMultiplier;

        [Header("Fog Settings")]
        public bool fogEnabled;
        public Color fogColor;
        public float fogStartDistance;
        public float fogEndDistance;

        [Header("Scenes par niveau")]
        public LevelScenes[] levelScenes;

        [Header("Settings Prefabs")]
        public GameObject[] settingsPrefabs;

        public Material robotsMaterial;

        public GameObject[] GetScenesForLevel(int level)
        {
            if (levelScenes == null)
                return null;

            for (int i = 0; i < levelScenes.Length; i++)
            {
                if (levelScenes[i].level == level)
                    return levelScenes[i].scenes;
            }

            return null;
        }
    }

    public class Freethespirit_EnvironmentSpawner : MonoBehaviour
    {
        [Header("Configurations Jour / Nuit")]
        public EnvironmentData dayEnvironment;
        public EnvironmentData nightEnvironment;

        [Header("Parents dans la hiérarchie")]
        public Transform settingsParent;
        public Transform envParent;

        private GameObject currentSettingsInstance;

        public GameObject currentEnvInstance;

        public void ApplyEnvironment(EnvironmentData data, int level)
        {
            // Skybox
            RenderSettings.skybox = data.skyboxMaterial;

            // Realtime Shadow Color
            RenderSettings.subtractiveShadowColor =
                data.realtimeShadowColor;

            // Éclairage ambiant
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor =
                data.ambientSkyColor;
            RenderSettings.ambientEquatorColor =
                data.ambientEquatorColor;
            RenderSettings.ambientGroundColor =
                data.ambientGroundColor;

            // Réflexions
            RenderSettings.reflectionBounces =
                data.reflectionBounces;

            RenderSettings.reflectionIntensity =
                data.reflectionIntensityMultiplier;

            // Fog
            RenderSettings.fog =
                data.fogEnabled;

            RenderSettings.fogColor =
                data.fogColor;

            RenderSettings.fogMode =
                FogMode.Linear;

            RenderSettings.fogStartDistance =
                data.fogStartDistance;

            RenderSettings.fogEndDistance =
                data.fogEndDistance;

            // Supprime les anciens prefabs
            if (currentSettingsInstance != null)
                Destroy(currentSettingsInstance);

            if (currentEnvInstance != null)
                Destroy(currentEnvInstance);

            // Récupère toutes les scènes correspondant au level
            GameObject[] targetEnvPool =
                data.GetScenesForLevel(level);

            // Spawn Settings
            currentSettingsInstance =
                SpawnRandomPrefab(
                    data.settingsPrefabs,
                    settingsParent
                );

            // Spawn une scène aléatoire du level
            currentEnvInstance =
                SpawnRandomPrefab(
                    targetEnvPool,
                    envParent
                );

            DynamicGI.UpdateEnvironment();
        }

        private GameObject SpawnRandomPrefab(
            GameObject[] pool,
            Transform parentTransform)
        {
            if (pool == null || pool.Length == 0)
                return null;

            GameObject prefab =
                pool[Random.Range(0, pool.Length)];

            if (prefab == null)
                return null;

            Transform targetParent =
                parentTransform != null
                ? parentTransform
                : transform;

            return Instantiate(
                prefab,
                Vector3.zero,
                Quaternion.identity,
                targetParent
            );
        }
    }
}