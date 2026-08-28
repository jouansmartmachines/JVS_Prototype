using UnityEngine;

namespace Challenge
{
    [CreateAssetMenu(menuName = "Challenge/Recipes/Explosion")]
    public class Recipe_Explosion : Challenge_DecoratorRecipe<Challenge_Target> {
        protected override void Apply(GameObject go, Challenge_Target target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt) {
            var d = go.AddComponent<Challenge_ExplosionD>();
            d.SetTarget(target);
            d.SetFragments(target.particlesImages);
            
            d.SetParticles(target.particlesEffects, manager.particleParent.transform);
        
        }
    } 
}
