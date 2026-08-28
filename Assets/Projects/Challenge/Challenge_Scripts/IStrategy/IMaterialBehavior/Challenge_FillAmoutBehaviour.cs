using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

namespace Challenge
{
    public class Challenge_FillAmountBehavior : MonoBehaviour, Challenge_IMaterialBehavior
    {
        private Material targetMaterial;
        private ITarget target;

        private float fillAmount = 1f;
        private Coroutine fillCoroutine;

        private List<Image> images = new List<Image>();
        private List<float> startImageAlphas = new List<float>();

        private List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
        private List<float> startTextAlphas = new List<float>();

        /// <summary>
        /// Initialise le script avec le matériel et la cible
        /// </summary>
        public void Initialize(Material material, ITarget target)
        {
            this.targetMaterial = material;
            this.target = target;

            fillAmount = 1f;
            CacheImagesAndTexts();
            ApplyFill();
        }

        /// <summary>
        /// Change la couleur du matériau cible
        /// </summary>
        public void SetColor(Color color)
        {
            if (targetMaterial != null)
                targetMaterial.SetColor("_Color", color);
        }

        /// <summary>
        /// Récupère toutes les Images et TextMeshProUGUI enfants
        /// </summary>
        private void CacheImagesAndTexts()
        {
            images.Clear();
            startImageAlphas.Clear();
            foreach (Image img in GetComponentsInChildren<Image>(true))
            {
                images.Add(img);
                startImageAlphas.Add(img.color.a);
            }


            texts.Clear();
            startTextAlphas.Clear();
            foreach (TextMeshProUGUI tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                texts.Add(tmp);
                startTextAlphas.Add(tmp.color.a);
            }
        }

        /// <summary>
        /// Lance la progression du fill sur une durée
        /// </summary>
        public void StartFillOverTime(float duration)
        {
            if (fillCoroutine != null)
                StopCoroutine(fillCoroutine);
            

            fillCoroutine = StartCoroutine(FillAndFadeRoutine(duration));
        }

        /// <summary>
        /// Coroutine principale de fill et fade
        /// </summary>
        private IEnumerator FillAndFadeRoutine(float duration)
        {
            float elapsed = 0f;

            float fillStartValue = 0.8f;
            float fillEndValue = 0.2f;

            float fadeStartRatio = 0.8f; 
            float soundDelay = 0.1f; 
            bool fadeSoundPlayed = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                // ---------- FILL ----------
                fillAmount = Mathf.Lerp(fillStartValue, fillEndValue, t);
                ApplyFill();

                // ---------- FADE ----------
                if (t >= fadeStartRatio)
                {
                    float fadeT = Mathf.InverseLerp(fadeStartRatio, 1f, t);
                    if (!fadeSoundPlayed && t >= fadeStartRatio + soundDelay )
                    {
                        Challenge_AudioManager.i.PlayOneShot(SoundType.Ephemere); 
                        fadeSoundPlayed = true;
                    }

                    // Images
                    for (int i = 0; i < images.Count; i++)
                    {
                        if (images[i] == null) continue;
                        Color c = images[i].color;
                        c.a = Mathf.Lerp(startImageAlphas[i], 0f, fadeT);
                        images[i].color = c;
                    }

                    // Texts
                    for (int i = 0; i < texts.Count; i++)
                    {
                        if (texts[i] == null) continue;
                        Color c = texts[i].color;
                        c.a = Mathf.Lerp(startTextAlphas[i], 0f, fadeT);
                        texts[i].color = c;
                    }
                }



                yield return null;
            }

            // Sécurité finale
            fillAmount = fillEndValue;
            ApplyFill();

            for (int i = 0; i < images.Count; i++)
            {
                if (images[i] == null) continue;
                Color c = images[i].color;
                c.a = 0f;
                images[i].color = c;
            }

            for (int i = 0; i < texts.Count; i++)
            {
                if (texts[i] == null) continue;
                Color c = texts[i].color;
                c.a = 0f;
                texts[i].color = c;
            }

            if (target is Challenge_Target targetComponent)
            {
                targetComponent.Die(DeathCause.Lifetime,0f);
            }
        }

        /// <summary>
        /// Applique le fill au matériau cible
        /// </summary>
        private void ApplyFill()
        {
            if (targetMaterial != null)
                targetMaterial.SetFloat("_ClipUvUp", fillAmount);
        }

        /// <summary>
        /// Applique immédiatement le fill
        /// </summary>
        public void Activate() => ApplyFill();

        /// <summary>
        /// Stoppe la coroutine si elle est en cours
        /// </summary>
        public void Stop()
        {
            if (fillCoroutine != null)
                StopCoroutine(fillCoroutine);
        }

    }
}
