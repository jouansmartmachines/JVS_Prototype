using UnityEngine;
using TMPro;

namespace Demolition
{
    public class Demolition_PopupText : MonoBehaviour
    {
        public float floatSpeed = 1.5f;
        public float fadeDuration = 1.0f;
        private TextMeshPro textMesh;
        private Color startColor;

        void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();
            if (textMesh == null)
                textMesh = gameObject.AddComponent<TextMeshPro>();

            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.fontSize = 3;
            textMesh.color = Color.yellow;
            textMesh.fontStyle = FontStyles.Bold;
            startColor = textMesh.color;

            // S'affiche devant tout
            textMesh.sortingOrder = 10;
        }

        public void SetText(string txt)
        {
            if (textMesh != null)
                textMesh.text = txt;
        }

        void Update()
        {
            // Flotte vers le haut
            transform.position += Vector3.up * floatSpeed * Time.deltaTime;

            // Disparaît progressivement
            startColor.a -= Time.deltaTime / fadeDuration;
            textMesh.color = startColor;

            if (startColor.a <= 0)
                Destroy(gameObject);
        }
    }
}
