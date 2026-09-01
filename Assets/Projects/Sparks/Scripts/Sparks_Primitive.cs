using UnityEngine;

namespace Sparks
{
    public class Sparks_Primitive : MonoBehaviour
    {
        public int points = 10;
        public float lifetime = 8f;
        private Rigidbody rb;
        private Collider col3d;
        public bool IsAlive { get; private set; } = true;
        private float elapsed = 0f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            col3d = GetComponent<Collider>();
            if (col3d == null) gameObject.AddComponent<SphereCollider>();
        }

        void Start()
        {
            var btn = gameObject.AddComponent<Universal_Button>();
            btn.IsActive = true;
            btn.Event.AddListener(OnClicked);
        }

        void Update()
        {
            if (!IsAlive) return;
            elapsed += Time.deltaTime;
            if (elapsed >= lifetime) { IsAlive = false; Destroy(gameObject, 0.3f); }
        }

        public void Init(int pts, float forceMin, float forceMax)
        {
            points = pts;
            Vector3 dir = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0f, 360f), 0) * Vector3.up;
            dir += Random.insideUnitSphere * 0.3f;
            dir.Normalize();
            rb.AddForce(dir * Random.Range(forceMin, forceMax), ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * Random.Range(1f, 5f), ForceMode.Impulse);
        }

        public void OnClicked()
        {
            if (!IsAlive) return;
            IsAlive = false;
            if (Sparks_GameManager.i != null) Sparks_GameManager.i.AddScore(points);
            Destroy(gameObject, 0.1f);
        }
    }
}