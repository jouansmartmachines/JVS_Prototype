using UnityEngine;

namespace Challenge
{
    public abstract class Challenge_DecoratorRecipe : ScriptableObject 
    {
        public TargetCategory[] targetCategories;

        // La méthode universelle
        public abstract void Execute(GameObject go, Challenge_BaseInteractive target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt);

        protected bool CanApply(TargetCategory cat) => 
            targetCategories == null || targetCategories.Length == 0 || System.Array.Exists(targetCategories, c => c == cat);
    }
    public abstract class Challenge_DecoratorRecipe<T> : Challenge_DecoratorRecipe where T : Challenge_BaseInteractive 
    {
        public override void Execute(GameObject go, Challenge_BaseInteractive target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt)
        {
            // Le "is T typed" fait le tri : si c'est un Mask alors que la recette attend un Target, ça s'arrête là.
            if (CanApply(data) && target is T typedTarget)
            {
                Apply(go, typedTarget, data, manager, rt);
            }
        }

        protected abstract void Apply(GameObject go, T target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt);
    }
}