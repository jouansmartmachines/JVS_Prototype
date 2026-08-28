using UnityEngine;


namespace Challenge
{

[CreateAssetMenu(menuName = "Challenge/Recipes/Score")]
    public class Recipe_Score : Challenge_DecoratorRecipe<Challenge_Target> {
        protected override void Apply(GameObject go, Challenge_Target target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt) {
            var d = go.AddComponent<Challenge_ScoreD>();
            d.SetTarget(target);
            d.scoreManager = manager.scoreManager;
            d.points = manager.LevelSettings.points;
            d.multiplier = target.scoreMultiplier;
            d.secondPopup = target.popUp;
            
            manager.scoreManager.Subscribe(target);
        }
    }

}
