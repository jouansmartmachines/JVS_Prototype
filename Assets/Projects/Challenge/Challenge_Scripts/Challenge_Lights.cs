using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Challenge
{
    public class Challenge_Lights : MonoBehaviour
    {
        [Header("Panel & Light Prefab")]
        public RectTransform panel;
        public GameObject lightPrefab;
        public int lightCount = 10;

        [Header("Scale Settings")]
        public Vector2 scaleRange = new Vector2(0.5f, 1.5f);

        [Header("Blink Settings")]
        [Range(1, 100)]
        public int blinkChancePerMille = 10; // 1 = 0.01%, 1000 = 10%
        public float blinkDuration = 0.5f;      
        public float blinkScaleMultiplier = 2f;

        private List<GameObject> lights = new List<GameObject>();
        private List<float> baseScales = new List<float>();
        private List<float> blinkTimers = new List<float>();
        private List<float> targetScales = new List<float>();
        private List<Image> images = new List<Image>();

        private Color[] colors = new Color[]
        {
            Color.yellow,
            new Color(1f, 0.5f, 0f),
            Color.white
        };

        void Start()
        {
            if (panel == null || lightPrefab == null)
            {
                Debug.LogWarning("Panel ou lightPrefab non assigné !");
                return;
            }

            for (int i = 0; i < lightCount; i++)
            {
                GameObject go = Instantiate(lightPrefab, panel);
                go.name = "Light_" + i;

                RectTransform rt = go.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(
                        Random.Range(-panel.rect.width / 2f, panel.rect.width / 2f),
                        Random.Range(-panel.rect.height / 2f, panel.rect.height / 2f)
                    );
                }

                float baseScale = Random.Range(scaleRange.x, scaleRange.y);
                baseScales.Add(baseScale);
                targetScales.Add(baseScale);
                go.transform.localScale = Vector3.one * baseScale;

                Image img = go.GetComponent<Image>();
                if (img != null)
                {
                    img.color = colors[Random.Range(0, colors.Length)];
                    img.enabled = false;
                    images.Add(img);
                }
                else
                {
                    images.Add(null);
                }

                blinkTimers.Add(0f);
                lights.Add(go);
            }
        }

        void Update()
        {
            // Conversion de blinkChancePerMille en probabilité réelle
            float blinkChance = blinkChancePerMille / 100000f; // 0.01% → 0.0001, 10% → 0.1

            for (int i = 0; i < lights.Count; i++)
            {
                if (lights[i] == null) continue;

                Image img = images[i];
                float baseScale = baseScales[i];

                if (blinkTimers[i] > 0)
                {
                    // Clignotement en cours
                    blinkTimers[i] -= Time.deltaTime;

                    if (img != null) img.enabled = true;
                    float progress = 1f - (blinkTimers[i] / blinkDuration);
                    targetScales[i] = Mathf.SmoothStep(baseScale, baseScale * blinkScaleMultiplier, progress);

                    if (blinkTimers[i] <= 0)
                    {
                        targetScales[i] = baseScale;
                        if (img != null) img.enabled = false;
                    }
                }
                else
                {
                    if (Random.value < blinkChance)
                    {
                        blinkTimers[i] = blinkDuration;
                    }
                }

                float currentScale = lights[i].transform.localScale.x;
                currentScale = Mathf.Lerp(currentScale, targetScales[i], Time.deltaTime * 10f);
                lights[i].transform.localScale = Vector3.one * currentScale;
            }
        }
    }
}
