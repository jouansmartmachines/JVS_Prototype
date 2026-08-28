using System;
using UnityEngine;

namespace Challenge
{
    [CreateAssetMenu(menuName = "Challenge/Recipes/Base TargetType")]
    public class Recipe_BaseTargetType : Challenge_DecoratorRecipe<Challenge_Target>
    {
        public float timeAdded;
        public float scoreMultiplier;
        //public TargetCategory category;
        public GameObject secondLayerPrefab;
        public GameObject vfxPrefab;
        public GameObject timePrefab;
        public int health = 5;

        [Header("Ephemere Only")]
        public float ephemereLifetime = 5f;

        protected override void Apply(GameObject go, Challenge_Target target, TargetCategory  data, Challenge_SpawnManager manager, RectTransform rt)
        {

            
            if (data == null || target == null) return;

            // --- Logique récupérée de ApplyBaseData ---
            target.timeAdded = timeAdded;
            target.scoreMultiplier = scoreMultiplier;
            //target.category.category = category;
            target.particlesEffects = vfxPrefab;
            //Debug.Log(target.particlesEffects.name);
            target.popUp = timePrefab;

            target.health = health;

            if (secondLayerPrefab != null && target.secondLayer != null)
            {
                // On instancie le visuel secondaire directement ici
                Instantiate(secondLayerPrefab, target.secondLayer.transform);
            }
            
            // Note : Pas besoin de "AddComponent" ici car on modifie 
            // directement les valeurs du composant Challenge_Target existant.
        }
    }
}