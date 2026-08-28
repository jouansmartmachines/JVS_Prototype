using UnityEngine;


namespace Challenge
{
    /*
    [CreateAssetMenu(menuName = "Challenge/Recipes/Grid Release")]
    public class Recipe_GridRelease : Challenge_DecoratorRecipe {
        protected override void Apply(GameObject go, Challenge_Target target, Challenge_TargetCategoryData data, Challenge_SpawnManager manager, RectTransform rt) {
            var d = go.AddComponent<Challenge_GridReleaseDecorator>();
            d.SetTarget(target);
            manager.activeTargets++; // Accès via SpawnManager
            d.Initialize(manager.gridManager, rt);
            d.OnCellReleased += _ => {
                manager.activeTargets--;
                manager.TrySpawnTarget();
            };
        }
    }
    */
}
