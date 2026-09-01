using UnityEngine;
using System.Collections;

namespace Demolition
{
    /// <summary>
    /// Bloc destructible avec haute durabilité et Juice avancé (squash & stretch, fissures, comportements cochons).
    /// </summary>
    public class Demolition_Block : MonoBehaviour
    {
        public enum MaterialType { Bois, Verre, Pierre, Cochon }

        [Header("Matériau & Statistiques")]
        public MaterialType materialType = MaterialType.Bois;
        public int hp = 4;
        public int points = 50;

        [Header("Cible Spéciale (Cochon)")]
        public bool isTarget = false;
        public int starValue = 1;

        [Header("Visuel & Rendu")]
        public SpriteRenderer spriteRenderer;
        public Sprite[] damageSprites;
        public GameObject popupTextPrefab;

        private Rigidbody2D rb;
        private int maxHp;
        private bool isDestroyed = false;
        private Demolition_Structure parentStructure;
        private Vector3 originalScale;
        private Coroutine flashCoroutine;
        private Coroutine squashCoroutine;
        private Demolition_PigBehavior pigBehavior;

        private static PhysicsMaterial2D defaultBlockMat;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody2D>();

            if (defaultBlockMat == null)
            {
                defaultBlockMat = new PhysicsMaterial2D("BlockPhysicsMat")
                {
                    friction = 0.8f,
                    bounciness = 0.01f
                };
            }

            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                col.sharedMaterial = defaultBlockMat;
            }

            ConfigurePhysicsAndStats();

            maxHp = hp;
            originalScale = transform.localScale;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            LoadMaterialVisuals();

