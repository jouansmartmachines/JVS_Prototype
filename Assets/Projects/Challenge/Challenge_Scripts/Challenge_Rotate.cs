using UnityEngine;
using TMPro;

namespace Challenge
{
    public class Challenge_Rotate : MonoBehaviour
    {
        [Header("Rotation")]
        public Vector2 offset = Vector2.zero;
        public float radius = 100f;
        public float speed = 1f;
        [Tooltip("Décalage de phase pour la taille en degrés")]
        public float startAngleDeg = 0f;

        [Header("Text Size")]
        public float minSize = 20f;
        public float maxSize = 40f;

        [Header("Hue Effect")]
        [Tooltip("Vitesse à laquelle le texte parcourt la roue des teintes")]
        public float hueSpeed = 0.2f; // vitesse du changement de teinte

        private RectTransform rectTransform;
        private TextMeshProUGUI tmpText;
        private Vector2 center;
        private float rotationAngle;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            tmpText = GetComponent<TextMeshProUGUI>();

            center = rectTransform.anchoredPosition + offset;
        }

        void Update()
        {
            // --- Rotation ---
            rotationAngle += speed * Time.deltaTime;
            float x = Mathf.Cos(rotationAngle) * radius;
            float y = Mathf.Sin(rotationAngle) * radius;
            rectTransform.anchoredPosition = center + new Vector2(x, y);

            // --- Taille du texte ---
            float startAngleRad = startAngleDeg * Mathf.Deg2Rad;
            float t = (Mathf.Sin(rotationAngle + startAngleRad) + 1f) * 0.5f;
            tmpText.fontSize = Mathf.Lerp(minSize, maxSize, t);

            // --- Parcours de la roue des teintes pour tout le texte ---
            float hue = (Time.time * hueSpeed) % 1f;
            Color c = Color.HSVToRGB(hue, 1f, 1f);
            tmpText.color = c;
        }
    }
}
