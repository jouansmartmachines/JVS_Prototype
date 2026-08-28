using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Challenge
{
    public class Challenge_ExplosionD : Challenge_TargetDecorator
    {
        [Header("Explosion Settings")]
        public float force = 500f;
        public float torque = 200f;
        public float fadeDuration = 0.5f;
        public float destroyDelay = 1f;

        public GameObject[] fragments;

        [Header("Particle Settings")]
        public GameObject deathParticles;

        public Transform particleParentT;
         // 🟢 GameObject de particules à spawn
         

        public override void SetTarget(ITarget t)
        {
            base.SetTarget(t);

            if (target != null)
                target.OnDeath += OnTargetDeath;
        }

        public void SetFragments(GameObject[] fragmentObjects)
        {
            fragments = fragmentObjects;
        }

        public void SetParticles(GameObject particlesEffect,Transform particleParent)
        {
            deathParticles = particlesEffect;
            particleParentT = particleParent;
        }

        private void OnTargetDeath(ITarget t, DeathCause cause)
        {
            if(cause != DeathCause.Lifetime)
            {
                if (fragments != null)
                {
                    foreach (var fragment in fragments)
                    {
                        if (fragment == null) continue;

                        fragment.transform.position = transform.position;

                        var rb = fragment.GetComponent<Rigidbody2D>();
                        if (rb == null)
                            rb = fragment.AddComponent<Rigidbody2D>();

                        rb.gravityScale = 20f;

                        Vector2 dir = Random.insideUnitCircle.normalized;
                        rb.AddForce(dir * Random.Range(force * 6f, force), ForceMode2D.Impulse);
                        //rb.AddTorque(Random.Range(-torque, torque));

                        var sr = fragment.GetComponent<Image>();
                        if (sr != null)
                            StartCoroutine(FadeAndDestroy(sr, fragment , fadeDuration));
                        else
                            Destroy(fragment, destroyDelay);
                    }
                }

  
                if (deathParticles != null)
                {
                    GameObject particles = Instantiate(deathParticles, transform.position, Quaternion.identity,particleParentT);
                    // Optionnel : détruire automatiquement après sa durée de vie
                    ParticleSystem ps = particles.GetComponent<ParticleSystem>();
                    if (ps != null)
                        Destroy(particles, ps.main.duration + ps.main.startLifetime.constantMax);
                    else
                        Destroy(particles, 3f); 
                }
            }
            
        }

        private IEnumerator FadeAndDestroy(Image img, GameObject obj, float destroyDelay)
        {
            float fadeDelay = 1f;
            float fadeDuration = destroyDelay - fadeDelay;

            yield return new WaitForSeconds(fadeDelay);

            float elapsed = 0f;
            Color originalColor = img.color;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                img.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }

            img.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
            Destroy(obj);
        }

        private void OnDestroy()
        {
            if (target != null)
                target.OnDeath -= OnTargetDeath;
        }
    }
}
