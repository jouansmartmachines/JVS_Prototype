using Tool;
using UnityEngine;

namespace Theme
{
    public class SwapSpriteRendererBehaviour : SwapObjectBehaviour
    {
        [SerializeField] SpriteRenderer spriteRenderer;
        [SerializeField] Collider2D targetCollider;

        protected override void Swap(GameTheme theme)
        {
            var entity = _swapObject.GetSwapEntity(theme) as SwapSprite;
            if (entity == null) return;

            if (entity.Sprites.Count > 0) spriteRenderer.sprite = entity.Sprites.RandomElement();
            if (entity.UseColor) spriteRenderer.color = entity.Color;

           if (targetCollider != null && targetCollider is PolygonCollider2D poly)
            {
                Sprite newSprite = spriteRenderer.sprite;

                if (newSprite != null)
                {

                    poly.pathCount = 0;
                    int shapeCount = newSprite.GetPhysicsShapeCount();
                    System.Collections.Generic.List<Vector2> pathPoints = new System.Collections.Generic.List<Vector2>();

                    for (int i = 0; i < shapeCount; i++)
                    {
                        pathPoints.Clear();

                        newSprite.GetPhysicsShape(i, pathPoints);
                        poly.pathCount++;
                        poly.SetPath(poly.pathCount - 1, pathPoints.ToArray());
                    }
                }
            }
                 
        }
    }
}