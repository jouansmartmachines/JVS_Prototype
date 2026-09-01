using UnityEngine;
using UnityEngine.Events;

namespace Sparks
{
    /// <summary>
    /// Primitive projetée par le volcan. Contient Universal_Button pour la détection 3D.
    /// Au clic : notifie le GameManager, joue un effet, se détruit.
    /// Auto-destruction après timeout si pas cliquée.
    /// </summary>
    public class Sparks_Primitive : MonoBehaviour
    {
        public enum PrimitiveType { Sphere, Cube, Capsule }

        [Header("Type & Score")]
        public PrimitiveType type = PrimitiveType.Sphere;
        public int points = 10;

        [Header("Auto-destruction")]
        public float lifetime = 8f;
        private float elapsed = 0f;
        private bool isDead = false;

        [Header("Événement")]
        public UnityEvent onHit;

        private Rigidbody rb;
        private Collider col;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();

            // Fallback Rigidbody si manquant
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = true;
            }
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Fallback Collider si manquant
            if (col == null)
            {
                if (type == PrimitiveType.Sphere)
                    gameObject.AddComponent<SphereCollider>();
                else if (type == PrimitiveType.Cube)
                    gameObject.AddComponent<BoxCollider>();
                else
                    gameObject.AddComponent<CapsuleCollider>();
            }
        }

        void Start()
        {
            // Ajouter et configurer Universal_Button pour la détection 3D
            var btn = gameObject.AddComponent<Universal_Button>();
            btn.IsActive = true;

            // Lier l'événement
            if (onHit == null)
                onHit = new UnityEvent();

            onHit.AddListener(OnClicked);
            btn.Event.AddListener(() => onHit?.Invoke());
        }

        void Update()
        {
            if (isDead) return;

            // Auto-destruction après timeout
            elapsed += Time.deltaTime;
            if (elapsed >= lifetime)
            {
                isDead = true;
                // Animation de fondu avant destruction
                StartCoroutine(FadeOutAndDestroy(0.3f));
            }
        }

        void OnClicked()
        {
            if (isDead) return;
            isDead = true;

            // Notifier le GameManager
            if (Sparks_GameManager.Instance != null)
            {
                Sparks_GameManager.Instance.AddScore(points, transform.position);
                Sparks_GameManager.Instance.PlayClickEffect(transform.position, type);
            }

            // Effet visuel : pop + scale
            transform.localScale = Vector3.one * 1.5f;
            Destroy(gameObject, 0.1f);
        }

        private System.Collections.IEnumerator FadeOutAndDestroy(float duration)
        {
            float t = 0;
            Vector3 startScale = transform.localScale;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                transform.localScale = startScale * (1f - p * 0.5f);
                yield return null;
            }
            Destroy(gameObject);
        }

        /// <summary>
        /// Lance la primitive depuis le volcan avec une force aléatoire
        /// </summary>
        public void Launch(Vector3 origin, float forceMin, float forceMax)
        {
            transform.position = origin;

            // Direction aléatoire vers le haut (cône)
            float angle = Random.Range(-30f, 30f);
            float elevation = Random.Range(60f, 85f);
            Vector3 dir = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), 0) * Vector3.up;
            dir += Random.insideUnitSphere * 0.3f;
            dir.Normalize();

            float force = Random.Range(forceMin, forceMax);
            rb.AddForce(dir * force, ForceMode.Impulse);

            // Torque aléatoire pour faire tourner
            rb.AddTorque(Random.insideUnitSphere * Random.Range(1f, 5f), ForceMode.Impulse);
        }
    }
}