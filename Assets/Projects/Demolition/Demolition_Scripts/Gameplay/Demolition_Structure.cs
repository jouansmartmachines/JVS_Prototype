using UnityEngine;
using System.Collections.Generic;

namespace Demolition
{
    /// <summary>
    /// Gère le cycle de vie d'une structure générée, le déclenchement de ralentis d'effondrement et le nettoyage hors écran.
    /// </summary>
    public class Demolition_Structure : MonoBehaviour
    {
        public List<Demolition_Block> blocks = new List<Demolition_Block>();
        private int destroyedCount = 0;
        private bool collapseTriggered = false;

        void Start()
        {
            blocks.AddRange(GetComponentsInChildren<Demolition_Block>());
        }

        public void OnBlockDestroyed(Demolition_Block block)
        {
            destroyedCount++;
            blocks.Remove(block);

            // Déclenchement d'un ralenti spectaculaire lors d'effondrements majeurs
            if (destroyedCount >= 3 && !collapseTriggered)
            {
                collapseTriggered = true;
                Demolition_GameManager gm = Demolition_GameManager.Instance;
                if (gm != null)
                {
                    gm.StartCoroutine(gm.CollapseSlowMo());
                    Demolition_DebrisSpawner.SpawnDustCloud(transform.position, 2.5f);
                }
            }

            // Nuage de poussière final quand toute la structure est rasée
            if (blocks.Count == 0 && !collapseTriggered)
            {
                collapseTriggered = true;
                Demolition_DebrisSpawner.SpawnDustCloud(transform.position, 2f);
            }
        }

        void Update()
        {
            // Nettoyage automatique dès que la structure sort complètement de l'écran à gauche
            if (Camera.main != null)
            {
                float leftEdge = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, -Camera.main.transform.position.z)).x;
                if (transform.position.x < leftEdge - 15f)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
