using UnityEngine;

namespace Challenge
{
    [CreateAssetMenu(menuName = "Challenge/Recipes/Mask Logic")]
    public class Recipe_MaskLogic : Challenge_DecoratorRecipe
    {
        public override void Execute(GameObject go, Challenge_BaseInteractive target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt)
        {
            if (!CanApply(data)) return;

            go.AddComponent<Challenge_MaskD>();
        
        }
    }
}