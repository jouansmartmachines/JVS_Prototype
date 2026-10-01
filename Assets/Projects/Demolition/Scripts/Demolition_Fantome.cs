using UnityEngine;
using System.Collections;

namespace Demolition
{
    public class Demolition_Fantome : MonoBehaviour
    {
        [Header("Cinématique & Délais")]
        [Tooltip("Pause (en secondes) après le 'pouf' avant que le fantôme ne commence à monter.")]
        public float delayBeforeFly = 0.4f;

        [Tooltip("Durée de l'accélération au démarrage pour un départ fluide.")]
        public float accelerationDuration = 1f;

        [Header("Envol & Zigzag")]
        [Tooltip("Vitesse maximale d'ascension.")]
        public float maxFlySpeed = 6f;

        [Tooltip("Largeur de l'oscillation (amplitude du zigzag).")]
        public float zigzagAmplitude = 1.2f;

        [Tooltip("Vitesse de l'oscillation (fréquence du zigzag).")]
        public float zigzagFrequency = 2.5f;

        [Header("Effets")]
        public GameObject releaseEffect;
        public AudioClip releaseSound;

        private bool isFlyingAway = false;
        private Rigidbody rb;
        private Collider col;
        private Renderer meshRenderer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            meshRenderer = GetComponent<Renderer>();
        }

        private void OnEnable()
        {
            if (Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.RegisterFantome(this);
        }

        private void OnDisable()
        {
            if (Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.UnregisterFantome(this);
        }

        /// <summary>
        /// Libère le fantôme et lance la séquence cinématique d'envol.
        /// </summary>
        public void ReleaseAndFlyAway()
        {
            if (isFlyingAway) return;
            isFlyingAway = true;

            StartCoroutine(FlyRoutine());
        }

        private IEnumerator FlyRoutine()
        {
            // 1. Désactive la physique
            if (rb != null) rb.isKinematic = true;
            if (col != null) col.enabled = false;

            // 2. Désactive le mesh de la cage
            if (meshRenderer != null)
                meshRenderer.enabled = false;

            // 3. Déclenche les effets ("pouf" + son)
            if (releaseEffect != null)
                Instantiate(releaseEffect, transform.position, Quaternion.identity);

            if (releaseSound != null && Demolition_GameManager.Instance != null)
                Demolition_GameManager.Instance.PlaySfx(releaseSound);

            // 4. Pause cinématique : on laisse respirer l'effet "pouf"
            yield return new WaitForSeconds(delayBeforeFly);

            // 5. Envol progressif en zigzag avec courbe d'accélération
            Vector3 basePosition = transform.position;
            Vector3 flyDirectionRight = transform.right != Vector3.zero ? transform.right : Vector3.right;
            
            float flyTime = 0f;
            float currentSpeed = 0f;

            while (true)
            {
                flyTime += Time.deltaTime;

                // Lissage de la vitesse au démarrage (Ease-In)
                float speedPercent = Mathf.Clamp01(flyTime / accelerationDuration);
                currentSpeed = Mathf.Lerp(0f, maxFlySpeed, speedPercent * speedPercent);

                // Ascension verticale
                basePosition += Vector3.up * (currentSpeed * Time.deltaTime);

                // Mouvement latéral en zigzag (sinusoïde)
                float horizontalOffset = Mathf.Sin(flyTime * zigzagFrequency) * zigzagAmplitude;

                // Application de la position finale
                transform.position = basePosition + (flyDirectionRight * horizontalOffset);

                yield return null;
            }
        }
    }
}