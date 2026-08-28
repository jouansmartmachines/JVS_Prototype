// Recipe_DoubleHit.cs
using UnityEngine;
using UnityEngine.UI;

namespace Challenge
{
    [CreateAssetMenu(menuName = "Challenge/Recipes/Double Hit (Evolutif)")]
    public class Recipe_DoubleHit : Challenge_DecoratorRecipe<Challenge_Target>
    {

        public string materialKey = "DoubleHitMaterial";

        public Sprite blueCircle;
        public Sprite greenCircle;

        [Header("Colors (if no children)")]
        public Color colorInit = Color.red;
        public Color colorHit = Color.white;

        protected override void Apply(GameObject go, Challenge_Target target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt)
        {
            /*var d = go.AddComponent<Challenge_MaterialBehaviorD>();
            d.materialManager = manager.materialManager;
            d.materialKey = materialKey;

            d.SetTarget(doubleHitImage);
            d.SetTarget(target);
            d.SetMaterial();
            */
            Image doubleHitImage = target.secondLayer.GetComponent<Image>();
            
            // On s'assure qu'elle est désactivée au début
            doubleHitImage.gameObject.SetActive(false);

            var hitBehavior = go.AddComponent<Challenge_MaterialOnHitBehavior>();
            
            hitBehavior.Initialize(target);
            hitBehavior.SetLayer(target.mainLayer); 
            hitBehavior.SetDoubleHitUI(doubleHitImage.gameObject);
            
            // On passe tout au behavior, il choisira selon la hiérarchie
            hitBehavior.Configure(blueCircle, greenCircle, colorInit, colorHit);
        }
    }
}