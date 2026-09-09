using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Demolition
{
    [System.Serializable]
    public struct EnvironmentData
    {
        public string environmentName;
        public Material skyboxMaterial;
        
        [Header("Lighting & Shadows")]
        [ColorUsage(true, true)] public Color realtimeShadowColor;

        [Header("Environment Lighting (Gradient)")]
        [ColorUsage(true, true)] public Color ambientSkyColor;
        [ColorUsage(true, true)] public Color ambientEquatorColor;
        [ColorUsage(true, true)] public Color ambientGroundColor;

        [Header("Environment Reflections")]
        [Range(1, 5)] public int reflectionBounces;
        public float reflectionIntensityMultiplier;

        [Header("Fog Settings")]
        public bool fogEnabled;
        public Color fogColor;
        public float fogStartDistance;
        public float fogEndDistance;

        [Header("Prefabs associés")]
        public GameObject[] envPrefabs;
        public GameObject[] settingsPrefabs;
    }

    public class Demolition_EnvironmentSpawner : MonoBehaviour
    {
        [Header("Configurations Jour / Nuit")]
        public EnvironmentData dayEnvironment;
        public EnvironmentData nightEnvironment;

        [Header("Parents dans la hiérarchie")]
        public Transform settingsParent;
        public Transform envParent;

        private GameObject currentSettingsInstance;
        public GameObject currentEnvInstance;




        public void ApplyEnvironment(EnvironmentData data)
        {
            // 1. Skybox
            RenderSettings.skybox = data.skyboxMaterial;

            // 2. Realtime Shadow Color
            RenderSettings.subtractiveShadowColor = data.realtimeShadowColor;

            // 3. Éclairage Ambiant (Trilight)
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = data.ambientSkyColor;
            RenderSettings.ambientEquatorColor = data.ambientEquatorColor;
            RenderSettings.ambientGroundColor = data.ambientGroundColor;

            // 4. Réflexions
            RenderSettings.reflectionBounces = data.reflectionBounces;
            RenderSettings.reflectionIntensity = data.reflectionIntensityMultiplier;

            // 5. Brouillard (Fog)
            RenderSettings.fog = data.fogEnabled;
            RenderSettings.fogColor = data.fogColor;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = data.fogStartDistance;
            RenderSettings.fogEndDistance = data.fogEndDistance;

            // 6. Nettoyage des anciens prefabs (chunks et réglages précédents)
            if (currentSettingsInstance != null) Destroy(currentSettingsInstance);
            if (currentEnvInstance != null) Destroy(currentEnvInstance);

            // 7. Instanciation des nouveaux prefabs correspondants
            currentSettingsInstance = SpawnRandomPrefab(data.settingsPrefabs, settingsParent);
            currentEnvInstance = SpawnRandomPrefab(data.envPrefabs, envParent);

            // Met à jour l'illumination globale en temps réel
            DynamicGI.UpdateEnvironment();
        }

        private GameObject SpawnRandomPrefab(GameObject[] pool, Transform parentTransform)
        {
            if (pool == null || pool.Length == 0) return null;

            GameObject prefab = pool[Random.Range(0, pool.Length)];
            if (!prefab) return null;

            Transform targetParent = parentTransform ? parentTransform : transform;
            return Instantiate(prefab, Vector3.zero, Quaternion.identity, targetParent);
        }
    }
}