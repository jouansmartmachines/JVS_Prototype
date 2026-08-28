using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace Challenge
{
    public class Challenge_RadialFillD : Challenge_TargetDecorator
    {
        private Image radialImage;
        private Coroutine fillCoroutine;

        private List<Image> allImages = new List<Image>();
        private List<float> targetImageAlphas = new List<float>();
        private List<TextMeshProUGUI> allTexts = new List<TextMeshProUGUI>();
        private List<float> targetTextAlphas = new List<float>();

        // Modifié à 0.7f pour attendre 70% de la durée avant de fade
        [Range(0f, 1f)]
        public float fadeOutStartThreshold = 0.7f; 

        public void Initialize(Image mainRadialImage)
        {
            this.radialImage = mainRadialImage;
            CacheAndResetAlphas();
        }

        private void CacheAndResetAlphas()
        {
            allImages.Clear();
            targetImageAlphas.Clear();
            foreach (Image img in GetComponentsInChildren<Image>(true))
            {
                if (img == radialImage || img.name == "End") continue;

                allImages.Add(img);
                targetImageAlphas.Add(img.color.a);
            }

            allTexts.Clear();
            targetTextAlphas.Clear();
            foreach (TextMeshProUGUI tmp in GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                allTexts.Add(tmp);
                targetTextAlphas.Add(tmp.color.a);
            }

            if (radialImage != null) radialImage.fillAmount = 0f;
        }

        public void StartFill(float duration)
        {
            if (fillCoroutine != null) StopCoroutine(fillCoroutine);
            fillCoroutine = StartCoroutine(RadialFillAndFadeOutRoutine(duration));
        }

        private IEnumerator RadialFillAndFadeOutRoutine(float duration)
        {
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                // t va de 0.0 (début) à 1.0 (fin de la durée)
                float t = Mathf.Clamp01(elapsed / duration);

                // 1. GESTION DU RADIAL FILL (0 -> 1)
                if (radialImage != null)
                {
                    radialImage.fillAmount = t;
                }

                // 2. GESTION DU FADE (Commence à 0.7)
                // InverseLerp renverra 0 tant que t < 0.7, puis montera jusqu'à 1 quand t atteindra 1.0
                float fadeProgress = Mathf.InverseLerp(fadeOutStartThreshold, 1f, t);

                // On applique l'alpha
                ApplyAlpha(fadeProgress);

                yield return null;
            }

            FinalizeEffect();
        }

        private void ApplyAlpha(float fadeProgress)
        {
            // Images
            for (int i = 0; i < allImages.Count; i++)
            {
                if (allImages[i] == null) continue;
                Color c = allImages[i].color;
                // On lerp de l'alpha initial vers 0 selon la progression du fade (après les 70%)
                c.a = Mathf.Lerp(targetImageAlphas[i], 0f, fadeProgress);
                allImages[i].color = c;
            }

            // Texts
            for (int i = 0; i < allTexts.Count; i++)
            {
                if (allTexts[i] == null) continue;
                Color c = allTexts[i].color;
                c.a = Mathf.Lerp(targetTextAlphas[i], 0f, fadeProgress);
                allTexts[i].color = c;
            }
        }

        private void FinalizeEffect()
        {
            radialImage.fillAmount = 1f;
    
            ApplyAlpha(1f);
            if (target is Challenge_Target targetComponent)
            {
                targetComponent.Die(DeathCause.Lifetime,0f);
            }
        }

        private void OnDisable()
        {
            if (fillCoroutine != null) StopCoroutine(fillCoroutine);
        }
    }
}