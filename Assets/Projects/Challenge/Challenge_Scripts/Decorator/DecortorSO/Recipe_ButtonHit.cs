using UnityEngine;

namespace Challenge
{
    [CreateAssetMenu(menuName = "Challenge/Recipes/Button Hit (")]
    // On hérite de la base non-générique pour être "neutre"
    public class Recipe_ButtonHit : Challenge_DecoratorRecipe 
    {
        // On override la méthode Execute universelle
        public override void Execute(GameObject go, Challenge_BaseInteractive target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt)
        {
           
            if (!CanApply(data)) return;
            var d = go.AddComponent<ButtonHitDecorator>(); 
        
            d.SetTarget(target);
        }
    }
}