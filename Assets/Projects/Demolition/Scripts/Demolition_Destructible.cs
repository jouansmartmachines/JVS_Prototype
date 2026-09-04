using UnityEngine;
using System.Collections;
using IndieKit; // Assurez-vous d'importer le namespace de DestructibleObject si nécessaire

namespace Demolition
{
    public class Demolition_Destructible : MonoBehaviour, IDamageable
    {
        [Header("Santé & Score")]
        [SerializeField] private float health = 3f;
        public int points = 100;

        [Header("Seuil de Dégâts (Physique)")]
        public float minForceToDamage = 5f;

        [Header("Effets")]
        [SerializeField] private GameObject DebrisPrefab;
        public GameObject destructionEffect;
        public AudioClip breakSound;

        private bool isDestroyed = false;
        private Rigidbody rb;
        private Renderer rend;
        private Color originalColor;
        private float maxHealth;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rend = GetComponent<Renderer>();
            maxHealth = health;
            if (rend != null)
                originalColor = rend.material.color;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (isDestroyed) return;

            float force = collision.impulse.magnitude;
            if (force < minForceToDamage) return;

            // Récupère le point de contact précis pour l'explosion des débris
            Vector3 hitPoint = collision.contacts.Length > 0 ? collision.contacts[0].point : transform.position;
            
            // Applique 1 point de dégât (ou adaptez selon la force)
            ApplyDamage(1f, hitPoint);
        }

        // Implémentation de l'interface IDamageable (provenant de DestructibleObject)
        public void ApplyDamage(float damage, Vector3 hitPoint)
        {
            if (isDestroyed) return;

            health -= damage;

            if (rend != null)
                StartCoroutine(FlashRoutine());

            if (health <= 0f)
            {
                DestroySelf(hitPoint);
            }
        }

        private IEnumerator FlashRoutine()
        {
            if (rend != null)
            {
                rend.material.color = Color.red;
                yield return new WaitForSeconds(0.08f);
                rend.material.color = Color.Lerp(originalColor, Color.white, 1f - (health / maxHealth));
            }
        }

        private void DestroySelf(Vector3 hitPoint)
        {
            if (isDestroyed) return;
            isDestroyed = true;

            // 1. Instanciation des débris façon DestructibleObject s'ils existent
            if (DebrisPrefab != null)
            {
                GameObject debris = Instantiate(DebrisPrefab, transform.position, transform.rotation);
                debris.transform.localScale = transform.localScale;

                for (int i = 0; i < debris.transform.childCount; i++)
                {
                    Transform child = debris.transform.GetChild(i);
                    if (child.TryGetComponent<Rigidbody>(out Rigidbody childRb))
                    {
                        childRb.AddExplosionForce(4f, hitPoint, 1.5f, 0f, ForceMode.Impulse);
                    }
                }
            }
            // 2. Sinon, effet de particules classique de démolition
            else if (destructionEffect != null)
            {
                Instantiate(destructionEffect, transform.position, Quaternion.identity);
            }

            // Gestion du son et du score via votre GameManager
            if (breakSound != null && Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.PlaySfx(breakSound);

            if (Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.AddScore(points, transform.position);

            Destroy(gameObject);
        }
    }
}