using UnityEngine;
using UnityEngine.Events;

namespace Sparks
{
    /// <summary>
    /// Primitive 3D projetée par le volcan. 
    /// Détection 3D : Universal_Button (Physics.Raycast) → OnClicked() → GameManager.AddScore().
    /// Auto-destruction après lifetime si non cliquée.
    /// </summary>
    public class Sparks_Primitive : MonoBehaviour
    {
        [Header("Stats")]
        public int points = 10;
        public float lifetime = 8f;

        [Header("Physique")]
        private Rigidbody rb;
        private Collider col_3d;

        public bool IsAlive { get; private set; } = true;

        private float elapsed = 0f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            col_3d = GetComponent<Collider>();
            if (col_3d == null)
            {
                if (GetComponent<SphereCollider>() == null &&
                    GetComponent<BoxCollider>() == null &&
                    GetComponent<CapsuleCollider>() == null)
                {
                    gameObject.AddComponent<SphereCollider>();
                }
                col_3d = GetComponent<Collider>();
            }
        }

        void Start()
        {
            // Universal_Button pour la détection 3D
            var btn = gameObject.AddComponent<Universal_Button>();
            btn.IsActive = true;
            btn.Event.AddListener(OnClicked);
        }

        void Update()
        {
            if (!IsAlive) return;

            elapsed += Time.deltaTime;
            if (elapsed >= lifetime)
            {
                IsAlive = false;
                StartCoroutine(FadeOutAndDestroy(0.3f));
            }
        }

        /// <summary>
        /// Initialiser après spawn
        /// </summary>
        public void Init(int pts, float forceMin, float forceMax)
        {
            points = pts;

            // Direction aléatoire vers le haut (cône de volcan)
            Vector3 dir = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), 0) * Vector3.up;
            dir += Random.insideUnitSphere * 0.3f;
            dir.Normalize();

            float force = Random.Range(forceMin, forceMax);
            rb.AddForce(dir * force, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * Random.Range(1f, 5f), ForceMode.Impulse);
        }

        public void OnClicked()
        {
            if (!IsAlive) return;
            IsAlive = false;

            if (Sparks_GameManager.i != null)
                Sparks_GameManager.i.AddScore(points);

            // Effet visuel de destruction
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
    }
}