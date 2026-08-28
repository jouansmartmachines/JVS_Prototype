using UnityEngine;


namespace Challenge
{

    [CreateAssetMenu(menuName = "Challenge/Recipes/Lifetime")]
    public class Recipe_Lifetime : Challenge_DecoratorRecipe<Challenge_Target> {
        protected override void Apply(GameObject go, Challenge_Target target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt) {
            var d = go.AddComponent<Challenge_LifetimeD>();
            d.SetTarget(target);
            d.lifetime = Challenge_GeneralVariables.GetEphemereTimeFromPrefs();
        }
    }

}
