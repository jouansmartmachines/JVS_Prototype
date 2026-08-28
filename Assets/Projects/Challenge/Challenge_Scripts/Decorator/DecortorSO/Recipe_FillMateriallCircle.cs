using UnityEngine;
using UnityEngine.UI;

namespace Challenge
{
    [CreateAssetMenu(menuName = "Challenge/Recipes/Fill Image Radial")]
    public class Recipe_FillImageRadial : Challenge_DecoratorRecipe<Challenge_Target>
    {
        public Color color = Color.white;
        public string imageName = "FillMaterial"; 

        protected override void Apply(GameObject go, Challenge_Target target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt)
        {
            // 1. Recherche de l'Image
            Image targetImage = null;
            foreach (Transform child in go.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == imageName)
                {
                    targetImage = child.GetComponent<Image>();
                    break;
                }
            }

            if (targetImage == null) return;

            // 2. Configuration UI (pour être sûr que le mode radial est actif)
            targetImage.type = Image.Type.Filled;
            targetImage.fillMethod = Image.FillMethod.Radial360;
            targetImage.fillOrigin = (int)Image.Origin360.Top;
            targetImage.color = color;

            // 3. Ajout et Initialisation du Decorator
            var fillBehavior = go.AddComponent<Challenge_RadialFillD>();
            
            // On passe la target (hérité de Challenge_TargetDecorator)
            fillBehavior.SetTarget(target); 
            
            // On passe l'image spécifique à gérer
            fillBehavior.Initialize(targetImage);
            
            // 4. Lancement de l'anim
            fillBehavior.StartFill(Challenge_GeneralVariables.GetEphemereTimeFromPrefs());
        }
    }
}