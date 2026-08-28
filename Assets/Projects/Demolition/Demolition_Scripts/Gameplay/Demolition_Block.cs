using UnityEngine;
using System.Collections;

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
        public Sprite[] damageSprites;
        public SpriteRenderer spriteRenderer;

        [Header("Débris")]
        public GameObject debrisPrefab;
        public float debrisForce = 200f;

        private Rigidbody2D rb;
        private int maxHp;
        private bool isDestroyed = false;
        private Demolition_Structure parentStructure;

        public enum MaterialType { Bois, Verre, Pierre, Cochon }
    
        [Header("Cible spéciale")]
        public bool isTarget = false;
        public int starValue = 0;
        public GameObject popupTextPrefab;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
                rb = gameObject.AddComponent<Rigidbody2D>();

            // Gravite active pour que les blocs tombent
            rb.gravityScale = 3f;
            rb.mass = 1f;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;

            maxHp = hp;
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite == null)
            {
                string texName = "bois";
                switch (materialType)
                {
                    case MaterialType.Verre: texName = "verre"; break;
                    case MaterialType.Pierre: texName = "pierre"; break;
                    case MaterialType.Cochon: texName = "cochon"; break;
                }
                Texture2D tex = Resources.Load<Texture2D>("Textures/" + texName);
                if (tex != null)
                    spriteRenderer.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
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

        void Start()
        {
            parentStructure = GetComponentInParent<Demolition_Structure>();

            // Pas de joint au parent - c'est le StructureBuilder qui connecte
            // les blocs entre eux (chaque bloc a son voisin du dessous)
        }

        public void TakeDamage(int amount)
        {
            if (isDestroyed) return;
            hp -= amount;

            if (damageSprites.Length > 0 && spriteRenderer != null)
            {
                float ratio = (float)hp / maxHp;
                int index = Mathf.Clamp(damageSprites.Length - 1 - 
                    Mathf.FloorToInt(ratio * damageSprites.Length), 0, damageSprites.Length - 1);
                if (index < damageSprites.Length)
                    spriteRenderer.sprite = damageSprites[index];
            }

            // Son cochon
            if (materialType == MaterialType.Cochon)
            {
                AudioSource aud = GetComponent<AudioSource>();
                if (aud != null)
                {
                    AudioClip pigHit = Resources.Load<AudioClip>("Sounds/pig_hit");
                    if (pigHit != null) aud.PlayOneShot(pigHit);
                }
            }

            if (hp <= 0)
                Destroy();
        }

        void Destroy()
        {
            if (isDestroyed) return;
            isDestroyed = true;

            // Casser nos joints (blocs relies a nous)
            var joints = GetComponents<FixedJoint2D>();
            foreach (var j in joints) Destroy(j);

            // Debris physiques
            Demolition_DebrisSpawner.SpawnDebris(transform.position, materialType, Random.Range(4, 8));

            // Score
            if (Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.AddScore(points, transform.position);

            // Popup flottant
            if (popupTextPrefab != null)
            {
                GameObject popup = Instantiate(popupTextPrefab, transform.position, Quaternion.identity);
                var popupText = popup.GetComponent<Demolition_PopupText>();
                if (popupText != null)
                    popupText.SetText("+" + points);
            }

            // Son
            AudioSource audio = GetComponent<AudioSource>();
            if (audio != null)
            {
                if (materialType == MaterialType.Cochon)
                {
                    audio.PlayOneShot(Resources.Load<AudioClip>("Sounds/pig_hit"));
                }
                else
                {
                    AudioClip clip = Resources.Load<AudioClip>("Sounds/destruction");
                    if (clip != null) audio.PlayOneShot(clip);
                }
            }

            // Secousse + ralenti si c'est un cochon
            if (materialType == MaterialType.Cochon && Demolition_GameManager.Instance != null)
            {
                Demolition_GameManager.Instance.StartCoroutine(
                    Demolition_GameManager.Instance.BigShake());
            }

            // Si la structure parente existe, elle peut declencher le slow-mo
            if (parentStructure != null)
                parentStructure.OnBlockDestroyed(this);

            Destroy(gameObject);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            float force = collision.relativeVelocity.magnitude;
            if (force > 3f)
            {
                TakeDamage(Mathf.FloorToInt(force / 3f));
            }
        }

        public float GetBreakForce()
        {
            switch (materialType)
            {
                case MaterialType.Verre: return 50f;
                case MaterialType.Bois: return 200f;
                case MaterialType.Pierre: return 500f;
                case MaterialType.Cochon: return 800f;
                default: return 200f;
            }
        }
    }
}