using UnityEngine;
using IndieKit; // Nécessaire pour accéder à l'interface IDamageable

namespace Demolition
{
    public class Demolition_Pushable : MonoBehaviour
    {
        [Header("Force")]
        public float pushForce = 100f;
        public float uplift = 50f;
        public float radiusVariation = 0.5f;

        [Header("Bonus de Hauteur (Multi-coups)")]
        public float upliftBonusPerHit = 2f;
        private int pushCount = 0;

        private Rigidbody rb;
        private IDamageable damageable;
        private float lastFallVelocityY = 0f;
        public Vector3 spawnRotationOffset;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            damageable = GetComponent<IDamageable>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            // Récupère l'interface de dégâts présente sur l'objet (DestructibleObject)
            if (TryGetComponent<Universal_Button>(out var btn))
            {
                btn.Event.RemoveListener(OnPushed);
                btn.Event.AddListener(OnPushed);
            }
            else
            {
                Debug.LogWarning($"[Demolition_Pushable] Aucun Universal_Button trouvé sur {gameObject.name} !");
            }
        }

        void FixedUpdate()
        {
            if (rb != null)
            {
                if (rb.isKinematic) return;
                lastFallVelocityY = rb.linearVelocity.y;

                // Référence du plan basée sur la caméra (profondeur alignée avec le regard de la caméra)
                Vector3 planeNormal = Camera.main != null ? Camera.main.transform.forward : Vector3.forward;

                // 1. Bloque tout déplacement sur l'axe de profondeur de la caméra
                float depthSpeed = Vector3.Dot(rb.linearVelocity, planeNormal);
                if (Mathf.Abs(depthSpeed) > 0.0001f)
                {
                    rb.linearVelocity -= planeNormal * depthSpeed;
                }

                // 2. Bloque les rotations hors-plan par rapport à la caméra
                float inPlaneAngularSpeed = Vector3.Dot(rb.angularVelocity, planeNormal);
                rb.angularVelocity = planeNormal * inPlaneAngularSpeed;
            }
        }

        public void OnPushed()
        {
            if (rb.isKinematic)
            {
                rb.isKinematic = false;
            }
            pushCount++;
            Debug.Log($"Demolition_Pushable: {gameObject.name} touché {pushCount} fois !");

            // 1. Un push enlève 2 HP
            SendDamage(2f);

            if (rb == null) return;

            // Détermine les axes du plan 2D selon l'orientation de la caméra (Écran)
            Vector3 planeRight = Camera.main != null ? Camera.main.transform.right : Vector3.right;
            Vector3 planeUp = Camera.main != null ? Camera.main.transform.up : Vector3.up;

            // Direction gauche / droite par rapport à l'écran
            float directionSign = Random.value > 0.5f ? 1f : -1f;
            Vector3 horizontalForce = planeRight * (directionSign * pushForce);

            // Force verticale (vers le haut de l'écran) avec bonus multi-coups
            float dynamicUplift = uplift + ((pushCount - 1) * upliftBonusPerHit);
            Vector3 verticalForce = planeUp * dynamicUplift;

            // Variation aléatoire strictement contenue dans le plan de la caméra
            Vector2 random2D = Random.insideUnitCircle * radiusVariation;
            Vector3 randomOffset = (planeRight * random2D.x) + (planeUp * random2D.y);

            // Force finale basée sur la caméra
            Vector3 finalForce = horizontalForce + verticalForce + randomOffset;

            // Point d'application décentré uniquement dans le plan 2D de la caméra
            Vector3 torqueOffset = (planeRight * Random.Range(-0.2f, 0.2f)) + (planeUp * Random.Range(-0.2f, 0.2f));

            rb.AddForceAtPosition(finalForce, transform.position + torqueOffset, ForceMode.Impulse);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (rb.isKinematic)
            {
                rb.isKinematic = false;
            }
            float fallSpeed = Mathf.Abs(lastFallVelocityY);
            if (fallSpeed > 5f) // Seuil minimal de chute
            {
                float fallDamage = 1f;

                if (fallSpeed > 15f) fallDamage = 3f;    
                else if (fallSpeed > 10f) fallDamage = 2f;
                else fallDamage = 1f;                

                //SendDamage(fallDamage, collision.contacts[0].point);
                return; 
            }

            if (collision.relativeVelocity.magnitude > 3f)
            {
                //SendDamage(1f, collision.contacts[0].point);
            }
        }

        private void SendDamage(float amount, Vector3? hitPoint = null)
        {
            Vector3 point = hitPoint ?? transform.position;

            if (damageable != null)
            {
                damageable.ApplyDamage(amount, point);
            }
        }
    }
}