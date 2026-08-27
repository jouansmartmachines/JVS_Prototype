using UnityEngine;

namespace Demolition
{
    public class Demolition_Block : MonoBehaviour
    {
        [Header("Santé")]
        public int hp = 1;
        public int points = 50;

        [Header("Matériau")]
        public MaterialType materialType = MaterialType.Bois;

        [Header("Apparence")]
        public Sprite[] damageSprites;  // 0 = intact, 1 = fissuré, 2 = très fissuré
        public SpriteRenderer spriteRenderer;

        [Header("Débris")]
        public GameObject debrisPrefab;
        public float debrisForce = 200f;

        private Rigidbody2D rb;
        private int maxHp;
        private bool isDestroyed = false;

        public enum MaterialType { Bois, Verre, Pierre }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody2D>();

            maxHp = hp;
        }

        void Start()
        {
            // Lier au parent structure
            FixedJoint2D joint = GetComponent<FixedJoint2D>();
            if (joint == null && transform.parent != null)
            {
                joint = gameObject.AddComponent<FixedJoint2D>();
                joint.connectedBody = transform.parent.GetComponent<Rigidbody2D>();
            }
        }

        public void TakeDamage(int amount)
        {
            if (isDestroyed) return;
            hp -= amount;

            // Mise à jour visuelle (fissures)
            if (damageSprites.Length > 0 && spriteRenderer != null)
            {
                float ratio = (float)hp / maxHp;
                int index = Mathf.Clamp(damageSprites.Length - 1 - 
                    Mathf.FloorToInt(ratio * damageSprites.Length), 0, damageSprites.Length - 1);
                if (index < damageSprites.Length)
                    spriteRenderer.sprite = damageSprites[index];
            }

            if (hp <= 0)
                Destroy();
        }

        void Destroy()
        {
            if (isDestroyed) return;
            isDestroyed = true;

            // Particules de débris
            if (debrisPrefab != null)
            {
                GameObject debris = Instantiate(debrisPrefab, transform.position, Quaternion.identity);
                foreach (Rigidbody2D part in debris.GetComponentsInChildren<Rigidbody2D>())
                {
                    part.AddForce(Random.insideUnitCircle * debrisForce, ForceMode2D.Impulse);
                }
                Destroy(debris, 3f);
            }

            // Score
            if (Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.AddScore(points, transform.position);

            // Son
            AudioSource audio = GetComponent<AudioSource>();
            if (audio != null) audio.Play();

            Destroy(gameObject);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            // Dégâts par chute/impact
            float force = collision.relativeVelocity.magnitude;
            if (force > 3f)
            {
                TakeDamage(Mathf.FloorToInt(force / 3f));
            }
        }

        // Force du joint qui se brise
        public float GetBreakForce()
        {
            switch (materialType)
            {
                case MaterialType.Verre: return 50f;
                case MaterialType.Bois: return 200f;
                case MaterialType.Pierre: return 500f;
                default: return 200f;
            }
        }
    }
}