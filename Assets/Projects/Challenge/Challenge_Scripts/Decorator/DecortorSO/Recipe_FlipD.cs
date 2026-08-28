using UnityEngine;

namespace Challenge
{
    [CreateAssetMenu(menuName = "Challenge/Recipes/Flip Sprite")]
    public class Recipe_FlipD : Challenge_DecoratorRecipe<Challenge_Mask>
    {
        [Header("Flip Settings")]
        public Sprite frontSprite;
        public Sprite backSprite;
        
        protected override void Apply(GameObject go, Challenge_Mask target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt)
        {
            SpriteRenderer sr = null;
            
            sr = go.GetComponent<SpriteRenderer>();
          

            if (sr == null)
            {
                Debug.LogWarning($"Recipe_FlipD: Aucun SpriteRenderer trouvé sur {go.name}");
                return;
            }

            var flipBehavior = go.AddComponent<Challenge_FlipD>();
            flipBehavior.SetTarget(target);
            flipBehavior.Setup(sr, frontSprite, backSprite);
        }
    }
}