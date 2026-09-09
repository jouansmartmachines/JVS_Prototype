using UnityEngine;

namespace Demolition
{
    /// <summary>
    /// Spawn 3 gardes autour du fantôme en fin de Start() du GameManager.
    /// Référencé dans Demolition_GameManager.guardSpawner.
    /// </summary>
    public class Demolition_GuardSpawner : MonoBehaviour
    {
        [Header("Prefab")]
        public GameObject guardPrefab;

        public void SpawnGuards()
        {
            var fantome = FindObjectOfType<Demolition_Fantome>();
            if (fantome == null) return;

            if (guardPrefab == null)
            {
                guardPrefab = Resources.Load<GameObject>("Prefabs/Garde");
                if (guardPrefab == null)
                {
                    Debug.LogWarning("Demolition_GuardSpawner: prefab Garde introuvable.");
                    return;
                }
            }

            int guardCount = 3;
            float angleStep = 360f / guardCount;

            for (int i = 0; i < guardCount; i++)
            {
                Vector3 pos = fantome.transform.position + new Vector3(
                    Mathf.Cos(i * angleStep * Mathf.Deg2Rad) * 1.5f,
                    Mathf.Sin(i * angleStep * Mathf.Deg2Rad) * 1.5f * 0.7f,
                    0
                );

                GameObject guardGO = Instantiate(guardPrefab, pos, Quaternion.identity);
                var guard = guardGO.GetComponent<Demolition_Guard>();
                if (guard != null)
                    guard.Initialize(fantome.transform, i * angleStep);

                var btn = guardGO.GetComponent<Universal_Button>();
                if (btn != null && guard != null)
                    btn.Event.AddListener(guard.OnTouched);
            }
        }
    }
}