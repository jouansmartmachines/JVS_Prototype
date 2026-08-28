using UnityEngine;
using System; 

namespace Challenge
{
    [Serializable]
    public class RecipeCategory
    {
        public string categoryName; 
        public Challenge_DecoratorRecipe[] recipes;
    }

    public class Challenge_RecipeContainer : MonoBehaviour
    {
        [Header("Recipes by Category")]
        public RecipeCategory[] globalRecipes;
    }
}