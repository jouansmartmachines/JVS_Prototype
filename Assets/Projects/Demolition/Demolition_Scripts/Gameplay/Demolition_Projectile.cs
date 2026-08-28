using UnityEngine;

namespace Demolition
{
    public class Demolition_Projectile : MonoBehaviour
    {
        [Header("Apparence")]
        public SpriteRenderer spriteRenderer;

        [Header("Mouvement Z (profondeur)")]
        public float vitesseZ = 3f;
        public float scaleMin = 0.1f;
        public float scaleMax = 1f;
        public float zTarget = 0f;  // Quand on atteint ce Z, on explose
        public float explosionRadius = 2f;
        public float explosionForce = 500f;

        private float startZ;
        private float totalDist;
        private bool launched = false;
        private float flightTime;
        private Vector3 hitPoint; // point X,Y où l'oiseau va exploser

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            // Charger le sprite oiseau depuis Resources
            Texture2D tex = Resources.Load<Texture2D>("Textures/oiseau_dos");
            if (tex != null)
            {
                Sprite s = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                spriteRenderer.sprite = s;
            }
            spriteRenderer.sortingOrder = 3;
        }

        public void Launch(Vector3 worldPos)
        {
            // Position de depart = la ou on a touche, mais profond
            startZ = Camera.main.transform.position.z + 2f; // juste devant la camera
            transform.position = new Vector3(worldPos.x, worldPos.y, startZ);
            hitPoint = new Vector3(worldPos.x, worldPos.y, 0); // le point cible sur le plan de jeu

            totalDist = Mathf.Abs(zTarget - startZ);
            flightTime = 0f;
            launched = true;
        }

        void Update()
        {
            if (!launched) return;

            flightTime += Time.deltaTime;

            // Avance sur Z (profondeur) vers le plan de jeu
            float step = vitesseZ * Time.deltaTime;
            Vector3 pos = transform.position;
            pos.z += step;

            // Si on a depasse zTarget, on explose
            if (pos.z >= zTarget)
            {
                pos.z = zTarget;
                transform.position = pos;
                Explode();
                return;
            }

            // Ratio de progression (0 = depart, 1 = arrivee)
            float t = Mathf.Clamp01((pos.z - startZ) / totalDist);

            // Rétrécit progressivement (effet de profondeur)
            float scale = Mathf.Lerp(scaleMax, scaleMin, t);
            transform.localScale = Vector3.one * scale;

            transform.position = pos;
        }

        void Explode()
        {
            // Explosion visuelle
            GameObject explosionPrefab = Resources.Load<GameObject>("Prefabs/ImpactExplosion");
            if (explosionPrefab != null)
            {
                GameObject effect = Instantiate(explosionPrefab, hitPoint, Quaternion.identity);
                var sr = effect.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite == null)
                {
                    Texture2D tex = Resources.Load<Texture2D>("Textures/impact");
                    if (tex != null)
                        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
                Destroy(effect, 2f);
            }

            // Dégâts aux blocs au point d'impact (en 2D, sur le plan)
            Collider2D[] hits = Physics2D.OverlapCircleAll(hitPoint, explosionRadius);
            foreach (var hit in hits)
            {
                Rigidbody2D rb = hit.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = (hit.transform.position - (Vector3)hitPoint).normalized;
                    float dist = Vector2.Distance(hit.transform.position, hitPoint);
                    rb.AddForce(dir * (explosionForce / Mathf.Max(0.1f, dist)), ForceMode2D.Impulse);

                    Demolition_Block block = hit.GetComponent<Demolition_Block>();
                    if (block != null)
                        block.TakeDamage(1);
                }
            }

            // Score
            if (Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.AddScore(100, hitPoint);

            // Son
            if (Demolition_GameManager.Instance != null)
            {
                AudioSource aud = Demolition_GameManager.Instance.GetComponent<AudioSource>();
                if (aud != null && Demolition_GameManager.Instance.impactSound != null)
                    aud.PlayOneShot(Demolition_GameManager.Instance.impactSound);
            }

            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}