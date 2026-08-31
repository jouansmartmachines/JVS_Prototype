using UnityEngine;
using TMPro;
using System.Collections;

namespace Demolition
{
    /// <summary>
    /// Popup de score flottant avec punch scale (overshoot bounce) et fondu dynamique.
    /// </summary>
    public class Demolition_PopupText : MonoBehaviour
    {
        public float floatSpeed = 2f;
        public float duration = 0.8f;
        private TextMeshPro textMesh;
        private Color baseColor = Color.yellow;
        private Vector3 baseScale = Vector3.one;

        void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();
            if (textMesh == null)
                textMesh = gameObject.AddComponent<TextMeshPro>();

            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.fontSize = 4f;
            textMesh.color = baseColor;
            textMesh.fontStyle = FontStyles.Bold;
            textMesh.sortingOrder = 10;
        }

        public void SetText(string txt, Color? customColor = null, float scaleMultiplier = 1f)
        {
            if (textMesh == null)
                textMesh = GetComponent<TextMeshPro>();

            textMesh.text = txt;
            if (customColor.HasValue)
                baseColor = customColor.Value;

            textMesh.color = baseColor;
            baseScale = Vector3.one * scaleMultiplier;
            transform.localScale = Vector3.zero;

            StartCoroutine(AnimatePopup());
        }

        private IEnumerator AnimatePopup()
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            // Légère déviation horizontale aléatoire
            float xOffset = Random.Range(-0.3f, 0.3f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                // 1. Position : monte doucement avec courbe amortie
                transform.position = startPos + new Vector3(xOffset * t, t * floatSpeed, 0);

                // 2. Échelle : Overshoot punch (grossit vite à 1.3x puis revient à 1x)
                float scaleT;
                if (t < 0.2f)
                {
                    scaleT = Mathf.Lerp(0f, 1.3f, t / 0.2f);
                }
                else if (t < 0.4f)
                {
                    scaleT = Mathf.Lerp(1.3f, 1f, (t - 0.2f) / 0.2f);
                }
                else
                {
                    scaleT = 1f;
                }
                transform.localScale = baseScale * scaleT;

                // 3. Couleur & Opacité : reste visible puis fondu rapide sur la fin
                Color c = baseColor;
                if (t > 0.6f)
                {
                    c.a = Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
                }
                textMesh.color = c;

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
