using UnityEngine;

namespace Demolition
{
    public class Demolition_Projectile : MonoBehaviour
    {
        [Header("Apparence")]
        public Sprite oiseauDos;
        public SpriteRenderer spriteRenderer;

        [Header("Mouvement")]
        public float vitesseDepart = 5f;
        public float acceleration = 2f;
        public float scaleMin = 0.1f;
        public float scaleMax = 1f;

        [Header("Destruction à l'arrivée")]
        public GameObject explosionPrefab;
        public float forceExplosion = 500f;
        public float radiusExplosion = 2f;

        private Transform cible;
        private float scrollSpeed;
        private bool launched = false;
        private Vector3 direction;
        private float currentSpeed;

        public void Launch(Transform targetParent, float bgScrollSpeed)
        {
            cible = targetParent;
            scrollSpeed = bgScrollSpeed;
            launched = true;
            currentSpeed = vitesseDepart;

            // De dos
            if (spriteRenderer != null && oiseauDos != null)
                spriteRenderer.sprite = oiseauDos;

            // Direction vers la gauche (vers la structure)
            direction = Vector3.left;
        }

        void Update()
        {
            if (!launched) return;

            // Avance vers la structure
            currentSpeed += acceleration * Time.deltaTime;
            transform.position += direction * currentSpeed * Time.deltaTime * 50f;

            // Rétrécit progressivement (effet de profondeur)
            float scale = Mathf.Lerp(scaleMax, scaleMin, 
                Mathf.Clamp01(currentSpeed / (vitesseDepart + 10f)));
            transform.localScale = Vector3.one * scale;

            // Détection collision avec la structure
            Collider2D hit = Physics2D.OverlapCircle(transform.position, 0.5f);
            if (hit != null && hit.GetComponent<Demolition_Block>() != null)
            {
                Explode();
            }

            // Auto-destruction si trop loin
            if (transform.position.x < -Camera.main.orthographicSize * 2f)
            {
                Destroy(gameObject);
            }
        }

        void Explode()
        {
            if (explosionPrefab != null)
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            // Force sur les blocs proches
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radiusExplosion);
            foreach (var hit in hits)
            {
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = (hit.transform.position - transform.position).normalized;
                    float dist = Vector2.Distance(hit.transform.position, transform.position);
                    rb.AddForce(dir * (forceExplosion / Mathf.Max(0.1f, dist)), ForceMode2D.Impulse);

                    Demolition_Block block = hit.GetComponent<Demolition_Block>();
                    if (block != null)
                        block.TakeDamage(1);
                }
            }

            // Particules
            if (Demolition_GameManager.Instance != null)
            {
                Demolition_GameManager.Instance.AddScore(100, transform.position);
            }

            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radiusExplosion);
        }
    }
}