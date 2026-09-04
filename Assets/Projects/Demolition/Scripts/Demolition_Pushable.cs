using UnityEngine;

namespace Demolition
{
    public class Demolition_Pushable : MonoBehaviour
    {
        [Header("Force")]
        public float pushForce = 8f;
        public float uplift = 4f;
        public float radiusVariation = 0.5f;

        [Header("Bonus de Hauteur (Multi-coups)")]
        public float upliftBonusPerHit = 2f;
        private int pushCount = 0;

        private Rigidbody rb;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody>();
        }

        public void OnPushed()
        {
            pushCount++;
            Debug.Log($"Demolition_Pushable: {gameObject.name} touché {pushCount} fois !");
            if (rb == null) return;

            Camera cam = Camera.main;
            Vector3 camForward = cam != null ? cam.transform.forward : Vector3.forward;

            // 1. Direction latérale (projection sur le plan de la caméra, SANS profondeur)
            Vector3 baseDir = cam != null ? (transform.position - cam.transform.position) : transform.forward;
            Vector3 lateralDir = Vector3.ProjectOnPlane(baseDir, camForward).normalized;
            if (lateralDir == Vector3.zero && cam != null)
            {
                lateralDir = cam.transform.right;
            }

            // 2. Variation aléatoire (uniquement sur le plan de l'écran)
            Vector3 randomOffset = Vector3.zero;
            if (cam != null)
            {
                Vector2 random2D = Random.insideUnitCircle * radiusVariation;
                randomOffset = (cam.transform.right * random2D.x) + (cam.transform.up * random2D.y);
            }
            else
            {
                randomOffset = Random.insideUnitSphere * radiusVariation;
                randomOffset.z = 0f;
            }

            // 3. Calcul de la hauteur dynamique (augmente avec le nombre de coups)
            float dynamicUplift = uplift + ((pushCount - 1) * upliftBonusPerHit);

            // 4. CONSTRUCTION DE LA FORCE AVEC PRIORITÉ AU HAUT
            // On applique la force latérale, mais on NE projette PAS la force finale sur le plan de la caméra.
            // Au lieu de cela, on ajoute la force "uplift" directement après, pour garantir l'envol vertical.
            Vector3 finalForce = (lateralDir * pushForce) + randomOffset;
            finalForce += Vector3.up * dynamicUplift;

            // Application de l'impulsion physique
            rb.AddForceAtPosition(
                finalForce,
                transform.position + (Random.insideUnitSphere * 0.3f),
                ForceMode.Impulse);
        }
    }
}