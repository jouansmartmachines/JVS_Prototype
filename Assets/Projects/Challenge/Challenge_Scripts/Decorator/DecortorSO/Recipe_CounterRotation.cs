using UnityEngine;

namespace Challenge
{
    /// <summary>
    /// Recipe qui fait tourner en sens opposés les 2 premiers enfants
    /// du premier enfant de target.FirstLayer, dès le spawn.
    ///
    /// Hiérarchie attendue :
    ///   FirstLayer
    ///   └── pivot  (child 0)
    ///       ├── elementA  (child 0)  → tourne à +speedA °/s
    ///       └── elementB  (child 1)  → tourne à -speedB °/s
    /// </summary>
    [CreateAssetMenu(menuName = "Challenge/Recipes/CounterRotation")]
    public class Recipe_CounterRotation : Challenge_DecoratorRecipe<Challenge_Target>
    {
        [Header("Rotation Speeds (degrés / seconde)")]
        [Tooltip("Vitesse de l'élément A (sens antihoraire si positif sur axe Z UI).")]
        public float speedA = 90f;

        [Tooltip("Vitesse de l'élément B (tournera dans le sens opposé à A).")]
        public float speedB = 90f;
        public GameObject rotatingCircles;

        // ----------------------------------------------------------------

        protected override void Apply(GameObject go,Challenge_Target target,TargetCategory data,Challenge_SpawnManager manager,RectTransform rt)
        {
            var d = go.AddComponent<Challenge_CounterRotationD>();
            d.SetTarget(target);
            d.Setup(target.mainLayer, rotatingCircles, speedA, speedB);
        }
    }
}
