using UnityEngine;

namespace Demolition
{
    public class Demolition_ScrollingBackground : MonoBehaviour
    {
        [Header("Parallaxe")]
        public Transform[] backgroundLayers;
        public float[] layerSpeeds;  // Plus lent = plus loin
        public float resetX = -20f;
        public float startX = 20f;

        [Header("Routeur de tableau")]
        public Transform tableauSpawnPoint;
        public GameObject[] tableauPrefabs;

        private float scrollSpeed;
        private GameObject currentTableau;

        void Start()
        {
            scrollSpeed = Demolition_GameManager.Instance != null
                ? Demolition_GameManager.Instance.currentScrollSpeed
                : 2f;

            // Spawn premier tableau
            SpawnTableau();
        }

        void Update()
        {
            // Dégradé de fond : chaque couche à sa vitesse
            for (int i = 0; i < backgroundLayers.Length && i < layerSpeeds.Length; i++)
            {
                Vector3 pos = backgroundLayers[i].position;
                pos.x -= layerSpeeds[i] * scrollSpeed * Time.deltaTime * 50f;
                backgroundLayers[i].position = pos;

                // Reset loop infini
                if (pos.x < resetX)
                {
                    pos.x = startX;
                    backgroundLayers[i].position = pos;
                }
            }
        }

        public void SpawnTableau()
        {
            if (tableauPrefabs.Length == 0) return;

            GameObject prefab = tableauPrefabs[Random.Range(0, tableauPrefabs.Length)];
            currentTableau = Instantiate(prefab, tableauSpawnPoint.position, Quaternion.identity);
            currentTableau.transform.SetParent(Demolition_GameManager.Instance.structuresParent);
        }

        void OnDestroy()
        {
            // Nettoyage si on change de scène
        }
    }
}