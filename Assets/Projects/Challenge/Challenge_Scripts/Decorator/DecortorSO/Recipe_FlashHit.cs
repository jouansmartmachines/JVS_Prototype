using UnityEngine;

namespace Challenge
{
    [CreateAssetMenu(menuName = "Challenge/Recipes/Flash Hit VFX")]
    public class Recipe_FlashHit : Challenge_DecoratorRecipe
    {
        public Color flashColor = Color.yellow;
        public float shakeIntensity = 60f;

        public override void Execute(GameObject go, Challenge_BaseInteractive target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt)
        {
            if (!CanApply(data)) return;

            // On récupère le renderer sur l'objet (ou ses enfants)
            Renderer rend = go.GetComponentInChildren<Renderer>();
            if (rend == null) return;

            // Ajout du composant
            var flashD = go.AddComponent<Challenge_FlashHitD>();
            
            // Configuration manuelle (on remplace le Setup car 'target' est la base)
            flashD.Initialize(target, rend, flashColor, shakeIntensity);
        }
    }
}