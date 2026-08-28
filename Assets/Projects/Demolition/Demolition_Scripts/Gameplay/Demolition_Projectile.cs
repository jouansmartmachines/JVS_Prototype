using UnityEngine;

namespace Demolition
{
    public class Demolition_Projectile : MonoBehaviour
    {
        [Header("Apparence")]
        public Sprite oiseauDos;
        public SpriteRenderer spriteRenderer;

        [Header("Mouvement")]
        public float vitesseDepart = 1.5f;
        public float acceleration = 0.3f;
        public float scaleMin = 0.1f;
        public float scaleMax = 1f;
        public float tempsVolMax = 5f;  // secondes avant auto-destruction

        [Header("Destruction à l'arrivée")]
        public GameObject explosionPrefab;
        public float forceExplosion = 500f;
        public float radiusExplosion = 2f;

        private Transform cible;
        private float scrollSpeed;
        private bool launched = false;
        private Vector3 direction;
        private float currentSpeed;
        private float flightTime;
        private float startX;

        public void Launch(Transform targetParent, float bgScrollSpeed)
        {
            cible = targetParent;
            scrollSpeed = bgScrollSpeed;
            launched = true;
            currentSpeed = vitesseDepart;
            flightTime = 0f;
            startX = transform.position.x;

            // De dos
            if (spriteRenderer != null && oiseauDos != null)
                spriteRenderer.sprite = oiseauDos;

            // Direction vers la gauche (vers la structure)
            direction = Vector3.left;
        }

        void Update()
        {
            if (!launched) return;

            flightTime += Time.deltaTime;

            // Avance vers la structure - vitesse lisible par le joueur
            currentSpeed += acceleration * Time.deltaTime;
            float moveSpeed = Mathf.Lerp(2f, 6f, flightTime / tempsVolMax);
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Petite ondulation verticale (effet de vol)
            float wave = Mathf.Sin(flightTime * 3f) * 0.15f;
            transform.position += Vector3.up * wave * Time.deltaTime;

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

            // Auto-destruction si trop loin ou temps ecoule
            if (flightTime >= tempsVolMax || transform.position.x < -Camera.main.orthographicSize * 2f)
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