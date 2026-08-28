using UnityEngine;

namespace Challenge
{
    [CreateAssetMenu(menuName = "Challenge/Recipes/Movement")]
    public class Recipe_Movement : Challenge_DecoratorRecipe
    {
        [Header("Stratégie de mouvement")]
        [SerializeReference] // Permet d'assigner des classes C# pures dans l'inspecteur
        private IMovementStrategy strategy;

        public override void Execute(GameObject go, Challenge_BaseInteractive target, TargetCategory data, Challenge_SpawnManager manager, RectTransform rt)
        {
            if (!CanApply(data)) return;
            Transform[] spawnPoints = null;
            if (manager.spawnZone != null)
            {
                int childCount = manager.spawnZone.childCount;
                spawnPoints = new Transform[childCount];
                for (int i = 0; i < childCount; i++)
                {
                    spawnPoints[i] = manager.spawnZone.GetChild(i);
                }
            }

            var moveD = go.AddComponent<Challenge_MovementD>();
            
            moveD.movementStrategy = new Challenge_LateralMovementStrategy(go.transform.localPosition.x, spawnPoints)
            {

                minX = -500f, 
                maxX = 500f
            };
            
        }
    }
}