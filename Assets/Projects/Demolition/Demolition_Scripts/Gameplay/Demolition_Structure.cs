using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Demolition
{
    public class Demolition_Structure : MonoBehaviour
    {
        public List<Demolition_Block> blocks = new List<Demolition_Block>();
        private int destroyedCount = 0;
        private bool collapseTriggered = false;

        void Start()
        {
            // Enregistrer tous les blocs enfants
            blocks.AddRange(GetComponentsInChildren<Demolition_Block>());
        }

        public void OnBlockDestroyed(Demolition_Block block)
        {
            destroyedCount++;
            blocks.Remove(block);

            // Si assez de blocs detruits rapidement -> effondrement
            if (destroyedCount >= 2 && !collapseTriggered)
            {
                collapseTriggered = true;
                Demolition_GameManager gm = Demolition_GameManager.Instance;
                if (gm != null)
                {
                    gm.StartCoroutine(gm.CollapseSlowMo());
                    // Nuage de poussière
                    Demolition_DebrisSpawner.SpawnDustCloud(transform.position, 2f);
                }
            }

            // Si tous les blocs detruits + poussiere
            if (blocks.Count == 0 && !collapseTriggered)
            {
                collapseTriggered = true;
                Demolition_DebrisSpawner.SpawnDustCloud(transform.position, 1.5f);
            }
        }

        void Update()
        {
            // Auto-destruction si trop loin
            if (transform.position.x < Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x - 15f)
            {
                Destroy(gameObject);
            }
        }
    }
}