// Recipe_FillMaterial.cs
using UnityEngine;
using UnityEngine.UI;

namespace Challenge
{
    [CreateAssetMenu(menuName = "Challenge/Recipes/Fill Material (Ephemere-Malus)")]
    public class Recipe_FillMaterial : Challenge_DecoratorRecipe<Challenge_Target>
    {
        public string materialKey = "FillMaterial";

        public Color color;

        protected override void Apply(GameObject go, Challenge_Target target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt)
        {
            var d = go.AddComponent<Challenge_MaterialBehaviorD>();
            d.materialManager = manager.materialManager;
            d.materialKey = materialKey;

            // ✅ FindChildImage fonctionne car d est sur le go, pas besoin de target
            Image targetImage = d.FindChildImage(materialKey);
            if (targetImage == null) return;

            d.SetTarget(targetImage);

            
            // ✅ SetTarget(ITarget) pour que OnHitEvent fonctionne
            d.SetTarget(target);
            d.SetMaterial();

            var fillBehavior = go.AddComponent<Challenge_FillAmountBehavior>();
            fillBehavior.Initialize(d.targetMaterial, target);

            fillBehavior.SetColor(color);

            fillBehavior.StartFillOverTime(Challenge_GeneralVariables.GetEphemereTimeFromPrefs());
            d.AddBehavior(fillBehavior);
        }
    }
}