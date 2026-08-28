using UnityEngine;

namespace Challenge
{
    public class Challenge_FlipD : Challenge_TargetDecorator
    {
        private SpriteRenderer spriteRenderer;
        private Sprite frontSprite;
        private Sprite backSprite;

        public void Setup(SpriteRenderer sr, Sprite front, Sprite back)
        {
            spriteRenderer = sr;
            frontSprite = front;
            backSprite = back;
            
            // On s'assure que le front est appliqué au départ
            if (spriteRenderer != null && frontSprite != null)
                spriteRenderer.sprite = frontSprite;
        }

        private void Update()
        {
            CheckRotation();
        }

        private void CheckRotation()
        {
            // On récupère la rotation Y locale
            float angle = transform.localEulerAngles.y;

            // Logique de bascule entre 90° et 270°
            if (angle > 90f && angle < 270f)
            {
                if (spriteRenderer.sprite != backSprite)
                    spriteRenderer.sprite = backSprite;
            }
            else
            {
                if (spriteRenderer.sprite != frontSprite)
                    spriteRenderer.sprite = frontSprite;
            }
        }
    }
}