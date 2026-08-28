using System.Collections;
using UnityEngine;

namespace Challenge
{
    public class Challenge_FlashHitD : MonoBehaviour
    {
        private Renderer targetRenderer;
        private Color hitColor;
        private float shakeRotationIntensity;

        private Color originalColor;
        private Quaternion originalRotation;
        private MaterialPropertyBlock _propBlock;
        private Coroutine flashCoroutine;
        private Challenge_BaseInteractive baseTarget;

        public void Initialize(Challenge_BaseInteractive target, Renderer renderer, Color color, float intensity)
        {
            baseTarget = target;
            targetRenderer = renderer;
            hitColor = color;
            shakeRotationIntensity = intensity;
            _propBlock = new MaterialPropertyBlock();

            // Sauvegarde de la couleur originale
            if (targetRenderer is SpriteRenderer sprite)
                originalColor = sprite.color;
            else
                originalColor = targetRenderer.sharedMaterial.color;

            // Abonnement à l'event de hit universel
            if (baseTarget != null)
                baseTarget.OnHitEvent += HandleHit;
        }

        private void OnDestroy()
        {
            if (baseTarget != null) baseTarget.OnHitEvent -= HandleHit;
        }

        private void HandleHit(ITarget source) 
        {
            if (targetRenderer == null) return;
            
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                targetRenderer.transform.localRotation = originalRotation;
            }
            else
            {
                originalRotation = targetRenderer.transform.localRotation;
            }

            flashCoroutine = StartCoroutine(FlashAndRotateYRoutine(0.5f, 0.05f, 0.15f));
        }

        private IEnumerator FlashAndRotateYRoutine(float duration, float initialInterval, float finalInterval)
        {
            float elapsed = 0f;
            bool toggle = false;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float interval = Mathf.Lerp(initialInterval, finalInterval, t);
                float halfInterval = interval / 2f;

                Color start = toggle ? originalColor : hitColor;
                Color end = toggle ? hitColor : originalColor;
                toggle = !toggle;

                float fade = 0f;
                while (fade < halfInterval)
                {
                    ApplyColor(Color.Lerp(start, end, fade / halfInterval));

                    float currentIntensity = Mathf.Lerp(shakeRotationIntensity, 0, t);
                    float randomY = Random.Range(-currentIntensity, currentIntensity);
                    targetRenderer.transform.localRotation = originalRotation * Quaternion.Euler(0, randomY, 0);

                    fade += Time.deltaTime;
                    yield return null;
                }
                elapsed += interval;
            }

            ApplyColor(originalColor);
            targetRenderer.transform.localRotation = originalRotation;
            flashCoroutine = null;
        }

        private void ApplyColor(Color color)
        {
            if (targetRenderer is SpriteRenderer sprite)
            {
                sprite.color = color;
            }
            else if (targetRenderer != null)
            {
                targetRenderer.GetPropertyBlock(_propBlock);
                _propBlock.SetColor("_Color", color);
                targetRenderer.SetPropertyBlock(_propBlock);
            }
        }
    }
}