using UnityEngine;
using System.Collections;

namespace Demolition
{
    public class Demolition_Destructible : MonoBehaviour
    {
        [Header("Santé")]
        public int hp = 3;
        public int points = 100;

        [Header("Seuil")]
        public float minForceToDamage = 5f;

        [Header("Effets")]
        public GameObject destructionEffect;
        public AudioClip breakSound;

        private bool isDestroyed = false;
        private Rigidbody rb;
        private Renderer rend;
        private Color originalColor;
        private int maxHp;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rend = GetComponent<Renderer>();
            maxHp = hp;
            if (rend != null)
                originalColor = rend.material.color;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (isDestroyed) return;

            float force = collision.impulse.magnitude;
            if (force < minForceToDamage) return;

            hp--;

            if (rend != null)
                StartCoroutine(FlashRoutine());

            if (hp <= 0)
                DestroySelf();
        }

        private IEnumerator FlashRoutine()
        {
            if (rend != null)
            {
                rend.material.color = Color.red;
                yield return new WaitForSeconds(0.08f);
                rend.material.color = Color.Lerp(originalColor, Color.white, 1f - (float)hp / maxHp);
            }
        }

        /// <summary>
        /// Applique des dégâts programmatiquement (appelé par GameManager).
        /// </summary>
        public void ApplyDamage(int damage)
        {
            if (isDestroyed) return;
            hp -= damage;
            if (rend != null)
                StartCoroutine(FlashRoutine());
            if (hp <= 0)
                DestroySelf();
        }

        private void DestroySelf()
        {
            if (isDestroyed) return;
            isDestroyed = true;

            if (destructionEffect != null)
                Instantiate(destructionEffect, transform.position, Quaternion.identity);

            if (breakSound != null && Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.PlaySfx(breakSound);

            if (Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.AddScore(points, transform.position);

            Destroy(gameObject);
        }
    }
}