            if (materialType == MaterialType.Cochon)
            {
                pigBehavior = GetComponent<Demolition_PigBehavior>();
                if (pigBehavior == null)
                    pigBehavior = gameObject.AddComponent<Demolition_PigBehavior>();
            }
        }

        void Start()
        {
            parentStructure = GetComponentInParent<Demolition_Structure>();
            if (popupTextPrefab == null)
                popupTextPrefab = Resources.Load<GameObject>("Prefabs/PopupText");
        }

        private void ConfigurePhysicsAndStats()
        {
            switch (materialType)
            {
                case MaterialType.Verre:
                    hp = 2;
                    points = 80;
                    rb.mass = 0.8f;
                    rb.linearDamping = 0.8f;
                    rb.angularDamping = 0.9f;
                    rb.gravityScale = 2.0f;
                    break;

                case MaterialType.Pierre:
                    hp = 8;
                    points = 40;
                    rb.mass = 4.0f;
                    rb.linearDamping = 1.0f;
                    rb.angularDamping = 1.2f;
                    rb.gravityScale = 2.5f;
                    break;

                case MaterialType.Cochon:
                    hp = starValue >= 3 ? 6 : (starValue == 2 ? 4 : 3);
                    points = starValue >= 3 ? 2000 : (starValue == 2 ? 1000 : 500);
                    rb.mass = 1.0f;
                    rb.linearDamping = 0.8f;
                    rb.angularDamping = 0.8f;
                    rb.gravityScale = 2.0f;
                    break;

                case MaterialType.Bois:
                default:
                    hp = 4;
                    points = 50;
                    rb.mass = 1.5f;
                    rb.linearDamping = 0.8f;
                    rb.angularDamping = 0.9f;
                    rb.gravityScale = 2.0f;
                    break;
            }
        }

        private void LoadMaterialVisuals()
        {
            if (spriteRenderer == null) return;

            if (spriteRenderer.sprite == null)
            {
                string texName = "bois";
                switch (materialType)
                {
                    case MaterialType.Verre: texName = "verre"; break;
                    case MaterialType.Pierre: texName = "pierre"; break;
                    case MaterialType.Cochon:
                        texName = starValue >= 3 ? "cochon_bleu" : (starValue == 2 ? "cochon_vert" : "cochon");
                        break;
                }
                Texture2D tex = Resources.Load<Texture2D>("Textures/" + texName);
                if (tex != null)
                {
                    spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }

            if (damageSprites == null || damageSprites.Length == 0 || damageSprites[0] == null)
            {
                damageSprites = new Sprite[2];
                Texture2D f1 = Resources.Load<Texture2D>("Textures/fissure1");
                Texture2D f2 = Resources.Load<Texture2D>("Textures/fissure2");
                if (f1 != null) damageSprites[0] = Sprite.Create(f1, new Rect(0, 0, f1.width, f1.height), new Vector2(0.5f, 0.5f));
                if (f2 != null) damageSprites[1] = Sprite.Create(f2, new Rect(0, 0, f2.width, f2.height), new Vector2(0.5f, 0.5f));
            }
        }

        public void TakeDamage(int amount, Vector2? hitDirection = null)
        {
            if (isDestroyed) return;

            hp -= amount;

            if (gameObject.activeInHierarchy)
            {
                if (squashCoroutine != null) StopCoroutine(squashCoroutine);
                squashCoroutine = StartCoroutine(AnimateSquash(hitDirection ?? Vector2.down));

                if (flashCoroutine != null) StopCoroutine(flashCoroutine);
                flashCoroutine = StartCoroutine(AnimateHitFlash());
            }

            if (pigBehavior != null)
            {
                pigBehavior.OnDamaged(hp, maxHp);
            }

            UpdateDamageSprite();
            PlayHitSound();

            if (hp <= 0)
            {
                DestroyBlock();
            }
        }

        private void UpdateDamageSprite()
        {
            if (damageSprites != null && damageSprites.Length > 0 && spriteRenderer != null && materialType != MaterialType.Cochon)
            {
                float ratio = (float)hp / maxHp;
                int index = Mathf.Clamp(damageSprites.Length - 1 - Mathf.FloorToInt(ratio * damageSprites.Length), 0, damageSprites.Length - 1);
                if (index >= 0 && index < damageSprites.Length && damageSprites[index] != null)
                {
                    spriteRenderer.sprite = damageSprites[index];
                }
            }
        }

        private void PlayHitSound()
        {
            AudioClip clip = null;
            if (materialType == MaterialType.Cochon)
            {
                clip = Resources.Load<AudioClip>("Sounds/pig_hit");
            }
            else
            {
                clip = Resources.Load<AudioClip>("Sounds/impact");
            }

            if (clip != null && Demolition_GameManager.Instance != null)
            {
                Demolition_GameManager.Instance.PlaySfx(clip, Random.Range(0.95f, 1.05f), 0.55f);
            }
        }

        private IEnumerator AnimateSquash(Vector2 direction)
        {
            float duration = 0.08f;
            float elapsed = 0f;

            Vector3 squashedScale = new Vector3(
                originalScale.x * (1f + Mathf.Abs(direction.y) * 0.1f - Mathf.Abs(direction.x) * 0.05f),
                originalScale.y * (1f + Mathf.Abs(direction.x) * 0.1f - Mathf.Abs(direction.y) * 0.05f),
                originalScale.z
            );

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.localScale = Vector3.Lerp(squashedScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        private IEnumerator AnimateHitFlash()
        {
            if (spriteRenderer == null) yield break;

            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.white * 1.3f;
            yield return new WaitForSeconds(0.03f);
            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
        }

        public void DestroyBlock()
        {
            if (isDestroyed) return;
            isDestroyed = true;

            var joints = GetComponents<FixedJoint2D>();
            foreach (var j in joints)
            {
                if (j != null) Destroy(j);
            }

            if (pigBehavior != null)
            {
                pigBehavior.OnDefeated();
            }

            Demolition_DebrisSpawner.SpawnDebris(transform.position, materialType, Random.Range(2, 4));
            Demolition_DebrisSpawner.SpawnDustCloud(transform.position, 0.4f);

            Color popupColor = Color.yellow;
            float popupScale = 1f;
            string prefix = "";
            switch (materialType)
            {
                case MaterialType.Verre:
                    popupColor = new Color(0.4f, 0.9f, 1f);
                    break;
                case MaterialType.Pierre:
                    popupColor = new Color(0.9f, 0.9f, 0.9f);
                    break;
                case MaterialType.Cochon:
                    popupColor = new Color(1f, 0.85f, 0.2f);
                    popupScale = 1.35f;
                    prefix = starValue >= 3 ? "👑 BOSS DOWN! " : (starValue == 2 ? "★ PIG CLEAR! " : "PIG HIT! ");
                    break;
                default:
                    popupColor = new Color(1f, 0.75f, 0.3f);
                    break;
            }

            if (Demolition_GameManager.Instance != null)
            {
                Demolition_GameManager.Instance.AddScore(points, transform.position, popupColor, popupScale, prefix);
            }

            if (Demolition_GameManager.Instance != null)
            {
                AudioClip destClip = materialType == MaterialType.Cochon
                    ? Resources.Load<AudioClip>("Sounds/pig_hit")
                    : Resources.Load<AudioClip>("Sounds/destruction");

                if (destClip != null)
                {
                    Demolition_GameManager.Instance.PlaySfx(destClip, Random.Range(0.95f, 1.05f), 0.65f);
                }
            }

            if (materialType == MaterialType.Cochon && Demolition_GameManager.Instance != null)
            {
                Demolition_GameManager.Instance.TriggerPigDestroyed(starValue);
            }

            if (parentStructure != null)
            {
                parentStructure.OnBlockDestroyed(this);
            }

            Destroy(gameObject);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDestroyed) return;

            if (collision.gameObject.name == "Ground" && collision.relativeVelocity.magnitude > 22f)
            {
                TakeDamage(1, collision.relativeVelocity.normalized);
            }
        }
    }
}
