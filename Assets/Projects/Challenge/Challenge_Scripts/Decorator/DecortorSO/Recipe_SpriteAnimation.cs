using UnityEngine;

namespace Challenge
{
    /// <summary>
    /// Recipe qui ajoute une animation frame-by-frame à la mort d'une cible.
    /// Glissez vos sprites dans <see cref="animationFrames"/> dans l'ordre de lecture,
    /// réglez le FPS et la taille, puis assignez dans globalRecipes du SpawnManager.
    /// </summary>
    [CreateAssetMenu(menuName = "Challenge/Recipes/SpriteAnimation")]
    public class Recipe_SpriteAnimation : Challenge_DecoratorRecipe<Challenge_Target>
    {
        [Header("Animation Frames")]
        [Tooltip("Sprites jouées dans l'ordre à la mort de la cible.")]
        public Sprite[] animationFrames;

        [Header("Playback")]
        [Tooltip("Images par seconde.")]
        [Range(1, 60)]
        public float fps = 15f;

        [Header("Display")]
        [Tooltip("Taille (en pixels UI) de l'image affichée.")]
        public Vector2 displaySize = new Vector2(200f, 200f);


        protected override void Apply(GameObject go, Challenge_Target target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt)
        {
            if (animationFrames == null || animationFrames.Length == 0)
            {
                Debug.LogWarning("[Recipe_SpriteAnimation] Aucune frame assignée !", this);
                return;
            }
            var d = go.AddComponent<Challenge_SpriteAnimationD>();
            d.SetTarget(target);
            d.Setup(animationFrames, fps, target.EndLayer);
        }
    }
}
