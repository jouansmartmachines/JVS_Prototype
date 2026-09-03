using UnityEngine;
using System.Collections;

namespace Demolition
{
    public class Demolition_Fantome : MonoBehaviour
    {
        [Header("Santé")]
        public float hp = 100f;
        public float maxHp = 100f;

        [Header("Dégâts")]
        public float damageMultiplier = 1f;
        public float minForceToDamage = 3f;

        [Header("Effets")]
        public GameObject destructionEffect;
        public AudioClip destructionSound;
        public float fadeDuration = 1.5f;

        private bool isDead = false;
        private Rigidbody rb;
        private Collider col;
        private Renderer[] renderers;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            renderers = GetComponentsInChildren<Renderer>();
            maxHp = hp;
        }

        void OnCollisionEnter(Collision collision)
        {
            if (isDead) return;

            float impactForce = collision.impulse.magnitude;
            if (impactForce < minForceToDamage) return;

            float damage = impactForce * damageMultiplier;
            hp -= damage;

            StartCoroutine(FlashDamage());

            if (hp <= 0)
                Die();
        }

        private IEnumerator FlashDamage()
        {
            foreach (var rend in renderers)
            {
                if (rend != null)
                    rend.material.color = Color.red;
            }
            yield return new WaitForSeconds(0.1f);
            foreach (var rend in renderers)
            {
                if (rend != null)
                    rend.material.color = Color.white;
            }
        }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            if (destructionEffect != null)
                Instantiate(destructionEffect, transform.position, Quaternion.identity);

            if (destructionSound != null && Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.PlaySfx(destructionSound);

            if (rb != null) rb.isKinematic = true;
            if (col != null) col.enabled = false;

            if (Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.OnFantomeKilled();
        }

        public float GetHealthRatio()
        {
            return Mathf.Clamp01(hp / maxHp);
        }
    }
}