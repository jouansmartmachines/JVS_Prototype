using UnityEngine;

namespace Demolition
{
    /// <summary>
    /// Projectile oiseau / impact précis et très doux.
    /// Ne touche strictement QUE le bloc cliqué avec une force modérée,
    /// permettant d'entamer la structure bloc par bloc sans jamais la one-shot.
    /// </summary>
    public class Demolition_Projectile : MonoBehaviour
    {
        [Header("Visuel & Rendu")]
        public SpriteRenderer spriteRenderer;
        public TrailRenderer trailRenderer;

        [Header("Vol en Profondeur")]
        public float flightDuration = 0.14f;
        public float scaleStart = 1.4f;
        public float scaleEnd = 0.55f;
        public float zStart = -6f;
        public float zTarget = 0f;

        [Header("Impact Doux & Précis")]
        public float hitRadius = 0.35f;
        public float pushForce = 2.2f;        // Poussée physique très douce (déséquilibre léger)
        public int directDamage = 1;          // 1 seul point de dégât au bloc touché

        private Vector3 targetPos;
        private Vector3 startPos;
        private float elapsedTime = 0f;
        private bool isFlying = false;
        private float rotationSpeed;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            spriteRenderer.sortingOrder = 8;

            if (spriteRenderer.sprite == null)
            {
                Texture2D tex = Resources.Load<Texture2D>("Textures/oiseau_dos");
                if (tex != null)
                {
                    spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }

            trailRenderer = GetComponent<TrailRenderer>();
            if (trailRenderer == null)
            {
                trailRenderer = gameObject.AddComponent<TrailRenderer>();
                trailRenderer.time = 0.08f;
                trailRenderer.startWidth = 0.2f;
                trailRenderer.endWidth = 0.02f;
                trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
                trailRenderer.startColor = new Color(1f, 0.65f, 0.15f, 0.6f);
                trailRenderer.endColor = new Color(1f, 0.9f, 0.3f, 0f);
                trailRenderer.sortingOrder = 7;
            }
        }

        public void Launch(Vector3 worldTarget)
        {
            targetPos = new Vector3(worldTarget.x, worldTarget.y, zTarget);
            startPos = new Vector3(worldTarget.x, worldTarget.y, zStart);

            transform.position = startPos;
            transform.localScale = Vector3.one * scaleStart;

            rotationSpeed = Random.Range(-360f, 360f);
            elapsedTime = 0f;
            isFlying = true;
        }

        void Update()
        {
            if (!isFlying) return;

            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / flightDuration);
            float easeT = t * t;

            transform.position = Vector3.Lerp(startPos, targetPos, easeT);
            transform.localScale = Vector3.Lerp(Vector3.one * scaleStart, Vector3.one * scaleEnd, t);
            transform.rotation = Quaternion.Euler(0, 0, elapsedTime * rotationSpeed);

            if (t >= 1.0f)
            {
                isFlying = false;
                transform.position = targetPos;
                Impact();
            }
        }

        private void Impact()
        {
            SpawnImpactVFX(targetPos);

            // Touche exclusivement le bloc cliqué
            Collider2D directHit = Physics2D.OverlapPoint(targetPos);
            if (directHit == null)
            {
                directHit = Physics2D.OverlapCircle(targetPos, hitRadius);
            }

            int hitCount = 0;

            if (directHit != null)
            {
                Rigidbody2D hitRb = directHit.GetComponent<Rigidbody2D>();
                Demolition_Block hitBlock = directHit.GetComponent<Demolition_Block>();

                Vector2 pushDirection = (Vector2)(directHit.transform.position - targetPos);
                if (pushDirection.sqrMagnitude < 0.01f)
                {
                    pushDirection = new Vector2(Random.Range(-0.15f, 0.15f), 0.25f).normalized;
                }
                else
                {
                    pushDirection.Normalize();
                }

                // Poussée douce et ciblée uniquement sur le bloc cliqué
                if (hitRb != null && hitRb.bodyType == RigidbodyType2D.Dynamic)
                {
                    hitRb.AddForceAtPosition(pushDirection * pushForce, targetPos, ForceMode2D.Impulse);
                }

                // 1 seul point de dégât sur le bloc cliqué
                if (hitBlock != null)
                {
                    hitBlock.TakeDamage(directDamage, pushDirection);
                    hitCount++;
                }
            }

            if (Demolition_GameManager.Instance != null)
            {
                Demolition_GameManager.Instance.TriggerImpactFeel(targetPos, hitCount);
            }

            Demolition_DebrisSpawner.SpawnDustCloud(targetPos, 0.4f);

            Destroy(gameObject);
        }

        private void SpawnImpactVFX(Vector3 pos)
        {
            GameObject explosionPrefab = Resources.Load<GameObject>("Prefabs/ImpactExplosion");
            if (explosionPrefab != null)
            {
                GameObject effect = Instantiate(explosionPrefab, pos, Quaternion.identity);
                var sr = effect.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite == null)
                {
                    Texture2D tex = Resources.Load<Texture2D>("Textures/impact");
                    if (tex != null)
                        sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
                Destroy(effect, 0.2f);
            }
        }
    }
}
